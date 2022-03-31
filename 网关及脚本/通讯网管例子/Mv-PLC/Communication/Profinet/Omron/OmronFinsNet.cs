using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Core.IMessage;
using Communication.Core.Net;
using Communication.Core;
using System.Net.Sockets;
using Communication.BasicFramework;
using Communication.Reflection;
using System.Threading.Tasks;


namespace Communication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙PLC通讯类，采用Fins-Tcp通信协议实现，支持的地址信息参见api文档信息。<br />
	/// Omron PLC communication class is implemented using Fins-Tcp communication protocol. For the supported address information, please refer to the api document information.
	/// </summary>
	/// <remarks>
	/// <note type="important">实例化之后，使用之前，需要初始化三个参数信息，具体见三个参数的说明：<see cref="SA1"/>，<see cref="DA1"/>，<see cref="DA2"/></note>
	/// <note type="important">第二个需要注意的是，当网络异常掉线时，无法立即连接上PLC，PLC对于当前的节点进行拒绝，如果想要支持在断线后的快速连接，就需要将
	/// <see cref="IsChangeSA1AfterReadFailed"/>设置为<c>True</c>，详细的可以参考 <see cref="IsChangeSA1AfterReadFailed"/></note>
	/// <br />
	/// <note type="warning">如果在测试的时候报错误码64，经网友 上海-Lex 指点，是因为PLC中产生了报警，如伺服报警，模块错误等产生的，但是数据还是能正常读到的，屏蔽64报警或清除plc错误可解决</note>
	/// 地址支持的列表如下：
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
	///     <term>DM Area</term>
	///     <term>D</term>
	///     <term>D100,D200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>CIO Area</term>
	///     <term>C</term>
	///     <term>C100,C200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Work Area</term>
	///     <term>W</term>
	///     <term>W100,W200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Holding Bit Area</term>
	///     <term>H</term>
	///     <term>H100,H200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Auxiliary Bit Area</term>
	///     <term>A</term>
	///     <term>A100,A200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>EM Area</term>
	///     <term>E</term>
	///     <term>E0.0,EF.200,E10.100</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="Usage" title="简单的短连接使用" />
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="Usage2" title="简单的长连接使用" />
	/// </example>
	public class OmronFinsNet : NetworkDeviceBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个欧姆龙PLC Fins帧协议的通讯对象<br />
		/// Instantiate a communication object of Omron PLC Fins frame protocol
		/// </summary>
		public OmronFinsNet()
		{
			this.WordLength = 1;
			this.ByteTransform = new ReverseWordTransform();
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
		}

		/// <summary>
		/// 指定ip地址和端口号来实例化一个欧姆龙PLC Fins帧协议的通讯对象<br />
		/// Specify the IP address and port number to instantiate a communication object of the Omron PLC Fins frame protocol
		/// </summary>
		/// <param name="ipAddress">PLCd的Ip地址</param>
		/// <param name="port">PLC的端口</param>
		public OmronFinsNet(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			this.ByteTransform = new ReverseWordTransform();
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage() => new FinsMessage();

		#endregion

		#region IpAddress Override

		/// <inheritdoc/>
		public override string IpAddress
		{
			get => base.IpAddress;
			set
			{
				DA1 = Convert.ToByte(value.Substring(value.LastIndexOf(".") + 1));
				base.IpAddress = value;
			}
		}

		#endregion

		#region Public Member

		/// <summary>
		/// 信息控制字段，默认0x80<br />
		/// Information control field, default 0x80
		/// </summary>
		public byte ICF { get; set; } = 0x80;

		/// <summary>
		/// 系统使用的内部信息<br />
		/// Internal information used by the system
		/// </summary>
		public byte RSV { get; private set; } = 0x00;

		/// <summary>
		/// 网络层信息，默认0x02，如果有八层消息，就设置为0x07<br />
		/// Network layer information, default is 0x02, if there are eight layers of messages, set to 0x07
		/// </summary>
		public byte GCT { get; set; } = 0x02;

		/// <summary>
		/// PLC的网络号地址，默认0x00<br />
		/// PLC network number address, default 0x00
		/// </summary>
		public byte DNA { get; set; } = 0x00;

		/// <summary>
		/// PLC的节点地址，这个值在配置了ip地址之后是默认赋值的，默认为Ip地址的最后一位<br />
		/// PLC node address. This value is assigned by default after the IP address is configured. The default is the last bit of the IP address.
		/// </summary>
		/// <remarks>
		/// <note type="important">假如你的PLC的Ip地址为192.168.0.10，那么这个值就是10</note>
		/// </remarks>
		public byte DA1 { get; set; } = 0x13;

		/// <summary>
		/// PLC的单元号地址，通常都为0<br />
		/// PLC unit number address, usually 0
		/// </summary>
		/// <remarks>
		/// <note type="important">通常都为0</note>
		/// </remarks>
		public byte DA2 { get; set; } = 0x00;

		/// <summary>
		/// 上位机的网络号地址<br />
		/// Network number and address of the computer
		/// </summary>
		public byte SNA { get; set; } = 0x00;

		private byte computerSA1 = 0x0B;

		/// <summary>
		/// 上位机的节点地址，假如你的电脑的Ip地址为192.168.0.13，那么这个值就是13<br />
		/// The node address of the upper computer. If your computer's IP address is 192.168.0.13, then this value is 13
		/// </summary>
		/// <remarks>
		/// <note type="important">假如你的电脑的Ip地址为192.168.0.13，那么这个值就是13</note>
		/// </remarks>
		public byte SA1
		{
			get { return computerSA1; }
			set
			{
				computerSA1 = value;
				handSingle[19] = value;
			}
		}

		/// <summary>
		/// 上位机的单元号地址<br />
		/// Unit number and address of the computer
		/// </summary>
		public byte SA2 { get; set; }

		/// <summary>
		/// 设备的标识号<br />
		/// Device identification number
		/// </summary>
		public byte SID { get; set; } = 0x00;

		/// <summary>
		/// 如果设置为<c>True</c>，当数据读取失败的时候，会自动变更当前的SA1值，会选择自动增加，但不会和DA1一致，本值需要在对象实例化之后立即设置。<br />
		/// If it is set to <c> True </c>, when data reading fails, the current SA1 value will be automatically changed, and the automatic increase will be selected, but it will not be consistent with DA1. This value needs to be set immediately after the object is instantiated .
		/// </summary>
		public bool IsChangeSA1AfterReadFailed { get; set; }

		#endregion

		#region Build Command

		/// <summary>
		/// 将普通的指令打包成完整的指令
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		private byte[] PackCommand(byte[] cmd)
		{
			byte[] buffer = new byte[26 + cmd.Length];
			Array.Copy(handSingle, 0, buffer, 0, 4);
			byte[] tmp = BitConverter.GetBytes(buffer.Length - 8);
			Array.Reverse(tmp);
			tmp.CopyTo(buffer, 4);
			buffer[11] = 0x02;

			buffer[16] = ICF;
			buffer[17] = RSV;
			buffer[18] = GCT;
			buffer[19] = DNA;
			buffer[20] = DA1;
			buffer[21] = DA2;
			buffer[22] = SNA;
			buffer[23] = SA1;
			buffer[24] = SA2;
			buffer[25] = SID;
			cmd.CopyTo(buffer, 26);

			return buffer;
		}

		/// <summary>
		/// 根据类型地址长度确认需要读取的指令头<br />
		/// Confirm the instruction header to be read according to the type address length
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">长度</param>
		/// <param name="isBit">是否是位读取</param>
		/// <returns>带有成功标志的报文数据</returns>
		public OperateResult<byte[]> BuildReadCommand(string address, ushort length, bool isBit)
		{
			var command = OmronFinsNetHelper.BuildReadCommand(address, length, isBit);
			if (!command.IsSuccess) return command;

			return OperateResult.CreateSuccessResult(PackCommand(command.Content));
		}

		/// <summary>
		/// 根据类型地址以及需要写入的数据来生成指令头<br />
		/// Generate instruction header based on type address and data to be writtens
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">真实的数据值信息</param>
		/// <param name="isBit">是否是位操作</param>
		/// <returns>带有成功标志的报文数据</returns>
		public OperateResult<byte[]> BuildWriteCommand(string address, byte[] value, bool isBit)
		{
			var command = OmronFinsNetHelper.BuildWriteWordCommand(address, value, isBit);
			if (!command.IsSuccess) return command;

			return OperateResult.CreateSuccessResult(PackCommand(command.Content));
		}

		#endregion

		#region Double Mode Override

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect(Socket socket)
		{
			// 握手信号
			OperateResult<byte[]> read = ReadFromCoreServer(socket, handSingle);
			if (!read.IsSuccess) return read;

			// 检查返回的状态
			byte[] buffer = new byte[4];
			buffer[0] = read.Content[15];
			buffer[1] = read.Content[14];
			buffer[2] = read.Content[13];
			buffer[3] = read.Content[12];

			int status = BitConverter.ToInt32(buffer, 0);
			if (status != 0) return new OperateResult(status, OmronFinsNetHelper.GetStatusDescription(status));

			// 提取PLC的节点地址
			if (read.Content.Length >= 24) DA1 = read.Content[23];

			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc/>
		protected override void ExtraAfterReadFromCoreServer(OperateResult read)
		{
			base.ExtraAfterReadFromCoreServer(read);
			if (!read.IsSuccess)
			{
				if (IsChangeSA1AfterReadFailed)
				{
					// when read failed, changed the SA1 value
					SA1++;
					if (SA1 > 253) SA1 = 1;
					if (SA1 == DA1) SA1++;
					if (SA1 > 253) SA1 = 1;
				}
			}
		}

		#endregion

		#region Async Double Mode Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
		{
			// 握手信号
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(socket, handSingle);
			if (!read.IsSuccess) return read;

			// 检查返回的状态
			byte[] buffer = new byte[4];
			buffer[0] = read.Content[15];
			buffer[1] = read.Content[14];
			buffer[2] = read.Content[13];
			buffer[3] = read.Content[12];

			int status = BitConverter.ToInt32(buffer, 0);
			if (status != 0) return new OperateResult(status, OmronFinsNetHelper.GetStatusDescription(status));

			// 提取PLC的节点地址
			if (read.Content.Length >= 24) DA1 = read.Content[23];

			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Read Write Override

		/// <summary>
		/// 从欧姆龙PLC中读取想要的数据，返回读取结果，读取长度的单位为字，地址格式为"D100","C100","W100","H100","A100"<br />
		/// Read the desired data from the Omron PLC and return the read result. The unit of the read length is word. The address format is "D100", "C100", "W100", "H100", "A100"
		/// </summary>
		/// <param name="address">读取地址，格式为"D100","C100","W100","H100","A100"</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102,D103存储了产量计数，读取如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadExample2" title="Read示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadExample1" title="Read示例" />
		/// </example>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 获取指令
			var command = BuildReadCommand(address, length, false);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(valid);

			// 读取到了正确的数据
			return OperateResult.CreateSuccessResult(valid.Content);
		}

		/// <summary>
		/// 向PLC写入数据，数据格式为原始的字节类型，地址格式为"D100","C100","W100","H100","A100"<br />
		/// Write data to PLC, the data format is the original byte type, and the address format is "D100", "C100", "W100", "H100", "A100"
		/// </summary>
		/// <param name="address">初始地址</param>
		/// <param name="value">原始的字节数据</param>
		/// <returns>结果</returns>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102,D103存储了产量计数，读取如下：
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteExample2" title="Write示例" />
		/// 以下是写入不同类型数据的示例
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteExample1" title="Write示例" />
		/// </example>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 获取指令
			var command = BuildWriteCommand(address, value, false);
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			// 获取指令
			var command = BuildReadCommand(address, length, false);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心数据交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(valid);

			// 读取到了正确的数据
			return OperateResult.CreateSuccessResult(valid.Content);
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 获取指令
			var command = BuildWriteCommand(address, value, false);
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Bool Read Write

		/// <summary>
		/// 从欧姆龙PLC中批量读取位软元件，地址格式为"D100.0","C100.0","W100.0","H100.0","A100.0"<br />
		/// Read bit devices in batches from Omron PLC with address format "D100.0", "C100.0", "W100.0", "H100.0", "A100.0"
		/// </summary>
		/// <param name="address">读取地址，格式为"D100","C100","W100","H100","A100"</param>
		/// <param name="length">读取的长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="ReadBool" title="ReadBool示例" />
		/// </example>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 获取指令
			var command = BuildReadCommand(address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(valid);

			// 返回正确的数据信息
			return OperateResult.CreateSuccessResult(valid.Content.Select(m => m != 0x00).ToArray());
		}

		/// <summary>
		/// 向PLC中位软元件写入bool数组，返回是否写入成功，比如你写入D100,values[0]对应D100.0，地址格式为"D100.0","C100.0","W100.0","H100.0","A100.0"<br />
		/// Write the bool array to the PLC's median device and return whether the write was successful. For example, if you write D100, values [0] corresponds to D100.0 
		/// and the address format is "D100.0", "C100.0", "W100. 0 "," H100.0 "," A100.0 "
		/// </summary>
		/// <param name="address">要写入的数据地址</param>
		/// <param name="values">要写入的实际数据，可以指定任意的长度</param>
		/// <returns>返回写入结果</returns>
		/// <example>
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Profinet\OmronFinsNet.cs" region="WriteBool" title="WriteBool示例" />
		/// </example>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			// 获取指令
			var command = BuildWriteCommand(address, values.Select(m => m ? (byte)0x01 : (byte)0x00).ToArray(), true);
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Bool Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			// 获取指令
			var command = BuildReadCommand(address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心数据交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(valid);

			// 返回正确的数据信息
			return OperateResult.CreateSuccessResult(valid.Content.Select(m => m != 0x00 ? true : false).ToArray());
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			// 获取指令
			var command = BuildWriteCommand(address, values.Select(m => m ? (byte)0x01 : (byte)0x00).ToArray(), true);
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Hand Single

		// 握手信号
		// 46494E530000000C0000000000000000000000D6 
		private readonly byte[] handSingle = new byte[]
		{
			0x46, 0x49, 0x4E, 0x53, // FINS
			0x00, 0x00, 0x00, 0x0C, // 后面的命令长度
			0x00, 0x00, 0x00, 0x00, // 命令码
			0x00, 0x00, 0x00, 0x00, // 错误码
			0x00, 0x00, 0x00, 0x01  // 节点号
		};

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"OmronFinsNet[{IpAddress}:{Port}]";

		#endregion
	}
}
