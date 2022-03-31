using Communication.BasicFramework;
using Communication.Core;
using Communication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Profinet.Keyence
{
	/// <summary>
	/// 基恩士的KV链路模式，PLC的两个端口默认9600bps,8位数据位，1位起始位，1位停止位，偶校验位，无校验和
	/// </summary>
	/// <remarks>
	/// 使用Kegence自己的协议进行通讯时使用的模式。对外围设备发送过来的指令，将自动返回响应。 
	/// CPU单元侧无需通讯程序。可与 PC 等连接。支持KV-5500/5000/3000
	/// </remarks>
	public class KeyenceKVSerial : SerialDeviceBase
	{
		#region Constructor

		public KeyenceKVSerial()
		{
			this.ByteTransform = new RegularByteTransform();
			this.WordLength = 1;
		}


		/// <inheritdoc/>
		protected override OperateResult InitializationOnOpen()
		{
			// 建立通讯连接{CR/r}
			var result = ReadBase(KeyenceNanoSerialOverTcp.ConnectCmd);
			if (!result.IsSuccess) return result;

			if (result.Content.Length > 2)
				if (result.Content[0] == 0x43 && result.Content[1] == 0x43)
					return OperateResult.CreateSuccessResult();

			return new OperateResult("Check Failed: " + SoftBasic.ByteToHexString(result.Content, ' '));
		}

		/// <inheritdoc/>
		protected override OperateResult ExtraOnClose()
		{
			// 断开通讯连接{CR/r}
			var result = ReadBase(KeyenceNanoSerialOverTcp.DisConnectCmd);
			if (!result.IsSuccess) return result;

			if (result.Content.Length > 2)
				if (result.Content[0] == 0x43 && result.Content[1] == 0x46)
					return OperateResult.CreateSuccessResult();

			return new OperateResult("Check Failed: " + SoftBasic.ByteToHexString(result.Content, ' '));
		}

		#endregion

	}
}
