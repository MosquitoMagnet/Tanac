using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.Net;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.Profinet.FATEK
{
	/// <summary>
	/// 台湾永宏公司的编程口协议，此处是基于tcp的实现，地址信息请查阅api文档信息<br />
	/// The programming port protocol of Taiwan Yonghong company, here is the implementation based on TCP, please refer to the API information for the address information
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
	///     <term>步进继电器</term>
	///     <term>S</term>
	///     <term>S100,S200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的触点</term>
	///     <term>T</term>
	///     <term>T100,T200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的当前值</term>
	///     <term>RT</term>
	///     <term>RT100,RT200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的触点</term>
	///     <term>C</term>
	///     <term>C100,C200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的当前</term>
	///     <term>RC</term>
	///     <term>RC100,RC200</term>
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
	public class FatekProgramOverTcp : NetworkDeviceSoloBase
	{
		#region Constructor

		/// <summary>
		/// 实例化默认的构造方法<br />
		/// Instantiate the default constructor
		/// </summary>
		public FatekProgramOverTcp()
		{
			this.WordLength = 1;
			this.ByteTransform = new RegularByteTransform();
		}

		/// <summary>
		/// 使用指定的ip地址和端口来实例化一个对象<br />
		/// Instantiate an object with the specified IP address and port
		/// </summary>
		/// <param name="ipAddress">设备的Ip地址</param>
		/// <param name="port">设备的端口号</param>
		public FatekProgramOverTcp(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			this.ByteTransform = new RegularByteTransform();
		}

		#endregion

		#region Public Member

		/// <summary>
		/// PLC的站号信息，需要和实际的设置值一致，默认为1<br />
		/// The station number information of the PLC needs to be consistent with the actual setting value. The default is 1.
		/// </summary>
		public byte Station { get => station; set => station = value; }

		#endregion

		#region Read Write Support

		/// <summary>
		/// 批量读取PLC的字节数据，以字为单位，支持读取X,Y,M,S,D,T,C,R,RT,RC具体的地址范围需要根据PLC型号来确认<br />
		/// Read PLC byte data in batches, in word units. Supports reading X, Y, M, S, D, T, C, R, RT, RC. The specific address range needs to be confirmed according to the PLC model.
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand(this.station, address, length, false);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult<byte[]>(read.Content[0], "Read Faild:" + BasicFramework.SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<byte[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			byte[] Content = new byte[length * 2];
			for (int i = 0; i < Content.Length / 2; i++)
			{
				ushort tmp = Convert.ToUInt16(Encoding.ASCII.GetString(read.Content, i * 4 + 6, 4), 16);
				BitConverter.GetBytes(tmp).CopyTo(Content, i * 2);
			}
			return OperateResult.CreateSuccessResult(Content);
		}

		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C,R,RT,RC具体的地址范围需要根据PLC型号来确认<br />
		/// The data written to the PLC in batches, in units of words, that is, at least 2 bytes of information, supporting X, Y, M, S, D, T, C, R, RT, and RC. The specific address range needs to be based on the PLC model To confirm
		/// </summary>
		/// <param name="address">地址信息，举例，D100，R200，RC100，RT200</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteByteCommand(this.station, address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult(read.Content[0], "Write Faild:" + SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<byte[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand(this.station, address, length, false);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult<byte[]>(read.Content[0], "Read Faild:" + SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<byte[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			byte[] Content = new byte[length * 2];
			for (int i = 0; i < Content.Length / 2; i++)
			{
				ushort tmp = Convert.ToUInt16(Encoding.ASCII.GetString(read.Content, i * 4 + 6, 4), 16);
				BitConverter.GetBytes(tmp).CopyTo(Content, i * 2);
			}
			return OperateResult.CreateSuccessResult(Content);
		}

		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteByteCommand(this.station, address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult(read.Content[0], "Write Faild:" + SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<byte[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Bool Read Write

		/// <summary>
		/// 批量读取bool类型数据，支持的类型为X,Y,M,S,T,C，具体的地址范围取决于PLC的类型<br />
		/// Read bool data in batches. The supported types are X, Y, M, S, T, C. The specific address range depends on the type of PLC.
		/// </summary>
		/// <param name="address">地址信息，比如X10，Y17，M100</param>
		/// <param name="length">读取的长度</param>
		/// <returns>读取结果信息</returns>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand(this.station, address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult<bool[]>(read.Content[0], "Read Faild:" + SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<bool[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			byte[] buffer = new byte[length];
			Array.Copy(read.Content, 6, buffer, 0, length);
			return OperateResult.CreateSuccessResult(buffer.Select(m => m == 0x31).ToArray());
		}

		/// <summary>
		/// 批量写入bool类型的数组，支持的类型为X,Y,M,S,T,C，具体的地址范围取决于PLC的类型<br />
		/// Write arrays of type bool in batches. The supported types are X, Y, M, S, T, C. The specific address range depends on the type of PLC.
		/// </summary>
		/// <param name="address">PLC的地址信息</param>
		/// <param name="value">数据信息</param>
		/// <returns>是否写入成功</returns>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteBoolCommand(this.station, address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult(read.Content[0], "Write Faild:" + SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<bool[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Bool Read Write
#if !NET35 && !NET20
		/// <inheritdoc cref="ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildReadCommand(this.station, address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult<bool[]>(read.Content[0], "Read Faild:" + SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<bool[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			byte[] buffer = new byte[length];
			Array.Copy(read.Content, 6, buffer, 0, length);
			return OperateResult.CreateSuccessResult(buffer.Select(m => m == 0x31).ToArray());
		}

		/// <inheritdoc cref="Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync(string address, bool[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = BuildWriteBoolCommand(this.station, address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult(read.Content[0], "Write Faild:" + SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<bool[]>(read.Content[5], GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"FatekProgramOverTcp[{IpAddress}:{Port}]";

		#endregion

		#region Private Member

		private byte station = 0x01;                 // PLC的站号信息

		#endregion

		#region Static Helper

		/// <summary>
		/// 解析数据地址成不同的三菱地址类型
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>地址结果对象</returns>
		public static OperateResult<string> FatekAnalysisAddress(string address)
		{
			var result = new OperateResult<string>();
			try
			{
				switch (address[0])
				{
					case 'X':
					case 'x':
						{
							result.Content = "X" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'Y':
					case 'y':
						{
							result.Content = "Y" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'M':
					case 'm':
						{
							result.Content = "M" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'S':
					case 's':
						{
							result.Content = "S" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'T':
					case 't':
						{
							result.Content = "T" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'C':
					case 'c':
						{
							result.Content = "C" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'D':
					case 'd':
						{
							result.Content = "D" + Convert.ToUInt16(address.Substring(1), 10).ToString("D5");
							break;
						}
					case 'R':
					case 'r':
						{
							if (address[1] == 'T' || address[1] == 't')
							{
								result.Content = "RT" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							}
							else if (address[1] == 'C' || address[1] == 'c')
							{
								result.Content = "RC" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							}
							else
							{
								result.Content = "R" + Convert.ToUInt16(address.Substring(1), 10).ToString("D5");
							}
							break;
						}
					default: throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				result.Message = ex.Message;
				return result;
			}

			result.IsSuccess = true;
			return result;
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
		/// 创建一条读取的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">PLCd的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="isBool">是否位读取</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildReadCommand(byte station, string address, ushort length, bool isBool)
		{
			OperateResult<string> addressAnalysis = FatekAnalysisAddress(address);
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressAnalysis);

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append((char)0x02);
			stringBuilder.Append(station.ToString("X2"));

			if (isBool)
			{
				stringBuilder.Append("44");
				stringBuilder.Append(length.ToString("X2"));
			}
			else
			{
				stringBuilder.Append("46");
				stringBuilder.Append(length.ToString("X2"));
				if (addressAnalysis.Content.StartsWith("X") ||
					addressAnalysis.Content.StartsWith("Y") ||
					addressAnalysis.Content.StartsWith("M") ||
					addressAnalysis.Content.StartsWith("S") ||
					addressAnalysis.Content.StartsWith("T") ||
					addressAnalysis.Content.StartsWith("C"))
				{
					stringBuilder.Append("W");
				}
			}

			stringBuilder.Append(addressAnalysis.Content);
			stringBuilder.Append(CalculateAcc(stringBuilder.ToString()));
			stringBuilder.Append((char)0x03);

			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
		}

		/// <summary>
		/// 创建一条别入bool数据的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteBoolCommand(byte station, string address, bool[] value)
		{
			OperateResult<string> addressAnalysis = FatekAnalysisAddress(address);
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressAnalysis);

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append((char)0x02);
			stringBuilder.Append(station.ToString("X2"));

			stringBuilder.Append("45");
			stringBuilder.Append(value.Length.ToString("X2"));

			stringBuilder.Append(addressAnalysis.Content);

			for (int i = 0; i < value.Length; i++)
			{
				stringBuilder.Append(value[i] ? "1" : "0");
			}

			stringBuilder.Append(CalculateAcc(stringBuilder.ToString()));
			stringBuilder.Append((char)0x03);

			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
		}

		/// <summary>
		/// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteByteCommand(byte station, string address, byte[] value)
		{
			OperateResult<string> addressAnalysis = FatekAnalysisAddress(address);
			if (!addressAnalysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(addressAnalysis);

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append((char)0x02);
			stringBuilder.Append(station.ToString("X2"));

			stringBuilder.Append("47");
			stringBuilder.Append((value.Length / 2).ToString("X2"));


			if (addressAnalysis.Content.StartsWith("X") ||
				addressAnalysis.Content.StartsWith("Y") ||
				addressAnalysis.Content.StartsWith("M") ||
				addressAnalysis.Content.StartsWith("S") ||
				addressAnalysis.Content.StartsWith("T") ||
				addressAnalysis.Content.StartsWith("C"))
			{
				stringBuilder.Append("W");
			}

			stringBuilder.Append(addressAnalysis.Content);

			byte[] buffer = new byte[value.Length * 2];
			for (int i = 0; i < value.Length / 2; i++)
			{
				SoftBasic.BuildAsciiBytesFrom(BitConverter.ToUInt16(value, i * 2)).CopyTo(buffer, 4 * i);
			}
			stringBuilder.Append(Encoding.ASCII.GetString(buffer));

			stringBuilder.Append(CalculateAcc(stringBuilder.ToString()));
			stringBuilder.Append((char)0x03);

			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetBytes(stringBuilder.ToString()));
		}

		/// <summary>
		/// 根据错误码获取到真实的文本信息
		/// </summary>
		/// <param name="code">错误码</param>
		/// <returns>错误的文本描述</returns>
		public static string GetErrorDescriptionFromCode(char code)
		{
			switch (code)
			{
				case '2': return StringResources.Language.FatekStatus02;
				case '3': return StringResources.Language.FatekStatus03;
				case '4': return StringResources.Language.FatekStatus04;
				case '5': return StringResources.Language.FatekStatus05;
				case '6': return StringResources.Language.FatekStatus06;
				case '7': return StringResources.Language.FatekStatus07;
				case '9': return StringResources.Language.FatekStatus09;
				case 'A': return StringResources.Language.FatekStatus10;
				default: return StringResources.Language.UnknownError;
			}
		}

		#endregion
	}
}

