using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Core.Address
{
	/// <summary>
	/// 西门子的地址数据信息，主要包含数据代码，DB块，偏移地址，当处于写入时，Length无效<br />
	/// Address data information of Siemens, mainly including data code, DB block, offset address, when writing, Length is invalid
	/// </summary>
	public class S7AddressData : DeviceAddressDataBase
	{
		/// <summary>
		/// 获取或设置等待读取的数据的代码<br />
		/// Get or set the code of the data waiting to be read
		/// </summary>
		public byte DataCode
		{
			get;
			set;
		}

		/// <summary>
		/// 获取或设置PLC的DB块数据信息<br />
		/// Get or set PLC DB data information
		/// </summary>
		public ushort DbBlock
		{
			get;
			set;
		}

		/// <summary>
		/// 从指定的地址信息解析成真正的设备地址信息
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		public override void Parse(string address, ushort length)
		{
			OperateResult<S7AddressData> operateResult = ParseFrom(address, length);
			if (operateResult.IsSuccess)
			{
				base.AddressStart = operateResult.Content.AddressStart;
				base.Length = operateResult.Content.Length;
				DataCode = operateResult.Content.DataCode;
				DbBlock = operateResult.Content.DbBlock;
			}
		}

		/// <summary>
		/// 计算特殊的地址信息<br />
		/// Calculate Special Address information
		/// </summary>
		/// <param name="address">字符串地址 -&gt; String address</param>
		/// <param name="isCT">是否是定时器和计数器的地址</param>
		/// <returns>实际值 -&gt; Actual value</returns>
		public static int CalculateAddressStarted(string address, bool isCT = false)
		{
			if (address.IndexOf('.') < 0)
			{
				if (isCT)
				{
					return Convert.ToInt32(address);
				}
				return Convert.ToInt32(address) * 8;
			}
			string[] array = address.Split('.');
			return Convert.ToInt32(array[0]) * 8 + Convert.ToInt32(array[1]);
		}

		/// <summary>
		/// 从实际的西门子的地址里面解析出地址对象<br />
		/// Resolve the address object from the actual Siemens address
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<S7AddressData> ParseFrom(string address)
		{
			return ParseFrom(address, 0);
		}

		/// <summary>
		/// 从实际的西门子的地址里面解析出地址对象<br />
		/// Resolve the address object from the actual Siemens address
		/// </summary>
		/// <param name="address">西门子的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<S7AddressData> ParseFrom(string address, ushort length)
		{
			S7AddressData s7AddressData = new S7AddressData();
			try
			{
				s7AddressData.Length = length;
				s7AddressData.DbBlock = 0;
				if (address[0] == 'I')
				{
					s7AddressData.DataCode = 129;
					s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
				}
				else if (address[0] == 'Q')
				{
					s7AddressData.DataCode = 130;
					s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
				}
				else if (address[0] == 'M')
				{
					s7AddressData.DataCode = 131;
					s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
				}
				else if (address[0] == 'D' || address.Substring(0, 2) == "DB")
				{
					s7AddressData.DataCode = 132;
					string[] array = address.Split('.');
					if (address[1] == 'B')
					{
						s7AddressData.DbBlock = Convert.ToUInt16(array[0].Substring(2));
					}
					else
					{
						s7AddressData.DbBlock = Convert.ToUInt16(array[0].Substring(1));
					}
					s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(address.IndexOf('.') + 1));
				}
				else if (address[0] == 'T')
				{
					s7AddressData.DataCode = 29;
					s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1), isCT: true);
				}
				else if (address[0] == 'C')
				{
					s7AddressData.DataCode = 28;
					s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1), isCT: true);
				}
				else
				{
					if (address[0] != 'V')
					{
						return new OperateResult<S7AddressData>(StringResources.Language.NotSupportedDataType);
					}
					s7AddressData.DataCode = 132;
					s7AddressData.DbBlock = 1;
					s7AddressData.AddressStart = CalculateAddressStarted(address.Substring(1));
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<S7AddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(s7AddressData);
		}
	}

}
