using System;
using System.Collections.Generic;
using System.Text;
using Communication.ModBus;

namespace Communication.Profinet.Inovance
{
	/// <summary>
	/// 汇川的辅助类，提供一些地址解析的方法<br />
	/// Auxiliary class of Yaskawa robot, providing some methods of address resolution
	/// </summary>
	public class InovanceHelper
	{
		#region Static Helper

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

		/// <summary>
		/// 根据汇川PLC的地址，解析出转换后的modbus协议信息
		/// </summary>
		/// <param name="address">汇川plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseInovanceAMAddress(string address, byte modbusCode)
		{
			try
			{
				if (address.StartsWith("QX") || address.StartsWith("qx"))
					return OperateResult.CreateSuccessResult(CalculateStartAddress(address.Substring(2)).ToString());
				else if (address.StartsWith("Q") || address.StartsWith("q"))
					return OperateResult.CreateSuccessResult(CalculateStartAddress(address.Substring(1)).ToString());
				else if (address.StartsWith("IX") || address.StartsWith("ix"))
					return OperateResult.CreateSuccessResult("x=2;" + CalculateStartAddress(address.Substring(2)).ToString());
				else if (address.StartsWith("I") || address.StartsWith("i"))
					return OperateResult.CreateSuccessResult("x=2;" + CalculateStartAddress(address.Substring(1)).ToString());
				else if (address.StartsWith("MW") || address.StartsWith("mw"))
					return OperateResult.CreateSuccessResult(address.Substring(2));
				else if (address.StartsWith("M") || address.StartsWith("m"))
					return OperateResult.CreateSuccessResult(address.Substring(1));
				else
				{
					if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
					{
						if (address.StartsWith("SMX") || address.StartsWith("smx"))
							return OperateResult.CreateSuccessResult($"x={modbusCode + 0x30};" + CalculateStartAddress(address.Substring(3)).ToString());
						else if (address.StartsWith("SM") || address.StartsWith("sm"))
							return OperateResult.CreateSuccessResult($"x={modbusCode + 0x30};" + CalculateStartAddress(address.Substring(2)).ToString());
					}
					else
					{
						if (address.StartsWith("SDW") || address.StartsWith("sdw"))
							return OperateResult.CreateSuccessResult($"x={modbusCode + 0x30};" + address.Substring(3));
						else if (address.StartsWith("SD") || address.StartsWith("sd"))
							return OperateResult.CreateSuccessResult($"x={modbusCode + 0x30};" + address.Substring(2));
					}
				}

				return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
			}
			catch (Exception ex)
			{
				return new OperateResult<string>(ex.Message);
			}
		}

		private static int CalculateH3UStartAddress(string address)
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
		/// 根据汇川PLC的地址，解析出转换后的modbus协议信息，适用H3U系列
		/// </summary>
		/// <param name="address">汇川plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseInovanceH3UAddress(string address, byte modbusCode)
		{
			try
			{
				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if (address.StartsWith("X") || address.StartsWith("x"))
						return OperateResult.CreateSuccessResult((CalculateH3UStartAddress(address.Substring(1)) + 0xF800).ToString());
					else if (address.StartsWith("Y") || address.StartsWith("y"))
						return OperateResult.CreateSuccessResult((CalculateH3UStartAddress(address.Substring(1)) + 0xFC00).ToString());
					else if (address.StartsWith("SM") || address.StartsWith("sm"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(2)) + 0x2400).ToString());
					else if (address.StartsWith("S") || address.StartsWith("s"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0xE000).ToString());
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0xF000).ToString());
					else if (address.StartsWith("C") || address.StartsWith("c"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0xF400).ToString());
					else if (address.StartsWith("M") || address.StartsWith("m"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 8000)
							return OperateResult.CreateSuccessResult((add - 8000 + 0x1F40).ToString());
						else
							return OperateResult.CreateSuccessResult(add.ToString());
					}
				}
				else
				{
					if (address.StartsWith("D") || address.StartsWith("d"))
						return OperateResult.CreateSuccessResult(Convert.ToInt32(address.Substring(1)).ToString());
					else if (address.StartsWith("SD") || address.StartsWith("sd"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(2)) + 0x2400).ToString());
					else if (address.StartsWith("R") || address.StartsWith("r"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x3000).ToString());
					else if (address.StartsWith("T") || address.StartsWith("t"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0xF000).ToString());
					else if (address.StartsWith("C") || address.StartsWith("c"))
					{
						int add = Convert.ToInt32(address.Substring(1));
						if (add >= 200)
							return OperateResult.CreateSuccessResult(((add - 200) * 2 + 0xF700).ToString());
						else
							return OperateResult.CreateSuccessResult((add + 0xF400).ToString());
					}
				}

				return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
			}
			catch (Exception ex)
			{
				return new OperateResult<string>(ex.Message);
			}
		}

		/// <summary>
		/// 根据汇川PLC的地址，解析出转换后的modbus协议信息，适用H5U系列
		/// </summary>
		/// <param name="address">汇川plc的地址信息</param>
		/// <param name="modbusCode">原始的对应的modbus信息</param>
		/// <returns>还原后的modbus地址</returns>
		public static OperateResult<string> PraseInovanceH5UAddress(string address, byte modbusCode)
		{
			try
			{
				if (modbusCode == ModbusInfo.ReadCoil || modbusCode == ModbusInfo.WriteCoil || modbusCode == ModbusInfo.WriteOneCoil)
				{
					if (address.StartsWith("X") || address.StartsWith("x"))
						return OperateResult.CreateSuccessResult((CalculateH3UStartAddress(address.Substring(1)) + 0xF800).ToString());
					else if (address.StartsWith("Y") || address.StartsWith("y"))
						return OperateResult.CreateSuccessResult((CalculateH3UStartAddress(address.Substring(1)) + 0xFC00).ToString());
					else if (address.StartsWith("S") || address.StartsWith("s"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0xE000).ToString());
					else if (address.StartsWith("B") || address.StartsWith("b"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x3000).ToString());
					else if (address.StartsWith("M") || address.StartsWith("m"))
						return OperateResult.CreateSuccessResult(Convert.ToInt32(address.Substring(1)).ToString());
				}
				else
				{
					if (address.StartsWith("D") || address.StartsWith("d"))
						return OperateResult.CreateSuccessResult(Convert.ToInt32(address.Substring(1)).ToString());
					else if (address.StartsWith("R") || address.StartsWith("r"))
						return OperateResult.CreateSuccessResult((Convert.ToInt32(address.Substring(1)) + 0x3000).ToString());
				}

				return new OperateResult<string>(StringResources.Language.NotSupportedDataType);
			}
			catch (Exception ex)
			{
				return new OperateResult<string>(ex.Message);
			}
		}

		#endregion
	}
}
