using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Core.Net;
using Communication.Core;
using System.Net;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的Udp的数据对象
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="OmronFinsNet" path="remarks"/>
	/// </remarks>
	public class OmronFinsUdp : NetworkUdpDeviceBase
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet(string, int)"/>
		public OmronFinsUdp(string ipAddress, int port)
		{
			this.WordLength = 1;
			this.IpAddress = ipAddress;
			this.Port = port;
			this.ByteTransform = new ReverseWordTransform();
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
		}

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronFinsUdp()
		{
			this.WordLength = 1;
			this.ByteTransform = new ReverseWordTransform();
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
		}

		#endregion

		#region IpAddress Override

		/// <inheritdoc/>
		public override string IpAddress
		{
			get => base.IpAddress;
			set
			{
				DA1 = Convert.ToByte(value.Substring(value.LastIndexOf(".") + 1));
				base.IpAddress = value;
			}
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="OmronFinsNet.ICF"/>
		public byte ICF { get; set; } = 0x80;

		/// <inheritdoc cref="OmronFinsNet.RSV"/>
		public byte RSV { get; private set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.GCT"/>
		public byte GCT { get; set; } = 0x02;

		/// <inheritdoc cref="OmronFinsNet.DNA"/>
		public byte DNA { get; set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.DA1"/>
		public byte DA1 { get; set; } = 0x13;

		/// <inheritdoc cref="OmronFinsNet.DA2"/>
		public byte DA2 { get; set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.SNA"/>
		public byte SNA { get; set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.SA1"/>
		public byte SA1 { get; set; } = 13;

		/// <inheritdoc cref="OmronFinsNet.SA2"/>
		public byte SA2 { get; set; }

		/// <inheritdoc cref="OmronFinsNet.SID"/>
		public byte SID { get; set; } = 0x00;

		#endregion

		#region Build Command

		/// <inheritdoc cref="OmronFinsNet.PackCommand(byte[])"/>
		private byte[] PackCommand(byte[] cmd)
		{
			byte[] buffer = new byte[10 + cmd.Length];
			buffer[0] = ICF;
			buffer[1] = RSV;
			buffer[2] = GCT;
			buffer[3] = DNA;
			buffer[4] = DA1;
			buffer[5] = DA2;
			buffer[6] = SNA;
			buffer[7] = SA1;
			buffer[8] = SA2;
			buffer[9] = SID;
			cmd.CopyTo(buffer, 10);

			return buffer;
		}

		/// <inheritdoc cref="OmronFinsNet.BuildReadCommand(string, ushort, bool)"/>
		public OperateResult<byte[]> BuildReadCommand(string address, ushort length, bool isBit)
		{
			var command = OmronFinsNetHelper.BuildReadCommand(address, length, isBit);
			if (!command.IsSuccess) return command;

			return OperateResult.CreateSuccessResult(PackCommand(command.Content));
		}

		/// <inheritdoc cref="OmronFinsNet.BuildWriteCommand(string, byte[], bool)"/>
		public OperateResult<byte[]> BuildWriteCommand(string address, byte[] value, bool isBit)
		{
			var command = OmronFinsNetHelper.BuildWriteWordCommand(address, value, isBit);
			if (!command.IsSuccess) return command;

			return OperateResult.CreateSuccessResult(PackCommand(command.Content));
		}

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 获取指令
			var command = BuildReadCommand(address, length, false);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(command);

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.UdpResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(valid);

			// 读取到了正确的数据
			return OperateResult.CreateSuccessResult(valid.Content);
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 获取指令
			var command = BuildWriteCommand(address, value, false);
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.UdpResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Read Write bool

		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 获取指令
			var command = BuildReadCommand(address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.UdpResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(valid);

			// 返回正确的数据信息
			return OperateResult.CreateSuccessResult(valid.Content.Select(m => m != 0x00 ? true : false).ToArray());
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] values)
		{
			// 获取指令
			var command = BuildWriteCommand(address, values.Select(m => m ? (byte)0x01 : (byte)0x00).ToArray(), true);
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronFinsNetHelper.UdpResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 写入成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"OmronFinsUdp[{IpAddress}:{Port}]";

		#endregion
	}
}
