using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.BasicFramework;
using Communication.Core.Address;
using Communication.Core.IMessage;
using Communication.Core;
using Communication.Core.Net;
using Communication.Reflection;
using System.Threading.Tasks;


namespace Communication.Profinet.Fuji
{
	/// <summary>
	/// 富士PLC的SPB协议，详细的地址信息见api文档说明<br />
	/// Fuji PLC's SPB protocol. For detailed address information, see the api documentation.
	/// </summary>
	/// <remarks>
	/// 其所支持的地址形式如下：
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
	///     <term>X10,X20</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y10,Y20</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>锁存继电器</term>
	///     <term>L</term>
	///     <term>L100,L200</term>
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
	///     <term>计数器的线圈</term>
	///     <term>CC</term>
	///     <term>CC100,CC200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的当前</term>
	///     <term>CN</term>
	///     <term>CN100,CN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
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
	///     <term>文件寄存器</term>
	///     <term>R</term>
	///     <term>R100,R200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	public class FujiSPBOverTcp : NetworkDeviceSoloBase
	{
		private byte station = 1;

		/// <summary>
		/// PLC的站号信息<br />
		/// PLC station number information
		/// </summary>
		public byte Station
		{
			get
			{
				return station;
			}
			set
			{
				station = value;
			}
		}

		/// <summary>
		/// 使用默认的构造方法实例化对象<br />
		/// Instantiate the object using the default constructor
		/// </summary>
		public FujiSPBOverTcp()
		{
			this.WordLength = 1;
			base.LogMsgFormatBinary = false;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <summary>
		/// 使用指定的ip地址和端口来实例化一个对象<br />
		/// Instantiate an object with the specified IP address and port
		/// </summary>
		/// <param name="ipAddress">设备的Ip地址</param>
		/// <param name="port">设备的端口号</param>
		public FujiSPBOverTcp(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			base.LogMsgFormatBinary = false;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc />
		protected override INetMessage GetNewNetMessage()
		{
			return new FujiSPBMessage();
		}


		/// <summary>
		/// 批量读取PLC的数据，以字为单位，支持读取X,Y,L,M,D,TN,CN,TC,CC,R具体的地址范围需要根据PLC型号来确认<br />
		/// Read PLC data in batches, in units of words. Supports reading X, Y, L, M, D, TN, CN, TC, CC, R. The specific address range needs to be confirmed according to the PLC model.
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<byte[]> operateResult = BuildReadCommand(station, address, length);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult2);
			}
			OperateResult<byte[]> operateResult3 = CheckResponseData(operateResult2.Content);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult3);
			}
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult3.Content.RemoveBegin(4)).ToHexBytes());
		}

		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持读取X,Y,L,M,D,TN,CN,TC,CC,R具体的地址范围需要根据PLC型号来确认<br />
		/// The data written to the PLC in batches, in units of words, that is, a minimum of 2 bytes of information. It supports reading X, Y, L, M, D, TN, CN, TC, CC, and R. The specific address range needs to be based on PLC model to confirm
		/// </summary>
		/// <param name="address">地址信息，举例，D100，R200，RC100，RT200</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<byte[]> operateResult = BuildWriteByteCommand(station, address, value);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return CheckResponseData(operateResult2.Content);
		}

		/// <summary>
		/// 批量读取PLC的Bool数据，以位为单位，支持读取X,Y,L,M,D,TN,CN,TC,CC,R,W，例如 M100, 如果是寄存器地址，可以使用D10.12来访问第10个字的12位，地址可以携带站号信息，例如：s=2;M100<br />
		/// Read PLC's Bool data in batches, in units of bits, support reading X, Y, L, M, D, TN, CN, TC, CC, R, W, such as M100, if it is a register address, 
		/// you can use D10. 12 to access the 12 bits of the 10th word, the address can carry station number information, for example: s=2;M100
		/// </summary>
		/// <param name="address">地址信息，举例：M100, D10.12</param>
		/// <param name="length">读取的bool长度信息</param>
		/// <returns>Bool[]的结果对象</returns>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			byte b = (byte)HslHelper.ExtractParameter(ref address, "s", station);
			OperateResult<FujiSPBAddress> operateResult = FujiSPBAddress.ParseFrom(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			if ((address.StartsWith("X") || address.StartsWith("Y") || address.StartsWith("M") || address.StartsWith("L") || address.StartsWith("TC") || address.StartsWith("CC")) && address.IndexOf('.') < 0)
			{
				operateResult.Content.BitIndex = (int)operateResult.Content.Address % 16;
				operateResult.Content.Address = (ushort)((int)operateResult.Content.Address / 16);
			}
			ushort length2 = (ushort)((operateResult.Content.GetBitIndex() + length - 1) / 16 - operateResult.Content.GetBitIndex() / 16 + 1);
			OperateResult<byte[]> operateResult2 = BuildReadCommand(b, operateResult.Content, length2);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult2);
			}
			OperateResult<byte[]> operateResult3 = ReadFromCoreServer(operateResult2.Content);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult3);
			}
			OperateResult<byte[]> operateResult4 = CheckResponseData(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult4);
			}
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult4.Content.RemoveBegin(4)).ToHexBytes().ToBoolArray()
				.SelectMiddle(operateResult.Content.BitIndex, length));
		}

		/// <summary>
		/// 写入一个Bool值到一个地址里，地址可以是线圈地址，也可以是寄存器地址，例如：M100, D10.12，地址可以携带站号信息，例如：s=2;D10.12<br />
		/// Write a Bool value to an address. The address can be a coil address or a register address, for example: M100, D10.12. 
		/// The address can carry station number information, for example: s=2;D10.12
		/// </summary>
		/// <param name="address">地址信息，举例：M100, D10.12</param>
		/// <param name="value">写入的bool值</param>
		/// <returns>是否写入成功的结果对象</returns>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			OperateResult<byte[]> operateResult = BuildWriteBoolCommand(station, address, value);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return CheckResponseData(operateResult2.Content);
		}

		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			OperateResult<byte[]> operateResult = BuildReadCommand(station, address, length);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = await ReadFromCoreServerAsync(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult2);
			}
			OperateResult<byte[]> operateResult3 = CheckResponseData(operateResult2.Content);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult3);
			}
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult3.Content.RemoveBegin(4)).ToHexBytes());
		}

		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			OperateResult<byte[]> operateResult = BuildWriteByteCommand(station, address, value);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = await ReadFromCoreServerAsync(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return CheckResponseData(operateResult2.Content);
		}

		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			byte b = (byte)HslHelper.ExtractParameter(ref address, "s", station);
			OperateResult<FujiSPBAddress> operateResult = FujiSPBAddress.ParseFrom(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			if ((address.StartsWith("X") || address.StartsWith("Y") || address.StartsWith("M") || address.StartsWith("L") || address.StartsWith("TC") || address.StartsWith("CC")) && address.IndexOf('.') < 0)
			{
				operateResult.Content.BitIndex = (int)operateResult.Content.Address % 16;
				operateResult.Content.Address = (ushort)((int)operateResult.Content.Address / 16);
			}
			ushort length2 = (ushort)((operateResult.Content.GetBitIndex() + length - 1) / 16 - operateResult.Content.GetBitIndex() / 16 + 1);
			OperateResult<byte[]> operateResult2 = BuildReadCommand(b, operateResult.Content, length2);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult2);
			}
			OperateResult<byte[]> operateResult3 = await ReadFromCoreServerAsync(operateResult2.Content);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult3);
			}
			OperateResult<byte[]> operateResult4 = CheckResponseData(operateResult3.Content);
			if (!operateResult4.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult4);
			}
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult4.Content.RemoveBegin(4)).ToHexBytes().ToBoolArray()
				.SelectMiddle(operateResult.Content.BitIndex, length));
		}

		public override async Task<OperateResult> WriteAsync(string address, bool value)
		{
			OperateResult<byte[]> operateResult = BuildWriteBoolCommand(station, address, value);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = await ReadFromCoreServerAsync(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			return CheckResponseData(operateResult2.Content);
		}

		/// <inheritdoc/>
		public override string ToString() => $"FujiSPBOverTcp[{IpAddress}:{Port}]";
		/// <summary>
		/// 将int数据转换成SPB可识别的标准的数据内容，例如 2转换为0200 , 200转换为0002
		/// </summary>
		/// <param name="address">等待转换的数据内容</param>
		/// <returns>转换之后的数据内容</returns>
		public static string AnalysisIntegerAddress(int address)
		{
			string tmp = address.ToString("D4");
			return tmp.Substring(2) + tmp.Substring(0, 2);
		}
	
		/// <summary>
		/// 计算指令的和校验码
		/// </summary>
		/// <param name="data">指令</param>
		/// <returns>校验之后的信息</returns>
		public static string CalculateAcc(string data)
		{
			byte[] buffer = Encoding.ASCII.GetBytes(data);

			int count = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				count += buffer[i];
			}

			return count.ToString("X4").Substring(2);
		}

		/// <summary>
		/// 创建一条读取的指令信息，需要指定一些参数，单次读取最大105个字
		/// </summary>
		/// <param name="station">PLC的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildReadCommand(byte station, string address, ushort length)
		{
			station = (byte)HslHelper.ExtractParameter(ref address, "s", station);
			OperateResult<FujiSPBAddress> operateResult = FujiSPBAddress.ParseFrom(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			return BuildReadCommand(station, operateResult.Content, length);
		}

		/// <summary>
		/// 创建一条读取的指令信息，需要指定一些参数，单次读取最大105个字
		/// </summary>
		/// <param name="station">PLC的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildReadCommand(byte station, FujiSPBAddress address, ushort length)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(':');
			stringBuilder.Append(station.ToString("X2"));
			stringBuilder.Append("09");
			stringBuilder.Append("FFFF");
			stringBuilder.Append("00");
			stringBuilder.Append("00");
			stringBuilder.Append(address.GetWordAddress());
			stringBuilder.Append(AnalysisIntegerAddress(length));
			stringBuilder.Append("\r\n");
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
		}

		/// <summary>
		/// 创建一条读取多个地址的指令信息，需要指定一些参数，单次读取最大105个字
		/// </summary>
		/// <param name="station">PLC的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="isBool">是否位读取</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildReadCommand(byte station, string[] address, ushort[] length, bool isBool)
		{
			if (address == null || length == null)
			{
				return new OperateResult<byte[]>("Parameter address or length can't be null");
			}
			if (address.Length != length.Length)
			{
				return new OperateResult<byte[]>(StringResources.Language.TwoParametersLengthIsNotSame);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(':');
			stringBuilder.Append(station.ToString("X2"));
			stringBuilder.Append((6 + address.Length * 4).ToString("X2"));
			stringBuilder.Append("FFFF");
			stringBuilder.Append("00");
			stringBuilder.Append("04");
			stringBuilder.Append("00");
			stringBuilder.Append(address.Length.ToString("X2"));
			for (int i = 0; i < address.Length; i++)
			{
				station = (byte)HslHelper.ExtractParameter(ref address[i], "s", station);
				OperateResult<FujiSPBAddress> operateResult = FujiSPBAddress.ParseFrom(address[i]);
				if (!operateResult.IsSuccess)
				{
					return OperateResult.CreateFailedResult<byte[]>(operateResult);
				}
				stringBuilder.Append(operateResult.Content.TypeCode);
				stringBuilder.Append(length[i].ToString("X2"));
				stringBuilder.Append(AnalysisIntegerAddress(operateResult.Content.Address));
			}
			stringBuilder[1] = station.ToString("X2")[0];
			stringBuilder[2] = station.ToString("X2")[1];
			stringBuilder.Append("\r\n");
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
		}

		/// <summary>
		/// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位，单次写入最大103个字
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteByteCommand(byte station, string address, byte[] value)
		{
			station = (byte)HslHelper.ExtractParameter(ref address, "s", station);
			OperateResult<FujiSPBAddress> operateResult = FujiSPBAddress.ParseFrom(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(':');
			stringBuilder.Append(station.ToString("X2"));
			stringBuilder.Append("00");
			stringBuilder.Append("FFFF");
			stringBuilder.Append("01");
			stringBuilder.Append("00");
			stringBuilder.Append(operateResult.Content.GetWordAddress());
			stringBuilder.Append(AnalysisIntegerAddress(value.Length / 2));
			stringBuilder.Append(value.ToHexString());
			stringBuilder[3] = ((stringBuilder.Length - 5) / 2).ToString("X2")[0];
			stringBuilder[4] = ((stringBuilder.Length - 5) / 2).ToString("X2")[1];
			stringBuilder.Append("\r\n");
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
		}

		/// <summary>
		/// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位，单次写入最大103个字
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteBoolCommand(byte station, string address, bool value)
		{
			station = (byte)HslHelper.ExtractParameter(ref address, "s", station);
			OperateResult<FujiSPBAddress> operateResult = FujiSPBAddress.ParseFrom(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			if ((address.StartsWith("X") || address.StartsWith("Y") || address.StartsWith("M") || address.StartsWith("L") || address.StartsWith("TC") || address.StartsWith("CC")) && address.IndexOf('.') < 0)
			{
				operateResult.Content.BitIndex = (int)operateResult.Content.Address % 16;
				operateResult.Content.Address = (ushort)((int)operateResult.Content.Address / 16);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(':');
			stringBuilder.Append(station.ToString("X2"));
			stringBuilder.Append("00");
			stringBuilder.Append("FFFF");
			stringBuilder.Append("01");
			stringBuilder.Append("02");
			stringBuilder.Append(operateResult.Content.GetWriteBoolAddress());
			stringBuilder.Append(value ? "01" : "00");
			stringBuilder[3] = ((stringBuilder.Length - 5) / 2).ToString("X2")[0];
			stringBuilder[4] = ((stringBuilder.Length - 5) / 2).ToString("X2")[1];
			stringBuilder.Append("\r\n");
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
		}

		/// <summary>
		/// 检查反馈的数据信息，是否包含了错误码，如果没有包含，则返回成功
		/// </summary>
		/// <param name="content">原始的报文返回</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> CheckResponseData(byte[] content)
		{
			if (content[0] != 58)
			{
				return new OperateResult<byte[]>(content[0], "Read Faild:" + SoftBasic.ByteToHexString(content, ' '));
			}
			string @string = Encoding.ASCII.GetString(content, 9, 2);
			if (@string != "00")
			{
				return new OperateResult<byte[]>(Convert.ToInt32(@string, 16), GetErrorDescriptionFromCode(@string));
			}
			if (content[content.Length - 2] == 13 && content[content.Length - 1] == 10)
			{
				content = content.RemoveLast(2);
			}
			return OperateResult.CreateSuccessResult(content.RemoveBegin(11));
		}

		/// <summary>
		/// 根据错误码获取到真实的文本信息
		/// </summary>
		/// <param name="code">错误码</param>
		/// <returns>错误的文本描述</returns>
		public static string GetErrorDescriptionFromCode(string code)
		{
			switch (code)
			{
				case "01":
					return StringResources.Language.FujiSpbStatus01;
				case "02":
					return StringResources.Language.FujiSpbStatus02;
				case "03":
					return StringResources.Language.FujiSpbStatus03;
				case "04":
					return StringResources.Language.FujiSpbStatus04;
				case "05":
					return StringResources.Language.FujiSpbStatus05;
				case "06":
					return StringResources.Language.FujiSpbStatus06;
				case "07":
					return StringResources.Language.FujiSpbStatus07;
				case "09":
					return StringResources.Language.FujiSpbStatus09;
				case "0C":
					return StringResources.Language.FujiSpbStatus0C;
				default:
					return StringResources.Language.UnknownError;
			}
		}


	}
}