using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Core;
using Communication.Reflection;
using Communication.BasicFramework;
using System.Threading.Tasks;
using Communication.Core.Net;

namespace Communication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙的HostLink的C-Mode实现形式，当前的类是通过以太网透传实现。
	/// </summary>
	public class OmronHostLinkCModeOverTcp : NetworkDeviceSoloBase
	{
		#region Constructor

		/// <inheritdoc cref="OmronFinsNet()"/>
		public OmronHostLinkCModeOverTcp()
		{
			this.ByteTransform = new ReverseWordTransform();
			this.WordLength = 1;
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
		}

		/// <inheritdoc cref="OmronCipNet(string,int)"/>
		public OmronHostLinkCModeOverTcp(string ipAddress, int port)
		{
			this.ByteTransform = new ReverseWordTransform();
			this.WordLength = 1;
			this.ByteTransform.DataFormat = DataFormat.CDAB;
			this.ByteTransform.IsStringReverseByteWord = true;
			this.IpAddress = ipAddress;
			this.Port = port;
		}

		#endregion

		#region Public Member

		/// <inheritdoc cref="OmronHostLinkOverTcp.UnitNumber"/>
		public byte UnitNumber { get; set; }

		#endregion

		#region Read Write Support

		/// <inheritdoc cref="OmronFinsNet.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			// 解析地址
			var command = OmronHostLinkCMode.BuildReadCommand(address, length, false);
			if (!command.IsSuccess) return command;

			// 核心交互
			OperateResult<byte[]> read = ReadFromCoreServer(OmronHostLinkCMode.PackCommand(command.Content, UnitNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronHostLinkCMode.ResponseValidAnalysis(read.Content, true);
			if (!valid.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(valid);

			// 读取到了正确的数据
			return OperateResult.CreateSuccessResult(valid.Content);
		}

		/// <inheritdoc cref="OmronFinsNet.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			// 获取指令
			var command = OmronHostLinkCMode.BuildWriteWordCommand(address, value); ;
			if (!command.IsSuccess) return command;

			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(OmronHostLinkCMode.PackCommand(command.Content, UnitNumber));
			if (!read.IsSuccess) return read;

			// 数据有效性分析
			OperateResult<byte[]> valid = OmronHostLinkCMode.ResponseValidAnalysis(read.Content, false);
			if (!valid.IsSuccess) return valid;

			// 成功
			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Bool Read Write


		#endregion

		#region Public Method

		/// <summary>
		/// 读取PLC的当前的型号信息
		/// </summary>
		/// <returns>型号</returns>
		[HslMqttApi]
		public OperateResult<string> ReadPlcModel()
		{
			// 核心数据交互
			OperateResult<byte[]> read = ReadFromCoreServer(OmronHostLinkCMode.PackCommand(Encoding.ASCII.GetBytes("MM"), UnitNumber));
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			// 数据有效性分析
			int err = Convert.ToInt32(Encoding.ASCII.GetString(read.Content, 5, 2), 16);
			if (err > 0) return new OperateResult<string>(err, "Unknown Error");

			// 成功
			string model = Encoding.ASCII.GetString(read.Content, 7, 2);
			return OmronHostLinkCMode.GetModelText(model);
		}

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"OmronHostLinkCModeOverTcp[{IpAddress}:{Port}]";

		#endregion

	}
}
