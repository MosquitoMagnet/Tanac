using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.Address;
using Communication.Core.Net;
using Communication.Reflection;
using System.Threading.Tasks;


namespace Communication.ModBus
{
	/// <summary>
	/// Modbus-Udp协议的客户端通讯类，方便的和服务器进行数据交互，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明<br />
	/// The client communication class of Modbus-Udp protocol is convenient for data interaction with the server. It supports standard function codes and also supports extended function codes. 
	/// The address is in rich text. For details, see the remarks.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="ModbusTcpNet" path="remarks"/>
	/// </remarks>
	/// <example>
	/// <inheritdoc cref="ModbusTcpNet" path="example"/>
	/// </example>
	public class ModbusUdpNet : NetworkUdpDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个MOdbus-Udp协议的客户端对象<br />
		/// Instantiate a client object of the MOdbus-Udp protocol
		/// </summary>
		public ModbusUdpNet()
		{
			ByteTransform = new ReverseWordTransform();
			softIncrementCount = new SoftIncrementCount(ushort.MaxValue);
			WordLength = 1;
			station = 1;
		}

		/// <inheritdoc cref="ModbusTcpNet(string,int,byte)"/>
		public ModbusUdpNet(string ipAddress, int port = 502, byte station = 0x01)
		{
			ByteTransform = new ReverseWordTransform();
			softIncrementCount = new SoftIncrementCount(ushort.MaxValue);
			IpAddress = ipAddress;
			Port = port;
			WordLength = 1;
			this.station = station;
		}

		#endregion

		#region Private Member

		private byte station = 0x01;                                // 本客户端的站号
		private SoftIncrementCount softIncrementCount;              // 自增消息的对象
		private bool isAddressStartWithZero = true;                 // 线圈值的地址值是否从零开始

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

		/// <inheritdoc cref="ModbusTcpNet.MessageId"/>
		public SoftIncrementCount MessageId => softIncrementCount;

		#endregion

		#region Read Support

		/// <inheritdoc cref="ModbusTcpNet.ReadCoil(string)"/>
		public OperateResult<bool> ReadCoil(string address) => ReadBool(address);

		/// <inheritdoc cref="ModbusTcpNet.ReadCoil(string, ushort)"/>
		public OperateResult<bool[]> ReadCoil(string address, ushort length) => ReadBool(address, length);

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscrete(string)"/>
		public OperateResult<bool> ReadDiscrete(string address) => ByteTransformHelper.GetResultFromArray(ReadDiscrete(address, 1));

		/// <inheritdoc cref="ReadDiscrete(string, ushort)"/>
		public OperateResult<bool[]> ReadDiscrete(string address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length, Station, AddressStartWithZero, ModbusInfo.ReadDiscrete);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			OperateResult<byte[]> read = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			OperateResult<byte[]> extract = ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(read.Content));
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(extract.Content, length));
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

		/// <inheritdoc cref="ModbusTcpNet.ReadModBus(ModbusAddress, ushort)"/>
		private OperateResult<byte[]> ReadModBus(ModbusAddress address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!read.IsSuccess) return read;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(read.Content));
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, short)"/>
		[HslMqttApi("WriteInt16", "")]
		public override OperateResult Write(string address, short value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, ushort)"/>
		[HslMqttApi("WriteUInt16", "")]
		public override OperateResult Write(string address, ushort value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="ModbusTcpNet.WriteMask(string, ushort, ushort)"/>
		[HslMqttApi("WriteMask", "")]
		public OperateResult WriteMask(string address, ushort andMask, ushort orMask)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteMaskModbusCommand(address, andMask, orMask, Station, AddressStartWithZero, ModbusInfo.WriteMaskRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		#endregion

		#region Write One Register

		/// <inheritdoc cref="ModbusTcpNet.WriteOneRegister(string, short)"/>
		public OperateResult WriteOneRegister(string address, short value) => Write(address, value);

		/// <inheritdoc cref="ModbusTcpNet.WriteOneRegister(string, ushort)"/>
		public OperateResult WriteOneRegister(string address, ushort value) => Write(address, value);

		#endregion

		#region Async Read Support
#if !NET35 && !NET20

		/// <inheritdoc cref="ModbusTcpNet.ReadCoilAsync(string)"/>
		public async Task<OperateResult<bool>> ReadCoilAsync(string address) => await Task.Run(() => ReadCoil(address));

		/// <inheritdoc cref="ModbusTcpNet.ReadCoilAsync(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length) => await Task.Run(() => ReadCoil(address, length));

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscreteAsync(string)"/>
		public async Task<OperateResult<bool>> ReadDiscreteAsync(string address) => await Task.Run(() => ReadDiscrete(address));

		/// <inheritdoc cref="ModbusTcpNet.ReadDiscreteAsync(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length) => await Task.Run(() => ReadDiscrete(address, length));

		/// <inheritdoc cref="Write(string, short)"/>/param>
		public async override Task<OperateResult> WriteAsync(string address, short value) => await Task.Run(() => Write(address, value));

		/// <inheritdoc cref="Write(string, ushort)"/>/param>
		public async override Task<OperateResult> WriteAsync(string address, ushort value) => await Task.Run(() => Write(address, value));

		/// <inheritdoc cref="ModbusTcpNet.WriteOneRegister(string, short)"/>
		public async Task<OperateResult> WriteOneRegisterAsync(string address, short value) => await Task.Run(() => WriteOneRegister(address, value));

		/// <inheritdoc cref="ModbusTcpNet.WriteOneRegister(string, ushort)"/>
		public async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value) => await Task.Run(() => WriteOneRegister(address, value));

		/// <inheritdoc cref="ModbusTcpNet.WriteMask(string, ushort, ushort)"/>
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

			OperateResult<byte[]> read = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			OperateResult<byte[]> extract = ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(read.Content));
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(extract.Content, length));
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool[])"/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, values, Station, AddressStartWithZero, ModbusInfo.WriteCoil);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool)"/>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneCoil);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		#endregion

		#region Async Bool Support
#if !NET35 && !NET20
		/// <inheritdoc cref="ModbusTcpNet.WriteAsync(string, bool)"/>
		public override async Task<OperateResult> WriteAsync(string address, bool value) => await Task.Run(() => Write(address, value));
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"ModbusUdpNet[{IpAddress}:{Port}]";

		#endregion
	}
}
