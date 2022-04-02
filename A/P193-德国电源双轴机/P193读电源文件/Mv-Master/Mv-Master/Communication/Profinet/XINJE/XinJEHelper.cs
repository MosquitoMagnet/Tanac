using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.ModBus;

namespace Communication.Profinet.XINJE
{
	/// <summary>
	/// 信捷PLC的相关辅助类
	/// </summary>
	public class XinJEHelper
	{
		private static int CalculateXinJEStartAddress(string address)
		{
			if (address.IndexOf('.') < 0)
				return Convert.ToInt32(address, 8);
			else
			{
				string[] splits = address.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
				return Convert.ToInt32(splits[0], 8) * 8 + int.Parse(splits[1]);
			}
		}

		/// <summary>
		/// 根据信捷PLC的地址，解析出转换后的modbus协议信息，适用XC系列
		/// </summary>
		/// <param name="address">安川plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseXinJEXCAddress(string address, byte modbusCode)
		{
			try
			{
				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if (address.StartsWith("X") || address.StartsWith("x"))
						return OperateResult.CreateSuccessResult((CalculateXinJEStartAddress(address.Substring(1)) + 0x4000).ToString());
					else if (address.StartsWith("Y") || address.StartsWith("y"))
						return OperateResult.CreateSuccessResult((CalculateXinJEStartAddress(address.Substring(1)) + 0x4800).ToString());
					else if (address.StartsWith("S") || address.StartsWith("s"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x5000).ToString());
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x6400).ToString());
					else if (address.StartsWith("C") || address.StartsWith("c"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x6C00).ToString());
					else if (address.StartsWith("M") || address.StartsWith("m"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 8000)
							return OperateResult.CreateSuccessResult((add - 8000 + 0x6000).ToString());
						else
							return OperateResult.CreateSuccessResult(add.ToString());
					}
				}
				else
				{
					if (address.StartsWith("D") || address.StartsWith("d"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 8000)
							return OperateResult.CreateSuccessResult((add - 8000 + 0x4000).ToString());
						else
							return OperateResult.CreateSuccessResult(add.ToString());
					}
					else if (address.StartsWith("F") || address.StartsWith("f"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 8000)
							return OperateResult.CreateSuccessResult((add - 8000 + 0x6800).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0x4800).ToString());
					}
					else if (address.StartsWith("E") || address.StartsWith("e"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x7000).ToString());
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x3000).ToString());
					else if (address.StartsWith("C") || address.StartsWith("c"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x3800).ToString());
				}

				return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
			}
			catch (Exception ex)
			{
				return new OperateResult<string>(ex.Message);
			}
		}

	}
}
