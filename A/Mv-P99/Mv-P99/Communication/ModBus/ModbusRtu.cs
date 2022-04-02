using Communication.BasicFramework;
using Communication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Core;
using Communication.Core.Address;
using Communication.Reflection;
using System.Threading.Tasks;


namespace Communication.ModBus
{
	/// <summary>
	/// Modbus-Rtu通讯协议的类库，多项式码0xA001，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明<br />
	/// Modbus-Rtu communication protocol class library, polynomial code 0xA001, supports standard function codes, 
	/// and also supports extended function code implementation. The address is in rich text. For details, see the remark
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="ModbusTcpNet" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="ModbusTcpNet" path="example"/>
	/// </example>
	public class ModbusRtu : SerialDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个Modbus-Rtu协议的客户端对象<br />
		/// Instantiate a client object of the Modbus-Rtu protocol
		/// </summary>
		public ModbusRtu()
		{
			this.ByteTransform = new ReverseWordTransform();
		}

		/// <summary>
		/// 指定客户端自己的站号来初始化<br />
		/// Specify the client's own station number to initialize
		/// </summary>
		/// <param name="station">客户端自身的站号</param>
		public ModbusRtu(byte station = 0x01)
		{
			this.station = station;
			this.ByteTransform = new ReverseWordTransform();
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

		/// <summary>
		/// 检查当前的Modbus-Rtu响应是否是正确的<br />
		/// Check if the current Modbus-Rtu response is correct
		/// </summary>
		/// <param name="send">发送的数据信息</param>
		/// <returns>带是否成功的结果数据</returns>
		protected virtual OperateResult<byte[]> CheckModbusTcpResponse(byte[] send)
		{
			// 追加crc
			send = ModbusInfo.PackCommandToRtu(send);

			// 核心交互
			OperateResult<byte[]> result = ReadBase(send);
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

		#region Read Support

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

			return OperateResult.CreateSuccessResult(read.Content.ToBoolArray(length));
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

		#region Async Read Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, short)"/>/param>
		public async override Task<OperateResult> WriteAsync(string address, short value) => await Task.Run(() => Write(address, value));

		/// <inheritdoc cref="Write(string, ushort)"/>/param>
		public async override Task<OperateResult> WriteAsync(string address, ushort value) => await Task.Run(() => Write(address, value));

		/// <inheritdoc cref="ReadCoil(string)"/>
		public async Task<OperateResult<bool>> ReadCoilAsync(string address) => await Task.Run(() => ReadCoil(address));

		/// <inheritdoc cref="ReadCoil(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length) => await Task.Run(() => ReadCoil(address, length));

		/// <inheritdoc cref="ReadDiscrete(string)"/>
		public async Task<OperateResult<bool>> ReadDiscreteAsync(string address) => await Task.Run(() => ReadDiscrete(address));

		/// <inheritdoc cref="ReadDiscrete(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length) => await Task.Run(() => ReadDiscrete(address, length));

		/// <inheritdoc cref="WriteOneRegister(string, short)"/>
		public async Task<OperateResult> WriteOneRegisterAsync(string address, short value) => await Task.Run(() => WriteOneRegister(address, value));

		/// <inheritdoc cref="WriteOneRegister(string, ushort)"/>
		public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value) => await Task.Run(() => WriteOneRegister(address, value));

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
		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync(string address, bool value) => await Task.Run(() => Write(address, value));
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"ModbusRtu[{PortName}:{BaudRate}]";

		#endregion

	}
}
