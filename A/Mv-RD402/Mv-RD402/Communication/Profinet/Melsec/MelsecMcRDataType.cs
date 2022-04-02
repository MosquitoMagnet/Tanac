using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communication.Profinet.Melsec
{
	/// <summary>
	/// 三菱R系列的PLC的数据类型
	/// </summary>
	public class MelsecMcRDataType
	{
		/// <summary>
		/// 如果您清楚类型代号，可以根据值进行扩展
		/// </summary>
		/// <param name="code">数据类型的代号</param>
		/// <param name="type">0或1，默认为0</param>
		/// <param name="asciiCode">ASCII格式的类型信息</param>
		/// <param name="fromBase">指示地址的多少进制的，10或是16</param>
		public MelsecMcRDataType(byte[] code, byte type, string asciiCode, int fromBase)
		{
			DataCode = code;
			AsciiCode = asciiCode;
			FromBase = fromBase;
			if (type < 2) DataType = type;
		}

		/// <summary>
		/// 类型的代号值
		/// </summary>
		public byte[] DataCode { get; private set; } = new byte[] { 0x00, 0x00 };

		/// <summary>
		/// 数据的类型，0代表按字，1代表按位
		/// </summary>
		public byte DataType { get; private set; } = 0x00;

		/// <summary>
		/// 当以ASCII格式通讯时的类型描述
		/// </summary>
		public string AsciiCode { get; private set; }

		/// <summary>
		/// 指示地址是10进制，还是16进制的
		/// </summary>
		public int FromBase { get; private set; }

		/// <summary>
		/// X输入继电器
		/// </summary>
		public readonly static MelsecMcRDataType X = new MelsecMcRDataType(new byte[] { 0x9C, 0x00 }, 0x01, "X***", 16);

		/// <summary>
		/// Y输入继电器
		/// </summary>
		public readonly static MelsecMcRDataType Y = new MelsecMcRDataType(new byte[] { 0x9D, 0x00 }, 0x01, "Y***", 16);

		/// <summary>
		/// M内部继电器
		/// </summary>
		public readonly static MelsecMcRDataType M = new MelsecMcRDataType(new byte[] { 0x90, 0x00 }, 0x01, "M***", 10);

		/// <summary>
		/// 特殊继电器
		/// </summary>
		public readonly static MelsecMcRDataType SM = new MelsecMcRDataType(new byte[] { 0x91, 0x00 }, 0x01, "SM**", 10);

		/// <summary>
		/// 锁存继电器
		/// </summary>
		public readonly static MelsecMcRDataType L = new MelsecMcRDataType(new byte[] { 0x92, 0x00 }, 0x01, "L***", 10);

		/// <summary>
		/// 报警器
		/// </summary>
		public readonly static MelsecMcRDataType F = new MelsecMcRDataType(new byte[] { 0x93, 0x00 }, 0x01, "F***", 10);

		/// <summary>
		/// 变址继电器
		/// </summary>
		public readonly static MelsecMcRDataType V = new MelsecMcRDataType(new byte[] { 0x94, 0x00 }, 0x01, "V***", 10);

		/// <summary>
		/// S步进继电器
		/// </summary>
		public readonly static MelsecMcRDataType S = new MelsecMcRDataType(new byte[] { 0x98, 0x00 }, 0x01, "S***", 10);

		/// <summary>
		/// 链接继电器
		/// </summary>
		public readonly static MelsecMcRDataType B = new MelsecMcRDataType(new byte[] { 0xA0, 0x00 }, 0x01, "B***", 16);

		/// <summary>
		/// 特殊链接继电器
		/// </summary>
		public readonly static MelsecMcRDataType SB = new MelsecMcRDataType(new byte[] { 0xA1, 0x00 }, 0x01, "SB**", 16);

		/// <summary>
		/// 直接访问输入继电器
		/// </summary>
		public readonly static MelsecMcRDataType DX = new MelsecMcRDataType(new byte[] { 0xA2, 0x00 }, 0x01, "DX**", 16);

		/// <summary>
		/// 直接访问输出继电器
		/// </summary>
		public readonly static MelsecMcRDataType DY = new MelsecMcRDataType(new byte[] { 0xA3, 0x00 }, 0x01, "DY**", 16);

		/// <summary>
		/// 数据寄存器
		/// </summary>
		public readonly static MelsecMcRDataType D = new MelsecMcRDataType(new byte[] { 0xA8, 0x00 }, 0x00, "D***", 10);

		/// <summary>
		/// 特殊数据寄存器
		/// </summary>
		public readonly static MelsecMcRDataType SD = new MelsecMcRDataType(new byte[] { 0xA9, 0x00 }, 0x00, "SD**", 10);

		/// <summary>
		/// 链接寄存器
		/// </summary>
		public readonly static MelsecMcRDataType W = new MelsecMcRDataType(new byte[] { 0xB4, 0x00 }, 0x00, "W***", 16);

		/// <summary>
		/// 特殊链接寄存器
		/// </summary>
		public readonly static MelsecMcRDataType SW = new MelsecMcRDataType(new byte[] { 0xB5, 0x00 }, 0x00, "SW**", 16);

		/// <summary>
		/// 文件寄存器
		/// </summary>
		public readonly static MelsecMcRDataType R = new MelsecMcRDataType(new byte[] { 0xAF, 0x00 }, 0x00, "R***", 10);

		/// <summary>
		/// 变址寄存器
		/// </summary>
		public readonly static MelsecMcRDataType Z = new MelsecMcRDataType(new byte[] { 0xCC, 0x00 }, 0x00, "Z***", 10);




		/// <summary>
		/// 长累计定时器触点
		/// </summary>
		public readonly static MelsecMcRDataType LSTS = new MelsecMcRDataType(new byte[] { 0x59, 0x00 }, 0x01, "LSTS", 10);
		/// <summary>
		/// 长累计定时器线圈
		/// </summary>
		public readonly static MelsecMcRDataType LSTC = new MelsecMcRDataType(new byte[] { 0x58, 0x00 }, 0x01, "LSTC", 10);
		/// <summary>
		/// 长累计定时器当前值
		/// </summary>
		public readonly static MelsecMcRDataType LSTN = new MelsecMcRDataType(new byte[] { 0x5A, 0x00 }, 0x00, "LSTN", 10);

		/// <summary>
		/// 累计定时器触点
		/// </summary>
		public readonly static MelsecMcRDataType STS = new MelsecMcRDataType(new byte[] { 0xC7, 0x00 }, 0x01, "STS*", 10);
		/// <summary>
		/// 累计定时器线圈
		/// </summary>
		public readonly static MelsecMcRDataType STC = new MelsecMcRDataType(new byte[] { 0xC6, 0x00 }, 0x01, "STC*", 10);
		/// <summary>
		/// 累计定时器当前值
		/// </summary>
		public readonly static MelsecMcRDataType STN = new MelsecMcRDataType(new byte[] { 0xC8, 0x00 }, 0x00, "STN*", 10);

		/// <summary>
		/// 长定时器触点
		/// </summary>
		public readonly static MelsecMcRDataType LTS = new MelsecMcRDataType(new byte[] { 0x51, 0x00 }, 0x01, "LTS*", 10);
		/// <summary>
		/// 长定时器线圈
		/// </summary>
		public readonly static MelsecMcRDataType LTC = new MelsecMcRDataType(new byte[] { 0x50, 0x00 }, 0x01, "LTC*", 10);
		/// <summary>
		/// 长定时器当前值
		/// </summary>
		public readonly static MelsecMcRDataType LTN = new MelsecMcRDataType(new byte[] { 0x52, 0x00 }, 0x00, "LTN*", 10);

		/// <summary>
		/// 定时器触点
		/// </summary>
		public readonly static MelsecMcRDataType TS = new MelsecMcRDataType(new byte[] { 0xC1, 0x00 }, 0x01, "TS**", 10);
		/// <summary>
		/// 定时器线圈
		/// </summary>
		public readonly static MelsecMcRDataType TC = new MelsecMcRDataType(new byte[] { 0xC0, 0x00 }, 0x01, "TC**", 10);
		/// <summary>
		/// 定时器当前值
		/// </summary>
		public readonly static MelsecMcRDataType TN = new MelsecMcRDataType(new byte[] { 0xC2, 0x00 }, 0x00, "TN**", 10);

		/// <summary>
		/// 长计数器触点
		/// </summary>
		public readonly static MelsecMcRDataType LCS = new MelsecMcRDataType(new byte[] { 0x55, 0x00 }, 0x01, "LCS*", 10);
		/// <summary>
		/// 长计数器线圈
		/// </summary>
		public readonly static MelsecMcRDataType LCC = new MelsecMcRDataType(new byte[] { 0x54, 0x00 }, 0x01, "LCC*", 10);
		/// <summary>
		/// 长计数器当前值
		/// </summary>
		public readonly static MelsecMcRDataType LCN = new MelsecMcRDataType(new byte[] { 0x56, 0x00 }, 0x00, "LCN*", 10);

		/// <summary>
		/// 计数器触点
		/// </summary>
		public readonly static MelsecMcRDataType CS = new MelsecMcRDataType(new byte[] { 0xC4, 0x00 }, 0x01, "CS**", 10);
		/// <summary>
		/// 计数器线圈
		/// </summary>
		public readonly static MelsecMcRDataType CC = new MelsecMcRDataType(new byte[] { 0xC3, 0x00 }, 0x01, "CC**", 10);
		/// <summary>
		/// 计数器当前值
		/// </summary>
		public readonly static MelsecMcRDataType CN = new MelsecMcRDataType(new byte[] { 0xC5, 0x00 }, 0x00, "CN**", 10);

	}
}
