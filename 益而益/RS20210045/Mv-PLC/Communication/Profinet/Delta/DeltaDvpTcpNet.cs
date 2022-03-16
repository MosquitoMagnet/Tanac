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
	/// 台达PLC的网口通讯类，基于Modbus-Rtu协议开发，按照台达的地址进行实现。<br />
	/// The tcp communication class of Delta PLC is developed based on the Modbus-Tcp protocol and implemented according to Delta's address.
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
	public class DeltaDvpTcpNet : ModbusTcpNet
	{
		#region Constructor

		/// <inheritdoc cref="DeltaDvpSerial()"/>
		public DeltaDvpTcpNet() : base() { ByteTransform.DataFormat = DataFormat.CDAB; }

		/// <inheritdoc cref="ModbusTcpNet(string, int, byte)"/>
		public DeltaDvpTcpNet(string ipAddress, int port = 502, byte station = 0x01) : base(ipAddress, port, station) { ByteTransform.DataFormat = DataFormat.CDAB; }

		#endregion

		#region Read Write Override

		/// <inheritdoc cref="DeltaDvpSerial.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "Read the original byte data content from the register, the address is mainly D, T, C")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.ReadRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Read(transModbus.Content, length);
		}

		/// <inheritdoc cref="DeltaDvpSerial.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "Write the original byte data content to the register, the address is mainly D, T, C")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Asycn Read Write Override
#if !NET20 && !NET35
		/// <inheritdoc cref="DeltaDvpSerial.Read(string, ushort)"/>
		public async override Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.ReadRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return await base.ReadAsync(transModbus.Content, length);
		}

		/// <inheritdoc cref="DeltaDvpSerial.Write(string, byte[])"/>
		public async override Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}
#endif
		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="DeltaDvpSerial.ReadBool(string, ushort)"/>
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

		/// <inheritdoc cref="DeltaDvpSerial.Write(string, bool)"/>
		[HslMqttApi("WriteBool", "Write bool data content to the coil, the address is mainly Y, S, M, T, C")]
		public override OperateResult Write(string address, bool value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteOneCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return base.Write(transModbus.Content, value);
		}

		#endregion

		#region Async Bool Read Write
#if !NET20 && !NET35
		/// <inheritdoc cref="DeltaDvpSerial.ReadBool(string, ushort)"/>
		public async override Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.ReadCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.ReadBoolAsync(transModbus.Content, length);
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		public async override Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, values);
		}

		/// <inheritdoc cref="DeltaDvpSerial.Write(string, bool)"/>
		public async override Task<OperateResult> WriteAsync(string address, bool value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteOneCoil);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}
#endif
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

		#region Short UShort Override
#if !NET20 && !NET35
		/// <inheritdoc cref="IReadWriteNet.Write(string, short)"/>
		public async override Task<OperateResult> WriteAsync(string address, short value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}

		/// <inheritdoc cref="IReadWriteNet.Write(string, ushort)"/>
		public async override Task<OperateResult> WriteAsync(string address, ushort value)
		{
			OperateResult<string> transModbus = DeltaHelper.PraseDeltaDvpAddress(address, ModbusInfo.WriteOneRegister);
			if (!transModbus.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(transModbus);

			return await base.WriteAsync(transModbus.Content, value);
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"DeltaDvpTcpNet[{IpAddress}:{Port}]";

		#endregion
	}
}
