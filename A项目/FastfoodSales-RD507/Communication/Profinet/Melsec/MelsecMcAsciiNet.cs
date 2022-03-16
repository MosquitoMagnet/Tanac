using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.Address;
using Communication.Core.IMessage;
using Communication.Core.Net;
using Communication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为ASCII通讯格式<br />
	/// Mitsubishi PLC communication class is implemented using Qna compatible 3E frame protocol. 
	/// The Ethernet module on the PLC side needs to be configured first. It must be ascii communication.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="MelsecMcNet" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage" title="简单的短连接使用" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage2" title="简单的长连接使用" />
	/// </example>
	public class MelsecMcAsciiNet : NetworkDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="MelsecMcNet()"/>
		public MelsecMcAsciiNet()
		{
			WordLength = 1;
			LogMsgFormatBinary = false;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc cref="MelsecMcNet(string,int)"/>
		public MelsecMcAsciiNet(string ipAddress, int port)
		{
			WordLength = 1;
			IpAddress = ipAddress;
			Port = port;
			LogMsgFormatBinary = false;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage() => new MelsecQnA3EAsciiMessage();

		#endregion

		#region Public Member

		/// <inheritdoc cref="MelsecMcNet.NetworkNumber"/>
		public byte NetworkNumber { get; set; } = 0x00;

		/// <inheritdoc cref="MelsecMcNet.NetworkStationNumber"/>
		public byte NetworkStationNumber { get; set; } = 0x00;

		#endregion

		#region Address Analysis

		/// <inheritdoc cref="MelsecMcNet.McAnalysisAddress(string, ushort)"/>
		protected virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length) => McAddressData.ParseMelsecFrom(address, length);

		#endregion

		#region Read Write Override

		/// <inheritdoc/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			List<byte> bytesContent = new List<byte>();
			ushort alreadyFinished = 0;
			while (alreadyFinished < length)
			{
				ushort readLength = (ushort)Math.Min(length - alreadyFinished, 450);
				addressResult.Content.Length = readLength;
				OperateResult<byte[]> read = ReadAddressData(addressResult.Content);
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

		private OperateResult<byte[]> ReadAddressData(McAddressData addressData)
		{
			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadMcCoreCommand(addressData, false);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content, false);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiWriteWordCoreCommand(addressResult.Content, value);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			List<byte> bytesContent = new List<byte>();
			ushort alreadyFinished = 0;
			while (alreadyFinished < length)
			{
				ushort readLength = (ushort)Math.Min(length - alreadyFinished, 450);
				addressResult.Content.Length = readLength;
				OperateResult<byte[]> read = await ReadAddressDataAsync(addressResult.Content);
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

		private async Task<OperateResult<byte[]>> ReadAddressDataAsync(McAddressData addressData)
		{
			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadMcCoreCommand(addressData, false);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content, false);
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiWriteWordCoreCommand(addressResult.Content, value);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Read Random

		/// <inheritdoc cref="MelsecMcNet.ReadRandom(string[])"/>
		[HslMqttApi]
		public OperateResult<byte[]> ReadRandom(string[] address)
		{
			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], 1);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadRandomWordCommand(mcAddressDatas);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content, false);
		}

		/// <inheritdoc cref="MelsecMcNet.ReadRandom(string[], ushort[])"/>
		public OperateResult<byte[]> ReadRandom(string[] address, ushort[] length)
		{
			if (length.Length != address.Length) return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);

			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], length[i]);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadRandomCommand(mcAddressDatas);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content, false);
		}

		/// <inheritdoc cref="MelsecMcNet.ReadRandomInt16(string[])"/>
		public OperateResult<short[]> ReadRandomInt16(string[] address)
		{
			OperateResult<byte[]> read = ReadRandom(address);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<short[]>(read);

			return OperateResult.CreateSuccessResult(ByteTransform.TransInt16(read.Content, 0, address.Length));
		}

		#endregion

		#region Async Read Random
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadRandom(string[])"/>
		public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address)
		{
			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], 1);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadRandomWordCommand(mcAddressDatas);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content, false);
		}

		/// <inheritdoc cref="ReadRandom(string[], ushort[])"/>
		public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address, ushort[] length)
		{
			if (length.Length != address.Length) return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);

			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], length[i]);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadRandomCommand(mcAddressDatas);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content, false);
		}

		/// <inheritdoc cref="ReadRandomInt16(string[])"/>
		public async Task<OperateResult<short[]>> ReadRandomInt16Async(string[] address)
		{
			OperateResult<byte[]> read = await ReadRandomAsync(address);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<short[]>(read);

			return OperateResult.CreateSuccessResult(ByteTransform.TransInt16(read.Content, 0, address.Length));
		}
#endif
		#endregion

		#region Bool Operate Support

		/// <inheritdoc/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = ExtractActualData(read.Content, true);
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			// 转化bool数组
			return OperateResult.CreateSuccessResult(extract.Content.Select(m => m == 0x01).Take(length).ToArray());
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return addressResult;

			// 解析指令
			byte[] coreResult = MelsecHelper.BuildAsciiWriteBitCoreCommand(addressResult.Content, values);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Bool Operate Support
#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = ExtractActualData(read.Content, true);
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			// 转化bool数组
			return OperateResult.CreateSuccessResult(extract.Content.Select(m => m == 0x01).Take(length).ToArray());
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return addressResult;

			// 解析指令
			byte[] coreResult = MelsecHelper.BuildAsciiWriteBitCoreCommand(addressResult.Content, values);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Remote Operate

		/// <inheritdoc cref="MelsecMcNet.RemoteRun"/>
		[HslMqttApi]
		public OperateResult RemoteRun()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("1001000000010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.RemoteStop"/>
		[HslMqttApi]
		public OperateResult RemoteStop()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("100200000001"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.RemoteReset"/>
		[HslMqttApi]
		public OperateResult RemoteReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("100600000001"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.ReadPlcType"/>
		[HslMqttApi]
		public OperateResult<string> ReadPlcType()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>(check);

			// 成功
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 22, 16).TrimEnd());
		}

		/// <inheritdoc cref="MelsecMcNet.ErrorStateReset"/>
		[HslMqttApi]
		public OperateResult ErrorStateReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Remote Operate
#if !NET35 && !NET20
		/// <inheritdoc cref="RemoteRun"/>
		public async Task<OperateResult> RemoteRunAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("1001000000010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="RemoteStop"/>
		public async Task<OperateResult> RemoteStopAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("100200000001"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="RemoteReset"/>
		public async Task<OperateResult> RemoteResetAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("100600000001"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="ReadPlcType"/>
		public async Task<OperateResult<string>> ReadPlcTypeAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>(check);

			// 成功
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 22, 16).TrimEnd());
		}

		/// <inheritdoc cref="ErrorStateReset"/>
		public async Task<OperateResult> ErrorStateResetAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"MelsecMcAsciiNet[{IpAddress}:{Port}]";

		#endregion

		#region Static Method Helper

		/// <summary>
		/// 将MC协议的核心报文打包成一个可以直接对PLC进行发送的原始报文
		/// </summary>
		/// <param name="mcCore">MC协议的核心报文</param>
		/// <param name="networkNumber">网络号</param>
		/// <param name="networkStationNumber">网络站号</param>
		/// <returns>原始报文信息</returns>
		public static byte[] PackMcCommand(byte[] mcCore, byte networkNumber = 0, byte networkStationNumber = 0)
		{
			byte[] plcCommand = new byte[22 + mcCore.Length];
			plcCommand[0] = 0x35;                                                                        // 副标题
			plcCommand[1] = 0x30;
			plcCommand[2] = 0x30;
			plcCommand[3] = 0x30;
			plcCommand[4] = SoftBasic.BuildAsciiBytesFrom(networkNumber)[0];                         // 网络号
			plcCommand[5] = SoftBasic.BuildAsciiBytesFrom(networkNumber)[1];
			plcCommand[6] = 0x46;                                                                        // PLC编号
			plcCommand[7] = 0x46;
			plcCommand[8] = 0x30;                                                                        // 目标模块IO编号
			plcCommand[9] = 0x33;
			plcCommand[10] = 0x46;
			plcCommand[11] = 0x46;
			plcCommand[12] = SoftBasic.BuildAsciiBytesFrom(networkStationNumber)[0];                  // 目标模块站号
			plcCommand[13] = SoftBasic.BuildAsciiBytesFrom(networkStationNumber)[1];
			plcCommand[14] = SoftBasic.BuildAsciiBytesFrom((ushort)(plcCommand.Length - 18))[0];     // 请求数据长度
			plcCommand[15] = SoftBasic.BuildAsciiBytesFrom((ushort)(plcCommand.Length - 18))[1];
			plcCommand[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(plcCommand.Length - 18))[2];
			plcCommand[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(plcCommand.Length - 18))[3];
			plcCommand[18] = 0x30;                                                                        // CPU监视定时器
			plcCommand[19] = 0x30;
			plcCommand[20] = 0x31;
			plcCommand[21] = 0x30;
			mcCore.CopyTo(plcCommand, 22);

			return plcCommand;
		}

		/// <summary>
		/// 从PLC反馈的数据中提取出实际的数据内容，需要传入反馈数据，是否位读取
		/// </summary>
		/// <param name="response">反馈的数据内容</param>
		/// <param name="isBit">是否位读取</param>
		/// <returns>解析后的结果对象</returns>
		public static OperateResult<byte[]> ExtractActualData(byte[] response, bool isBit)
		{
			if (isBit)
				return OperateResult.CreateSuccessResult(response.RemoveBegin(22).Select(m => m == 0x30 ? (byte)0x00 : (byte)0x01).ToArray());
			else
				return OperateResult.CreateSuccessResult(MelsecHelper.TransAsciiByteArrayToByteArray(response.RemoveBegin(22)));
		}

		/// <summary>
		/// 检查反馈的内容是否正确的
		/// </summary>
		/// <param name="content">MC的反馈的内容</param>
		/// <returns>是否正确</returns>
		public static OperateResult CheckResponseContent(byte[] content)
		{
			ushort errorCode = Convert.ToUInt16(Encoding.ASCII.GetString(content, 18, 4), 16);
			if (errorCode != 0) return new OperateResult(errorCode, MelsecHelper.GetErrorDescription(errorCode));

			return OperateResult.CreateSuccessResult();
		}

		#endregion
	}
}
