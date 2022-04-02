using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.IMessage;
using Communication.Core.Net;
using Communication.Profinet.AllenBradley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.Profinet.Omron
{
	/// <summary>
	/// 基于连接的对象访问的CIP协议的实现
	/// </summary>
	public class OmronConnectedCipNet : NetworkDeviceBase
	{
		#region Contructor

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public OmronConnectedCipNet()
		{
			WordLength = 2;
			ByteTransform = new RegularByteTransform();
		}

		/// <summary>
		/// 根据指定的IP及端口来实例化这个连接对象
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口号信息</param>
		public OmronConnectedCipNet(string ipAddress, int port = 44818)
		{
			IpAddress = ipAddress;
			Port = port;
			WordLength = 2;
			ByteTransform = new RegularByteTransform();
		}

		/// <inheritdoc/>
		protected override INetMessage GetNewNetMessage() => new AllenBradleyMessage();

		#endregion

		#region Public Properties

		/// <inheritdoc cref="AllenBradleyNet.SessionHandle"/>
		public uint SessionHandle { get; protected set; }

		/// <inheritdoc cref="AllenBradleyNet.Slot"/>
		public byte Slot { get; set; } = 0;

		#endregion

		#region Double Mode Override

		/// <inheritdoc/>
		protected override OperateResult InitializationOnConnect(Socket socket)
		{
			// Registering Session Information
			OperateResult<byte[]> read = ReadFromCoreServer(socket, AllenBradleyHelper.RegisterSessionHandle());
			if (!read.IsSuccess) return read;

			// Check the returned status
			OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
			if (!check.IsSuccess) return check;

			// Extract session ID
			SessionHandle = ByteTransform.TransUInt32(read.Content, 4);

			// get attributes
			OperateResult<byte[]> read2 = ReadFromCoreServer(socket, AllenBradleyHelper.PackCommandGetAttributesAll(new byte[] { 0x01, Slot }, SessionHandle));
			if (!read2.IsSuccess) return read2;

			// 检查反馈 -> Check Feedback
			OperateResult check2 = AllenBradleyHelper.CheckResponse(read2.Content);
			if (!check2.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check2);

			// large forward open
			byte[] send = AllenBradleyHelper.PackRequestHeader(0x6f, SessionHandle,
				("0000" +
				"00000000020000000000b20036005b02" +
				"2006240106ea020000800100fe800200" +
				"1b0560e8a9050200000080841e00cc07" +
				"004280841e00cc070042a30401002002" +
				"24012c01").ToHexBytes());
			OperateResult<byte[]> forward = ReadFromCoreServer(socket, send);
			if (!forward.IsSuccess) return read;

			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc/>
		protected override OperateResult ExtraOnDisconnect(Socket socket)
		{
			// Unregister session Information
			OperateResult<byte[]> read = ReadFromCoreServer(socket, AllenBradleyHelper.UnRegisterSessionHandle(SessionHandle));
			if (!read.IsSuccess) return read;

			return OperateResult.CreateSuccessResult();
		}

		#endregion

		#region Async Double Mode Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		protected override async Task<OperateResult> InitializationOnConnectAsync(Socket socket)
		{
			// Registering Session Information
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.RegisterSessionHandle());
			if (!read.IsSuccess) return read;

			// Check the returned status
			OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
			if (!check.IsSuccess) return check;

			// Extract session ID
			SessionHandle = ByteTransform.TransUInt32(read.Content, 4);

			// get attributes
			OperateResult<byte[]> read2 = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.PackCommandGetAttributesAll(new byte[] { 0x01, Slot }, SessionHandle));
			if (!read2.IsSuccess) return read2;

			// 检查反馈 -> Check Feedback
			OperateResult check2 = AllenBradleyHelper.CheckResponse(read2.Content);
			if (!check2.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check2);

			// large forward open
			byte[] send = AllenBradleyHelper.PackRequestHeader(0x6f, SessionHandle, "000000000000020000000000b20036005b022006240106ea020000800100fe8002001b0560e8a9050200000080841e00cc07004280841e00cc070042a3040100200224012c01".ToHexBytes());
			OperateResult<byte[]> forward = await ReadFromCoreServerAsync(socket, send);
			if (!forward.IsSuccess) return forward;

			return OperateResult.CreateSuccessResult();
		}

		/// <inheritdoc/>
		protected override async Task<OperateResult> ExtraOnDisconnectAsync(Socket socket)
		{
			// Unregister session Information
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(socket, AllenBradleyHelper.UnRegisterSessionHandle(SessionHandle));
			if (!read.IsSuccess) return read;

			return OperateResult.CreateSuccessResult();
		}

#endif
		#endregion

		private OperateResult<byte[]> BuildReadCommand(string address, int length)
		{
			try
			{
				byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(
				"a1 00 04 00 41 01 85 53".ToHexBytes(), PackCommandService(AllenBradleyHelper.PackRequsetRead(address, length, true)));

				return OperateResult.CreateSuccessResult(AllenBradleyHelper.PackRequestHeader(0x70, SessionHandle, commandSpecificData));
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
			}
		}

		private OperateResult<byte[]> BuildWriteCommand(string address, ushort typeCode, byte[] data, int length = 1)
		{
			try
			{
				byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData("a1 00 04 00 41 01 85 53".ToHexBytes(),
					PackCommandService(AllenBradleyHelper.PackRequestWrite(address, typeCode, data, length, true)));

				return OperateResult.CreateSuccessResult(AllenBradleyHelper.PackRequestHeader(0x70, SessionHandle, commandSpecificData));
			}
			catch (Exception ex)
			{
				return new OperateResult<byte[]>("Address Wrong:" + ex.Message);
			}
		}

		private byte[] PackCommandService(byte[] cip)
		{
			MemoryStream ms = new MemoryStream();
			// type id   0xB2:UnConnected Data Item  0xB1:Connected Data Item  0xA1:Connect Address Item
			ms.WriteByte(0xB1);
			ms.WriteByte(0x00);
			ms.WriteByte(0x00);     // 后续数据的长度
			ms.WriteByte(0x00);

			ms.WriteByte(0x15);     // ?? 这个数据一直在变化，不知道含义
			ms.WriteByte(0x00);

			ms.Write(cip, 0, cip.Length);

			byte[] data = ms.ToArray();
			ms.Dispose();
			BitConverter.GetBytes((short)(data.Length - 4)).CopyTo(data, 2);
			return data;
		}

		private OperateResult<byte[], ushort, bool> ReadWithType(string address, int length)
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>(command); ;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>(read); ;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>(check);

			// 提取数据 -> Extracting data
			return ExtractActualData(read.Content, true);
		}
#if !NET35 && !NET20
		private async Task<OperateResult<byte[], ushort, bool>> ReadWithTypeAsync(string address, int length)
		{
			// 指令生成 -> Instruction Generation
			OperateResult<byte[]> command = BuildReadCommand(address, length);
			if (!command.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>(command); ;

			// 核心交互 -> Core Interactions
			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>(read); ;

			// 检查反馈 -> Check Feedback
			OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[], ushort, bool>(check);

			// 提取数据 -> Extracting data
			return ExtractActualData(read.Content, true);
		}
#endif
		/// <inheritdoc/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<byte[], ushort, bool> read = ReadWithType(address, length);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			return OperateResult.CreateSuccessResult(read.Content1);
		}
#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			OperateResult<byte[], ushort, bool> read = await ReadWithTypeAsync(address, length);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(read);

			return OperateResult.CreateSuccessResult(read.Content1);
		}
#endif

		#region Write Support

		/// <summary>
		/// 当前的PLC不支持该功能，需要调用 <see cref="WriteTag(string, ushort, byte[], int)"/> 方法来实现。<br />
		/// The current PLC does not support this function, you need to call the <see cref = "WriteTag (string, ushort, byte [], int)" /> method to achieve it.
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="value">值</param>
		/// <returns>写入结果值</returns>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value) => new OperateResult(StringResources.Language.NotSupportedFunction + " Please refer to use WriteTag instead ");

		/// <summary>
		/// 使用指定的类型写入指定的节点数据<br />
		/// Writes the specified node data with the specified type
		/// </summary>
		/// <param name="address">节点的名称 -> Name of the node </param>
		/// <param name="typeCode">类型代码，详细参见<see cref="AllenBradleyHelper"/>上的常用字段 ->  Type code, see the commonly used Fields section on the <see cref= "AllenBradleyHelper"/> in detail</param>
		/// <param name="value">实际的数据值 -> The actual data value </param>
		/// <param name="length">如果节点是数组，就是数组长度 -> If the node is an array, it is the array length </param>
		/// <returns>是否写入成功 -> Whether to write successfully</returns>
		public virtual OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
		{
			OperateResult<byte[]> command = BuildWriteCommand(address, typeCode, value, length);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
			if (!read.IsSuccess) return read;

			OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			return AllenBradleyHelper.ExtractActualData(read.Content, false);
		}

		#endregion

		#region Async Write Support
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, byte[])"/>
		public override async Task<OperateResult> WriteAsync(string address, byte[] value) => await Task.Run(() => Write(address, value));

		/// <inheritdoc cref="WriteTag(string, ushort, byte[], int)"/>
		public virtual async Task<OperateResult> WriteTagAsync(string address, ushort typeCode, byte[] value, int length = 1)
		{
			OperateResult<byte[]> command = BuildWriteCommand(address, typeCode, value, length);
			if (!command.IsSuccess) return command;

			OperateResult<byte[]> read = await ReadFromCoreServerAsync(command.Content);
			if (!read.IsSuccess) return read;

			OperateResult check = AllenBradleyHelper.CheckResponse(read.Content);
			if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

			return AllenBradleyHelper.ExtractActualData(read.Content, false);
		}
#endif
		#endregion

		#region Device Override

		/// <inheritdoc/>
		[HslMqttApi("ReadInt16Array", "")]
		public override OperateResult<short[]> ReadInt16(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransInt16(m, 0, length));

		/// <inheritdoc/>
		[HslMqttApi("ReadUInt16Array", "")]
		public override OperateResult<ushort[]> ReadUInt16(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransUInt16(m, 0, length));

		/// <inheritdoc/>
		[HslMqttApi("ReadInt32Array", "")]
		public override OperateResult<int[]> ReadInt32(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransInt32(m, 0, length));

		/// <inheritdoc/>
		[HslMqttApi("ReadUInt32Array", "")]
		public override OperateResult<uint[]> ReadUInt32(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransUInt32(m, 0, length));

		/// <inheritdoc/>
		[HslMqttApi("ReadFloatArray", "")]
		public override OperateResult<float[]> ReadFloat(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransSingle(m, 0, length));

		/// <inheritdoc/>
		[HslMqttApi("ReadInt64Array", "")]
		public override OperateResult<long[]> ReadInt64(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransInt64(m, 0, length));

		/// <inheritdoc/>
		[HslMqttApi("ReadUInt64Array", "")]
		public override OperateResult<ulong[]> ReadUInt64(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransUInt64(m, 0, length));

		/// <inheritdoc/>
		[HslMqttApi("ReadDoubleArray", "")]
		public override OperateResult<double[]> ReadDouble(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(Read(address, length), m => ByteTransform.TransDouble(m, 0, length));

		///<inheritdoc/>
		public OperateResult<string> ReadString(string address) => ReadString(address, 1, Encoding.ASCII);

		/// <inheritdoc/>
		public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
		{
			OperateResult<byte[]> read = Read(address, length);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			if (read.Content.Length >= 6)
			{
				int strLength = ByteTransform.TransInt32(read.Content, 2);
				return OperateResult.CreateSuccessResult(encoding.GetString(read.Content, 6, strLength));
			}
			else
			{
				return OperateResult.CreateSuccessResult(encoding.GetString(read.Content));
			}
		}

		#endregion

		#region Async Device Override
#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransInt16(m, 0, length));

		/// <inheritdoc/>
		public override async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransUInt16(m, 0, length));

		/// <inheritdoc/>
		public override async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransInt32(m, 0, length));

		/// <inheritdoc/>
		public override async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransUInt32(m, 0, length));

		/// <inheritdoc/>
		public override async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransSingle(m, 0, length));

		/// <inheritdoc/>
		public override async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransInt64(m, 0, length));

		/// <inheritdoc/>
		public override async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransUInt64(m, 0, length));

		/// <inheritdoc/>
		public override async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length) => ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), m => ByteTransform.TransDouble(m, 0, length));

		/// <inheritdoc/>
		public async Task<OperateResult<string>> ReadStringAsync(string address) => await ReadStringAsync(address, 1, Encoding.ASCII);

		/// <inheritdoc/>
		public override async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
		{
			OperateResult<byte[]> read = await ReadAsync(address, length);
			if (!read.IsSuccess) return OperateResult.CreateFailedResult<string>(read);

			if (read.Content.Length >= 6)
			{
				int strLength = ByteTransform.TransInt32(read.Content, 2);
				return OperateResult.CreateSuccessResult(encoding.GetString(read.Content, 6, strLength));
			}
			else
			{
				return OperateResult.CreateSuccessResult(encoding.GetString(read.Content));
			}
		}
#endif
		#endregion

		#region Write Override

		/// <inheritdoc/>
		[HslMqttApi("WriteInt16Array", "")]
		public override OperateResult Write(string address, short[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteUInt16Array", "")]
		public override OperateResult Write(string address, ushort[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteInt32Array", "")]
		public override OperateResult Write(string address, int[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteUInt32Array", "")]
		public override OperateResult Write(string address, uint[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteFloatArray", "")]
		public override OperateResult Write(string address, float[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_Real, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteInt64Array", "")]
		public override OperateResult Write(string address, long[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteUInt64Array", "")]
		public override OperateResult Write(string address, ulong[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteDoubleArray", "")]
		public override OperateResult Write(string address, double[] values) => WriteTag(address, AllenBradleyHelper.CIP_Type_Double, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc/>
		[HslMqttApi("WriteString", "")]
		public override OperateResult Write(string address, string value)
		{
			if (string.IsNullOrEmpty(value)) value = string.Empty;

			byte[] data = Encoding.ASCII.GetBytes(value);
			OperateResult write = Write($"{address}.LEN", data.Length);
			if (!write.IsSuccess) return write;

			byte[] buffer = SoftBasic.ArrayExpandToLengthEven(data);
			return WriteTag($"{address}.DATA[0]", AllenBradleyHelper.CIP_Type_Byte, buffer, data.Length);
		}

		/// <inheritdoc/>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value) => WriteTag(address, AllenBradleyHelper.CIP_Type_Bool, value ? new byte[] { 0xFF, 0xFF } : new byte[] { 0x00, 0x00 });

		/// <inheritdoc/>
		[HslMqttApi("WriteByte", "")]
		public OperateResult Write(string address, byte value) => WriteTag(address, AllenBradleyHelper.CIP_Type_Byte, new byte[] { value, 0x00 });

		#endregion

		#region Async Write Override
#if !NET35 && !NET20
		/// <inheritdoc cref="Write(string, short[])"/>
		public override async Task<OperateResult> WriteAsync(string address, short[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, ushort[])"/>
		public override async Task<OperateResult> WriteAsync(string address, ushort[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_Word, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, int[])"/>
		public override async Task<OperateResult> WriteAsync(string address, int[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, uint[])"/>
		public override async Task<OperateResult> WriteAsync(string address, uint[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_DWord, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, float[])"/>
		public override async Task<OperateResult> WriteAsync(string address, float[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_Real, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, long[])"/>
		public override async Task<OperateResult> WriteAsync(string address, long[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, ulong[])"/>
		public override async Task<OperateResult> WriteAsync(string address, ulong[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_LInt, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, double[])"/>
		public override async Task<OperateResult> WriteAsync(string address, double[] values) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_Double, ByteTransform.TransByte(values), values.Length);

		/// <inheritdoc cref="Write(string, string)"/>
		public override async Task<OperateResult> WriteAsync(string address, string value)
		{
			if (string.IsNullOrEmpty(value)) value = string.Empty;

			byte[] data = Encoding.ASCII.GetBytes(value);
			OperateResult write = await WriteAsync($"{address}.LEN", data.Length);
			if (!write.IsSuccess) return write;

			byte[] buffer = SoftBasic.ArrayExpandToLengthEven(data);
			return await WriteTagAsync($"{address}.DATA[0]", AllenBradleyHelper.CIP_Type_Byte, buffer, data.Length);
		}

		/// <inheritdoc cref="Write(string, bool)"/>
		public override async Task<OperateResult> WriteAsync(string address, bool value) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_Bool, value ? new byte[] { 0xFF, 0xFF } : new byte[] { 0x00, 0x00 });

		/// <inheritdoc cref="Write(string, byte)"/>
		public async Task<OperateResult> WriteAsync(string address, byte value) => await WriteTagAsync(address, AllenBradleyHelper.CIP_Type_Byte, new byte[] { value, 0x00 });
#endif
		#endregion


		/// <summary>
		/// 从PLC反馈的数据解析
		/// </summary>
		/// <param name="response">PLC的反馈数据</param>
		/// <param name="isRead">是否是返回的操作</param>
		/// <returns>带有结果标识的最终数据</returns>
		public static OperateResult<byte[], ushort, bool> ExtractActualData(byte[] response, bool isRead)
		{
			List<byte> data = new List<byte>();

			int offset = 42;
			bool hasMoreData = false;
			ushort dataType = 0;
			ushort count = BitConverter.ToUInt16(response, offset);    // 剩余总字节长度，在剩余的字节里，有可能是一项数据，也有可能是多项
			byte err = response[offset + 6];
			switch (err)
			{
				case 0x04: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.AllenBradley04 };
				case 0x05: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.AllenBradley05 };
				case 0x06: hasMoreData = true; break;
				case 0x0A: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.AllenBradley0A };
				case 0x13: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.AllenBradley13 };
				case 0x1C: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.AllenBradley1C };
				case 0x1E: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.AllenBradley1E };
				case 0x26: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.AllenBradley26 };
				case 0x00: break;
				default: return new OperateResult<byte[], ushort, bool>() { ErrorCode = err, Message = StringResources.Language.UnknownError };
			}

			if (response[offset + 4] == 0xCD || response[offset + 4] == 0xD3) return OperateResult.CreateSuccessResult(data.ToArray(), dataType, hasMoreData);

			if (response[offset + 4] == 0xCC || response[offset + 4] == 0xD2)
			{
				for (int i = offset + 10; i < offset + 2 + count; i++)
				{
					data.Add(response[i]);
				}
				dataType = BitConverter.ToUInt16(response, offset + 8);
			}
			else if (response[offset + 4] == 0xD5)
			{
				for (int i = offset + 8; i < offset + 2 + count; i++)
				{
					data.Add(response[i]);
				}
			}

			return OperateResult.CreateSuccessResult(data.ToArray(), dataType, hasMoreData);
		}
	}
}
