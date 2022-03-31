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
using System.Threading.Tasks;


namespace Communication.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为二进制通讯<br />
	/// Mitsubishi PLC communication class is implemented using Qna compatible 3E frame protocol. 
	/// The Ethernet module on the PLC side needs to be configured first. It must be binary communication.
	/// </summary>
	/// <remarks>
	/// 目前组件测试通过的PLC型号列表，有些来自于网友的测试
	/// <list type="number">
	/// <item>Q06UDV PLC  感谢hwdq0012</item>
	/// <item>fx5u PLC  感谢山楂</item>
	/// <item>Q02CPU PLC </item>
	/// <item>L02CPU PLC </item>
	/// </list>
	/// 地址的输入的格式支持多种复杂的地址表示方式：
	/// <list type="number">
	/// <item>扩展的数据地址: 表示为 ext=1;W100  访问扩展区域为1的W100的地址信息</item>
	/// <item>缓冲存储器地址: 表示为 mem=32  访问地址为32的本站缓冲存储器地址</item>
	/// <item>基于标签的地址: 表示位 s=AAA  假如标签的名称为AAA，但是标签的读取是有条件的，详细参照<see cref="ReadTags(string, ushort)"/></item>
	/// <item>普通的数据地址，参照下面的信息</item>
	/// </list>
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
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X100,X1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y100,Y1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///    <item>
	///     <term>锁存继电器</term>
	///     <term>L</term>
	///     <term>L100,L200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>报警器</term>
	///     <term>F</term>
	///     <term>F100,F200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>边沿继电器</term>
	///     <term>V</term>
	///     <term>V100,V200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>链接继电器</term>
	///     <term>B</term>
	///     <term>B100,B1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>步进继电器</term>
	///     <term>S</term>
	///     <term>S100,S200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D1000,D2000</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>链接寄存器</term>
	///     <term>W</term>
	///     <term>W100,W1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>文件寄存器</term>
	///     <term>R</term>
	///     <term>R100,R200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>ZR文件寄存器</term>
	///     <term>ZR</term>
	///     <term>ZR100,ZR2A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>变址寄存器</term>
	///     <term>Z</term>
	///     <term>Z100,Z200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的触点</term>
	///     <term>TS</term>
	///     <term>TS100,TS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的线圈</term>
	///     <term>TC</term>
	///     <term>TC100,TC200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的当前值</term>
	///     <term>TN</term>
	///     <term>TN100,TN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>累计定时器的触点</term>
	///     <term>SS</term>
	///     <term>SS100,SS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>累计定时器的线圈</term>
	///     <term>SC</term>
	///     <term>SC100,SC200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>累计定时器的当前值</term>
	///     <term>SN</term>
	///     <term>SN100,SN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的触点</term>
	///     <term>CS</term>
	///     <term>CS100,CS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的线圈</term>
	///     <term>CC</term>
	///     <term>CC100,CC200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的当前值</term>
	///     <term>CN</term>
	///     <term>CN100,CN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage" title="简单的短连接使用" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="Usage2" title="简单的长连接使用" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample1" title="基本的读取示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample2" title="批量读取示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample3" title="随机字读取示例" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample4" title="随机批量字读取示例" />
	/// </example>
	public class MelsecMcNet : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化三菱的Qna兼容3E帧协议的通讯对象<br />
		/// Instantiate the communication object of Mitsubishi's Qna compatible 3E frame protocol
		/// </summary>
		public MelsecMcNet()
		{
			this.WordLength = 1;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <summary>
		/// 指定ip地址和端口号来实例化一个默认的对象<br />
		/// Specify the IP address and port number to instantiate a default object
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口</param>
		public MelsecMcNet(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage() => new MelsecQnA3EBinaryMessage();

		#endregion

		#region Public Member

		/// <summary>
		/// 网络号，通常为0<br />
		/// Network number, usually 0
		/// </summary>
		/// <remarks>
		/// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0
		/// </remarks>
		public byte NetworkNumber { get; set; } = 0x00;

		/// <summary>
		/// 网络站号，通常为0<br />
		/// Network station number, usually 0
		/// </summary>
		/// <remarks>
		/// 依据PLC的配置而配置，如果PLC配置了1，那么此处也填0，如果PLC配置了2，此处就填2，测试不通的话，继续测试0
		/// </remarks>
		public byte NetworkStationNumber { get; set; } = 0x00;

		#endregion

		#region Virtual Address Analysis

		/// <summary>
		/// 当前MC协议的分析地址的方法，对传入的字符串格式的地址进行数据解析。<br />
		/// The current MC protocol's address analysis method performs data parsing on the address of the incoming string format.
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>解析后的数据信息</returns>
		protected virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length) => McAddressData.ParseMelsecFrom(address, length);

		#endregion

		#region Read Write Support

		/// <inheritdoc/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			if (address.StartsWith("s=") || address.StartsWith("S="))
			{
				return ReadTags(address.Substring(2), length);
			}
			else if (System.Text.RegularExpressions.Regex.IsMatch(address, "ext=[0-9]+;", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
			{
				string extStr = System.Text.RegularExpressions.Regex.Match(address, "ext=[0-9]+;").Value;
				ushort ext = ushort.Parse(System.Text.RegularExpressions.Regex.Match(extStr, "[0-9]+").Value);
				return ReadExtend(ext, address.Substring(extStr.Length), length);
			}
			else if (System.Text.RegularExpressions.Regex.IsMatch(address, "mem=", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
			{
				return ReadMemory(address.Substring(4), length);
			}
			else
			{
				// 分析地址
				OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				List<byte> bytesContent = new List<byte>();
				ushort alreadyFinished = 0;
				while (alreadyFinished < length)
				{
					ushort readLength = (ushort)Math.Min(length - alreadyFinished, 900);
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
		}

		private OperateResult<byte[]> ReadAddressData(McAddressData addressData)
		{
			byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressData, false);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			return WriteAddressData(addressResult.Content, value);
		}

		private OperateResult WriteAddressData(McAddressData addressData, byte[] value)
		{
			// 创建核心报文
			byte[] coreResult = MelsecHelper.BuildWriteWordCoreCommand(addressData, value);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Support

#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			if (address.StartsWith("s="))
			{
				return await ReadTagsAsync(address.Substring(2), length);
			}
			else if (System.Text.RegularExpressions.Regex.IsMatch(address, "ext=[0-9]+;"))
			{
				string extStr = System.Text.RegularExpressions.Regex.Match(address, "ext=[0-9]+;").Value;
				ushort ext = ushort.Parse(System.Text.RegularExpressions.Regex.Match(extStr, "[0-9]+").Value);
				return await ReadExtendAsync(ext, address.Substring(extStr.Length), length);
			}
			else if (System.Text.RegularExpressions.Regex.IsMatch(address, "mem=", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
			{
				return await ReadMemoryAsync(address.Substring(4), length);
			}
			else
			{
				// 分析地址
				OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				List<byte> bytesContent = new List<byte>();
				ushort alreadyFinished = 0;
				while (alreadyFinished < length)
				{
					ushort readLength = (ushort)Math.Min(length - alreadyFinished, 900);
					addressResult.Content.Length = readLength;
					OperateResult<byte[]> read = await ReadAddressDataAsync(addressResult.Content);
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
		}

		private async Task<OperateResult<byte[]>> ReadAddressDataAsync(McAddressData addressData)
		{
			byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressData, false);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			return await WriteAddressDataAsync(addressResult.Content, value);
		}

		private async Task<OperateResult> WriteAddressDataAsync(McAddressData addressData, byte[] value)
		{
			// 创建核心报文
			byte[] coreResult = MelsecHelper.BuildWriteWordCoreCommand(addressData, value);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Read Random

		/// <summary>
		/// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，需要自行解析数据<br />
		/// Randomly read PLC data information, which can be combined across addresses and types, but each address can only read one word, 
		/// which is the content of 2 bytes. After receiving the results, you need to parse the data yourself
		/// </summary>
		/// <param name="address">所有的地址的集合</param>
		/// <remarks>
		/// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站 时
		/// 访问点数········1≦ 字访问点数 双字访问点数 ≦192
		/// <br />
		/// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数········1≦ 字访问点数 双字访问点数 ≦96
		/// <br />
		/// 访问上述以外的 PLC CPU 其他站 时访问点数········1≦字访问点数≦10
		/// </remarks>
		/// <example>
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample3" title="随机字读取示例" /></example>
		/// <returns>结果</returns>
		public OperateResult<byte[]> ReadRandom(string[] address)
		{
			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], 1);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			byte[] coreResult = MelsecHelper.BuildReadRandomWordCommand(mcAddressDatas);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}


		/// <summary>
		/// 随机读取PLC的数据信息，可以跨地址，跨类型组合，每个地址是任意的长度。收到结果后，需要自行解析数据，目前只支持字地址，比如D区，W区，R区，不支持X，Y，M，B，L等等<br />
		/// Read the data information of the PLC randomly. It can be combined across addresses and types. Each address is of any length. After receiving the results, 
		/// you need to parse the data yourself. Currently, only word addresses are supported, such as D area, W area, R area. X, Y, M, B, L, etc
		/// </summary>
		/// <param name="address">所有的地址的集合</param>
		/// <param name="length">每个地址的长度信息</param>
		/// <remarks>
		/// 实际测试不一定所有的plc都可以读取成功，具体情况需要具体分析
		/// <br />
		/// 1 块数按照下列要求指定 120 ≧ 字软元件块数 + 位软元件块数
		/// <br />
		/// 2 各软元件点数按照下列要求指定 960 ≧ 字软元件各块的合计点数 + 位软元件各块的合计点数
		/// </remarks>
		/// <example>
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\melsecTest.cs" region="ReadExample4" title="随机批量字读取示例" />
		/// </example>
		/// <returns>结果</returns>
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

			byte[] coreResult = MelsecHelper.BuildReadRandomCommand(mcAddressDatas);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}

		/// <summary>
		/// 随机读取PLC的数据信息，可以跨地址，跨类型组合，但是每个地址只能读取一个word，也就是2个字节的内容。收到结果后，自动转换为了short类型的数组<br />
		/// Randomly read PLC data information, which can be combined across addresses and types, but each address can only read one word, 
		/// which is the content of 2 bytes. After receiving the result, it is automatically converted to an array of type short.
		/// </summary>
		/// <param name="address">所有的地址的集合</param>
		/// <remarks>
		/// 访问安装有 Q 系列 C24/E71 的站 QCPU 上位站 经由 Q 系列兼容网络系统 MELSECNET/H MELSECNET/10 Ethernet 的 QCPU 其他站 时
		/// 访问点数········1≦ 字访问点数 双字访问点数 ≦192
		/// 
		/// 访问 QnACPU 其他站 经由 QnA 系列兼容网络系统 MELSECNET/10 Ethernet 的 Q/QnACPU 其他站 时访问点数········1≦ 字访问点数 双字访问点数 ≦96
		/// 
		/// 访问上述以外的 PLC CPU 其他站 时访问点数········1≦字访问点数≦10
		/// </remarks>
		/// <returns>结果</returns>
		public OperateResult<short[]> ReadRandomInt16(string[] address)
		{
			OperateResult<byte[]> read = ReadRandom(address);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<short[]>(read);

			return OperateResult.CreateSuccessResult(ByteTransform.TransInt16(read.Content, 0, address.Length));
		}

		#endregion

		#region Async Read Random
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadRandom(string[])"/>
		public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address)
		{
			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], 1);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			byte[] coreResult = MelsecHelper.BuildReadRandomWordCommand(mcAddressDatas);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}


		/// <inheritdoc cref="ReadRandom(string[], ushort[])"/>
		public async Task<OperateResult<byte[]>> ReadRandomAsync(string[] address, ushort[] length)
		{
			if (length.Length != address.Length) return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);

			McAddressData[] mcAddressDatas = new McAddressData[address.Length];
			for (int i = 0; i < address.Length; i++)
			{
				OperateResult<McAddressData> addressResult = McAddressData.ParseMelsecFrom(address[i], length[i]);
				if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

				mcAddressDatas[i] = addressResult.Content;
			}

			byte[] coreResult = MelsecHelper.BuildReadRandomCommand(mcAddressDatas);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}

		/// <inheritdoc cref="ReadRandomInt16(string[])"/>
		public async Task<OperateResult<short[]>> ReadRandomInt16Async(string[] address)
		{
			OperateResult<byte[]> read = await ReadRandomAsync(address);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<short[]>(read);

			return OperateResult.CreateSuccessResult(ByteTransform.TransInt16(read.Content, 0, address.Length));
		}
#endif
		#endregion

		#region Bool Operate Support

		/// <inheritdoc/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 获取指令
			byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = ExtractActualData(read.Content.RemoveBegin(11), true);
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

			byte[] coreResult = MelsecHelper.BuildWriteBitCoreCommand(addressResult.Content, values);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Bool Operate Support
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 获取指令
			byte[] coreResult = MelsecHelper.BuildReadMcCoreCommand(addressResult.Content, true);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(check);

			// 数据解析，需要传入是否使用位的参数
			var extract = ExtractActualData(read.Content.RemoveBegin(11), true);
			if (!extract.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(extract);

			// 转化bool数组
			return OperateResult.CreateSuccessResult(extract.Content.Select(m => m == 0x01).Take(length).ToArray());
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			// 分析地址
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, 0);
			if (!addressResult.IsSuccess) return addressResult;

			byte[] coreResult = MelsecHelper.BuildWriteBitCoreCommand(addressResult.Content, values);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Tag Read Write

		/// <summary>
		/// 读取PLC的标签信息，需要传入标签的名称，读取的字长度，标签举例：A; label[1]; bbb[10,10,10]<br />
		/// To read the label information of the PLC, you need to pass in the name of the label, 
		/// the length of the word read, and an example of the label: A; label [1]; bbb [10,10,10]
		/// </summary>
		/// <param name="tag">标签名</param>
		/// <param name="length">读取长度</param>
		/// <returns>是否成功</returns>
		/// <remarks>
		///  不可以访问局部标签。<br />
		///  不可以访问通过GX Works2设置的全局标签。<br />
		///  为了访问全局标签，需要通过GX Works3的全局标签设置编辑器将“来自于外部设备的访问”的设置项目置为有效。(默认为无效。)<br />
		///  以ASCII代码进行数据通信时，由于需要从UTF-16将标签名转换为ASCII代码，因此报文容量将增加
		/// </remarks>
		[HslMqttApi]
		public OperateResult<byte[]> ReadTags(string tag, ushort length) => ReadTags(new string[] { tag }, new ushort[] { length });

		/// <inheritdoc cref="ReadTags(string, ushort)"/>
		public OperateResult<byte[]> ReadTags(string[] tags, ushort[] length)
		{
			byte[] coreResult = MelsecHelper.BuildReadTag(tags, length);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据初级解析，需要传入是否使用位的参数
			OperateResult<byte[]> extract = ExtractActualData(read.Content.RemoveBegin(11), false);
			if (!extract.IsSuccess) return extract;

			return MelsecHelper.ExtraTagData(extract.Content);
		}

		#endregion

		#region Async Tag Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadTags(string, ushort)"/>
		public async Task<OperateResult<byte[]>> ReadTagsAsync(string tag, ushort length) => await ReadTagsAsync(new string[] { tag }, new ushort[] { length });

		/// <inheritdoc cref="ReadTags(string, ushort)"/>
		public async Task<OperateResult<byte[]>> ReadTagsAsync(string[] tags, ushort[] length)
		{
			byte[] coreResult = MelsecHelper.BuildReadTag(tags, length);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据初级解析，需要传入是否使用位的参数
			OperateResult<byte[]> extract = ExtractActualData(read.Content.RemoveBegin(11), false);
			if (!extract.IsSuccess) return extract;

			return MelsecHelper.ExtraTagData(extract.Content);
		}
#endif
		#endregion

		#region Extend Read Write

		/// <summary>
		/// 读取扩展的数据信息，需要在原有的地址，长度信息之外，输入扩展值信息<br />
		/// To read the extended data information, you need to enter the extended value information in addition to the original address and length information
		/// </summary>
		/// <param name="extend">扩展信息</param>
		/// <param name="address">地址</param>
		/// <param name="length">数据长度</param>
		/// <returns>返回结果</returns>
		public OperateResult<byte[]> ReadExtend(ushort extend, string address, ushort length)
		{
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			byte[] coreResult = MelsecHelper.BuildReadMcCoreExtendCommand(addressResult.Content, extend, false);

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据初级解析，需要传入是否使用位的参数
			OperateResult<byte[]> extract = ExtractActualData(read.Content.RemoveBegin(11), false);
			if (!extract.IsSuccess) return extract;

			return MelsecHelper.ExtraTagData(extract.Content);
		}

		#endregion

		#region Async Extend Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadExtend(ushort, string, ushort)"/>
		public async Task<OperateResult<byte[]>> ReadExtendAsync(ushort extend, string address, ushort length)
		{
			OperateResult<McAddressData> addressResult = McAnalysisAddress(address, length);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			byte[] coreResult = MelsecHelper.BuildReadMcCoreExtendCommand(addressResult.Content, extend, false);

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据初级解析，需要传入是否使用位的参数
			OperateResult<byte[]> extract = ExtractActualData(read.Content.RemoveBegin(11), false);
			if (!extract.IsSuccess) return extract;

			return MelsecHelper.ExtraTagData(extract.Content);
		}
#endif
		#endregion

		#region Memory Read Write

		/// <summary>
		/// 读取缓冲寄存器的数据信息，地址直接为偏移地址<br />
		/// Read the data information of the buffer register, the address is directly the offset address
		/// </summary>
		/// <param name="address">偏移地址</param>
		/// <param name="length">读取长度</param>
		/// <returns>读取的内容</returns>
		public OperateResult<byte[]> ReadMemory(string address, ushort length)
		{
			OperateResult<byte[]> coreResult = MelsecHelper.BuildReadMemoryCommand(address, length);
			if (!coreResult.IsSuccess) return coreResult;

			// 核心交互
			var read = ReadFromCoreServer(PackMcCommand(coreResult.Content, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据初级解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}

		#endregion

		#region Async Memory Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadMemory(string, ushort)"/>
		public async Task<OperateResult<byte[]>> ReadMemoryAsync(string address, ushort length)
		{
			OperateResult<byte[]> coreResult = MelsecHelper.BuildReadMemoryCommand(address, length);
			if (!coreResult.IsSuccess) return coreResult;

			// 核心交互
			var read = await ReadFromCoreServerAsync(PackMcCommand(coreResult.Content, this.NetworkNumber, this.NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 错误代码验证
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			// 数据初级解析，需要传入是否使用位的参数
			return ExtractActualData(read.Content.RemoveBegin(11), false);
		}
#endif
		#endregion

		#region Remote Operate

		/// <summary>
		/// 远程Run操作<br />
		/// Remote Run Operation
		/// </summary>
		/// <returns>是否成功</returns>
		[HslMqttApi]
		public OperateResult RemoteRun()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(new byte[] { 0x01, 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 远程Stop操作<br />
		/// Remote Stop operation
		/// </summary>
		/// <returns>是否成功</returns>
		[HslMqttApi]
		public OperateResult RemoteStop()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(new byte[] { 0x02, 0x10, 0x00, 0x00, 0x01, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 远程Reset操作<br />
		/// Remote Reset Operation
		/// </summary>
		/// <returns>是否成功</returns>
		[HslMqttApi]
		public OperateResult RemoteReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(new byte[] { 0x06, 0x10, 0x00, 0x00, 0x01, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 读取PLC的型号信息，例如 Q02HCPU<br />
		/// Read PLC model information, such as Q02HCPU
		/// </summary>
		/// <returns>返回型号的结果对象</returns>
		[HslMqttApi]
		public OperateResult<string> ReadPlcType()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(new byte[] { 0x01, 0x01, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>(check);

			// 成功
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 11, 16).TrimEnd());
		}

		/// <summary>
		/// LED 熄灭 出错代码初始化<br />
		/// LED off Error code initialization
		/// </summary>
		/// <returns>是否成功</returns>
		[HslMqttApi]
		public OperateResult ErrorStateReset()
		{
			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackMcCommand(new byte[] { 0x17, 0x16, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Remote Operate
#if !NET35 && !NET20
		/// <inheritdoc cref="RemoteRun"/>
		public async Task<OperateResult> RemoteRunAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(new byte[] { 0x01, 0x10, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="RemoteStop"/>
		public async Task<OperateResult> RemoteStopAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(new byte[] { 0x02, 0x10, 0x00, 0x00, 0x01, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="RemoteReset"/>
		public async Task<OperateResult> RemoteResetAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(new byte[] { 0x06, 0x10, 0x00, 0x00, 0x01, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc cref="ReadPlcType"/>
		public async Task<OperateResult<string>> ReadPlcTypeAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(new byte[] { 0x01, 0x01, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<string>(check);

			// 成功
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(read.Content, 11, 16).TrimEnd());
		}

		/// <inheritdoc cref="ErrorStateReset"/>
		public async Task<OperateResult> ErrorStateResetAsync()
		{
			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackMcCommand(new byte[] { 0x17, 0x16, 0x00, 0x00 }, NetworkNumber, NetworkStationNumber));
			if (!read.IsSuccess) return read;

			// 错误码校验
			OperateResult check = CheckResponseContent(read.Content);
			if (!check.IsSuccess) return check;

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"MelsecMcNet[{IpAddress}:{Port}]";

		#endregion

		#region Static Method Helper

		/// <summary>
		/// 将MC协议的核心报文打包成一个可以直接对PLC进行发送的原始报文
		/// </summary>
		/// <param name="mcCore">MC协议的核心报文</param>
		/// <param name="networkNumber">网络号</param>
		/// <param name="networkStationNumber">网络站号</param>
		/// <returns>原始报文信息</returns>
		public static byte[] PackMcCommand(byte[] mcCore, byte networkNumber = 0, byte networkStationNumber = 0)
		{
			byte[] _PLCCommand = new byte[11 + mcCore.Length];
			_PLCCommand[0] = 0x50;                                               // 副标题
			_PLCCommand[1] = 0x00;
			_PLCCommand[2] = networkNumber;                                      // 网络号
			_PLCCommand[3] = 0xFF;                                               // PLC编号
			_PLCCommand[4] = 0xFF;                                               // 目标模块IO编号
			_PLCCommand[5] = 0x03;
			_PLCCommand[6] = networkStationNumber;                               // 目标模块站号
			_PLCCommand[7] = (byte)((_PLCCommand.Length - 9) % 256);             // 请求数据长度
			_PLCCommand[8] = (byte)((_PLCCommand.Length - 9) / 256);
			_PLCCommand[9] = 0x0A;                                               // CPU监视定时器
			_PLCCommand[10] = 0x00;
			mcCore.CopyTo(_PLCCommand, 11);

			return _PLCCommand;
		}

		/// <summary>
		/// 从PLC反馈的数据中提取出实际的数据内容，需要传入反馈数据，是否位读取
		/// </summary>
		/// <param name="response">反馈的数据内容</param>
		/// <param name="isBit">是否位读取</param>
		/// <returns>解析后的结果对象</returns>
		public static OperateResult<byte[]> ExtractActualData(byte[] response, bool isBit)
		{
			if (isBit)
			{
				// 位读取
				byte[] Content = new byte[response.Length * 2];
				for (int i = 0; i < response.Length; i++)
				{
					if ((response[i] & 0x10) == 0x10)
					{
						Content[i * 2 + 0] = 0x01;
					}

					if ((response[i] & 0x01) == 0x01)
					{
						Content[i * 2 + 1] = 0x01;
					}
				}

				return OperateResult.CreateSuccessResult(Content);
			}
			else
			{
				// 字读取
				return OperateResult.CreateSuccessResult(response);
			}
		}

		/// <summary>
		/// 检查从MC返回的数据是否是合法的。
		/// </summary>
		/// <param name="content">数据内容</param>
		/// <returns>是否合法</returns>
		public static OperateResult CheckResponseContent(byte[] content)
		{
			ushort errorCode = BitConverter.ToUInt16(content, 9);
			if (errorCode != 0) return new OperateResult<byte[]>(errorCode, MelsecHelper.GetErrorDescription(errorCode));

			return OperateResult.CreateSuccessResult();
		}
		#endregion
	}
}
