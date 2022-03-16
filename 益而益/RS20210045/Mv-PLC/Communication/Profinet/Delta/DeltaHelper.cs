using Communication.ModBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communication.Profinet.Delta
{
	/// <summary>
	/// 
	/// </summary>
	public class DeltaHelper
	{

		/// <summary>
		/// 根据台达DVP-PLC的地址，解析出转换后的modbus协议信息，适用DVP系列
		/// </summary>
		/// <param name="address">台达plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseDeltaDvpAddress(string address, byte modbusCode)
		{
			try
			{
				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if (address.StartsWith("S") || address.StartsWith("s"))
						return OperateResult.CreateSuccessResult(Convert.ToInt32(address.Substring(1)).ToString());
					else if (address.StartsWith("X") || address.StartsWith("x"))
						return OperateResult.CreateSuccessResult("x=2;" + (Convert.ToInt32(address.Substring(1), 8) + 0x400).ToString());
					else if (address.StartsWith("Y") || address.StartsWith("y"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1), 8) + 0x500).ToString());
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x600).ToString());
					else if (address.StartsWith("C") || address.StartsWith("c"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0xE00).ToString());
					else if (address.StartsWith("M") || address.StartsWith("m"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 1536)
							return OperateResult.CreateSuccessResult((add - 1536 + 0xB000).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0x800).ToString());
					}
				}
				else
				{
					if (address.StartsWith("D") || address.StartsWith("d"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 4096)
							return OperateResult.CreateSuccessResult((add - 4096 + 0x9000).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0x1000).ToString());
					}
					else if (address.StartsWith("C") || address.StartsWith("c"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 200)
							return OperateResult.CreateSuccessResult((add - 200 + 0x0EC8).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0x0E00).ToString());
					}
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x600).ToString());
				}

				return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
			}
			catch (Exception ex)
			{
				return new OperateResult<string>(ex.Message);
			}
		}

		/// <summary>
		/// 根据台达AS300/200PLC的地址，解析出转换后的modbus协议信息，适用DVP系列
		/// </summary>
		/// <param name="address">台达plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseDeltaASAddress(string address, byte modbusCode)
		{
			try
			{
				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if (address.StartsWith("S") || address.StartsWith("s"))
						return OperateResult.CreateSuccessResult(Convert.ToInt32(address.Substring(1)).ToString());
					else if (address.StartsWith("X") || address.StartsWith("x"))
						return OperateResult.CreateSuccessResult("x=2;" + (Convert.ToInt32(address.Substring(1), 8) + 0x400).ToString());
					else if (address.StartsWith("Y") || address.StartsWith("y"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1), 8) + 0x500).ToString());
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x600).ToString());
					else if (address.StartsWith("C") || address.StartsWith("c"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0xE00).ToString());
					else if (address.StartsWith("M") || address.StartsWith("m"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 1536)
							return OperateResult.CreateSuccessResult((add - 1536 + 0xB000).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0x800).ToString());
					}
				}
				else
				{
					if (address.StartsWith("D") || address.StartsWith("d"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 4096)
							return OperateResult.CreateSuccessResult((add - 4096 + 0x9000).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0x1000).ToString());
					}
					else if (address.StartsWith("C") || address.StartsWith("c"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 200)
							return OperateResult.CreateSuccessResult((add - 200 + 0x0EC8).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0x0E00).ToString());
					}
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x600).ToString());
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
