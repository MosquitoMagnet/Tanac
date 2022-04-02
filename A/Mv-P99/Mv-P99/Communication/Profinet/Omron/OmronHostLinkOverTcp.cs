using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink协议的实现，基于Tcp实现，地址支持示例 DM区:D100; CIO区:C100; Work区:W100; Holding区:H100; Auxiliary区: A100<br />
	/// Implementation of Omron's HostLink protocol, based on tcp protocol, address support example DM area: D100; CIO area: C100; Work area: W100; Holding area: H100; Auxiliary area: A100
	/// </summary>
	/// <remarks>
	/// 感谢 深圳～拾忆 的测试
	/// 欧姆龙的地址参考如下：
	/// 地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址进制</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>DM Area</term>
	///     <term>D</term>
	///     <term>D100,D200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>CIO Area</term>
	///     <term>C</term>
	///     <term>C100,C200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Work Area</term>
	///     <term>W</term>
	///     <term>W100,W200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Holding Bit Area</term>
	///     <term>H</term>
	///     <term>H100,H200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>Auxiliary Bit Area</term>
	///     <term>A</term>
	///     <term>A100,A200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// <br />
	/// <note type="important">
	/// 如果发现串口线和usb同时打开才能通信的情况，需要按照如下的操作：<br />
	/// 串口线不是标准的串口线，电脑的串口线的235引脚分别接PLC的329引脚，45线短接，就可以通讯，感谢 深圳-小君(QQ932507362)提供的解决方案。
	/// </note>
	/// </remarks>
	public class OmronHostLinkOverTcp : NetworkDeviceSoloBase
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronHostLinkOverTcp()
		{
			this.ByteTransform = new ReverseWordTransform();
			this.WordLength = 1;
			this.ByteTransform.DataFormat = DataFormat.CDAB;
		}

		/// <inheritdoc cref="OmronCipNet(string,int)"/>
		public OmronHostLinkOverTcp(string ipAddress, int port)
		{
			this.ByteTransform = new ReverseWordTransform();
			this.WordLength = 1;
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.IpAddress = ipAddress;
			this.Port = port;
		}

		#endregion

		#region Public Member

		/// <summary>
		/// Specifies whether or not there are network relays. Set “80” (ASCII: 38,30) 
		/// when sending an FINS command to a CPU Unit on a network.Set “00” (ASCII: 30,30) 
		/// when sending to a CPU Unit connected directly to the host computer.
		/// </summary>
		public byte ICF { get; set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.DA2"/>
		public byte DA2 { get; set; } = 0x00;

		/// <inheritdoc cref="OmronFinsNet.SA2"/>
		public byte SA2 { get; set; }

		/// <inheritdoc cref="OmronFinsNet.SID"/>
		public byte SID { get; set; } = 0x00;

		/// <summary>
		/// The response wait time sets the time from when the CPU Unit receives a command block until it starts 
		/// to return a response.It can be set from 0 to F in hexadecimal, in units of 10 ms.
		/// If F(15) is set, the response will begin to be returned 150 ms (15 × 10 ms) after the command block was received.
		/// </summary>
		public byte ResponseWaitTime { get; set; } = 0x30;

		/// <summary>
		/// PLC设备的站号信息<br />
		/// PLC device station number information
		/// </summary>
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
			OperateResult<byte[]> read = ReadFromCoreServer(PackCommand(command.Content));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, true);
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
			OperateResult<byte[]> read = ReadFromCoreServer(PackCommand(command.Content));
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			// 解析地址
			var command = OmronFinsNetHelper.BuildReadCommand(address, length, false);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command.Content));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(valid);

			// 读取到了正确的数据
			return OperateResult.CreateSuccessResult(valid.Content);
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand(address, value, false); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command.Content));
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Read Write Bool

		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			// 获取指令
			var command = OmronFinsNetHelper.BuildReadCommand(address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(PackCommand(command.Content));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, true);
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
			OperateResult<byte[]> read = ReadFromCoreServer(PackCommand(command.Content));
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Read Write Bool
#if !NET35 && !NET20
		/// <inheritdoc cref="OmronFinsNet.ReadBool(string, ushort)"/>
		public override async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			// 获取指令
			var command = OmronFinsNetHelper.BuildReadCommand(address, length, true);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(command);

			// 核心交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command.Content));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(valid);

			// 返回正确的数据信息
			return OperateResult.CreateSuccessResult(valid.Content.Select(m => m != 0x00 ? true : false).ToArray());
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, bool[])"/>
		public override async Task<OperateResult> WriteAsync(string address, bool[] values)
		{
			// 获取指令
			var command = OmronFinsNetHelper.BuildWriteWordCommand(address, values.Select(m => m ? (byte)0x01 : (byte)0x00).ToArray(), true); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(PackCommand(command.Content));
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"OmronHostLinkOverTcp[{IpAddress}:{Port}]";

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
			string output = Encoding.ASCII.GetString(buffer);
			Console.WriteLine(output);
			return buffer;
		}

		/// <summary>
		/// 验证欧姆龙的Fins-TCP返回的数据是否正确的数据，如果正确的话，并返回所有的数据内容
		/// </summary>
		/// <param name="response">来自欧姆龙返回的数据内容</param>
		/// <param name="isRead">是否读取</param>
		/// <returns>带有是否成功的结果对象</returns>
		public static OperateResult<byte[]> ResponseValidAnalysis(byte[] response, bool isRead)
		{
			// 数据有效性分析
			if (response.Length >= 27)
			{
				// 提取错误码
				if (int.TryParse(Encoding.ASCII.GetString(response, 19, 4), out int err))
				{
					byte[] Content = null;

					if (response.Length > 27)
					{
						Content = SoftBasic.HexStringToBytes(Encoding.ASCII.GetString(response, 23, response.Length - 27));
					}

					if (err > 0) return new OperateResult<byte[]>()
					{
						ErrorCode = err,
						Content = Content
					};
					else
					{
						return OperateResult.CreateSuccessResult(Content);
					}
				}
				else
				{
					return new OperateResult<byte[]>($"Parse error code failed, [{ Encoding.ASCII.GetString(response, 19, 4)}] {Environment.NewLine}Source Data: " + response.ToHexString(' '));
				}
			}

			return new OperateResult<byte[]>(StringResources.Language.OmronReceiveDataError + " Source Data: " + response.ToHexString(' '));
		}

		#endregion
	}
}
