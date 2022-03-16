using Communication.Core;
using Communication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.BasicFramework;
using Communication.Reflection;

namespace Communication.Profinet.FATEK
{
	/// <summary>
	/// 台湾永宏公司的编程口协议，具体的地址信息请查阅api文档信息<br />
	/// The programming port protocol of Taiwan Yonghong company, please refer to the api document for specific address information
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="FatekProgramOverTcp" path="remarks"/>
	/// </remarks>
	public class FatekProgram : SerialDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="FatekProgramOverTcp()"/>
		public FatekProgram()
		{
			ByteTransform = new RegularByteTransform();
			WordLength = 1;
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="FatekProgramOverTcp.Station"/>
		public byte Station { get => station; set => station = value; }

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="FatekProgramOverTcp.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 解析指令
			OperateResult<byte[]> command = FatekProgramOverTcp.BuildReadCommand(this.station, address, length, false);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult<byte[]>(read.Content[0], "Read Faild:" + BasicFramework.SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<byte[]>(read.Content[5], FatekProgramOverTcp.GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			byte[] Content = new byte[length * 2];
			for (int i = 0; i < Content.Length / 2; i++)
			{
				ushort tmp = Convert.ToUInt16(Encoding.ASCII.GetString(read.Content, i * 4 + 6, 4), 16);
				BitConverter.GetBytes(tmp).CopyTo(Content, i * 2);
			}
			return OperateResult.CreateSuccessResult(Content);
		}

		/// <inheritdoc cref="FatekProgramOverTcp.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = FatekProgramOverTcp.BuildWriteByteCommand(this.station, address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult(read.Content[0], "Write Faild:" + BasicFramework.SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<byte[]>(read.Content[5], FatekProgramOverTcp.GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="FatekProgramOverTcp.ReadBool(string, ushort)"/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 解析指令
			OperateResult<byte[]> command = FatekProgramOverTcp.BuildReadCommand(this.station, address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult<bool[]>(read.Content[0], "Read Faild:" + BasicFramework.SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<bool[]>(read.Content[5], FatekProgramOverTcp.GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			byte[] buffer = new byte[length];
			Array.Copy(read.Content, 6, buffer, 0, length);
			return OperateResult.CreateSuccessResult(buffer.Select(m => m == 0x31).ToArray());
		}

		/// <inheritdoc cref="FatekProgramOverTcp.Write(string, bool[])"/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] value)
		{
			// 解析指令
			OperateResult<byte[]> command = FatekProgramOverTcp.BuildWriteBoolCommand(this.station, address, value);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadBase(command.Content);
			if (!read.IsSuccess) return read;

			// 结果验证
			if (read.Content[0] != 0x02) return new OperateResult(read.Content[0], "Write Faild:" + BasicFramework.SoftBasic.ByteToHexString(read.Content, ' '));
			if (read.Content[5] != 0x30) return new OperateResult<bool[]>(read.Content[5], FatekProgramOverTcp.GetErrorDescriptionFromCode((char)read.Content[5]));

			// 提取结果
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"FatekProgram[{PortName}:{BaudRate}]";

		#endregion

		#region Private Member

		private byte station = 0x01;                 // PLC的站号信息

		#endregion
	}
}
