using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Communication.BasicFramework;
using Communication.Core.Net;
using Communication.Core;
using Communication.Reflection;
#if !NET35 && !NET20
using System.Threading.Tasks;
#endif

namespace Communication.Profinet.Keyence
{
	/// <summary>
	/// 基恩士KV上位链路串口通信的对象,适用于Nano系列串口数据,以及L20V通信模块，本类是基于tcp通信方式的类<br />
	/// Keyence KV upper link serial communication object, suitable for Nano series serial data and L20V communication module, this class is a class based on TCP communication method
	/// </summary>
	/// <remarks>
	/// 当读取Bool的输入的格式说明如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>继电器</term>
	///     <term>R</term>
	///     <term>R0,R100</term>
	///     <term>0-59915</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>控制继电器</term>
	///     <term>CR</term>
	///     <term>CR0,CR100</term>
	///     <term>0-3915</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>内部辅助继电器</term>
	///     <term>MR</term>
	///     <term>MR0,MR100</term>
	///     <term>0-99915</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>锁存继电器</term>
	///     <term>LR</term>
	///     <term>LR0,LR100</term>
	///     <term>0-99915</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器</term>
	///     <term>T</term>
	///     <term>T0,T100</term>
	///     <term>0-3999</term>
	///     <term>通断</term>
	///   </item>
	///   <item>
	///     <term>计数器</term>
	///     <term>C</term>
	///     <term>C0,C100</term>
	///     <term>0-3999</term>
	///     <term>通断</term>
	///   </item>
	///   <item>
	///     <term>高速计数器</term>
	///     <term>CTH</term>
	///     <term>CTH0,CTH1</term>
	///     <term>0-1</term>
	///     <term>通断</term>
	///   </item>
	/// </list>
	/// 读取数据的地址如下：
	/// 
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>数据存储器</term>
	///     <term>DM</term>
	///     <term>DM0,DM100</term>
	///     <term>0-65534</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>控制存储器</term>
	///     <term>CM</term>
	///     <term>CM0,CM100</term>
	///     <term>0-11998</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>临时数据存储器</term>
	///     <term>TM</term>
	///     <term>TM0,TM100</term>
	///     <term>0-511</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>扩展数据存储器</term>
	///     <term>EM</term>
	///     <term>EM0,EM100</term>
	///     <term>0-65534</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>扩展数据存储器</term>
	///     <term>FM</term>
	///     <term>FM0,FM100</term>
	///     <term>0-32766</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>变址寄存器</term>
	///     <term>Z</term>
	///     <term>Z1,Z5</term>
	///     <term>1-12</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>数字微调器</term>
	///     <term>AT</term>
	///     <term>AT0,AT5</term>
	///     <term>0-7</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器</term>
	///     <term>T</term>
	///     <term>T0,T100</term>
	///     <term>0-3999</term>
	///     <term>当前值(current value), 读int</term>
	///   </item>
	///   <item>
	///     <term>计数器</term>
	///     <term>C</term>
	///     <term>C0,C100</term>
	///     <term>0-3999</term>
	///     <term>当前值(current value), 读int</term>
	///   </item>
	/// </list>
	/// </remarks>
	public class KeyenceNanoSerialOverTcp : NetworkDeviceSoloBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public KeyenceNanoSerialOverTcp()
		{
			this.WordLength = 1;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <summary>
		/// 使用指定的ip地址和端口号来初始化对象<br />
		/// Initialize the object with the specified IP address and port number
		/// </summary>
		/// <param name="ipAddress">Ip地址数据</param>
		/// <param name="port">端口号</param>
		public KeyenceNanoSerialOverTcp(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			this.ByteTransform = new RegularByteTransform();
		}

		#endregion

		#region Initialization Override

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect(Socket socket)
		{
			// 建立通讯连接{CR/r}
			var result = ReadFromCoreServer(socket, ConnectCmd);
			if (!result.IsSuccess) return result;

			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc/>
		protected override OperateResult ExtraOnDisconnect(Socket socket)
		{
			var result = ReadFromCoreServer(socket, DisConnectCmd);
			if (!result.IsSuccess) return result;

			return OperateResult.CreateSuccessResult();
		}
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected async override Task<OperateResult> InitializationOnConnectAsync(Socket socket)
		{
			// 建立通讯连接{CR/r}
			var result = await ReadFromCoreServerAsync(socket, ConnectCmd);
			if (!result.IsSuccess) return result;

			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc/>
		protected async override Task<OperateResult> ExtraOnDisconnectAsync(Socket socket)
		{
			var result = await ReadFromCoreServerAsync(socket, DisConnectCmd);
			if (!result.IsSuccess) return result;

			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Read Write Support

		/// <inheritdoc/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 获取指令
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 反馈检查
			OperateResult ackResult = KeyenceNanoSerialOverTcp.CheckPlcReadResponse(read.Content);
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(ackResult);

			var addressResult = KeyenceNanoSerialOverTcp.KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			// 数据提炼
			return KeyenceNanoSerialOverTcp.ExtractActualData(addressResult.Content1, read.Content);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 获取写入
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildWriteCommand(address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = KeyenceNanoSerialOverTcp.CheckPlcWriteResponse(read.Content);
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			// 获取指令
			OperateResult<byte[]> command = BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 反馈检查
			OperateResult ackResult = CheckPlcReadResponse(read.Content);
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(ackResult);

			var addressResult = KeyenceNanoSerialOverTcp.KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			// 数据提炼
			return KeyenceNanoSerialOverTcp.ExtractActualData(addressResult.Content1, read.Content);
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 获取写入
			OperateResult<byte[]> command = BuildWriteCommand(address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = CheckPlcWriteResponse(read.Content);
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Read Write Bool

		/// <inheritdoc/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 获取指令
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 反馈检查
			OperateResult ackResult = KeyenceNanoSerialOverTcp.CheckPlcReadResponse(read.Content);
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(ackResult);

			var addressResult = KeyenceNanoSerialOverTcp.KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 数据提炼
			return KeyenceNanoSerialOverTcp.ExtractActualBoolData(addressResult.Content1, read.Content);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			// 获取写入
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildWriteCommand(address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = KeyenceNanoSerialOverTcp.CheckPlcWriteResponse(read.Content);
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			// 获取指令
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 反馈检查
			OperateResult ackResult = KeyenceNanoSerialOverTcp.CheckPlcReadResponse(read.Content);
			if (!ackResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(ackResult);

			var addressResult = KeyenceNanoSerialOverTcp.KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(addressResult);

			// 数据提炼
			return KeyenceNanoSerialOverTcp.ExtractActualBoolData(addressResult.Content1, read.Content);
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync(string address, bool value)
		{
			// 获取写入
			OperateResult<byte[]> command = KeyenceNanoSerialOverTcp.BuildWriteCommand(address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			OperateResult checkResult = KeyenceNanoSerialOverTcp.CheckPlcWriteResponse(read.Content);
			if (!checkResult.IsSuccess) return checkResult;

			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Private Member

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"KeyenceNanoSerialOverTcp[{IpAddress}:{Port}]";

		#endregion

		#region Static Method Helper

		/// <summary>
		/// 连接PLC的命令报文<br />
		/// Command message to connect to PLC
		/// </summary>
		public static byte[] ConnectCmd = new byte[3] { 0x43, 0x52, 0x0d };        // 建立通讯连接{CR\r}

		/// <summary>
		/// 断开PLC连接的命令报文<br />
		/// Command message to disconnect PLC
		/// </summary>
		public static byte[] DisConnectCmd = new byte[3] { 0x43, 0x51, 0x0d };     // 关闭通讯连接{CQ\r}

		/// <summary>
		/// 建立读取PLC数据的指令，需要传入地址数据，以及读取的长度，地址示例参照类的说明文档<br />
		/// To create a command to read PLC data, you need to pass in the address data, and the length of the read. For an example of the address, refer to the class documentation
		/// </summary>
		/// <param name="address">软元件地址</param>
		/// <param name="length">读取长度</param>
		/// <returns>是否建立成功</returns>
		public static OperateResult<byte[]> BuildReadCommand(string address, ushort length)
		{
			var addressResult = KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			if (addressResult.Content1 == "CTH" || addressResult.Content1 == "CTC" || addressResult.Content1 == "C" || addressResult.Content1 == "T")
			{
				if (length > 1) length = (ushort)(length / 2);
			}

			StringBuilder StrCommand = new StringBuilder();
			StrCommand.Append("RDS");                               // 批量读取
			StrCommand.Append(" ");                                 // 空格符
			StrCommand.Append(addressResult.Content1);              // 软元件类型，如DM
			StrCommand.Append(addressResult.Content2.ToString());  // 软元件的地址，如1000
			StrCommand.Append(" ");                                 // 空格符
			StrCommand.Append(length.ToString());
			StrCommand.Append("\r");                                //结束符

			byte[] _PLCCommand = Encoding.ASCII.GetBytes(StrCommand.ToString());
			return OperateResult.CreateSuccessResult(_PLCCommand);
		}

		/// <summary>
		/// 建立写入PLC数据的指令，需要传入地址数据，以及写入的数据信息，地址示例参照类的说明文档<br />
		/// To create a command to write PLC data, you need to pass in the address data and the written data information. For an example of the address, refer to the class documentation
		/// </summary>
		/// <param name="address">软元件地址</param>
		/// <param name="value">转换后的数据</param>
		/// <returns>是否成功的信息</returns>
		public static OperateResult<byte[]> BuildWriteCommand(string address, byte[] value)
		{
			var addressResult = KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			StringBuilder StrCommand = new StringBuilder();
			StrCommand.Append("WRS");                         // 批量读取
			StrCommand.Append(" ");                           // 空格符
			StrCommand.Append(addressResult.Content1);        // 软元件地址
			StrCommand.Append(addressResult.Content2);        // 软元件地址
			StrCommand.Append(" ");                           // 空格符

			if (addressResult.Content1 == "DM" || addressResult.Content1 == "CM" || addressResult.Content1 == "TM" ||
				addressResult.Content1 == "EM" || addressResult.Content1 == "FM" || addressResult.Content1 == "Z")
			{
				int length = value.Length / 2;
				StrCommand.Append(length.ToString());
				StrCommand.Append(" ");
				for (int i = 0; i < length; i++)
				{
					StrCommand.Append(BitConverter.ToUInt16(value, i * 2));
					if (i != length - 1) StrCommand.Append(" ");
				}
			}
			else if (addressResult.Content1 == "T" || addressResult.Content1 == "C" || addressResult.Content1 == "CTH")
			{
				int length = value.Length / 4;
				StrCommand.Append(length.ToString());
				StrCommand.Append(" ");
				for (int i = 0; i < length; i++)
				{
					StrCommand.Append(BitConverter.ToUInt32(value, i * 4));
					if (i != length - 1) StrCommand.Append(" ");
				}
			}
			StrCommand.Append("\r");

			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(StrCommand.ToString()));
		}

		/// <summary>
		/// 建立写入bool数据的指令，针对地址类型为 R,CR,MR,LR<br />
		/// Create instructions to write bool data, address type is R, CR, MR, LR
		/// </summary>
		/// <param name="address">软元件地址</param>
		/// <param name="value">转换后的数据</param>
		/// <returns>是否成功的信息</returns>
		public static OperateResult<byte[]> BuildWriteCommand(string address, bool value)
		{
			var addressResult = KvAnalysisAddress(address);
			if (!addressResult.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressResult);

			StringBuilder StrCommand = new StringBuilder();
			if (value)
				StrCommand.Append("ST");                      // 置位
			else
				StrCommand.Append("RS");                      // 复位
			StrCommand.Append(" ");                           // 空格符
			StrCommand.Append(addressResult.Content1);        // 软元件地址
			StrCommand.Append(addressResult.Content2);        // 软元件地址
			StrCommand.Append("\r");                          // 空格符
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(StrCommand.ToString()));
		}

		/// <summary>
		/// 校验读取返回数据状态，主要返回的第一个字节是不是E<br />
		/// Check the status of the data returned from reading, whether the first byte returned is E
		/// </summary>
		/// <param name="ack">反馈信息</param>
		/// <returns>是否成功的信息</returns>
		public static OperateResult CheckPlcReadResponse(byte[] ack)
		{
			if (ack.Length == 0) return new OperateResult(StringResources.Language.MelsecFxReceiveZero);
			if (ack[0] == 0x45) return new OperateResult(StringResources.Language.MelsecFxAckWrong + " Actual: " + Encoding.ASCII.GetString(ack));
			if ((ack[ack.Length - 1] != 0x0A) && (ack[ack.Length - 2] != 0x0D)) return new OperateResult(StringResources.Language.MelsecFxAckWrong + " Actual: " + SoftBasic.ByteToHexString(ack, ' '));
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 校验写入返回数据状态，检测返回的数据是不是OK<br />
		/// Verify the status of the returned data written and check whether the returned data is OK
		/// </summary>
		/// <param name="ack">反馈信息</param>
		/// <returns>是否成功的信息</returns>
		public static OperateResult CheckPlcWriteResponse(byte[] ack)
		{
			if (ack.Length == 0) return new OperateResult(StringResources.Language.MelsecFxReceiveZero);
			if (ack[0] == 0x4F && ack[1] == 0x4B) return OperateResult.CreateSuccessResult();
			return new OperateResult(StringResources.Language.MelsecFxAckWrong + " Actual: " + SoftBasic.ByteToHexString(ack, ' '));
		}

		/// <summary>
		/// 从PLC反馈的数据进行提炼Bool操作<br />
		/// Refine Bool operation from data fed back from PLC
		/// </summary>
		/// <param name="addressType">地址的数据类型</param>
		/// <param name="response">PLC反馈的真实数据</param>
		/// <returns>数据提炼后的真实数据</returns>
		public static OperateResult<bool[]> ExtractActualBoolData(string addressType, byte[] response)
		{
			try
			{
				if (string.IsNullOrEmpty(addressType)) addressType = "R";

				string strResponse = Encoding.Default.GetString(response.RemoveLast(2));
				if (addressType == "R" || addressType == "CR" || addressType == "MR" || addressType == "LR")
				{
					return OperateResult.CreateSuccessResult(strResponse.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m == "1").ToArray());
				}
				else if (addressType == "T" || addressType == "C" || addressType == "CTH" || addressType == "CTC")
				{
					return OperateResult.CreateSuccessResult(
						strResponse.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.StartsWith("1")).ToArray());
				}
				else
				{
					return new OperateResult<bool[]>(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<bool[]>()
				{
					Message = "Extract Msg：" + ex.Message + Environment.NewLine +
					"Data: " + BasicFramework.SoftBasic.ByteToHexString(response)
				};
			}
		}

		/// <summary>
		/// 从PLC反馈的数据进行提炼操作<br />
		/// Refining operation from data fed back from PLC
		/// </summary>
		/// <param name="addressType">地址的数据类型</param>
		/// <param name="response">PLC反馈的真实数据</param>
		/// <returns>数据提炼后的真实数据</returns>
		public static OperateResult<byte[]> ExtractActualData(string addressType, byte[] response)
		{
			try
			{
				if (string.IsNullOrEmpty(addressType)) addressType = "R";

				string strResponse = Encoding.Default.GetString(response.RemoveLast(2));
				string[] splits = strResponse.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				if (addressType == "DM" || addressType == "CM" || addressType == "TM" || addressType == "EM" || addressType == "FM" || addressType == "Z")
				{
					byte[] buffer = new byte[splits.Length * 2];
					for (int i = 0; i < splits.Length; i++)
					{
						BitConverter.GetBytes(ushort.Parse(splits[i])).CopyTo(buffer, i * 2);
					}
					return OperateResult.CreateSuccessResult(buffer);
				}
				else if (addressType == "AT")
				{
					byte[] buffer = new byte[splits.Length * 4];
					for (int i = 0; i < splits.Length; i++)
					{
						BitConverter.GetBytes(uint.Parse(splits[i])).CopyTo(buffer, i * 4);
					}
					return OperateResult.CreateSuccessResult(buffer);
				}
				else if (addressType == "T" || addressType == "C" || addressType == "CTH" || addressType == "CTC")
				{
					byte[] buffer = new byte[splits.Length * 4];
					for (int i = 0; i < splits.Length; i++)
					{
						string[] datas = splits[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						BitConverter.GetBytes(uint.Parse(datas[1])).CopyTo(buffer, i * 4);
					}
					return OperateResult.CreateSuccessResult(buffer);
				}
				else
				{
					return new OperateResult<byte[]>(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>()
				{
					Message = "Extract Msg：" + ex.Message + Environment.NewLine +
					"Data: " + BasicFramework.SoftBasic.ByteToHexString(response)
				};
			}
		}

		/// <summary>
		/// 解析数据地址成不同的Keyence地址类型<br />
		/// Parse data addresses into different keyence address types
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>地址结果对象</returns>
		public static OperateResult<string, int> KvAnalysisAddress(string address)
		{
			try
			{
				if (address.StartsWith("CTH") || address.StartsWith("cth"))
					return OperateResult.CreateSuccessResult("CTH", int.Parse(address.Substring(3)));
				else if (address.StartsWith("CTC") || address.StartsWith("ctc"))
					return OperateResult.CreateSuccessResult("CTC", int.Parse(address.Substring(3)));
				else if (address.StartsWith("CR") || address.StartsWith("cr"))
					return OperateResult.CreateSuccessResult("CR", int.Parse(address.Substring(2)));
				else if (address.StartsWith("MR") || address.StartsWith("mr"))
					return OperateResult.CreateSuccessResult("MR", int.Parse(address.Substring(2)));
				else if (address.StartsWith("LR") || address.StartsWith("lr"))
					return OperateResult.CreateSuccessResult("LR", int.Parse(address.Substring(2)));
				else if (address.StartsWith("DM") || address.StartsWith("DM"))
					return OperateResult.CreateSuccessResult("DM", int.Parse(address.Substring(2)));
				else if (address.StartsWith("CM") || address.StartsWith("cm"))
					return OperateResult.CreateSuccessResult("CM", int.Parse(address.Substring(2)));
				else if (address.StartsWith("TM") || address.StartsWith("tm"))
					return OperateResult.CreateSuccessResult("TM", int.Parse(address.Substring(2)));
				else if (address.StartsWith("EM") || address.StartsWith("em"))
					return OperateResult.CreateSuccessResult("EM", int.Parse(address.Substring(2)));
				else if (address.StartsWith("FM") || address.StartsWith("fm"))
					return OperateResult.CreateSuccessResult("FM", int.Parse(address.Substring(2)));
				else if (address.StartsWith("AT") || address.StartsWith("at"))
					return OperateResult.CreateSuccessResult("AT", int.Parse(address.Substring(2)));
				else if (address.StartsWith("Z") || address.StartsWith("z"))
					return OperateResult.CreateSuccessResult("Z", int.Parse(address.Substring(1)));
				else if (address.StartsWith("R") || address.StartsWith("r"))
					return OperateResult.CreateSuccessResult("", int.Parse(address.Substring(1)));
				else if (address.StartsWith("T") || address.StartsWith("t"))
					return OperateResult.CreateSuccessResult("T", int.Parse(address.Substring(1)));
				else if (address.StartsWith("C") || address.StartsWith("c"))
					return OperateResult.CreateSuccessResult("C", int.Parse(address.Substring(1)));
				else
					throw new Exception(StringResources.Language.NotSupportedDataType);
			}
			catch (Exception ex)
			{
				return new OperateResult<string, int>(ex.Message);
			}
		}

		#endregion
	}
}
