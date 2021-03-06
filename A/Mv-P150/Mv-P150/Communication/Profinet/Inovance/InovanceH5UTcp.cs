using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication;
using Communication.ModBus;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.Profinet.Inovance
{
	/// <summary>
	/// 汇川的网络通信协议，适用于H5U 系列，底层走的是MODBUS-TCP协议，地址说明参见标记<br />
	/// Huichuan's network communication protocol is suitable for H5U series. 
	/// The bottom layer is MODBUS-TCP protocol. For the address description, please refer to the mark
	/// </summary>
	/// <remarks>
	/// H5U 系列控制器支持 M/B/S/X/Y 等 bit 型变量（也称线圈） 的访问、 D/R 等 word 型变量的访问；<br />
	/// 其中 M/B/S/X/Y 等 bit 型变量的访问， 是以不同的地址偏移来区分的， D/R 等 word 型变量的访问， 也是以不同的地址偏移来区分的；<br />
	/// H5U 控制器内部 W 元件， 不支持通信访问。<br /><br />
	/// 我们来看看本组件支持的地址类型及范围，首先是位操作的地址
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>中间寄电器</term>
	///     <term>M</term>
	///     <term>M0-M7999</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term></term>
	///     <term>B</term>
	///     <term>B0-B32767</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term></term>
	///     <term>S</term>
	///     <term>S0-S4095</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入</term>
	///     <term>X</term>
	///     <term>X0-X1777 或者X0.0-X177.7</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出</term>
	///     <term>Y</term>
	///     <term>Y0-Y1777 或者Y0.0-Y177.7</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// 然后是字操作的地址
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D0-D7999</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term></term>
	///     <term>R</term>
	///     <term>R0-R32767</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	public class InovanceH5UTcp : ModbusTcpNet
	{
		#region Constructor

		/// <summary>
		/// 实例化一个安川H5U 系列的网络通讯协议<br />
		/// Instantiate a network communication protocol of Yaskawa H5U series
		/// </summary>
		public InovanceH5UTcp() : base() { }

		/// <summary>
		/// 指定服务器地址，端口号，客户端自己的站号来实例化一个安川H5U 系列的网络通讯协议<br />
		/// Specify the server address, port number, and client's own station number to instantiate a Yaskawa H5U series network communication protocol
		/// </summary>
		/// <param name="ipAddress">服务器的Ip地址</param>
		/// <param name="port">服务器的端口号</param>
		/// <param name="station">客户端自身的站号</param>
		public InovanceH5UTcp(string ipAddress, int port = 502, byte station = 0x01) : base(ipAddress, port, station) { }

		#endregion

		#region Read Write Override

		/// <summary>
		/// 按字读取汇川PLC的数据信息，可以输入D0,R0 类型地址<br />
		/// Read Huichuan PLC's data information by word, you can enter D0, R0 type address
		/// </summary>
		/// <param name="address">PLC的真实的地址信息，可以输入D0,R0 类型地址</param>
		/// <param name="length">读取的数据的长度，按照字为单位</param>
		/// <returns>包含是否成功的结果对象信息</returns>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.ReadRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Read(transModbus.Content, length);
		}

		/// <summary>
		/// 按字写入汇川PLC的数据信息，可以输入D0,R0 类型地址<br />
		/// Data information written into Huichuan PLC by word, can enter D0, R0 type
		/// </summary>
		/// <param name="address">PLC的真实的地址信息，可以输入D0,R0 类型地址</param>
		/// <param name="value">等待写入的原始数据，长度为2的倍数</param>
		/// <returns>是否写入成功的结果信息</returns>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Async Read Write Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.ReadRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return await base.ReadAsync(transModbus.Content, length);
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}
#endif
		#endregion

		#region Bool Read Write

		/// <summary>
		/// 按位读取汇川PLC的数据信息，可以输入M0,B0,S0,X0,Y0<br />
		/// Read the data of Huichuan PLC bit by bit, you can enter M0, B0, S0, X0, Y0
		/// </summary>
		/// <param name="address">汇川PLC的真实的位地址信息，可以输入M0,B0,S0,X0,Y0</param>
		/// <param name="length">等待读取的长度，按照位为单位</param>
		/// <returns>包含是否成功的结果对象</returns>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.ReadCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.ReadBool(transModbus.Content, length);
		}

		/// <summary>
		/// 按位写入汇川PLC的数据信息，可以输入M0,B0,S0,X0,Y0<br />
		/// Write the data information of Huichuan PLC bit by bit, you can enter M0, B0, S0, X0, Y0
		/// </summary>
		/// <param name="address">汇川PLC的真实的位地址信息，可以输入M0,B0,S0,X0,Y0</param>
		/// <param name="values">等待写入的原始数据</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, values);
		}

		/// <summary>
		/// 写入汇川PLC一个bool数据，可以输入M0,B0,S0,X0,Y0<br />
		/// Write a bool data to Huichuan PLC, you can enter M0, B0, S0, X0, Y0
		/// </summary>
		/// <param name="address">汇川PLC的真实的位地址信息，可以输入M0,B0,S0,X0,Y0</param>
		/// <param name="value">bool数据值</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteOneCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Async Bool Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.ReadCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.ReadBoolAsync(transModbus.Content, length);
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public async override Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, values);
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		public async override Task<OperateResult> WriteAsync(string address, bool value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteOneCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}
#endif
		#endregion

		#region Short UShort Override

		/// <summary>
		/// 写入汇川PLC的一个字数据，可以输入D0,R0 类型地址<br />
		/// Write a word of data to Huichuan PLC, you can enter D0, R0 type address
		/// </summary>
		/// <param name="address">汇川PLC的真实地址，可以输入D0,R0 类型地址</param>
		/// <param name="value">short数据</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteInt16", "")]
		public override OperateResult Write(string address, short value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		/// <summary>
		/// 写入汇川PLC的一个字数据，可以输入D0,R0 类型地址>br />
		/// Write a word of data to Huichuan PLC, you can enter D0, R0 type address
		/// </summary>
		/// <param name="address">汇川PLC的真实地址，可以输入D0,R0 类型地址</param>
		/// <param name="value">ushort数据</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteUInt16", "")]
		public override OperateResult Write(string address, ushort value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Async Short UShort Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, short)"/>
		public async override Task<OperateResult> WriteAsync(string address, short value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}

		/// <inheritdoc cref="Write(string, ushort)"/>
		public async override Task<OperateResult> WriteAsync(string address, ushort value)
		{
			OperateResult<string> transModbus = InovanceHelper.PraseInovanceH5UAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"InovanceH5UTcp[{IpAddress}:{Port}]";

		#endregion
	}
}
