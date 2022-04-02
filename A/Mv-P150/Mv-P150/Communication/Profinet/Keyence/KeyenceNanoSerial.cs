using Communication.BasicFramework;
using Communication.Core;
using Communication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Reflection;

namespace Communication.Profinet.Keyence
{
	/// <summary>
	/// 基恩士KV上位链路串口通信的对象,适用于Nano系列串口数据,KV1000以及L20V通信模块，地址格式参考api文档<br />
	/// Keyence KV upper link serial communication object, suitable for Nano series serial data, and L20V communication module, please refer to api document for address format
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="KeyenceNanoSerialOverTcp" path="remarks"/>
	/// </remarks>
	public class KeyenceNanoSerial : SerialDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化基恩士的串口协议的通讯对象<br />
		/// Instantiate the communication object of Keyence's serial protocol
		/// </summary>
		public KeyenceNanoSerial()
		{
			this.ByteTransform = new RegularByteTransform();
			this.WordLength = 1;
		}

		/// <inheritdoc/>
		protected override OperateResult InitializationOnOpen()
		{
			// 建立通讯连接{CR/r}
			var result = ReadBase(KeyenceNanoSerialOverTcp.ConnectCmd);
			if (!result.IsSuccess) return result;

			if (result.Content.Length > 2)
				if (result.Content[0] == 0x43 && result.Content[1] == 0x43)
					return OperateResult.CreateSuccessResult();

			return new OperateResult("Check Failed: " + SoftBasic.ByteToHexString(result.Content, ' '));
		}

		/// <inheritdoc/>
		protected override OperateResult ExtraOnClose()
		{
			// 断开通讯连接{CR/r}
			var result = ReadBase(KeyenceNanoSerialOverTcp.DisConnectCmd);
			if (!result.IsSuccess) return result;

			if (result.Content.Length > 2)
				if (result.Content[0] == 0x43 && result.Content[1] == 0x46)
					return OperateResult.CreateSuccessResult();

			return new OperateResult("Check Failed: " + SoftBasic.ByteToHexString(result.Content, ' '));
		}

		#endregion

		#region Read Write Support

		/// <inheritdoc/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 获取指令
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 反馈检查
			OperateResult ackResult = KeyenceNanoSerialOverTcp.CheckPlcReadResponse(read.Content);
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(ackResult);

			var addressResult = KeyenceNanoSerialOverTcp.KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			// 数据提炼
			return KeyenceNanoSerialOverTcp.ExtractActualData(addressResult.Content1, read.Content);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 获取写入
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildWriteCommand(address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = KeyenceNanoSerialOverTcp.CheckPlcWriteResponse(read.Content);
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Read Write Bool

		/// <inheritdoc/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 获取指令
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 反馈检查
			OperateResult ackResult = KeyenceNanoSerialOverTcp.CheckPlcReadResponse(read.Content);
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(ackResult);

			var addressResult = KeyenceNanoSerialOverTcp.KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 数据提炼
			return KeyenceNanoSerialOverTcp.ExtractActualBoolData(addressResult.Content1, read.Content);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			// 获取写入
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildWriteCommand(address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = KeyenceNanoSerialOverTcp.CheckPlcWriteResponse(read.Content);
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"KeyenceNanoSerial[{PortName}:{BaudRate}]";

		#endregion

	}
}
