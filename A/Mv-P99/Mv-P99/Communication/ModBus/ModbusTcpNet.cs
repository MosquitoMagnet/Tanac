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
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication.ModBus
{
	/// <summary>
	/// Modbus-Tcp协议的客户端通讯类，方便的和服务器进行数据交互，支持标准的功能码，也支持扩展的功能码实现，地址采用富文本的形式，详细见备注说明<br />
	/// The client communication class of Modbus-Tcp protocol is convenient for data interaction with the server. It supports standard function codes and also supports extended function codes. 
	/// The address is in rich text. For details, see the remarks.
	/// </summary>
	/// <remarks>
	/// 本客户端支持的标准的modbus协议，Modbus-Tcp及Modbus-Udp内置的消息号会进行自增，比如我们想要控制消息号在0-1000之间自增，不能超过一千，可以写如下的代码：
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Modbus\Modbus.cs" region="Sample1" title="序号示例" />
	/// <note type="important">
	/// 地址共可以携带3个信息，最完整的表示方式"s=2;x=3;100"，对应的modbus报文是 02 03 00 64 00 01 的前四个字节，站号，功能码，起始地址，下面举例
	/// </note>
	/// <list type="definition">
	/// <item>
	///     <term>读取线圈</term>
	///     <description>ReadCoil("100")表示读取线圈100的值，ReadCoil("s=2;100")表示读取站号为2，线圈地址为100的值</description>
	/// </item>
	/// <item>
	///     <term>读取离散输入</term>
	///     <description>ReadDiscrete("100")表示读取离散输入100的值，ReadDiscrete("s=2;100")表示读取站号为2，离散地址为100的值</description>
	/// </item>
	/// <item>
	///     <term>读取寄存器</term>
	///     <description>ReadInt16("100")表示读取寄存器100的值，ReadInt16("s=2;100")表示读取站号为2，寄存器100的值</description>
	/// </item>
	/// <item>
	///     <term>读取输入寄存器</term>
	///     <description>ReadInt16("x=4;100")表示读取输入寄存器100的值，ReadInt16("s=2;x=4;100")表示读取站号为2，输入寄存器100的值</description>
	/// </item>
	/// </list>
	/// 对于写入来说也是一致的
	/// <list type="definition">
	/// <item>
	///     <term>写入线圈</term>
	///     <description>WriteCoil("100",true)表示读取线圈100的值，WriteCoil("s=2;100",true)表示读取站号为2，线圈地址为100的值</description>
	/// </item>
	/// <item>
	///     <term>写入寄存器</term>
	///     <description>Write("100",(short)123)表示写寄存器100的值123，Write("s=2;100",(short)123)表示写入站号为2，寄存器100的值123</description>
	/// </item>
	/// </list>
	/// 特殊说明部分：
	///  <list type="definition">
	/// <item>
	///     <term>01功能码</term>
	///     <description>ReadBool("100")</description>
	/// </item>
	/// <item>
	///     <term>02功能码</term>
	///     <description>ReadBool("x=2;100")</description>
	/// </item>
	/// <item>
	///     <term>03功能码</term>
	///     <description>Read("100")</description>
	/// </item>
	/// <item>
	///     <term>04功能码</term>
	///     <description>Read("x=4;100")</description>
	/// </item>
	/// <item>
	///     <term>05功能码</term>
	///     <description>Write("100", True)</description>
	/// </item>
	/// <item>
	///     <term>06功能码</term>
	///     <description>Write("100", (short)100);Write("100", (ushort)100)</description>
	/// </item>
	/// <item>
	///     <term>0F功能码</term>
	///     <description>Write("100", new bool[]{True})</description>
	/// </item>
	/// <item>
	///     <term>10功能码</term>
	///     <description>写入寄存器的方法出去上述06功能码的示例，如果写一个short想用10功能码：Write("100", new short[]{100})</description>
	/// </item>
	/// <item>
	///     <term>16功能码</term>
	///     <description>Write("100.2", True) 当写入bool值的方法里，地址格式变为字地址时，就使用16功能码，通过掩码的方式来修改寄存器的某一位，需要Modbus服务器支持，否则写入无效。</description>
	/// </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// 基本的用法请参照下面的代码示例
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Modbus\Modbus.cs" region="Example1" title="Modbus示例" />
	/// </example>
	public class ModbusTcpNet : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个Modbus-Tcp协议的客户端对象<br />
		/// Instantiate a client object of the Modbus-Tcp protocol
		/// </summary>
		public ModbusTcpNet()
		{
			this.softIncrementCount = new SoftIncrementCount(ushort.MaxValue);
			this.WordLength = 1;
			this.station = 1;
			this.ByteTransform = new ReverseWordTransform();
		}

		/// <summary>
		/// 指定服务器地址，端口号，客户端自己的站号来初始化<br />
		/// Specify the server address, port number, and client's own station number to initialize
		/// </summary>
		/// <param name="ipAddress">服务器的Ip地址</param>
		/// <param name="port">服务器的端口号</param>
		/// <param name="station">客户端自身的站号</param>
		public ModbusTcpNet(string ipAddress, int port = 502, byte station = 0x01)
		{
			this.softIncrementCount = new SoftIncrementCount(ushort.MaxValue);
			this.IpAddress = ipAddress;
			this.Port = port;
			this.WordLength = 1;
			this.station = station;
			this.ByteTransform = new ReverseWordTransform();
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage() => new ModbusTcpMessage();

		#endregion

		#region Private Member

		private byte station = 0x01;                                         // 本客户端的站号
		private readonly SoftIncrementCount softIncrementCount;              // 自增消息的对象
		private bool isAddressStartWithZero = true;                          // 线圈值的地址值是否从零开始

		#endregion

		#region Override Method

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect(Socket socket)
		{
			if (isUseAccountCertificate) return AccountCertificate(socket);

			return base.InitializationOnConnect(socket);
		}
#if !NET35 && !NET20

		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
		{
			if (isUseAccountCertificate) return await AccountCertificateAsync(socket);

			return await base.InitializationOnConnectAsync(socket);
		}
#endif
		#endregion

		#region Public Member

		/// <summary>
		/// 获取或设置起始的地址是否从0开始，默认为True<br />
		/// Gets or sets whether the starting address starts from 0. The default is True
		/// </summary>
		/// <remarks>
		/// <note type="warning">因为有些设备的起始地址是从1开始的，就要设置本属性为<c>False</c></note>
		/// </remarks>
		public bool AddressStartWithZero
		{
			get { return isAddressStartWithZero; }
			set { isAddressStartWithZero = value; }
		}

		/// <summary>
		/// 获取或者重新修改服务器的默认站号信息，当然，你可以再读写的时候动态指定，参见备注<br />
		/// Get or modify the default station number information of the server. Of course, you can specify it dynamically when reading and writing, see note
		/// </summary>
		/// <remarks>
		/// 当你调用 ReadCoil("100") 时，对应的站号就是本属性的值，当你调用 ReadCoil("s=2;100") 时，就忽略本属性的值，读写寄存器的时候同理
		/// </remarks>
		public byte Station
		{
			get { return station; }
			set { station = value; }
		}

		/// <inheritdoc cref="ByteTransformBase.DataFormat"/>
		public DataFormat DataFormat
		{
			get { return ByteTransform.DataFormat; }
			set { ByteTransform.DataFormat = value; }
		}

		/// <summary>
		/// 字符串数据是否按照字来反转，默认为False<br />
		/// Whether the string data is reversed according to words. The default is False.
		/// </summary>
		/// <remarks>
		/// 字符串按照2个字节的排列进行颠倒，根据实际情况进行设置
		/// </remarks>
		public bool IsStringReverse
		{
			get { return ByteTransform.IsStringReverseByteWord; }
			set { ByteTransform.IsStringReverseByteWord = value; }
		}

		/// <summary>
		/// 获取modbus协议自增的消息号，你可以自定义modbus的消息号的规则，详细参见<see cref="ModbusTcpNet"/>说明，也可以查找<see cref="SoftIncrementCount"/>说明。<br />
		/// Get the message number incremented by the modbus protocol. You can customize the rules of the message number of the modbus. For details, please refer to the description of <see cref = "ModbusTcpNet" />, or you can find the description of <see cref = "SoftIncrementCount" />
		/// </summary>
		public SoftIncrementCount MessageId => softIncrementCount;

		#endregion

		#region Read Write Support

		/// <summary>
		/// 读取线圈，需要指定起始地址，如果富文本地址不指定，默认使用的功能码是 0x01<br />
		/// To read the coil, you need to specify the start address. If the rich text address is not specified, the default function code is 0x01.
		/// </summary>
		/// <param name="address">起始地址，格式为"1234"</param>
		/// <returns>带有成功标志的bool对象</returns>
		public OperateResult<bool> ReadCoil(string address) => ReadBool(address);

		/// <summary>
		/// 批量的读取线圈，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x01<br />
		/// For batch reading coils, you need to specify the start address and read length. If the rich text address is not specified, the default function code is 0x01.
		/// </summary>
		/// <param name="address">起始地址，格式为"1234"</param>
		/// <param name="length">读取长度</param>
		/// <returns>带有成功标志的bool数组对象</returns>
		public OperateResult<bool[]> ReadCoil(string address, ushort length) => ReadBool(address, length);

		/// <summary>
		/// 读取输入线圈，需要指定起始地址，如果富文本地址不指定，默认使用的功能码是 0x02<br />
		/// To read the input coil, you need to specify the start address. If the rich text address is not specified, the default function code is 0x02.
		/// </summary>
		/// <param name="address">起始地址，格式为"1234"</param>
		/// <returns>带有成功标志的bool对象</returns>
		public OperateResult<bool> ReadDiscrete(string address) => ByteTransformHelper.GetResultFromArray(ReadDiscrete(address, 1));

		/// <summary>
		/// 批量的读取输入点，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x02<br />
		/// To read input points in batches, you need to specify the start address and read length. If the rich text address is not specified, the default function code is 0x02
		/// </summary>
		/// <param name="address">起始地址，格式为"1234"</param>
		/// <param name="length">读取长度</param>
		/// <returns>带有成功标志的bool数组对象</returns>
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

		/// <summary>
		/// 从Modbus服务器批量读取寄存器的信息，需要指定起始地址，读取长度，如果富文本地址不指定，默认使用的功能码是 0x03，如果需要使用04功能码，那么地址就写成 x=4;100<br />
		/// To read the register information from the Modbus server in batches, you need to specify the start address and read length. If the rich text address is not specified, 
		/// the default function code is 0x03. If you need to use the 04 function code, the address is written as x = 4; 100
		/// </summary>
		/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
		/// <param name="length">读取的数量</param>
		/// <returns>带有成功标志的字节信息</returns>
		/// <remarks>
		/// 富地址格式，支持携带站号信息，功能码信息，具体参照类的示例代码
		/// </remarks>
		/// <example>
		/// 此处演示批量读取的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Modbus\Modbus.cs" region="ReadExample1" title="Read示例" />
		/// </example>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, Station, AddressStartWithZero, ModbusInfo.ReadRegister);
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

		/// <summary>
		/// 从Modbus服务器批量读取寄存器的信息，需要指定Modbus地址，该地址可以携带站号信息，功能码<br />
		/// To read register information in batches from the Modbus server, you need to specify the Modbus address, which can carry station number information and function code
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="length">长度</param>
		/// <returns>带是否成功的结果数据</returns>
		private OperateResult<byte[]> ReadModBus(ModbusAddress address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!read.IsSuccess) return read;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(read.Content));
		}

		/// <summary>
		/// 将数据写入到Modbus的寄存器上去，需要指定起始地址和数据内容，如果富文本地址不指定，默认使用的功能码是 0x10<br />
		/// To write data to Modbus registers, you need to specify the start address and data content. If the rich text address is not specified, the default function code is 0x10
		/// </summary>
		/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
		/// <param name="value">写入的数据，长度根据data的长度来指示</param>
		/// <returns>返回写入结果</returns>
		/// <remarks>
		/// 富地址格式，支持携带站号信息，功能码信息，具体参照类的示例代码
		/// </remarks>
		/// <example>
		/// 此处演示批量写入的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Modbus\Modbus.cs" region="WriteExample1" title="Write示例" />
		/// </example>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <summary>
		/// 将数据写入到Modbus的单个寄存器上去，需要指定起始地址和数据值，如果富文本地址不指定，默认使用的功能码是 0x06<br />
		/// To write data to a single register of Modbus, you need to specify the start address and data value. If the rich text address is not specified, the default function code is 0x06.
		/// </summary>
		/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
		/// <param name="value">写入的short数据</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteInt16", "")]
		public override OperateResult Write(string address, short value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <summary>
		/// 将数据写入到Modbus的单个寄存器上去，需要指定起始地址和数据值，如果富文本地址不指定，默认使用的功能码是 0x06<br />
		/// To write data to a single register of Modbus, you need to specify the start address and data value. If the rich text address is not specified, the default function code is 0x06.
		/// </summary>
		/// <param name="address">起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
		/// <param name="value">写入的ushort数据</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteUInt16", "")]
		public override OperateResult Write(string address, ushort value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <summary>
		/// 向设备写入掩码数据，使用0x16功能码，需要确认对方是否支持相关的操作，掩码数据的操作主要针对寄存器。<br />
		/// To write mask data to the server, using the 0x16 function code, you need to confirm whether the other party supports related operations. 
		/// The operation of mask data is mainly directed to the register.
		/// </summary>
		/// <param name="address">起始地址，起始地址，比如"100"，"x=4;100"，"s=1;100","s=1;x=4;100"</param>
		/// <param name="andMask">等待与操作的掩码数据</param>
		/// <param name="orMask">等待或操作的掩码数据</param>
		/// <returns>是否写入成功</returns>
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

		/// <inheritdoc cref="Write(string, short)"/>
		public OperateResult WriteOneRegister(string address, short value) => Write(address, value);

		/// <inheritdoc cref="Write(string, ushort)"/>
		public OperateResult WriteOneRegister(string address, ushort value) => Write(address, value);

		#endregion

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadCoil(string)"/>
		public async Task<OperateResult<bool>> ReadCoilAsync(string address) => await ReadBoolAsync(address);

		///<inheritdoc cref="ReadCoil(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadCoilAsync(string address, ushort length) => await ReadBoolAsync(address, length);

		/// <inheritdoc cref="ReadDiscrete(string)"/>
		public async Task<OperateResult<bool>> ReadDiscreteAsync(string address) => ByteTransformHelper.GetResultFromArray(await ReadDiscreteAsync(address, 1));

		/// <inheritdoc cref="ReadDiscrete(string, ushort)"/>
		public async Task<OperateResult<bool[]>> ReadDiscreteAsync(string address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length, Station, AddressStartWithZero, ModbusInfo.ReadDiscrete);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			OperateResult<byte[]> read = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			OperateResult<byte[]> extract = ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(read.Content));
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(extract.Content, length));
		}

		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
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

		/// <inheritdoc cref="ReadModBus(ModbusAddress, ushort)"/>
		private async Task<OperateResult<byte[]>> ReadModBusAsync(ModbusAddress address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!read.IsSuccess) return read;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(read.Content));
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="Write(string, short)"/>
		public async override Task<OperateResult> WriteAsync(string address, short value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="Write(string, ushort)"/>
		public async override Task<OperateResult> WriteAsync(string address, ushort value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteWordModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="WriteMask(string, ushort, ushort)"/>
		public async Task<OperateResult> WriteMaskAsync(string address, ushort andMask, ushort orMask)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteMaskModbusCommand(address, andMask, orMask, Station, AddressStartWithZero, ModbusInfo.WriteMaskRegister);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

#endif
		#endregion

		#region Async Write One Register
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, short)"/>
		public virtual async Task<OperateResult> WriteOneRegisterAsync(string address, short value) => await WriteAsync(address, value);

		/// <inheritdoc cref="Write(string, ushort)"/>
		public virtual async Task<OperateResult> WriteOneRegisterAsync(string address, ushort value) => await WriteAsync(address, value);
#endif
		#endregion

		#region Read Write Bool

		/// <summary>
		/// 批量读取线圈或是离散的数据信息，需要指定地址和长度，具体的结果取决于实现，如果富文本地址不指定，默认使用的功能码是 0x01<br />
		/// To read coils or discrete data in batches, you need to specify the address and length. The specific result depends on the implementation. If the rich text address is not specified, the default function code is 0x01.
		/// </summary>
		/// <param name="address">数据地址，比如 "1234" </param>
		/// <param name="length">数据长度</param>
		/// <returns>带有成功标识的bool[]数组</returns>
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

		/// <summary>
		/// 向线圈中写入bool数组，返回是否写入成功，如果富文本地址不指定，默认使用的功能码是 0x0F<br />
		/// Write the bool array to the coil, and return whether the writing is successful. If the rich text address is not specified, the default function code is 0x0F.
		/// </summary>
		/// <param name="address">要写入的数据地址，比如"1234"</param>
		/// <param name="values">要写入的实际数组</param>
		/// <returns>返回写入结果</returns>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, values, Station, AddressStartWithZero, ModbusInfo.WriteCoil);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = ReadFromCoreServer(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <summary>
		/// 向线圈中写入bool数值，返回是否写入成功，如果富文本地址不指定，默认使用的功能码是 0x05，
		/// 如果你的地址为字地址，例如100.2，那么将使用0x16的功能码，通过掩码的方式来修改寄存器的某一位，需要Modbus服务器支持，否则写入无效。<br />
		/// Write bool value to the coil and return whether the writing is successful. If the rich text address is not specified, the default function code is 0x05.
		/// If your address is a word address, such as 100.2, then you will use the function code of 0x16 to modify a bit of the register through a mask. 
		/// It needs Modbus server support, otherwise the writing is invalid.
		/// </summary>
		/// <param name="address">要写入的数据地址，比如"12345"</param>
		/// <param name="value">要写入的实际数据</param>
		/// <returns>返回写入结果</returns>
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

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildReadModbusCommand(address, length, Station, AddressStartWithZero, ModbusInfo.ReadCoil);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			OperateResult<byte[]> read = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			OperateResult<byte[]> extract = ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(read.Content));
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			return OperateResult.CreateSuccessResult(SoftBasic.ByteToBoolArray(extract.Content, length));
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, values, Station, AddressStartWithZero, ModbusInfo.WriteCoil);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync(string address, bool value)
		{
			OperateResult<byte[]> command = ModbusInfo.BuildWriteBoolModbusCommand(address, value, Station, AddressStartWithZero, ModbusInfo.WriteOneCoil);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> write = await ReadFromCoreServerAsync(ModbusInfo.PackCommandToTcp(command.Content, (ushort)softIncrementCount.GetCurrentValue()));
			if (!write.IsSuccess) return write;

			return ModbusInfo.ExtractActualData(ModbusInfo.ExplodeTcpCommandToCore(write.Content));
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"ModbusTcpNet[{IpAddress}:{Port}]";

		#endregion
	}
}
