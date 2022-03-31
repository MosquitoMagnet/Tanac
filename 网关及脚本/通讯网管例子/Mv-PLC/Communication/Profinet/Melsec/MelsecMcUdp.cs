using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.IMessage;
using Communication.Core.Net;
using Communication.Core.Address;
using Communication.Reflection;

namespace Communication.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC通讯类，采用UDP的协议实现，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯<br />
	/// Mitsubishi PLC communication class is implemented using UDP protocol and Qna compatible 3E frame protocol. 
	/// The Ethernet module needs to be configured first on the PLC side, and it must be binary communication.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="MelsecMcNet" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage" title="简单的短连接使用" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage2" title="简单的长连接使用" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample1" title="基本的读取示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample2" title="批量读取示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample3" title="随机字读取示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample4" title="随机批量字读取示例" />
	/// </example>
	public class MelsecMcUdp : NetworkUdpDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="MelsecMcNet()"/>
		public MelsecMcUdp()
		{
			this.WordLength = 1;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc cref="MelsecMcNet(string,int)"/>
		public MelsecMcUdp(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			this.ByteTransform = new RegularByteTransform();
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="MelsecMcNet.NetworkNumber"/>
		public byte NetworkNumber { get; set; } = 0x00;

		/// <inheritdoc cref="MelsecMcNet.NetworkStationNumber"/>
		public byte NetworkStationNumber { get; set; } = 0x00;


		#endregion

		#region Virtual Address Analysis

		/// <inheritdoc cref="MelsecMcNet.McAnalysisAddress(string, ushort)"/>
		protected virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length) => McAddressData.ParseMelsecFrom(address, length);

		#endregion

		#region Read Write Support

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
				ushort readLength = (ushort)Math.Min(length - alreadyFinished, 900);
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
			byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressData, false);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(read.Content, 11), false);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			return WriteAddressData(addressResult.Content, value);
		}

		private OperateResult WriteAddressData(McAddressData addressData, byte[] value)
		{
			// 创建核心报文
			byte[] coreResult = MelsecHelper.BuildWriteWordCoreCommand(addressData, value);

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

		#region Read Random

		/// <inheritdoc cref="MelsecMcNet.ReadRandom(string[])"/>
		public OperateResult<byte[]> ReadRandom(string[] address)
		{
			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], 1);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			byte[] coreResult = MelsecHelper.BuildReadRandomWordCommand(mcAddressDatas);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(read.Content, 11), false);
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

			byte[] coreResult = MelsecHelper.BuildReadRandomCommand(mcAddressDatas);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(read.Content, 11), false);
		}

		/// <inheritdoc cref="MelsecMcNet.ReadRandomInt16(string[])"/>
		public OperateResult<short[]> ReadRandomInt16(string[] address)
		{
			OperateResult<byte[]> read = ReadRandom(address);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<short[]>(read);

			return OperateResult.CreateSuccessResult(ByteTransform.TransInt16(read.Content, 0, address.Length));
		}

		#endregion

		#region Bool Operate Support

		/// <inheritdoc/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 获取指令
			byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = MelsecMcNet.ExtractActualData(SoftBasic.ArrayRemoveBegin(read.Content, 11), true);
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

			byte[] coreResult = MelsecHelper.BuildWriteBitCoreCommand(addressResult.Content, values);

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

		#region Remote Operate

		/// <inheritdoc cref="MelsecMcNet.RemoteRun"/>
		[HslMqttApi]
		public OperateResult RemoteRun()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[] { 0x01, 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.RemoteStop"/>
		[HslMqttApi]
		public OperateResult RemoteStop()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[] { 0x02, 0x10, 0x00, 0x00, 0x01, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.RemoteReset"/>
		[HslMqttApi]
		public OperateResult RemoteReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[] { 0x06, 0x10, 0x00, 0x00, 0x01, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.ReadPlcType"/>
		[HslMqttApi]
		public OperateResult<string> ReadPlcType()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[] { 0x01, 0x01, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>(check);

			// 成功
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 11, 16).TrimEnd());
		}

		/// <inheritdoc cref="MelsecMcNet.ErrorStateReset"/>
		[HslMqttApi]
		public OperateResult ErrorStateReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcNet.PackMcCommand(new byte[] { 0x17, 0x16, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"MelsecMcUdp[{IpAddress}:{Port}]";

		#endregion
	}
}
