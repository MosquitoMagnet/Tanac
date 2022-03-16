using System;
using System.Collections.Generic;
using System.Text;
using Communication;

namespace CommonDriver.Inovance
{
	/// <summary>
	/// 汇川的数据地址表示形式<br />
	/// Inovance's data address representation
	/// </summary>
	public class InovanceAddressData
	{
		/// <summary>
		/// 数字的起始地址，也就是偏移地址<br />
		/// The starting address of the number, which is the offset address
		/// </summary>
		public int AddressStart
		{
			get;
			set;
		}

		/// <summary>
		/// 读取的数据长度，单位是字节还是字取决于设备方<br />
		/// The length of the data read, the unit is byte or word depends on the device side
		/// </summary>
		public ushort Length
		{
			get;
			set;
		}
		/// <summary>
		/// 汇川的数据类型及地址信息
		/// </summary>
		public InovanceDataType InovanceDataType
		{
			get;
			set;
		}


		/// <summary>
		/// 从实际汇川H5U的地址里面解析出我们需要的地址类型<br />
		/// Resolve the type of address we need from the actual Inovance address
		/// </summary>
		/// <param name="address">汇川的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<InovanceAddressData> ParseH5UFrom(string address, ushort length)
		{
			InovanceAddressData InovanceAddressData = new InovanceAddressData();
			InovanceAddressData.Length = length;
			try
			{
				switch (address[0])
				{
					case 'M':
					case 'm':
						InovanceAddressData.InovanceDataType = InovanceDataType.H5U_M;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H5U_M.FromBase);
						break;
					case 'B':
					case 'b':
						InovanceAddressData.InovanceDataType = InovanceDataType.H5U_B;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H5U_B.FromBase);
						break;
					case 'S':
					case 's':
						InovanceAddressData.InovanceDataType = InovanceDataType.H5U_S;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H5U_S.FromBase);
						break;
					case 'X':
					case 'x':
						InovanceAddressData.InovanceDataType = InovanceDataType.H5U_X;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H5U_X.FromBase);
						break;
					case 'Y':
					case 'y':
						InovanceAddressData.InovanceDataType = InovanceDataType.H5U_Y;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H5U_Y.FromBase);
						break;
					case 'D':
					case 'd':
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H5U_D;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H5U_D.FromBase);
						}
						break;
					case 'R':
					case 'r':
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H5U_R;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H5U_R.FromBase);
						}
						break;
					default:
						throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<InovanceAddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(InovanceAddressData);
		}


		/// <summary>
		/// 从实际汇川H3U的地址里面解析出我们需要的地址类型<br />
		/// Resolve the type of address we need from the actual Inovance address
		/// </summary>
		/// <param name="address">汇川的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<InovanceAddressData> ParseH3UFrom(string address, ushort length)
		{
			InovanceAddressData InovanceAddressData = new InovanceAddressData();
			InovanceAddressData.Length = length;
			try
			{
				switch (address[0])
				{
					case 'M':
					case 'm':
						InovanceAddressData.InovanceDataType = InovanceDataType.H3U_M;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H3U_M.FromBase);
						break;
					case 'X':
					case 'x':
						InovanceAddressData.InovanceDataType = InovanceDataType.H3U_X;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H3U_X.FromBase);
						break;
					case 'Y':
					case 'y':
						InovanceAddressData.InovanceDataType = InovanceDataType.H3U_Y;
						InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H3U_Y.FromBase);
						break;
					case 'D':
					case 'd':
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_D;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H3U_D.FromBase);
						}
						break;
					case 'R':
					case 'r':
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_R;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H3U_R.FromBase);
						}
						break;
					case 'T':
					case 't':
						if (address[1] == 'N' || address[1] == 'n')
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_TN;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(2), InovanceDataType.H3U_TN.FromBase);
						}
						else if (address[1] == 'S' || address[1] == 's')
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_TS;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(2), InovanceDataType.H3U_TS.FromBase);
						}
						break;
					case 'C':
					case 'c':
						if (address[1] == 'N' || address[1] == 'n')
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_CN;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(2), InovanceDataType.H3U_CN.FromBase);
						}
						else if (address[1] == 'S' || address[1] == 's')
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_CS;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(2), InovanceDataType.H3U_CS.FromBase);
						}
						break;
					case 'S':
					case 's':
						if (address[1] == 'M' || address[1] == 'm')
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_SM;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(2), InovanceDataType.H3U_SM.FromBase);
						}
						else if (address[1] == 'D' || address[1] == 'd')
						{
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_SD;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(2), InovanceDataType.H3U_SD.FromBase);
						}
						else
                        {
							InovanceAddressData.InovanceDataType = InovanceDataType.H3U_S;
							InovanceAddressData.AddressStart = Convert.ToInt32(address.Substring(1), InovanceDataType.H3U_S.FromBase);
						}
						break;
					default:
						throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<InovanceAddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(InovanceAddressData);
		}

		/// <summary>
		/// 从实际汇川AM的地址里面解析出我们需要的地址类型<br />
		/// Resolve the type of address we need from the actual Inovance address
		/// </summary>
		/// <param name="address">汇川的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<InovanceAddressData> ParseAMFrom(string address, ushort length)
		{
			InovanceAddressData InovanceAddressData = new InovanceAddressData();
			InovanceAddressData.Length = length;
			try
			{
				if (address.StartsWith("QX") || address.StartsWith("qx"))
                {
					InovanceAddressData.InovanceDataType = InovanceDataType.AM_Q;
					InovanceAddressData.AddressStart = Convert.ToInt32(CalculateStartAddress(address.Substring(2)).ToString(), InovanceDataType.AM_Q.FromBase);
				}
				else if (address.StartsWith("Q") || address.StartsWith("q"))
                {
					InovanceAddressData.InovanceDataType = InovanceDataType.AM_Q;
					InovanceAddressData.AddressStart = Convert.ToInt32(CalculateStartAddress(address.Substring(1)).ToString(), InovanceDataType.AM_Q.FromBase);
				}
				else if (address.StartsWith("IX") || address.StartsWith("ix"))
                {
					InovanceAddressData.InovanceDataType = InovanceDataType.AM_I;
					InovanceAddressData.AddressStart = Convert.ToInt32(CalculateStartAddress(address.Substring(2)).ToString(), InovanceDataType.AM_I.FromBase);
				}
				else if (address.StartsWith("I") || address.StartsWith("i"))
                {
					InovanceAddressData.InovanceDataType = InovanceDataType.AM_I;
					InovanceAddressData.AddressStart = Convert.ToInt32(CalculateStartAddress(address.Substring(1)).ToString(), InovanceDataType.AM_I.FromBase);
				}
				else if (address.StartsWith("MW") || address.StartsWith("mw"))
                {
					InovanceAddressData.InovanceDataType = InovanceDataType.AM_M;
					InovanceAddressData.AddressStart = Convert.ToInt32(CalculateStartAddress(address.Substring(2)).ToString(), InovanceDataType.AM_M.FromBase);
				}
				else if (address.StartsWith("M") || address.StartsWith("m"))
                {
					InovanceAddressData.InovanceDataType = InovanceDataType.AM_M;
					InovanceAddressData.AddressStart = Convert.ToInt32(CalculateStartAddress(address.Substring(1)).ToString(), InovanceDataType.AM_M.FromBase);
				}
				else
                {
					throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<InovanceAddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(InovanceAddressData);
		}
		private static int CalculateStartAddress(string address)
		{
			if (address.IndexOf('.') < 0)
				return int.Parse(address);
			else
			{
				string[] splits = address.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
				return int.Parse(splits[0]) * 8 + int.Parse(splits[1]);
			}
		}
	}
	/// <summary>
	/// 汇川PLC的数据类型，此处包含了几个常用的类型<br />
	/// Data types of Mitsubishi PLC, here contains several commonly used types
	/// </summary>
	public class InovanceDataType
	{
		/// <summary>
		/// 实例化一个汇川数据类型对象，如果您清楚类型代号，可以根据值进行扩展<br />
		/// Instantiate a Mitsubishi data type object, if you know the type code, you can expand according to the value
		/// </summary>
		/// <param name="code">数据类型的代号</param>
		/// <param name="type">0或1，默认为0</param>
		/// <param name="asciiCode">ASCII格式的类型信息</param>
		/// <param name="fromBase">指示地址的多少进制的，10或是16</param>
		public InovanceDataType(byte code, byte type, string asciiCode, int fromBase)
		{
			DataCode = code;
			AsciiCode = asciiCode;
			FromBase = fromBase;
			if (type < 2) DataType = type;
		}

		/// <summary>
		/// 类型的代号值
		/// </summary>
		public byte DataCode { get; private set; } = 0x00;

		/// <summary>
		/// 数据的类型，0代表按字，1代表按位
		/// </summary>
		public byte DataType { get; private set; } = 0x00;

		/// <summary>
		/// 当以ASCII格式通讯时的类型描述
		/// </summary>
		public string AsciiCode { get; private set; }

		/// <summary>
		/// 指示地址是8进制，10进制，还是16进制的
		/// </summary>
		public int FromBase { get; private set; }

        #region H5U
        /// <summary>
        /// X输入继电器
        /// </summary>
        public readonly static InovanceDataType H5U_X = new InovanceDataType(0x01, 0x01, "X*", 8);

		/// <summary>
		/// Y输出继电器
		/// </summary>
		public readonly static InovanceDataType H5U_Y = new InovanceDataType(0x02, 0x01, "Y*", 8);

		/// <summary>
		/// M内部继电器
		/// </summary>
		public readonly static InovanceDataType H5U_M = new InovanceDataType(0x03, 0x01, "M*", 10);


		/// <summary>
		/// S步进继电器
		/// </summary>
		public readonly static InovanceDataType H5U_S = new InovanceDataType(0x04, 0x01, "S*", 10);

		/// <summary>
		/// S步进继电器
		/// </summary>
		public readonly static InovanceDataType H5U_B = new InovanceDataType(0x05, 0x01, "B*", 10);

		/// <summary>
		/// D数据寄存器
		/// </summary>
		public readonly static InovanceDataType H5U_D = new InovanceDataType(0x0A, 0x00, "D*", 10);
		/// <summary>
		/// R数据寄存器
		/// </summary>
		public readonly static InovanceDataType H5U_R = new InovanceDataType(0x0B, 0x00, "R*", 10);

		#endregion

		#region H3U
		/// <summary>
		/// X输入继电器
		/// </summary>
		public readonly static InovanceDataType H3U_X = new InovanceDataType(0x01, 0x01, "X*", 8);

		/// <summary>
		/// Y输出继电器
		/// </summary>
		public readonly static InovanceDataType H3U_Y = new InovanceDataType(0x02, 0x01, "Y*", 8);

		/// <summary>
		/// M内部继电器
		/// </summary>
		public readonly static InovanceDataType H3U_M = new InovanceDataType(0x03, 0x01, "M*", 10);


		/// <summary>
		/// S步进继电器
		/// </summary>
		public readonly static InovanceDataType H3U_S = new InovanceDataType(0x04, 0x01, "S*", 10);

		/// <summary>
		/// 定时器的触点
		/// </summary>
		public readonly static InovanceDataType H3U_TS = new InovanceDataType(0x05, 0x01, "T*", 10);

		/// <summary>
		/// 计数器的触点
		/// </summary>
		public readonly static InovanceDataType H3U_CS = new InovanceDataType(0x06, 0x01, "C*", 10);

		/// <summary>
		/// SM的触点
		/// </summary>
		public readonly static InovanceDataType H3U_SM = new InovanceDataType(0x07, 0x01, "SM*", 10);
		/// <summary>
		/// D数据寄存器
		/// </summary>
		public readonly static InovanceDataType H3U_D = new InovanceDataType(0x0A, 0x00, "D*", 10);

		/// <summary>
		/// 定时器的当前值
		/// </summary>
		public readonly static InovanceDataType H3U_TN = new InovanceDataType(0x0B, 0x00, "T*", 10);

		/// <summary>
		/// 计数器的当前值
		/// </summary>
		public readonly static InovanceDataType H3U_CN = new InovanceDataType(0x0C, 0x00, "C*", 10);
		/// <summary>
		/// D数据寄存器
		/// </summary>
		public readonly static InovanceDataType H3U_SD = new InovanceDataType(0x0D, 0x00, "SD*", 10);
		/// <summary>
		/// R数据寄存器
		/// </summary>
		public readonly static InovanceDataType H3U_R = new InovanceDataType(0x0E, 0x00, "R*", 10);
		#endregion

		#region AM
		/// <summary>
		/// I输入继电器
		/// </summary>
		public readonly static InovanceDataType AM_I = new InovanceDataType(0x01, 0x01, "I*", 8);

		/// <summary>
		/// Q输出继电器
		/// </summary>
		public readonly static InovanceDataType AM_Q = new InovanceDataType(0x02, 0x01, "Q*", 8);

		/// <summary>
		/// M内部继电器
		/// </summary>
		public readonly static InovanceDataType AM_M = new InovanceDataType(0x0A, 0x00, "M*", 10);
        #endregion

    }
}
