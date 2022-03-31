using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.BasicFramework;
using Communication.Profinet.AllenBradley;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.Profinet.Omron
{
	/// <summary>
	/// 欧姆龙PLC的CIP协议的类，支持NJ,NX,NY系列PLC，支持tag名的方式读写数据，假设你读取的是局部变量，那么使用 Program:MainProgram.变量名<br />
	/// Omron PLC's CIP protocol class, support NJ, NX, NY series PLC, support tag name read and write data, assuming you read local variables, then use Program: MainProgram. Variable name
	/// </summary>
	public class OmronCipNet : AllenBradley.AllenBradleyNet
	{
		#region Constructor

		/// <summary>
		/// Instantiate a communication object for a OmronCipNet PLC protocol
		/// </summary>
		public OmronCipNet() : base() { }

		/// <summary>
		/// Specify the IP address and port to instantiate a communication object for a OmronCipNet PLC protocol
		/// </summary>
		/// <param name="ipAddress">PLC IpAddress</param>
		/// <param name="port">PLC Port</param>
		public OmronCipNet(string ipAddress, int port = 44818) : base(ipAddress, port) { }

		#endregion

		#region Read Write Override

		/// <inheritdoc/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			if (length > 1)
				return Read(new string[] { address }, new int[] { 1 });
			else
				return Read(new string[] { address }, new int[] { length });
		}

		/// <inheritdoc/>
		public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
		{
			OperateResult<byte[]> read = Read(address, length);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			int strLen = ByteTransform.TransUInt16(read.Content, 0);
			return OperateResult.CreateSuccessResult(encoding.GetString(read.Content, 2, strLen));
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteString", "")]
		public override OperateResult Write(string address, string value)
		{
			if (string.IsNullOrEmpty(value)) value = string.Empty;

			byte[] data = SoftBasic.SpliceArray<byte>(new byte[2], SoftBasic.ArrayExpandToLengthEven(Encoding.ASCII.GetBytes(value)));
			data[0] = BitConverter.GetBytes(data.Length - 2)[0];
			data[1] = BitConverter.GetBytes(data.Length - 2)[1];
			return base.WriteTag(address, AllenBradleyHelper.CIP_Type_String, data, 1);
		}

		/// <inheritdoc/>
		public override OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
		{
			return base.WriteTag(address, typeCode, value, 1);
		}

		#endregion

		#region Read Write Override Async
#if !NET35 && !NET20
		/// <inheritdoc cref="Read(string, ushort)"/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			if (length > 1)
				return await ReadAsync(new string[] { address }, new int[] { 1 });
			else
				return await ReadAsync(new string[] { address }, new int[] { length });
		}

		/// <inheritdoc/>
		public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
		{
			OperateResult<byte[]> read = await ReadAsync(address, length);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			int strLen = ByteTransform.TransUInt16(read.Content, 0);
			return OperateResult.CreateSuccessResult(encoding.GetString(read.Content, 2, strLen));
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteAsync(string address, string value)
		{
			if (string.IsNullOrEmpty(value)) value = string.Empty;

			byte[] data = SoftBasic.SpliceArray<byte>(new byte[2], SoftBasic.ArrayExpandToLengthEven(Encoding.ASCII.GetBytes(value)));
			data[0] = BitConverter.GetBytes(data.Length - 2)[0];
			data[1] = BitConverter.GetBytes(data.Length - 2)[1];
			return await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_String, data, 1);
		}

		/// <inheritdoc/>
		public override async Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1)
		{
			return await base.WriteTagAsync(address, typeCode, value, 1);
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"OmronCipNet[{IpAddress}:{Port}]";

		#endregion
	}
}
