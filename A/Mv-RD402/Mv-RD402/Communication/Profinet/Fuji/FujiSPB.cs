using Communication.BasicFramework;
using Communication.Core;
using Communication.Serial;
using Communication.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communication.Profinet.Fuji
{
	/// <summary>
	/// 富士PLC的SPB协议，详细的地址信息见api文档说明<br />
	/// Fuji PLC's SPB protocol. For detailed address information, see the api documentation.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="FujiSPBOverTcp" path="remarks"/>
	/// </remarks>
	public class FujiSPB : SerialDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="FujiSPBOverTcp()"/>
		public FujiSPB()
		{
			this.ByteTransform = new RegularByteTransform();
			this.WordLength = 1;
			base.LogMsgFormatBinary = false;
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="FujiSPBOverTcp.Station"/>
		public byte Station { get => station; set => station = value; }

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="FujiSPBOverTcp.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 解析指令
			OperateResult<byte[]> command = FujiSPBOverTcp.BuildReadCommand(this.station, address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 结果验证
			if (read.Content[0] != ':') return new OperateResult<byte[]>(read.Content[0], "Read Faild:" + BasicFramework.SoftBasic.ByteToHexString(read.Content, ' '));
			if (Encoding.ASCII.GetString(read.Content, 9, 2) != "00") return new OperateResult<byte[]>(read.Content[5], FujiSPBOverTcp.GetErrorDescriptionFromCode(Encoding.ASCII.GetString(read.Content, 9, 2)));

			// 提取结果
			byte[] Content = new byte[length * 2];
			for (int i = 0; i < Content.Length / 2; i++)
			{
				ushort tmp = Convert.ToUInt16(Encoding.ASCII.GetString(read.Content, i * 4 + 6, 4), 16);
				BitConverter.GetBytes(tmp).CopyTo(Content, i * 2);
			}
			return OperateResult.CreateSuccessResult(Content);
		}

		/// <inheritdoc cref="FujiSPBOverTcp.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = FujiSPBOverTcp.BuildWriteByteCommand(this.station, address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			if (read.Content[0] != ':') return new OperateResult<byte[]>(read.Content[0], "Read Faild:" + BasicFramework.SoftBasic.ByteToHexString(read.Content, ' '));
			if (Encoding.ASCII.GetString(read.Content, 9, 2) != "00") return new OperateResult<byte[]>(read.Content[5], FujiSPBOverTcp.GetErrorDescriptionFromCode(Encoding.ASCII.GetString(read.Content, 9, 2)));

			// 提取结果
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Private Member

		private byte station = 0x01;                 // PLC的站号信息

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"FujiSPB[{PortName}:{BaudRate}]";

		#endregion
	}
}
