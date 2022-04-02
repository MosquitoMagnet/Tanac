using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.ModBus;
using Communication.Reflection;
using Communication.Core;
using System.Threading.Tasks;

namespace Communication.Profinet.Delta
{
	/// <summary>
	/// 台达PLC的串口通讯类，基于Modbus-Rtu协议开发，按照台达的地址进行实现。<br />
	/// The serial communication class of Delta PLC is developed based on the Modbus-Rtu protocol and implemented according to Delta's address.
	/// </summary>
	/// <remarks>
	/// 适用于DVP-ES/EX/EC/SS型号，DVP-SA/SC/SX/EH型号，地址参考API文档
	/// </remarks>
	/// <example>
	/// 地址的格式如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址进制</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term></term>
	///     <term>S</term>
	///     <term>S0-S1023</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X0-X377</term>
	///     <term>8</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term>只读</term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y0-Y377</term>
	///     <term>8</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器</term>
	///     <term>T</term>
	///     <term>T0-T255</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term>如果是读位，就是通断继电器，如果是读字，就是当前值</term>
	///   </item>
	///   <item>
	///     <term>计数器</term>
	///     <term>C</term>
	///     <term>C0-C255</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term>如果是读位，就是通断继电器，如果是读字，就是当前值</term>
	///   </item>
	///   <item>
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M0-M4095</term>
	///     <term>10</term>
	///     <term>×</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D0-D9999</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </example>
	public class DeltaDvpSerial : ModbusRtu
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public DeltaDvpSerial() : base() { ByteTransform.DataFormat = DataFormat.CDAB; }

		/// <summary>
		/// 指定客户端自己的站号来初始化<br />
		/// Specify the client's own station number to initialize
		/// </summary>
		/// <param name="station">客户端自身的站号</param>
		public DeltaDvpSerial(byte station = 0x01) : base(station) { ByteTransform.DataFormat = DataFormat.CDAB; }

		#endregion

		#region Read Write Override

		/// <summary>
		/// 从寄存器里读取原始的字节数据内容，地址主要是 D,T,C，每个类型的地址范围不一样，具体参考API文档<br />
		/// Read the original byte data content from the register, the address is mainly D, T, C, 
		/// the address range of each type is different, please refer to the API document for details
		/// </summary>
		/// <param name="address">D,T,C 类型地址，举例：D100, C100</param>
		/// <param name="length">读取的地址长度，一个地址是2个字节</param>
		/// <returns>带有成功标志的字节数据</returns>
		[HslMqttApi("ReadByteArray", "Read the original byte data content from the register, the address is mainly D, T, C")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.ReadRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Read(transModbus.Content, length);
		}

		/// <summary>
		/// 向寄存器里写入原始的字节数据内容，地址主要是 D,T,C，每个类型的地址范围不一样，具体参考API文档<br />
		/// Write the original byte data content to the register, the address is mainly D, T, C, 
		/// the address range of each type is different, please refer to the API document for details
		/// </summary>
		/// <param name="address">D,T,C 类型地址，举例：D100, C100</param>
		/// <param name="value">等待写入的原始字节值</param>
		/// <returns>返回写入结果</returns>
		[HslMqttApi("WriteByteArray", "Write the original byte data content to the register, the address is mainly D, T, C")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Bool Read Write

		/// <summary>
		/// 从线圈里批量读取bool数据内容，地址主要是 X,Y,S,M,T,C，其中X,Y的地址是8进制的，X0-X377，Y0-Y377<br />
		/// Read the contents of bool data in batches from the coil, the address is mainly X, Y, S, M, T, C, where X, Y address is in octal, X0-X377, Y0-Y377
		/// </summary>
		/// <param name="address">X,Y,S,M,T,C，其中X,Y的地址是8进制的，X0-X377，Y0-Y377</param>
		/// <param name="length">数据长度</param>
		/// <returns>带有成功标识的bool[]数组</returns>
		[HslMqttApi("ReadBoolArray", "Read the contents of bool data in batches from the coil, the address is mainly X, Y, S, M, T, C")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.ReadCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.ReadBool(transModbus.Content, length);
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		[HslMqttApi("WriteBoolArray", "Read the contents of bool data in batches from the coil, the address is mainly X, Y, S, M, T, C")]
		public override OperateResult Write(string address, bool[] values)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, values);
		}

		/// <summary>
		/// 向线圈里写入bool数据内容，地址主要是 Y,S,M,T,C，其中Y的地址是8进制的，Y0-Y377，注意 X 地址不能写入！<br />
		/// Write bool data content to the coil, the address is mainly Y, S, M, T, C, where the address of Y is in octal, Y0-Y377, note that X address cannot be written!
		/// </summary>
		/// <param name="address">X,Y,S,M,T,C，其中X,Y的地址是8进制的，X0-X1037，Y0-Y1037</param>
		/// <param name="value">等待写入的Bool值</param>
		/// <returns>返回写入结果</returns>
		[HslMqttApi("WriteBool", "Write bool data content to the coil, the address is mainly Y, S, M, T, C")]
		public override OperateResult Write(string address, bool value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteOneCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Short UShort Override

		/// <inheritdoc cref="IReadWriteNet.Write(string, short)"/>
		[HslMqttApi("WriteInt16", "Write short data, returns whether success")]
		public override OperateResult Write(string address, short value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, ushort)"/>
		[HslMqttApi("WriteUInt16", "Write ushort data, return whether the write was successful")]
		public override OperateResult Write(string address, ushort value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"DeltaDvpSerial[{PortName}:{BaudRate}]";

		#endregion
	}
}
