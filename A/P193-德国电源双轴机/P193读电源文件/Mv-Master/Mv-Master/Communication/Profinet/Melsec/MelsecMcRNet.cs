using Communication.Core;
using Communication.Core.Address;
using Communication.Core.IMessage;
using Communication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Reflection;
using System.Threading.Tasks;


namespace Communication.Profinet.Melsec
{
	/// <summary>
	/// 三菱的R系列的MC协议，支持的地址类型和 <see cref="MelsecMcNet"/> 有区别，详细请查看对应的API文档说明
	/// </summary>
	public class MelsecMcRNet : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化三菱R系列的Qna兼容3E帧协议的通讯对象<br />
		/// Instantiate the communication object of Mitsubishi's Qna compatible 3E frame protocol
		/// </summary>
		public MelsecMcRNet()
		{
			this.WordLength = 1;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <summary>
		/// 指定ip地址和端口号来实例化一个默认的对象<br />
		/// Specify the IP address and port number to instantiate a default object
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口</param>
		public MelsecMcRNet(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage() => new MelsecQnA3EBinaryMessage();

		#endregion

		#region Public Member

		/// <inheritdoc cref="MelsecMcNet.NetworkNumber"/>
		public byte NetworkNumber { get; set; } = 0x00;

		/// <inheritdoc cref="MelsecMcNet.NetworkStationNumber"/>
		public byte NetworkStationNumber { get; set; } = 0x00;

		#endregion

		#region Read Write

		/// <inheritdoc/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			List<byte> bytesContent = new List<byte>();
			ushort alreadyFinished = 0;
			while (alreadyFinished < length)
			{
				ushort readLength = (ushort)Math.Min(length - alreadyFinished, 900);
				addressResult.Content.Length = readLength;
				OperateResult<byte[]> read = ReadAddressData(addressResult.Content, false);
				if (!read.IsSuccess) return read;

				bytesContent.AddRange(read.Content);
				alreadyFinished += readLength;

				// 字的话就是正常的偏移位置，如果是位的话，就转到位的数据
				if (addressResult.Content.McDataType.DataType == 0)
					addressResult.Content.AddressStart += readLength;
				else
					addressResult.Content.AddressStart += readLength * 16;
			}
			return OperateResult.CreateSuccessResult(bytesContent.ToArray());
		}

		private OperateResult<byte[]> ReadAddressData(McRAddressData address, bool isBit)
		{
			byte[] coreResult = BuildReadMcCoreCommand(address, isBit);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcNet.ExtractActualData(read.Content.RemoveBegin(11), isBit);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, 0);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			return WriteAddressData(addressResult.Content, value);
		}

		private OperateResult WriteAddressData(McRAddressData addressData, byte[] value)
		{
			// 创建核心报文
			byte[] coreResult = BuildWriteWordCoreCommand(addressData, value);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Read Write Async
#if !NET35 && !NET20
		/// <inheritdoc/>
		public async override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			List<byte> bytesContent = new List<byte>();
			ushort alreadyFinished = 0;
			while (alreadyFinished < length)
			{
				ushort readLength = (ushort)Math.Min(length - alreadyFinished, 900);
				addressResult.Content.Length = readLength;
				OperateResult<byte[]> read = await ReadAddressDataAsync(addressResult.Content, false);
				if (!read.IsSuccess) return read;

				bytesContent.AddRange(read.Content);
				alreadyFinished += readLength;

				// 字的话就是正常的偏移位置，如果是位的话，就转到位的数据
				if (addressResult.Content.McDataType.DataType == 0)
					addressResult.Content.AddressStart += readLength;
				else
					addressResult.Content.AddressStart += readLength * 16;
			}
			return OperateResult.CreateSuccessResult(bytesContent.ToArray());
		}

		private async Task<OperateResult<byte[]>> ReadAddressDataAsync(McRAddressData address, bool isBit)
		{
			byte[] coreResult = BuildReadMcCoreCommand(address, isBit);

			// 核心交互
			var read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcNet.ExtractActualData(read.Content.RemoveBegin(11), isBit);
		}

		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, 0);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			return await WriteAddressDataAsync(addressResult.Content, value);
		}

		private async Task<OperateResult> WriteAddressDataAsync(McRAddressData addressData, byte[] value)
		{
			// 创建核心报文
			byte[] coreResult = BuildWriteWordCoreCommand(addressData, value);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Bool Operate Support

		/// <inheritdoc/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 获取指令
			byte[] coreResult = BuildReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = MelsecMcNet.ExtractActualData(read.Content.RemoveBegin(11), true);
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			// 转化bool数组
			return OperateResult.CreateSuccessResult(extract.Content.Select(m => m == 0x01).Take(length).ToArray());
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, 0);
			if (!addressResult.IsSuccess) return addressResult;

			byte[] coreResult = BuildWriteBitCoreCommand(addressResult.Content, values);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Bool Operate Support
#if !NET35 && !NET20
		/// <inheritdoc/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 获取指令
			byte[] coreResult = BuildReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = MelsecMcNet.ExtractActualData(read.Content.RemoveBegin(11), true);
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			// 转化bool数组
			return OperateResult.CreateSuccessResult(extract.Content.Select(m => m == 0x01).Take(length).ToArray());
		}

		/// <inheritdoc/>
		public async override Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			// 分析地址
			OperateResult<McRAddressData> addressResult = McRAddressData.ParseMelsecRFrom(address, 0);
			if (!addressResult.IsSuccess) return addressResult;

			byte[] coreResult = BuildWriteBitCoreCommand(addressResult.Content, values);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Static Helper

		/// <summary>
		/// 分析三菱R系列的地址，并返回解析后的数据对象
		/// </summary>
		/// <param name="address">字符串地址</param>
		/// <returns>是否解析成功</returns>
		public static OperateResult<MelsecMcRDataType, int> AnalysisAddress(string address)
		{
			try
			{
				if (address.StartsWith("LSTS")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LSTS, Convert.ToInt32(address.Substring(4), MelsecMcRDataType.LSTS.FromBase));
				else if (address.StartsWith("LSTC")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LSTC, Convert.ToInt32(address.Substring(4), MelsecMcRDataType.LSTC.FromBase));
				else if (address.StartsWith("LSTN")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LSTN, Convert.ToInt32(address.Substring(4), MelsecMcRDataType.LSTN.FromBase));
				else if (address.StartsWith("STS")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.STS, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.STS.FromBase));
				else if (address.StartsWith("STC")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.STC, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.STC.FromBase));
				else if (address.StartsWith("STN")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.STN, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.STN.FromBase));
				else if (address.StartsWith("LTS")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LTS, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.LTS.FromBase));
				else if (address.StartsWith("LTC")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LTC, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.LTC.FromBase));
				else if (address.StartsWith("LTN")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LTN, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.LTN.FromBase));
				else if (address.StartsWith("LCS")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LCS, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.LCS.FromBase));
				else if (address.StartsWith("LCC")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LCC, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.LCC.FromBase));
				else if (address.StartsWith("LCN")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.LCN, Convert.ToInt32(address.Substring(3), MelsecMcRDataType.LCN.FromBase));
				else if (address.StartsWith("TS")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.TS, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.TS.FromBase));
				else if (address.StartsWith("TC")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.TC, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.TC.FromBase));
				else if (address.StartsWith("TN")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.TN, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.TN.FromBase));
				else if (address.StartsWith("CS")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.CS, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.CS.FromBase));
				else if (address.StartsWith("CC")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.CC, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.CC.FromBase));
				else if (address.StartsWith("CN")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.CN, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.CN.FromBase));
				else if (address.StartsWith("SM")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.SM, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.SM.FromBase));
				else if (address.StartsWith("SB")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.SB, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.SB.FromBase));
				else if (address.StartsWith("DX")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.DX, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.DX.FromBase));
				else if (address.StartsWith("DY")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.DY, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.DY.FromBase));
				else if (address.StartsWith("SD")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.SD, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.SD.FromBase));
				else if (address.StartsWith("SW")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.SW, Convert.ToInt32(address.Substring(2), MelsecMcRDataType.SW.FromBase));
				else if (address.StartsWith("X")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.X, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.X.FromBase));
				else if (address.StartsWith("Y")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.Y, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.Y.FromBase));
				else if (address.StartsWith("M")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.M, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.M.FromBase));
				else if (address.StartsWith("L")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.L, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.L.FromBase));
				else if (address.StartsWith("F")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.F, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.F.FromBase));
				else if (address.StartsWith("V")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.V, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.V.FromBase));
				else if (address.StartsWith("S")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.S, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.S.FromBase));
				else if (address.StartsWith("B")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.B, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.B.FromBase));
				else if (address.StartsWith("D")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.D, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.D.FromBase));
				else if (address.StartsWith("W")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.W, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.W.FromBase));
				else if (address.StartsWith("R")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.R, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.R.FromBase));
				else if (address.StartsWith("Z")) return OperateResult.CreateSuccessResult(MelsecMcRDataType.Z, Convert.ToInt32(address.Substring(1), MelsecMcRDataType.Z.FromBase));
				else return new OperateResult<MelsecMcRDataType, int>(StringResources.Language.NotSupportedDataType);
			}
			catch (Exception ex)
			{
				return new OperateResult<MelsecMcRDataType, int>(ex.Message);
			}
		}

		/// <summary>
		/// 从三菱地址，是否位读取进行创建读取的MC的核心报文
		/// </summary>
		/// <param name="address">地址数据</param>
		/// <param name="isBit">是否进行了位读取操作</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildReadMcCoreCommand(McRAddressData address, bool isBit)
		{
			byte[] command = new byte[12];
			command[0] = 0x01;                                                      // 批量读取数据命令
			command[1] = 0x04;
			command[2] = isBit ? (byte)0x01 : (byte)0x00;                           // 以点为单位还是字为单位成批读取
			command[3] = 0x00;
			command[4] = BitConverter.GetBytes(address.AddressStart)[0];          // 起始地址的地位
			command[5] = BitConverter.GetBytes(address.AddressStart)[1];
			command[6] = BitConverter.GetBytes(address.AddressStart)[2];
			command[7] = BitConverter.GetBytes(address.AddressStart)[3];
			command[8] = address.McDataType.DataCode[0];                            // 指明读取的数据
			command[9] = address.McDataType.DataCode[1];
			command[10] = (byte)(address.Length % 256);                              // 软元件的长度
			command[11] = (byte)(address.Length / 256);

			return command;
		}

		/// <summary>
		/// 以字为单位，创建数据写入的核心报文
		/// </summary>
		/// <param name="address">三菱的数据地址</param>
		/// <param name="value">实际的原始数据信息</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildWriteWordCoreCommand(McRAddressData address, byte[] value)
		{
			if (value == null) value = new byte[0];
			byte[] command = new byte[12 + value.Length];
			command[0] = 0x01;                                                        // 批量写入数据命令
			command[1] = 0x14;
			command[2] = 0x00;                                                        // 以字为单位成批读取
			command[3] = 0x00;
			command[4] = BitConverter.GetBytes(address.AddressStart)[0];            // 起始地址的地位
			command[5] = BitConverter.GetBytes(address.AddressStart)[1];
			command[6] = BitConverter.GetBytes(address.AddressStart)[2];
			command[7] = BitConverter.GetBytes(address.AddressStart)[3];
			command[8] = address.McDataType.DataCode[0];                              // 指明读取的数据
			command[9] = address.McDataType.DataCode[1];
			command[10] = (byte)(value.Length / 2 % 256);                              // 软元件长度的地位
			command[11] = (byte)(value.Length / 2 / 256);
			value.CopyTo(command, 12);

			return command;
		}

		/// <summary>
		/// 以位为单位，创建数据写入的核心报文
		/// </summary>
		/// <param name="address">三菱的地址信息</param>
		/// <param name="value">原始的bool数组数据</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildWriteBitCoreCommand(McRAddressData address, bool[] value)
		{
			if (value == null) value = new bool[0];
			byte[] buffer = MelsecHelper.TransBoolArrayToByteData(value);
			byte[] command = new byte[12 + buffer.Length];
			command[0] = 0x01;                                                        // 批量写入数据命令
			command[1] = 0x14;
			command[2] = 0x01;                                                        // 以位为单位成批写入
			command[3] = 0x00;
			command[4] = BitConverter.GetBytes(address.AddressStart)[0];            // 起始地址的地位
			command[5] = BitConverter.GetBytes(address.AddressStart)[1];
			command[6] = BitConverter.GetBytes(address.AddressStart)[2];
			command[7] = BitConverter.GetBytes(address.AddressStart)[3];
			command[8] = address.McDataType.DataCode[0];                              // 指明读取的数据
			command[9] = address.McDataType.DataCode[1];
			command[10] = (byte)(value.Length % 256);                                  // 软元件长度的地位
			command[11] = (byte)(value.Length / 256);
			buffer.CopyTo(command, 12);

			return command;
		}

		#endregion
	}
}
