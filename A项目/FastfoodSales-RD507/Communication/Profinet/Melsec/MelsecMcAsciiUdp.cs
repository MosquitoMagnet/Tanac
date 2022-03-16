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

namespace Communication.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC通讯类，采用UDP的协议实现，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为ascii通讯<br />
	/// Mitsubishi PLC communication class is implemented using UDP protocol and Qna compatible 3E frame protocol. 
	/// The Ethernet module needs to be configured first on the PLC side, and it must be ascii communication.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="MelsecMcNet" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage" title="简单的短连接使用" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage2" title="简单的长连接使用" />
	/// </example>
	public class MelsecMcAsciiUdp : NetworkUdpDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="MelsecMcNet()"/>
		public MelsecMcAsciiUdp()
		{
			this.WordLength = 1;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc cref="MelsecMcNet(string,int)"/>
		public MelsecMcAsciiUdp(string ipAddress, int port)
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
			var read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcAsciiNet.ExtractActualData(read.Content, false);
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
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码验证
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 写入成功
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

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadRandomWordCommand(mcAddressDatas);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcAsciiNet.ExtractActualData(read.Content, false);
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
			var read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return MelsecMcAsciiNet.ExtractActualData(read.Content, false);
		}

		/// <inheritdoc cref="ReadRandomInt16(string[])"/>
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

			// 地址分析
			byte[] coreResult = MelsecHelper.BuildAsciiReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = MelsecMcAsciiNet.ExtractActualData(read.Content, true);
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
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码验证
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Remote Operate

		/// <inheritdoc cref="MelsecMcNet.RemoteRun"/>
		[HslMqttApi]
		public OperateResult RemoteRun()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(Encoding.ASCII.GetBytes("1001000000010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.RemoteStop"/>
		[HslMqttApi]
		public OperateResult RemoteStop()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(Encoding.ASCII.GetBytes("100200000001"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.RemoteReset"/>
		[HslMqttApi]
		public OperateResult RemoteReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(Encoding.ASCII.GetBytes("100600000001"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="MelsecMcNet.ReadPlcType"/>
		[HslMqttApi]
		public OperateResult<string> ReadPlcType()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			// 错误码校验
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>(check);

			// 成功
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 22, 16).TrimEnd());
		}

		/// <inheritdoc cref="MelsecMcNet.ErrorStateReset"/>
		[HslMqttApi]
		public OperateResult ErrorStateReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(MelsecMcAsciiNet.PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = MelsecMcAsciiNet.CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"MelsecMcAsciiUdp[{IpAddress}:{Port}]";

		#endregion
	}
}
