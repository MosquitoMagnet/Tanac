using Communication.BasicFramework;
using Communication.Core;
using Communication.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Reflection;

namespace Communication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink协议的实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100<br />
	/// Implementation of Omron's HostLink protocol, address support example DM area: D100; CIO area: C100; Work area: W100; Holding area: H100; Auxiliary area: A100
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="OmronHostLinkOverTcp" path="remarks"/>
	/// </remarks>
	public class OmronHostLink : SerialDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronHostLink()
		{
			this.ByteTransform = new ReverseWordTransform();
			this.WordLength = 1;
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="OmronHostLinkOverTcp.ICF"/>
		public byte ICF { get; set; } = 0x00;

		/// <inheritdoc cref="OmronHostLinkOverTcp.DA2"/>
		public byte DA2 { get; set; } = 0x00;

		/// <inheritdoc cref="OmronHostLinkOverTcp.SA2"/>
		public byte SA2 { get; set; }

		/// <inheritdoc cref="OmronHostLinkOverTcp.SID"/>
		public byte SID { get; set; } = 0x00;

		/// <inheritdoc cref="OmronHostLinkOverTcp.ResponseWaitTime"/>
		public byte ResponseWaitTime { get; set; } = 0x30;

		/// <inheritdoc cref="OmronHostLinkOverTcp.UnitNumber"/>
		public byte UnitNumber { get; set; }

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 解析地址
			var command = OmronFinsNetHelper.BuildReadCommand(address, length, false);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadBase(PackCommand(command.Content));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronHostLinkOverTcp.ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(valid);

			// 读取到了正确的数据
			return OperateResult.CreateSuccessResult(valid.Content);
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand(address, value, false); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = ReadBase(PackCommand(command.Content));
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronHostLinkOverTcp.ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Bool Read Write

		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 获取指令
			var command = OmronFinsNetHelper.BuildReadCommand(address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadBase(PackCommand(command.Content));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronHostLinkOverTcp.ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(valid);

			// 返回正确的数据信息
			return OperateResult.CreateSuccessResult(valid.Content.Select(m => m != 0x00 ? true : false).ToArray());
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand(address, values.Select(m => m ? (byte)0x01 : (byte)0x00).ToArray(), true); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = ReadBase(PackCommand(command.Content));
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronHostLinkOverTcp.ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"OmronHostLink[{PortName}:{BaudRate}]";

		#endregion

		#region Build Command

		/// <summary>
		/// 将普通的指令打包成完整的指令
		/// </summary>
		/// <param name="cmd">fins指令</param>
		/// <returns>完整的质量</returns>
		private byte[] PackCommand(byte[] cmd)
		{
			cmd = BasicFramework.SoftBasic.BytesToAsciiBytes(cmd);

			byte[] buffer = new byte[18 + cmd.Length];

			buffer[0] = (byte)'@';
			buffer[1] = SoftBasic.BuildAsciiBytesFrom(this.UnitNumber)[0];
			buffer[2] = SoftBasic.BuildAsciiBytesFrom(this.UnitNumber)[1];
			buffer[3] = (byte)'F';
			buffer[4] = (byte)'A';
			buffer[5] = ResponseWaitTime;
			buffer[6] = SoftBasic.BuildAsciiBytesFrom(this.ICF)[0];
			buffer[7] = SoftBasic.BuildAsciiBytesFrom(this.ICF)[1];
			buffer[8] = SoftBasic.BuildAsciiBytesFrom(this.DA2)[0];
			buffer[9] = SoftBasic.BuildAsciiBytesFrom(this.DA2)[1];
			buffer[10] = SoftBasic.BuildAsciiBytesFrom(this.SA2)[0];
			buffer[11] = SoftBasic.BuildAsciiBytesFrom(this.SA2)[1];
			buffer[12] = SoftBasic.BuildAsciiBytesFrom(this.SID)[0];
			buffer[13] = SoftBasic.BuildAsciiBytesFrom(this.SID)[1];
			buffer[buffer.Length - 2] = (byte)'*';
			buffer[buffer.Length - 1] = 0x0D;
			cmd.CopyTo(buffer, 14);
			// 计算FCS
			int tmp = buffer[0];
			for (int i = 1; i < buffer.Length - 4; i++)
			{
				tmp = (tmp ^ buffer[i]);
			}
			buffer[buffer.Length - 4] = SoftBasic.BuildAsciiBytesFrom((byte)tmp)[0];
			buffer[buffer.Length - 3] = SoftBasic.BuildAsciiBytesFrom((byte)tmp)[1];
			return buffer;
		}

		#endregion
	}
}
