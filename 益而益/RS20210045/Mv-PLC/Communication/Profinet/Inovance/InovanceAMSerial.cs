using System.Linq;
using System.Text;
using Communication;
using Communication.ModBus;
using Communication.Reflection;
using System.Threading.Tasks;


namespace Communication.Profinet.Inovance
{
	/// <summary>
	/// 汇川的串口通信协议，适用于AM400、 AM400_800、 AC800 等系列，底层走的是MODBUS-RTU协议，地址说明参见标记<br />
	/// Huichuan's serial communication protocol is applicable to AM400, AM400_800, AC800 and other series. The bottom layer is MODBUS-RTU protocol. For the address description, please refer to the mark
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="InovanceAMTcp" path="remarks"/>
	/// </remarks>
	public class InovanceAMSerial : ModbusRtu
	{
		#region Constructor

		/// <inheritdoc cref="InovanceAMTcp()"/>
		public InovanceAMSerial() : base() { }

		/// <summary>
		/// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
		/// Specify the server address, port number, and client's own station number to initialize
		/// </summary>
		/// <param name="station">客户端自身的站号</param>
		public InovanceAMSerial(byte station = 0x01) : base(station) { }

		#endregion

		#region Read Write Override

		/// <inheritdoc cref="InovanceAMTcp.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceAMAddress(address, ModbusInfo.ReadRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Read(transModbus.Content, length);
		}

		/// <inheritdoc cref="InovanceAMTcp.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceAMAddress(address, ModbusInfo.WriteRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="InovanceAMTcp.ReadBool(string, ushort)"/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceAMAddress(address, ModbusInfo.ReadCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.ReadBool(transModbus.Content, length);
		}

		/// <inheritdoc cref="InovanceAMTcp.Write(string, bool[])"/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceAMAddress(address, ModbusInfo.WriteCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, values);
		}

		/// <inheritdoc cref="InovanceAMTcp.Write(string, bool)"/>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceAMAddress(address, ModbusInfo.WriteOneCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Short UShort Override

		/// <inheritdoc cref="InovanceAMTcp.Write(string, short)"/>
		[HslMqttApi("WriteInt16", "")]
		public override OperateResult Write(string address, short value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceAMAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		/// <inheritdoc cref="InovanceAMTcp.Write(string, ushort)"/>
		[HslMqttApi("WriteUInt16", "")]
		public override OperateResult Write(string address, ushort value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceAMAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"InovanceAMSerial[{PortName}:{BaudRate}]";

		#endregion
	}
}
