using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.Address;
using Communication.Core.Net;
using Communication.Serial;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.ModBus
{
	/// <inheritdoc cref="ModbusRtu"/>
	public class ModbusRtuOverTcp : NetworkDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="ModbusRtu()"/>
		public ModbusRtuOverTcp()
		{
			ByteTransform = new ReverseWordTransform();
			WordLength = 1;
			station = 1;
			this.SleepTime = 20;
		}

		/// <inheritdoc cref="ModbusTcpNet(string,int,byte)"/>
		public ModbusRtuOverTcp(string ipAddress, int port = 502, byte station = 0x01)
		{
			ByteTransform = new ReverseWordTransform();
			IpAddress = ipAddress;
			Port = port;
			WordLength = 1;
			this.station = station;
			this.SleepTime = 20;
		}

		#endregion

		#region Private Member

		private byte station = 0x01;                                 // 本客户端的站号
		private bool isAddressStartWithZero = true;                  // 线圈值的地址值是否从零开始

		#endregion

		#region Public Member

		/// <inheritdoc cref="ModbusTcpNet.AddressStartWithZero"/>
		public bool AddressStartWithZero
		{
			get { return isAddressStartWithZero; }
			set { isAddressStartWithZero = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.Station"/>
		public byte Station
		{
			get { return station; }
			set { station = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.DataFormat"/>
		public DataFormat DataFormat
		{
			get { return ByteTransform.DataFormat; }
			set { ByteTransform.DataFormat = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.IsStringReverse"/>
		public bool IsStringReverse
		{
			get { return ByteTransform.IsStringReverseByteWord; }
			set { ByteTransform.IsStringReverseByteWord = value; }
		}

		#endregion

		#region Core Interative

		/// <inheritdoc cref="ModbusRtu.CheckModbusTcpResponse(byte[])"/>
		protected virtual OperateResult<byte[]> CheckModbusTcpResponse(byte[] send)
		{
			// 追加crc
			send = ModbusInfo.PackCommandToRtu(send);

			// 核心交互
			OperateResult<byte[]> result = ReadFromCoreServer(send);
			if (!result.IsSuccess) return result;

			// 长度校验
			if (result.Content.Length < 5) return new OperateResult<byte[]>(StringResources.Language.ReceiveDataLengthTooShort + "5");

			// 检查crc
			if (!SoftCRC16.CheckCRC16(result.Content)) return new OperateResult<byte[]>(StringResources.Language.ModbusCRCCheckFailed +
			  SoftBasic.ByteToHexString(result.Content, ' '));

			// 发生了错误
			if ((send[1] + 0x80) == result.Content[1]) return new OperateResult<byte[]>(result.Content[2], ModbusInfo.GetDescriptionByErrorCode(result.Content[2]));

			if (send[1] != result.Content[1]) return new OperateResult<byte[]>(result.Content[1], $"Receive Command Check Failed: ");

			// 移除CRC校验，返回真实数据
			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeRtuCommandToCore(result.Content));
		}

		#endregion

		#region Async Core Interative
#if !NET35 && !NET20
		/// <inheritdoc cref="CheckModbusTcpResponse(byte[])"/>
		protected virtual async Task<OperateResult<byte[]>> CheckModbusTcpResponseAsync(byte[] send)
		{
			// 追加crc
			send = ModbusInfo.PackCommandToRtu(send);

			// 核心交互
			OperateResult<byte[]> result = await ReadFromCoreServerAsync(send);
			if (!result.IsSuccess) return result;

			// 长度校验
			if (result.Content.Length < 5) return new OperateResult<byte[]>(StringResources.Language.ReceiveDataLengthTooShort + "5");

			// 检查crc
			if (!SoftCRC16.CheckCRC16(result.Content)) return new OperateResult<byte[]>(StringResources.Language.ModbusCRCCheckFailed +
			  SoftBasic.ByteToHexString(result.Content, ' '));

			// 发生了错误
			if ((send[1] + 0x80) == result.Content[1]) return new OperateResult<byte[]>(result.Content[2], ModbusInfo.GetDescriptionByErrorCode(result.Content[2]));

			if (send[1] != result.Content[1]) return new OperateResult<byte[]>(result.Content[1], $"Receive Command Check Failed: ");

			// 移除CRC校验，返回真实数据
			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeRtuCommandToCore(result.Content));
		}
#endif
		#endregion

		#region Read Write Support

		/// <inheritdoc cref="ModbusTcpNet.ReadModBus(ModbusAddress, ushort)"/>
		private OperateResult<byte[]> ReadModBus(ModbusAddress address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length);
			if (!command.IsSuccess) return command;

			return CheckModbusTcpResponse(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.ReadCoil(string)"/>
		public OperateResult<bool> ReadCoil(string address) => ReadBool(address);

		/// <inheritdoc cref="ModbusTcpNet.ReadCoil(string, ushort)"/>
		public OperateResult<bool[]> ReadCoil(string address, ushort length) => ReadBool(address, length);

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscrete(string)"/>
		public OperateResult<bool> ReadDiscrete(string address) => ByteTransformHelper.GetResultFromArray(ReadDiscrete(address, 1));

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscrete(string, ushort)"/>
		public OperateResult<bool[]> ReadDiscrete(string address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length, Station, AddressStartWithZero, ModbusInfo.ReadDiscrete);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			OperateResult<byte[]> read = CheckModbusTcpResponse(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(read.Content, length));
		}

		/// <inheritdoc cref="ModbusTcpNet.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, Station, isAddressStartWithZero, ModbusInfo.ReadRegister);
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(analysis);

			List<byte> lists = new List<byte>();
			ushort alreadyFinished = 0;
			while (alreadyFinished < length)
			{
				ushort lengthTmp = (ushort)Math.Min((length - alreadyFinished), 120);
				OperateResult<byte[]> read = ReadModBus(analysis.Content.AddressAdd(alreadyFinished), lengthTmp);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

				lists.AddRange(read.Content);
				alreadyFinished += lengthTmp;
			}
			return OperateResult.CreateSuccessResult(lists.ToArray());
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteRegister);
			if (!command.IsSuccess) return command;

			// 核心交互
			return CheckModbusTcpResponse(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, short)"/>
		[HslMqttApi("WriteInt16", "")]
		public override OperateResult Write(string address, short value)
		{
			// 解析指令
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			// 核心交互
			return CheckModbusTcpResponse(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, ushort)"/>
		[HslMqttApi("WriteUInt16", "")]
		public override OperateResult Write(string address, ushort value)
		{
			// 解析指令
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			// 核心交互
			return CheckModbusTcpResponse(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.WriteMask(string, ushort, ushort)"/>
		[HslMqttApi("WriteMask", "")]
		public OperateResult WriteMask(string address, ushort andMask, ushort orMask)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteMaskModbusCommand(address, andMask, orMask, Station, AddressStartWithZero, ModbusInfo.WriteMaskRegister);
			if (!command.IsSuccess) return command;

			return CheckModbusTcpResponse(command.Content);
		}

		#endregion

		#region Write One Registe

		/// <inheritdoc cref="Write(string, short)"/>
		public OperateResult WriteOneRegister(string address, short value) => Write(address, value);

		/// <inheritdoc cref="Write(string, ushort)"/>
		public OperateResult WriteOneRegister(string address, ushort value) => Write(address, value);

		#endregion

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadModBus(ModbusAddress, ushort)"/>
		private async Task<OperateResult<byte[]>> ReadModBusAsync(ModbusAddress address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length);
			if (!command.IsSuccess) return command;

			return await CheckModbusTcpResponseAsync(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.ReadCoilAsync(string)"/>
		public async Task<OperateResult<bool>> ReadCoilAsync(string address) => await ReadBoolAsync(address);

		/// <inheritdoc cref="ModbusTcpNet.ReadCoilAsync(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length) => await ReadBoolAsync(address, length);

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscreteAsync(string)"/>
		public async Task<OperateResult<bool>> ReadDiscreteAsync(string address) => ByteTransformHelper.GetResultFromArray(await ReadDiscreteAsync(address, 1));

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscreteAsync(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length, Station, AddressStartWithZero, ModbusInfo.ReadDiscrete);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			OperateResult<byte[]> read = await CheckModbusTcpResponseAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(read.Content, length));
		}

		/// <inheritdoc cref="ModbusTcpNet.ReadAsync(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, Station, isAddressStartWithZero, ModbusInfo.ReadRegister);
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(analysis);

			List<byte> lists = new List<byte>();
			ushort alreadyFinished = 0;
			while (alreadyFinished < length)
			{
				ushort lengthTmp = (ushort)Math.Min((length - alreadyFinished), 120);
				OperateResult<byte[]> read = await ReadModBusAsync(analysis.Content.AddressAdd(alreadyFinished), lengthTmp);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

				lists.AddRange(read.Content);
				alreadyFinished += lengthTmp;
			}
			return OperateResult.CreateSuccessResult(lists.ToArray());
		}

		/// <inheritdoc cref="ModbusTcpNet.WriteAsync(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteRegister);
			if (!command.IsSuccess) return command;

			// 核心交互
			return await CheckModbusTcpResponseAsync(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.WriteOneRegisterAsync(string, short)"/>
		public async Task<OperateResult> WriteOneRegisterAsync(string address, short value)
		{
			// 解析指令
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			// 核心交互
			return await CheckModbusTcpResponseAsync(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.WriteOneRegisterAsync(string, ushort)"/>
		public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value)
		{
			// 解析指令
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			// 核心交互
			return await CheckModbusTcpResponseAsync(command.Content);
		}

		/// <inheritdoc cref="WriteMask(string, ushort, ushort)"/>
		public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask) => await Task.Run(() => WriteMask(address, andMask, orMask));
#endif
		#endregion

		#region Bool Support

		/// <inheritdoc cref="ModbusTcpNet.ReadBool(string, ushort)"/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length, Station, AddressStartWithZero, ModbusInfo.ReadCoil);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			OperateResult<byte[]> read = CheckModbusTcpResponse(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(read.Content, length));
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool[])"/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, values, Station, AddressStartWithZero, ModbusInfo.WriteCoil);
			if (!command.IsSuccess) return command;

			return CheckModbusTcpResponse(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool)"/>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneCoil);
			if (!command.IsSuccess) return command;

			return CheckModbusTcpResponse(command.Content);
		}

		#endregion

		#region Async Bool Support
#if !NET35 && !NET20
		/// <inheritdoc cref="ModbusTcpNet.ReadBoolAsync(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length, Station, AddressStartWithZero, ModbusInfo.ReadCoil);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			OperateResult<byte[]> read = await CheckModbusTcpResponseAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(read.Content, length));
		}

		/// <inheritdoc cref="ModbusTcpNet.WriteAsync(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, values, Station, AddressStartWithZero, ModbusInfo.WriteCoil);
			if (!command.IsSuccess) return command;

			return await CheckModbusTcpResponseAsync(command.Content);
		}

		/// <inheritdoc cref="ModbusTcpNet.WriteAsync(string, bool)"/>
		public override async Task<OperateResult> WriteAsync(string address, bool value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneCoil);
			if (!command.IsSuccess) return command;

			return await CheckModbusTcpResponseAsync(command.Content);
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"ModbusRtuOverTcp[{IpAddress}:{Port}]";

		#endregion
	}
}
