using Communication.BasicFramework;
using Communication.Core.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Communication.Profinet.Melsec
{
	/// <summary>
	/// 所有三菱通讯类的通用辅助工具类，包含了一些通用的静态方法，可以使用本类来获取一些原始的报文信息。详细的操作参见例子<br />
	/// All general auxiliary tool classes of Mitsubishi communication class include some general static methods. 
	/// You can use this class to get some primitive message information. See the example for detailed operation
	/// </summary>
	public class MelsecHelper
	{
		#region Melsec Mc Address

		/// <summary>
		/// 解析A1E协议数据地址<br />
		/// Parse A1E protocol data address
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>结果对象</returns>
		public static OperateResult<MelsecA1EDataType, int> McA1EAnalysisAddress(string address)
		{
			var result = new OperateResult<MelsecA1EDataType, int>();
			try
			{
				switch (address[0])
				{
					case 'T':
					case 't':
						{
							if (address[1] == 'S' || address[1] == 's')
							{
								result.Content1 = MelsecA1EDataType.TS;
								result.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.TS.FromBase);
							}
							else if (address[1] == 'C' || address[1] == 'c')
							{
								result.Content1 = MelsecA1EDataType.TC;
								result.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.TC.FromBase);
							}
							else if (address[1] == 'N' || address[1] == 'n')
							{
								result.Content1 = MelsecA1EDataType.TN;
								result.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.TN.FromBase);
							}
							else
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							break;
						}
					case 'C':
					case 'c':
						{
							if (address[1] == 'S' || address[1] == 's')
							{
								result.Content1 = MelsecA1EDataType.CS;
								result.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.CS.FromBase);
							}
							else if (address[1] == 'C' || address[1] == 'c')
							{
								result.Content1 = MelsecA1EDataType.CC;
								result.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.CC.FromBase);
							}
							else if (address[1] == 'N' || address[1] == 'n')
							{
								result.Content1 = MelsecA1EDataType.CN;
								result.Content2 = Convert.ToInt32(address.Substring(2), MelsecA1EDataType.CN.FromBase);
							}
							else
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							break;
						}
					case 'X':
					case 'x':
						{
							result.Content1 = MelsecA1EDataType.X;
							address = address.Substring(1);
							if (address.StartsWith("0"))
								result.Content2 = Convert.ToInt32(address, 8);
							else
								result.Content2 = Convert.ToInt32(address, MelsecA1EDataType.X.FromBase);
							break;
						}
					case 'Y':
					case 'y':
						{
							result.Content1 = MelsecA1EDataType.Y;
							address = address.Substring(1);
							if (address.StartsWith("0"))
								result.Content2 = Convert.ToInt32(address, 8);
							else
								result.Content2 = Convert.ToInt32(address, MelsecA1EDataType.Y.FromBase);
							break;
						}
					case 'M':
					case 'm':
						{
							result.Content1 = MelsecA1EDataType.M;
							result.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.M.FromBase);
							break;
						}
					case 'S':
					case 's':
						{
							result.Content1 = MelsecA1EDataType.S;
							result.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.S.FromBase);
							break;
						}
					case 'F':
					case 'f':
						{
							result.Content1 = MelsecA1EDataType.F;
							result.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.F.FromBase);
							break;
						}
					case 'B':
					case 'b':
						{
							result.Content1 = MelsecA1EDataType.B;
							result.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.B.FromBase);
							break;
						}
					case 'D':
					case 'd':
						{
							result.Content1 = MelsecA1EDataType.D;
							result.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.D.FromBase);
							break;
						}
					case 'R':
					case 'r':
						{
							result.Content1 = MelsecA1EDataType.R;
							result.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.R.FromBase);
							break;
						}
					case 'W':
					case 'w':
						{
							result.Content1 = MelsecA1EDataType.W;
							result.Content2 = Convert.ToInt32(address.Substring(1), MelsecA1EDataType.W.FromBase);
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

		#region Read Write Basic

		/// <summary>
		/// 从三菱地址，是否位读取进行创建读取的MC的核心报文<br />
		/// From the Mitsubishi address, whether to read the core message of the MC for creating and reading
		/// </summary>
		/// <param name="isBit">是否进行了位读取操作</param>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildReadMcCoreCommand(McAddressData addressData, bool isBit)
		{
			byte[] command = new byte[10];
			command[0] = 0x01;                                                      // 批量读取数据命令
			command[1] = 0x04;
			command[2] = isBit ? (byte)0x01 : (byte)0x00;                           // 以点为单位还是字为单位成批读取
			command[3] = 0x00;
			command[4] = BitConverter.GetBytes(addressData.AddressStart)[0];      // 起始地址的地位
			command[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
			command[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
			command[7] = addressData.McDataType.DataCode;                           // 指明读取的数据
			command[8] = (byte)(addressData.Length % 256);                          // 软元件的长度
			command[9] = (byte)(addressData.Length / 256);

			return command;
		}

		/// <summary>
		/// 从三菱地址，是否位读取进行创建读取Ascii格式的MC的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="isBit">是否进行了位读取操作</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildAsciiReadMcCoreCommand(McAddressData addressData, bool isBit)
		{
			byte[] command = new byte[20];
			command[0] = 0x30;                                                               // 批量读取数据命令
			command[1] = 0x34;
			command[2] = 0x30;
			command[3] = 0x31;
			command[4] = 0x30;                                                               // 以点为单位还是字为单位成批读取
			command[5] = 0x30;
			command[6] = 0x30;
			command[7] = isBit ? (byte)0x31 : (byte)0x30;
			command[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];          // 软元件类型
			command[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
			command[10] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];            // 起始地址的地位
			command[11] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
			command[12] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
			command[13] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
			command[14] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
			command[15] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
			command[16] = SoftBasic.BuildAsciiBytesFrom(addressData.Length)[0];                                             // 软元件点数
			command[17] = SoftBasic.BuildAsciiBytesFrom(addressData.Length)[1];
			command[18] = SoftBasic.BuildAsciiBytesFrom(addressData.Length)[2];
			command[19] = SoftBasic.BuildAsciiBytesFrom(addressData.Length)[3];

			return command;
		}

		/// <summary>
		/// 以字为单位，创建数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">实际的原始数据信息</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildWriteWordCoreCommand(McAddressData addressData, byte[] value)
		{
			if (value == null) value = new byte[0];
			byte[] command = new byte[10 + value.Length];
			command[0] = 0x01;                                                        // 批量写入数据命令
			command[1] = 0x14;
			command[2] = 0x00;                                                        // 以字为单位成批读取
			command[3] = 0x00;
			command[4] = BitConverter.GetBytes(addressData.AddressStart)[0];        // 起始地址的地位
			command[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
			command[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
			command[7] = addressData.McDataType.DataCode;                             // 指明写入的数据
			command[8] = (byte)(value.Length / 2 % 256);                              // 软元件长度的地位
			command[9] = (byte)(value.Length / 2 / 256);
			value.CopyTo(command, 10);

			return command;
		}

		/// <summary>
		/// 以字为单位，创建ASCII数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">实际的原始数据信息</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildAsciiWriteWordCoreCommand(McAddressData addressData, byte[] value)
		{
			value = TransByteArrayToAsciiByteArray(value);

			byte[] command = new byte[20 + value.Length];
			command[0] = 0x31;                                                                                         // 批量写入的命令
			command[1] = 0x34;
			command[2] = 0x30;
			command[3] = 0x31;
			command[4] = 0x30;                                                                                         // 子命令
			command[5] = 0x30;
			command[6] = 0x30;
			command[7] = 0x30;
			command[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];                               // 软元件类型
			command[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
			command[10] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];    // 起始地址的地位
			command[11] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
			command[12] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
			command[13] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
			command[14] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
			command[15] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
			command[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[0];                               // 软元件点数
			command[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[1];
			command[18] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[2];
			command[19] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[3];
			value.CopyTo(command, 20);

			return command;
		}

		/// <summary>
		/// 以位为单位，创建数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">原始的bool数组数据</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildWriteBitCoreCommand(McAddressData addressData, bool[] value)
		{
			if (value == null) value = new bool[0];
			byte[] buffer = MelsecHelper.TransBoolArrayToByteData(value);
			byte[] command = new byte[10 + buffer.Length];
			command[0] = 0x01;                                                        // 批量写入数据命令
			command[1] = 0x14;
			command[2] = 0x01;                                                        // 以位为单位成批写入
			command[3] = 0x00;
			command[4] = BitConverter.GetBytes(addressData.AddressStart)[0];        // 起始地址的地位
			command[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
			command[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
			command[7] = addressData.McDataType.DataCode;                             // 指明写入的数据
			command[8] = (byte)(value.Length % 256);                                  // 软元件长度的地位
			command[9] = (byte)(value.Length / 256);
			buffer.CopyTo(command, 10);

			return command;
		}

		/// <summary>
		/// 以位为单位，创建ASCII数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">原始的bool数组数据</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildAsciiWriteBitCoreCommand(McAddressData addressData, bool[] value)
		{
			if (value == null) value = new bool[0];
			byte[] buffer = value.Select(m => m ? (byte)0x31 : (byte)0x30).ToArray();

			byte[] command = new byte[20 + buffer.Length];
			command[0] = 0x31;                                                                              // 批量写入的命令
			command[1] = 0x34;
			command[2] = 0x30;
			command[3] = 0x31;
			command[4] = 0x30;                                                                              // 子命令
			command[5] = 0x30;
			command[6] = 0x30;
			command[7] = 0x31;
			command[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];                         // 软元件类型
			command[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
			command[10] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];     // 起始地址的地位
			command[11] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
			command[12] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
			command[13] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
			command[14] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
			command[15] = MelsecHelper.BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
			command[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length))[0];              // 软元件点数
			command[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length))[1];
			command[18] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length))[2];
			command[19] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length))[3];
			buffer.CopyTo(command, 20);

			return command;
		}

		#endregion

		#region Read Write Extend

		/// <summary>
		/// 从三菱扩展地址，是否位读取进行创建读取的MC的核心报文
		/// </summary>
		/// <param name="isBit">是否进行了位读取操作</param>
		/// <param name="extend">扩展指定</param>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildReadMcCoreExtendCommand(McAddressData addressData, ushort extend, bool isBit)
		{
			byte[] command = new byte[17];
			command[0] = 0x01;                                                      // 批量读取数据命令
			command[1] = 0x04;
			command[2] = isBit ? (byte)0x81 : (byte)0x80;                           // 以点为单位还是字为单位成批读取
			command[3] = 0x00;
			command[4] = 0x00;
			command[5] = 0x00;
			command[6] = BitConverter.GetBytes(addressData.AddressStart)[0];      // 起始地址的地位
			command[7] = BitConverter.GetBytes(addressData.AddressStart)[1];
			command[8] = BitConverter.GetBytes(addressData.AddressStart)[2];
			command[9] = addressData.McDataType.DataCode;                           // 指明读取的数据
			command[10] = 0x00;
			command[11] = 0x00;
			command[12] = BitConverter.GetBytes(extend)[0];
			command[13] = BitConverter.GetBytes(extend)[1];
			command[14] = 0xF9;
			command[15] = (byte)(addressData.Length % 256);                          // 软元件的长度
			command[16] = (byte)(addressData.Length / 256);

			return command;
		}

		#endregion

		#region Read Write Random

		/// <summary>
		/// 按字为单位随机读取的指令创建
		/// </summary>
		/// <param name="address">地址数组</param>
		/// <returns>指令</returns>
		public static byte[] BuildReadRandomWordCommand(McAddressData[] address)
		{
			byte[] command = new byte[6 + address.Length * 4];
			command[0] = 0x03;                                                                  // 批量读取数据命令
			command[1] = 0x04;
			command[2] = 0x00;
			command[3] = 0x00;
			command[4] = (byte)address.Length;                                                  // 访问的字点数
			command[5] = 0x00;                                                                  // 双字访问点数
			for (int i = 0; i < address.Length; i++)
			{
				command[i * 4 + 6] = BitConverter.GetBytes(address[i].AddressStart)[0];       // 软元件起始地址
				command[i * 4 + 7] = BitConverter.GetBytes(address[i].AddressStart)[1];
				command[i * 4 + 8] = BitConverter.GetBytes(address[i].AddressStart)[2];
				command[i * 4 + 9] = address[i].McDataType.DataCode;                            // 软元件代号
			}
			return command;
		}

		/// <summary>
		/// 随机读取的指令创建
		/// </summary>
		/// <param name="address">地址数组</param>
		/// <returns>指令</returns>
		public static byte[] BuildReadRandomCommand(McAddressData[] address)
		{
			byte[] command = new byte[6 + address.Length * 6];
			command[0] = 0x06;                                                                  // 批量读取数据命令
			command[1] = 0x04;
			command[2] = 0x00;                                                                  // 子命令
			command[3] = 0x00;
			command[4] = (byte)address.Length;                                                  // 字软元件的块数
			command[5] = 0x00;                                                                  // 位软元件的块数
			for (int i = 0; i < address.Length; i++)
			{
				command[i * 6 + 6] = BitConverter.GetBytes(address[i].AddressStart)[0];      // 字软元件的编号
				command[i * 6 + 7] = BitConverter.GetBytes(address[i].AddressStart)[1];
				command[i * 6 + 8] = BitConverter.GetBytes(address[i].AddressStart)[2];
				command[i * 6 + 9] = address[i].McDataType.DataCode;                           // 字软元件的代码
				command[i * 6 + 10] = (byte)(address[i].Length % 256);                          // 软元件的长度
				command[i * 6 + 11] = (byte)(address[i].Length / 256);
			}
			return command;
		}

		/// <summary>
		/// 按字为单位随机读取的指令创建
		/// </summary>
		/// <param name="address">地址数组</param>
		/// <returns>指令</returns>
		public static byte[] BuildAsciiReadRandomWordCommand(McAddressData[] address)
		{
			byte[] command = new byte[12 + address.Length * 8];
			command[0] = 0x30;                                                               // 批量读取数据命令
			command[1] = 0x34;
			command[2] = 0x30;
			command[3] = 0x33;
			command[4] = 0x30;                                                               // 以点为单位还是字为单位成批读取
			command[5] = 0x30;
			command[6] = 0x30;
			command[7] = 0x30;
			command[8] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[0];
			command[9] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[1];
			command[10] = 0x30;
			command[11] = 0x30;
			for (int i = 0; i < address.Length; i++)
			{
				command[i * 8 + 12] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[0];          // 软元件类型
				command[i * 8 + 13] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[1];
				command[i * 8 + 14] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[0];            // 起始地址的地位
				command[i * 8 + 15] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[1];
				command[i * 8 + 16] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[2];
				command[i * 8 + 17] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[3];
				command[i * 8 + 18] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[4];
				command[i * 8 + 19] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[5];
			}
			return command;
		}

		/// <summary>
		/// 随机读取的指令创建
		/// </summary>
		/// <param name="address">地址数组</param>
		/// <returns>指令</returns>
		public static byte[] BuildAsciiReadRandomCommand(McAddressData[] address)
		{
			byte[] command = new byte[12 + address.Length * 12];
			command[0] = 0x30;                                                               // 批量读取数据命令
			command[1] = 0x34;
			command[2] = 0x30;
			command[3] = 0x36;
			command[4] = 0x30;                                                               // 以点为单位还是字为单位成批读取
			command[5] = 0x30;
			command[6] = 0x30;
			command[7] = 0x30;
			command[8] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[0];
			command[9] = SoftBasic.BuildAsciiBytesFrom((byte)address.Length)[1];
			command[10] = 0x30;
			command[11] = 0x30;
			for (int i = 0; i < address.Length; i++)
			{
				command[i * 12 + 12] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[0];          // 软元件类型
				command[i * 12 + 13] = Encoding.ASCII.GetBytes(address[i].McDataType.AsciiCode)[1];
				command[i * 12 + 14] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[0];            // 起始地址的地位
				command[i * 12 + 15] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[1];
				command[i * 12 + 16] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[2];
				command[i * 12 + 17] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[3];
				command[i * 12 + 18] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[4];
				command[i * 12 + 19] = MelsecHelper.BuildBytesFromAddress(address[i].AddressStart, address[i].McDataType)[5];
				command[i * 12 + 20] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[0];
				command[i * 12 + 21] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[1];
				command[i * 12 + 22] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[2];
				command[i * 12 + 23] = SoftBasic.BuildAsciiBytesFrom(address[i].Length)[3];
			}
			return command;
		}

		#endregion

		#region Read Write Tag

		/// <summary>
		/// 创建批量读取标签的报文数据信息
		/// </summary>
		/// <param name="tags">标签名</param>
		/// <param name="lengths">长度信息</param>
		/// <returns>报文名称</returns>
		public static byte[] BuildReadTag(string[] tags, ushort[] lengths)
		{
			if (tags.Length != lengths.Length) throw new Exception(StringResources.Language.TwoParametersLengthIsNotSame);

			MemoryStream command = new MemoryStream();
			command.WriteByte(0x1A);                                                          // 批量读取标签的指令
			command.WriteByte(0x04);
			command.WriteByte(0x00);                                                          // 子命令
			command.WriteByte(0x00);
			command.WriteByte(BitConverter.GetBytes(tags.Length)[0]);                       // 排列点数
			command.WriteByte(BitConverter.GetBytes(tags.Length)[1]);
			command.WriteByte(0x00);                                                          // 省略指定
			command.WriteByte(0x00);
			for (int i = 0; i < tags.Length; i++)
			{
				byte[] tagBuffer = Encoding.Unicode.GetBytes(tags[i]);
				command.WriteByte(BitConverter.GetBytes(tagBuffer.Length / 2)[0]);          // 标签长度
				command.WriteByte(BitConverter.GetBytes(tagBuffer.Length / 2)[1]);
				command.Write(tagBuffer, 0, tagBuffer.Length);                                // 标签名称
				command.WriteByte(0x01);                                                      // 单位指定
				command.WriteByte(0x00);                                                      // 固定值
				command.WriteByte(BitConverter.GetBytes(lengths[i] * 2)[0]);                // 排列数据长
				command.WriteByte(BitConverter.GetBytes(lengths[i] * 2)[1]);
			}
			byte[] buffer = command.ToArray();
			command.Dispose();
			return buffer;
		}

		/// <summary>
		/// 解析出标签读取的数据内容
		/// </summary>
		/// <param name="content">返回的数据信息</param>
		/// <returns>解析结果</returns>
		public static OperateResult<byte[]> ExtraTagData(byte[] content)
		{
			try
			{
				int count = BitConverter.ToUInt16(content, 0);
				int index = 2;
				List<byte> array = new List<byte>(20);
				for (int i = 0; i < count; i++)
				{
					int length = BitConverter.ToUInt16(content, index + 2);
					array.AddRange(SoftBasic.ArraySelectMiddle(content, index + 4, length));
					index += 4 + length;
				}
				return OperateResult.CreateSuccessResult(array.ToArray());
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>(ex.Message + " Source:" + SoftBasic.ByteToHexString(content, ' '));
			}
		}

		#endregion

		#region Read Write Memory

		/// <summary>
		/// 读取本站缓冲寄存器的数据信息，需要指定寄存器的地址，和读取的长度
		/// </summary>
		/// <param name="address">寄存器的地址</param>
		/// <param name="length">数据长度</param>
		/// <returns>结果内容</returns>
		public static OperateResult<byte[]> BuildReadMemoryCommand(string address, ushort length)
		{
			try
			{
				uint add = uint.Parse(address);
				byte[] command = new byte[8];
				command[0] = 0x13;                                                      // 读取缓冲数据命令
				command[1] = 0x06;
				command[2] = BitConverter.GetBytes(add)[0];                           // 起始地址的地位
				command[3] = BitConverter.GetBytes(add)[1];
				command[4] = BitConverter.GetBytes(add)[2];
				command[5] = BitConverter.GetBytes(add)[3];
				command[6] = (byte)(length % 256);                                      // 软元件的长度
				command[7] = (byte)(length / 256);

				return OperateResult.CreateSuccessResult(command);
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>(ex.Message);
			}
		}

		#endregion

		/// <summary>
		/// 根据三菱的错误码去查找对象描述信息
		/// </summary>
		/// <param name="code">错误码</param>
		/// <returns>描述信息</returns>
		public static string GetErrorDescription(int code)
		{
			switch (code)
			{
				case 0x0002: return StringResources.Language.MelsecError02;
				case 0x0051: return StringResources.Language.MelsecError51;
				case 0x0052: return StringResources.Language.MelsecError52;
				case 0x0054: return StringResources.Language.MelsecError54;
				case 0x0055: return StringResources.Language.MelsecError55;
				case 0x0056: return StringResources.Language.MelsecError56;
				case 0x0058: return StringResources.Language.MelsecError58;
				case 0x0059: return StringResources.Language.MelsecError59;
				case 0xC04D: return StringResources.Language.MelsecErrorC04D;
				case 0xC050: return StringResources.Language.MelsecErrorC050;
				case 0xC051:
				case 0xC052:
				case 0xC053:
				case 0xC054: return StringResources.Language.MelsecErrorC051_54;
				case 0xC055: return StringResources.Language.MelsecErrorC055;
				case 0xC056: return StringResources.Language.MelsecErrorC056;
				case 0xC057: return StringResources.Language.MelsecErrorC057;
				case 0xC058: return StringResources.Language.MelsecErrorC058;
				case 0xC059: return StringResources.Language.MelsecErrorC059;
				case 0xC05A:
				case 0xC05B: return StringResources.Language.MelsecErrorC05A_B;
				case 0xC05C: return StringResources.Language.MelsecErrorC05C;
				case 0xC05D: return StringResources.Language.MelsecErrorC05D;
				case 0xC05E: return StringResources.Language.MelsecErrorC05E;
				case 0xC05F: return StringResources.Language.MelsecErrorC05F;
				case 0xC060: return StringResources.Language.MelsecErrorC060;
				case 0xC061: return StringResources.Language.MelsecErrorC061;
				case 0xC062: return StringResources.Language.MelsecErrorC062;
				case 0xC070: return StringResources.Language.MelsecErrorC070;
				case 0xC072: return StringResources.Language.MelsecErrorC072;
				case 0xC074: return StringResources.Language.MelsecErrorC074;
				default: return StringResources.Language.MelsecPleaseReferToManualDocument;
			}
		}

		#endregion

		#region Common Logic

		/// <summary>
		/// 从三菱的地址中构建MC协议的6字节的ASCII格式的地址
		/// </summary>
		/// <param name="address">三菱地址</param>
		/// <param name="type">三菱的数据类型</param>
		/// <returns>6字节的ASCII格式的地址</returns>
		internal static byte[] BuildBytesFromAddress(int address, MelsecMcDataType type)
		{
			return Encoding.ASCII.GetBytes(address.ToString(type.FromBase == 10 ? "D6" : "X6"));
		}

		/// <summary>
		/// 将0，1，0，1的字节数组压缩成三菱格式的字节数组来表示开关量的
		/// </summary>
		/// <param name="value">原始的数据字节</param>
		/// <returns>压缩过后的数据字节</returns>
		internal static byte[] TransBoolArrayToByteData(byte[] value) => TransBoolArrayToByteData(value.Select(m => m != 0x00).ToArray());

		/// <summary>
		/// 将bool的组压缩成三菱格式的字节数组来表示开关量的
		/// </summary>
		/// <param name="value">原始的数据字节</param>
		/// <returns>压缩过后的数据字节</returns>
		internal static byte[] TransBoolArrayToByteData(bool[] value)
		{
			int length = (value.Length + 1) / 2;
			byte[] buffer = new byte[length];

			for (int i = 0; i < length; i++)
			{
				if (value[i * 2 + 0]) buffer[i] += 0x10;
				if ((i * 2 + 1) < value.Length)
				{
					if (value[i * 2 + 1]) buffer[i] += 0x01;
				}
			}
			return buffer;
		}

		internal static byte[] TransByteArrayToAsciiByteArray(byte[] value)
		{
			if (value == null) return new byte[0];

			byte[] buffer = new byte[value.Length * 2];
			for (int i = 0; i < value.Length / 2; i++)
			{
				SoftBasic.BuildAsciiBytesFrom(BitConverter.ToUInt16(value, i * 2)).CopyTo(buffer, 4 * i);
			}
			return buffer;
		}

		internal static byte[] TransAsciiByteArrayToByteArray(byte[] value)
		{
			byte[] Content = new byte[value.Length / 2];
			for (int i = 0; i < Content.Length / 2; i++)
			{
				ushort tmp = Convert.ToUInt16(Encoding.ASCII.GetString(value, i * 4, 4), 16);
				BitConverter.GetBytes(tmp).CopyTo(Content, i * 2);
			}
			return Content;
		}

		#endregion

		#region CRC Check

		/// <summary>
		/// 计算Fx协议指令的和校验信息
		/// </summary>
		/// <param name="data">字节数据</param>
		/// <returns>校验之后的数据</returns>
		internal static byte[] FxCalculateCRC(byte[] data)
		{
			int sum = 0;
			for (int i = 1; i < data.Length - 2; i++)
			{
				sum += data[i];
			}
			return SoftBasic.BuildAsciiBytesFrom((byte)sum);
		}

		/// <summary>
		/// 检查指定的和校验是否是正确的
		/// </summary>
		/// <param name="data">字节数据</param>
		/// <returns>是否成功</returns>
		internal static bool CheckCRC(byte[] data)
		{
			byte[] crc = FxCalculateCRC(data);
			if (crc[0] != data[data.Length - 2]) return false;
			if (crc[1] != data[data.Length - 1]) return false;
			return true;
		}

		#endregion
	}
}
