using System;
using System.Collections.Generic;
using System.Text;
using Communication;

namespace CommonDriver.Delta
{
	/// <summary>
	/// 台达的数据地址表示形式<br />
	/// Delta's data address representation
	/// </summary>
	public class DeltaAddressData 
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
		/// 台达的数据类型及地址信息
		/// </summary>
		public DeltaDataType DeltaDataType
		{
			get;
			set;
		}




		/// <summary>
		/// 从实际台达Dvp的地址里面解析出我们需要的地址类型<br />
		/// Resolve the type of address we need from the actual Delta address
		/// </summary>
		/// <param name="address">台达的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<DeltaAddressData> ParseDvpFrom(string address, ushort length)
		{
			DeltaAddressData DeltaAddressData = new DeltaAddressData();
			DeltaAddressData.Length = length;
			try
			{
				switch (address[0])
				{
					case 'M':
					case 'm':
						DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_M;
						DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(1), DeltaDataType.Dvp_M.FromBase);
						break;
					case 'X':
					case 'x':
						DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_X;
						DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(1), DeltaDataType.Dvp_X.FromBase);
						break;
					case 'Y':
					case 'y':
						DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_Y;
						DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(1), DeltaDataType.Dvp_Y.FromBase);
						break;
					case 'D':
					case 'd':
						{
							DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_D;
							DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(1), DeltaDataType.Dvp_D.FromBase);
						}
						break;				
					case 'T':
					case 't':
						if (address[1] == 'N' || address[1] == 'n')
						{
							DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_TN;
							DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(2), DeltaDataType.Dvp_TN.FromBase);
						}
						else if (address[1] == 'S' || address[1] == 's')
						{
							DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_TS;
							DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(2), DeltaDataType.Dvp_TS.FromBase);
						}					
						break;
					case 'C':
					case 'c':
						if (address[1] == 'N' || address[1] == 'n')
						{
							DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_CN;
							DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(2), DeltaDataType.Dvp_CN.FromBase);
						}
						else if (address[1] == 'S' || address[1] == 's')
						{
							DeltaAddressData.DeltaDataType = DeltaDataType.Dvp_CS;
							DeltaAddressData.AddressStart = Convert.ToInt32(address.Substring(2), DeltaDataType.Dvp_CS.FromBase);
						}
						break;
					default:
						throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<DeltaAddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(DeltaAddressData);
		}		
	}
	/// <summary>
	/// 台达PLC的数据类型，此处包含了几个常用的类型<br />
	/// Data types of Mitsubishi PLC, here contains several commonly used types
	/// </summary>
	public class DeltaDataType
	{
		/// <summary>
		/// 实例化一个台达数据类型对象，如果您清楚类型代号，可以根据值进行扩展<br />
		/// Instantiate a Mitsubishi data type object, if you know the type code, you can expand according to the value
		/// </summary>
		/// <param name="code">数据类型的代号</param>
		/// <param name="type">0或1，默认为0</param>
		/// <param name="asciiCode">ASCII格式的类型信息</param>
		/// <param name="fromBase">指示地址的多少进制的，10或是16</param>
		public DeltaDataType(byte code, byte type, string asciiCode, int fromBase)
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

		/// <summary>
		/// X输入继电器
		/// </summary>
		public readonly static DeltaDataType Dvp_X = new DeltaDataType(0x01, 0x01, "X*", 8);

		/// <summary>
		/// Y输出继电器
		/// </summary>
		public readonly static DeltaDataType Dvp_Y = new DeltaDataType(0x02, 0x01, "Y*", 8);

		/// <summary>
		/// M内部继电器
		/// </summary>
		public readonly static DeltaDataType Dvp_M = new DeltaDataType(0x03, 0x01, "M*", 10);


		/// <summary>
		/// S步进继电器
		/// </summary>
		public readonly static DeltaDataType Dvp_S = new DeltaDataType(0x04, 0x01, "S*", 10);

		/// <summary>
		/// 定时器的触点
		/// </summary>
		public readonly static DeltaDataType Dvp_TS = new DeltaDataType(0x05, 0x01, "T*", 10);
		
		/// <summary>
		/// 计数器的触点
		/// </summary>
		public readonly static DeltaDataType Dvp_CS = new DeltaDataType(0x06, 0x01, "C*", 10);

		/// <summary>
		/// D数据寄存器
		/// </summary>
		public readonly static DeltaDataType Dvp_D = new DeltaDataType(0x0A, 0x00, "D*", 10);

		/// <summary>
		/// 定时器的当前值
		/// </summary>
		public readonly static DeltaDataType Dvp_TN = new DeltaDataType(0x0B, 0x00, "T*", 10);
	
		/// <summary>
		/// 计数器的当前值
		/// </summary>
		public readonly static DeltaDataType Dvp_CN = new DeltaDataType(0x0C, 0x00, "C*", 10);


		/// <summary>
		/// X输入继电器
		/// </summary>
		public readonly static DeltaDataType As_X = new DeltaDataType(0x01, 0x01, "X*", 8);

		/// <summary>
		/// Y输出继电器
		/// </summary>
		public readonly static DeltaDataType As_Y = new DeltaDataType(0x02, 0x01, "Y*", 8);

		/// <summary>
		/// M内部继电器
		/// </summary>
		public readonly static DeltaDataType As_M = new DeltaDataType(0x03, 0x01, "M*", 10);

		/// <summary>
		/// SM继电器
		/// </summary>
		public readonly static DeltaDataType As_SM = new DeltaDataType(0x03, 0x01, "M*", 10);

		/// <summary>
		/// S步进继电器
		/// </summary>
		public readonly static DeltaDataType As_S = new DeltaDataType(0x04, 0x01, "S*", 10);

		/// <summary>
		/// 定时器的触点
		/// </summary>
		public readonly static DeltaDataType As_TS = new DeltaDataType(0x05, 0x01, "T*", 10);

		/// <summary>
		/// 计数器的触点
		/// </summary>
		public readonly static DeltaDataType As_CS = new DeltaDataType(0x06, 0x01, "C*", 10);

		/// <summary>
		/// 高速计数器的触点
		/// </summary>
		public readonly static DeltaDataType As_HCS = new DeltaDataType(0x06, 0x01, "HC*", 10);

		/// <summary>
		/// D数据寄存器
		/// </summary>
		public readonly static DeltaDataType As_D = new DeltaDataType(0x0A, 0x00, "D*", 10);

		/// <summary>
		/// SR数据寄存器
		/// </summary>
		public readonly static DeltaDataType As_SR = new DeltaDataType(0x0A, 0x00, "SR*", 10);

		/// <summary>
		/// 定时器的当前值
		/// </summary>
		public readonly static DeltaDataType As_TN = new DeltaDataType(0x0B, 0x00, "T*", 10);

		/// <summary>
		/// 计数器的当前值
		/// </summary>
		public readonly static DeltaDataType As_CN = new DeltaDataType(0x0C, 0x00, "C*", 10);

		/// <summary>
		/// 高速计数器的当前值
		/// </summary>
		public readonly static DeltaDataType As_HCN = new DeltaDataType(0x0C, 0x00, "HC*", 10);

		/// <summary>
		/// 变址寄存器的当前值
		/// </summary>
		public readonly static DeltaDataType As_E = new DeltaDataType(0x0C, 0x00, "E*", 10);

	}
}
