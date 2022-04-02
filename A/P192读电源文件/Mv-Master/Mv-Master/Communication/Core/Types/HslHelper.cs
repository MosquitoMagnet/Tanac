using Communication;
using Communication.BasicFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Communication
{
	/// <summary>
	/// HslCommunication的一些静态辅助方法<br />
	/// Some static auxiliary methods of HslCommunication
	/// </summary>
	/// <summary>
	/// HslCommunication的一些静态辅助方法<br />
	/// Some static auxiliary methods of HslCommunication
	/// </summary>
	public class HslHelper
	{
		/// <summary>
		/// 解析地址的附加参数方法，比如你的地址是s=100;D100，可以提取出"s"的值的同时，修改地址本身，如果"s"不存在的话，返回给定的默认值<br />
		/// The method of parsing additional parameters of the address, for example, if your address is s=100;D100, you can extract the value of "s" and modify the address itself. If "s" does not exist, return the given default value
		/// </summary>
		/// <param name="address">复杂的地址格式，比如：s=100;D100</param>
		/// <param name="paraName">等待提取的参数名称</param>
		/// <param name="defaultValue">如果提取的参数信息不存在，返回的默认值信息</param>
		/// <returns>解析后的新的数据值或是默认的给定的数据值</returns>
		public static int ExtractParameter(ref string address, string paraName, int defaultValue)
		{
			OperateResult<int> operateResult = ExtractParameter(ref address, paraName);
			return operateResult.IsSuccess ? operateResult.Content : defaultValue;
		}

		/// <summary>
		/// 解析地址的附加参数方法，比如你的地址是s=100;D100，可以提取出"s"的值的同时，修改地址本身，如果"s"不存在的话，返回错误的消息内容<br />
		/// The method of parsing additional parameters of the address, for example, if your address is s=100;D100, you can extract the value of "s" and modify the address itself. 
		/// If "s" does not exist, return the wrong message content
		/// </summary>
		/// <param name="address">复杂的地址格式，比如：s=100;D100</param>
		/// <param name="paraName">等待提取的参数名称</param>
		/// <returns>解析后的参数结果内容</returns>
		public static OperateResult<int> ExtractParameter(ref string address, string paraName)
		{
			try
			{
				Match match = Regex.Match(address, paraName + "=[0-9A-Fa-fx]+;");
				if (!match.Success)
				{
					return new OperateResult<int>("Address [" + address + "] can't find [" + paraName + "] Parameters. for example : " + paraName + "=1;100");
				}
				string text = match.Value.Substring(paraName.Length + 1, match.Value.Length - paraName.Length - 2);
				int value = text.StartsWith("0x") ? Convert.ToInt32(text.Substring(2), 16) : (text.StartsWith("0") ? Convert.ToInt32(text, 8) : Convert.ToInt32(text));
				address = address.Replace(match.Value, "");
				return OperateResult.CreateSuccessResult(value);
			}
			catch (Exception ex)
			{
				return new OperateResult<int>("Address [" + address + "] Get [" + paraName + "] Parameters failed: " + ex.Message);
			}
		}

		/// <summary>
		/// 切割当前的地址数据信息，根据读取的长度来分割成多次不同的读取内容，需要指定地址，总的读取长度，切割读取长度<br />
		/// Cut the current address data information, and divide it into multiple different read contents according to the read length. 
		/// You need to specify the address, the total read length, and the cut read length
		/// </summary>
		/// <param name="address">整数的地址信息</param>
		/// <param name="length">读取长度信息</param>
		/// <param name="segment">切割长度信息</param>
		/// <returns>切割结果</returns>
		public static OperateResult<int[], int[]> SplitReadLength(int address, ushort length, ushort segment)
		{
			int[] array = SoftBasic.SplitIntegerToArray(length, segment);
			int[] array2 = new int[array.Length];
			for (int i = 0; i < array2.Length; i++)
			{
				if (i == 0)
				{
					array2[i] = address;
				}
				else
				{
					array2[i] = array2[i - 1] + array[i - 1];
				}
			}
			return OperateResult.CreateSuccessResult(array2, array);
		}

		/// <summary>
		/// 根据指定的长度切割数据数组，返回地址偏移量信息和数据分割信息
		/// </summary>
		/// <typeparam name="T">数组类型</typeparam>
		/// <param name="address">起始的地址</param>
		/// <param name="value">实际的数据信息</param>
		/// <param name="segment">分割的基本长度</param>
		/// <param name="addressLength">一个地址代表的数据长度</param>
		/// <returns>切割结果内容</returns>
		public static OperateResult<int[], List<T[]>> SplitWriteData<T>(int address, T[] value, ushort segment, int addressLength)
		{
			List<T[]> list = SoftBasic.ArraySplitByLength(value, segment * addressLength);
			int[] array = new int[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				if (i == 0)
				{
					array[i] = address;
				}
				else
				{
					array[i] = array[i - 1] + list[i - 1].Length / addressLength;
				}
			}
			return OperateResult.CreateSuccessResult(array, list);
		}

		/// <summary>
		/// 获取地址信息的位索引，在地址最后一个小数点的位置
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <returns>位索引的位置</returns>
		public static int GetBitIndexInformation(ref string address)
		{
			int result = 0;
			int num = address.LastIndexOf('.');
			if (num > 0 && num < address.Length - 1)
			{
				string text = address.Substring(num + 1);
				result = ((!text.Contains("A") && !text.Contains("B") && !text.Contains("C") && !text.Contains("D") && !text.Contains("E") && !text.Contains("F")) ? Convert.ToInt32(text) : Convert.ToInt32(text, 16));
				address = address.Substring(0, num);
			}
			return result;
		}

		/// <summary>
		/// 从当前的字符串信息获取IP地址数据，如果是ip地址直接返回，如果是域名，会自动解析IP地址，否则抛出异常<br />
		/// Get the IP address data from the current string information, if it is an ip address, return directly, 
		/// if it is a domain name, it will automatically resolve the IP address, otherwise an exception will be thrown
		/// </summary>
		/// <param name="value">输入的字符串信息</param>
		/// <returns>真实的IP地址信息</returns>
		public static string GetIpAddressFromInput(string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (Regex.IsMatch(value, "^[0-9]+\\.[0-9]+\\.[0-9]+\\.[0-9]+$"))
				{
					if (!IPAddress.TryParse(value, out IPAddress _))
					{
						throw new Exception(StringResources.Language.IpAddressError);
					}
					return value;
				}
				IPHostEntry hostEntry = Dns.GetHostEntry(value);
				IPAddress[] addressList = hostEntry.AddressList;
				if (addressList.Length != 0)
				{
					return addressList[0].ToString();
				}
			}
			return "127.0.0.1";
		}

		/// <summary>
		/// 从流中接收指定长度的字节数组
		/// </summary>
		/// <param name="stream">流</param>
		/// <param name="length">数据长度</param>
		/// <returns>二进制的字节数组</returns>
		public static byte[] ReadSpecifiedLengthFromStream(Stream stream, int length)
		{
			byte[] array = new byte[length];
			int num = 0;
			while (num < length)
			{
				int num2 = stream.Read(array, num, array.Length - num);
				num += num2;
				if (num2 == 0)
				{
					break;
				}
			}
			return array;
		}

		/// <summary>
		/// 将字符串的内容写入到流中去
		/// </summary>
		/// <param name="stream">数据流</param>
		/// <param name="value">字符串内容</param>
		public static void WriteStringToStream(Stream stream, string value)
		{
			byte[] value2 = string.IsNullOrEmpty(value) ? new byte[0] : Encoding.UTF8.GetBytes(value);
			WriteBinaryToStream(stream, value2);
		}

		/// <summary>
		/// 从流中读取一个字符串内容
		/// </summary>
		/// <param name="stream">数据流</param>
		/// <returns>字符串信息</returns>
		public static string ReadStringFromStream(Stream stream)
		{
			byte[] bytes = ReadBinaryFromStream(stream);
			return Encoding.UTF8.GetString(bytes);
		}

		/// <summary>
		/// 将二进制的内容写入到数据流之中
		/// </summary>
		/// <param name="stream">数据流</param>
		/// <param name="value">原始字节数组</param>
		public static void WriteBinaryToStream(Stream stream, byte[] value)
		{
			stream.Write(BitConverter.GetBytes(value.Length), 0, 4);
			stream.Write(value, 0, value.Length);
		}

		/// <summary>
		/// 从流中读取二进制的内容
		/// </summary>
		/// <param name="stream">数据流</param>
		/// <returns>字节数组</returns>
		public static byte[] ReadBinaryFromStream(Stream stream)
		{
			byte[] value = ReadSpecifiedLengthFromStream(stream, 4);
			int num = BitConverter.ToInt32(value, 0);
			if (num <= 0)
			{
				return new byte[0];
			}
			return ReadSpecifiedLengthFromStream(stream, num);
		}
	}
}
