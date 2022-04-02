using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communication.Core;
using Communication;
using Communication.Core.Net;
using System.Net.Sockets;
using Communication.Core.IMessage;
using Communication.Core.Address;
using Communication.BasicFramework;
using Communication.Reflection;
using Communication.LogNet;
using RJCP.IO.Ports;

namespace Communication.ModBus
{
	/// <summary>
	/// <b>[商业授权]</b> Modbus的虚拟服务器，同时支持Tcp和Rtu的机制，支持线圈，离散输入，寄存器和输入寄存器的读写操作，同时支持掩码写入功能，可以用来当做系统的数据交换池<br />
	/// <b>[Authorization]</b> Modbus virtual server supports Tcp and Rtu mechanisms at the same time, supports read and write operations of coils, discrete inputs, r
	/// egisters and input registers, and supports mask write function, which can be used as a system data exchange pool
	/// </summary>
	/// <remarks>
	/// 可以基于本类实现一个功能复杂的modbus服务器，支持Modbus-Tcp，启动串口后，还支持Modbus-Rtu和Modbus-ASCII，会根据报文进行动态的适配。
	/// <list type="number">
	/// <item>线圈，功能码对应01，05，15</item>
	/// <item>离散输入，功能码对应02</item>
	/// <item>寄存器，功能码对应03，06，16</item>
	/// <item>输入寄存器，功能码对应04，输入寄存器在服务器端可以实现读写的操作</item>
	/// <item>掩码写入，功能码对应22，可以对字寄存器进行位操作</item>
	/// </list>
	/// </remarks>
	/// <example>
	/// 读写的地址格式为富文本地址，具体请参照下面的示例代码。
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Modbus\ModbusTcpServer.cs" region="ModbusTcpServerExample" title="ModbusTcpServer示例" />
	/// </example>
	public class ModbusTcpServer : NetworkDataServerBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个Modbus Tcp及Rtu的服务器，支持数据读写操作
		/// </summary>
		public ModbusTcpServer()
		{
			// 四个数据池初始化，线圈，输入线圈，寄存器，只读寄存器
			coilBuffer = new SoftBuffer(DataPoolLength);
			inputBuffer = new SoftBuffer(DataPoolLength);
			registerBuffer = new SoftBuffer(DataPoolLength * 2);
			inputRegisterBuffer = new SoftBuffer(DataPoolLength * 2);

			registerBuffer.IsBoolReverseByWord = true;
			inputRegisterBuffer.IsBoolReverseByWord = true;

			subscriptions = new List<ModBusMonitorAddress>();
			subcriptionHybirdLock = new SimpleHybirdLock();
			ByteTransform = new ReverseWordTransform();
			WordLength = 1;
			serialPort = new SerialPortStream();
		}

		#endregion

		#region Public Members

		/// <inheritdoc cref="ModbusTcpNet.DataFormat"/>
		public DataFormat DataFormat
		{
			get { return ByteTransform.DataFormat; }
			set { ByteTransform.DataFormat = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.IsStringReverse"/>
		public bool IsStringReverse
		{
			get { return ((ReverseWordTransform)ByteTransform).IsStringReverseByteWord; }
			set { ((ReverseWordTransform)ByteTransform).IsStringReverseByteWord = value; }
		}

		/// <inheritdoc cref="ModbusTcpNet.Station"/>
		public int Station
		{
			get { return station; }
			set { station = value; }
		}

		#endregion

		#region Data Persistence

		/// <inheritdoc/>
		protected override byte[] SaveToBytes()
		{
			byte[] buffer = new byte[DataPoolLength * 6];
			Array.Copy(coilBuffer.GetBytes(), 0, buffer, DataPoolLength * 0, DataPoolLength);
			Array.Copy(inputBuffer.GetBytes(), 0, buffer, DataPoolLength * 1, DataPoolLength);
			Array.Copy(registerBuffer.GetBytes(), 0, buffer, DataPoolLength * 2, DataPoolLength * 2);
			Array.Copy(inputRegisterBuffer.GetBytes(), 0, buffer, DataPoolLength * 4, DataPoolLength * 2);
			return buffer;
		}

		/// <inheritdoc/>
		protected override void LoadFromBytes(byte[] content)
		{
			if (content.Length < DataPoolLength * 6) throw new Exception("File is not correct");

			coilBuffer.SetBytes(content, DataPoolLength * 0, 0, DataPoolLength);
			inputBuffer.SetBytes(content, DataPoolLength * 1, 0, DataPoolLength);
			registerBuffer.SetBytes(content, DataPoolLength * 2, 0, DataPoolLength * 2);
			inputRegisterBuffer.SetBytes(content, DataPoolLength * 4, 0, DataPoolLength * 2);
		}

		#endregion

		#region Coil Read Write

		/// <summary>
		/// 读取地址的线圈的通断情况
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <returns><c>True</c>或是<c>False</c></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public bool ReadCoil(string address)
		{
			ushort add = ushort.Parse(address);
			return coilBuffer.GetByte(add) != 0x00;
		}

		/// <summary>
		/// 批量读取地址的线圈的通断情况
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <param name="length">读取长度</param>
		/// <returns><c>True</c>或是<c>False</c></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public bool[] ReadCoil(string address, ushort length)
		{
			ushort add = ushort.Parse(address);
			return coilBuffer.GetBytes(add, length).Select(m => m != 0x00).ToArray();
		}

		/// <summary>
		/// 写入线圈的通断值
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <param name="data">是否通断</param>
		/// <returns><c>True</c>或是<c>False</c></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public void WriteCoil(string address, bool data)
		{
			ushort add = ushort.Parse(address);
			coilBuffer.SetValue((byte)(data ? 0x01 : 0x00), add);
		}

		/// <summary>
		/// 写入线圈数组的通断值
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <param name="data">是否通断</param>
		/// <returns><c>True</c>或是<c>False</c></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public void WriteCoil(string address, bool[] data)
		{
			if (data == null) return;

			ushort add = ushort.Parse(address);
			coilBuffer.SetBytes(data.Select(m => (byte)(m ? 0x01 : 0x00)).ToArray(), add);
		}

		#endregion

		#region Discrete Read Write

		/// <summary>
		/// 读取地址的离散线圈的通断情况
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <returns><c>True</c>或是<c>False</c></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public bool ReadDiscrete(string address)
		{
			ushort add = ushort.Parse(address);
			return inputBuffer.GetByte(add) != 0x00;
		}

		/// <summary>
		/// 批量读取地址的离散线圈的通断情况
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <param name="length">读取长度</param>
		/// <returns><c>True</c>或是<c>False</c></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public bool[] ReadDiscrete(string address, ushort length)
		{
			ushort add = ushort.Parse(address);
			return inputBuffer.GetBytes(add, length).Select(m => m != 0x00).ToArray();
		}

		/// <summary>
		/// 写入离散线圈的通断值
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <param name="data">是否通断</param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public void WriteDiscrete(string address, bool data)
		{
			ushort add = ushort.Parse(address);
			inputBuffer.SetValue((byte)(data ? 0x01 : 0x00), add);
		}

		/// <summary>
		/// 写入离散线圈数组的通断值
		/// </summary>
		/// <param name="address">起始地址，示例："100"</param>
		/// <param name="data">是否通断</param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public void WriteDiscrete(string address, bool[] data)
		{
			if (data == null) return;

			ushort add = ushort.Parse(address);
			inputBuffer.SetBytes(data.Select(m => (byte)(m ? 0x01 : 0x00)).ToArray(), add);
		}

		#endregion

		#region NetworkDataServerBase Override

		/// <inheritdoc cref="ModbusTcpNet.Read(string, ushort)"/>
		[HslMqttApi("ReadByteArray", "")]
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, (byte)Station, true, ModbusInfo.ReadRegister);
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(analysis);

			if (analysis.Content.Function == ModbusInfo.ReadRegister)
				return OperateResult.CreateSuccessResult(registerBuffer.GetBytes(analysis.Content.Address * 2, length * 2));
			else if (analysis.Content.Function == ModbusInfo.ReadInputRegister)
				return OperateResult.CreateSuccessResult(inputRegisterBuffer.GetBytes(analysis.Content.Address * 2, length * 2));
			else
				return new OperateResult<byte[]>(StringResources.Language.NotSupportedDataType);
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, byte[])"/>
		[HslMqttApi("WriteByteArray", "")]
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, (byte)Station, true, ModbusInfo.ReadRegister);
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(analysis);

			if (analysis.Content.Function == ModbusInfo.ReadRegister)
			{
				registerBuffer.SetBytes(value, analysis.Content.Address * 2);
				return OperateResult.CreateSuccessResult();
			}
			else if (analysis.Content.Function == ModbusInfo.ReadInputRegister)
			{
				inputRegisterBuffer.SetBytes(value, analysis.Content.Address * 2);
				return OperateResult.CreateSuccessResult();
			}
			else
			{
				return new OperateResult<byte[]>(StringResources.Language.NotSupportedDataType);
			}
		}

		/// <inheritdoc cref="ModbusTcpNet.ReadBool(string, ushort)"/>
		[HslMqttApi("ReadBoolArray", "")]
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, (byte)Station, true, ModbusInfo.ReadCoil);
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<bool[]>(analysis);

			if (analysis.Content.Function == ModbusInfo.ReadCoil)
				return OperateResult.CreateSuccessResult(coilBuffer.GetBytes(analysis.Content.Address, length).Select(m => m != 0x00).ToArray());
			else if (analysis.Content.Function == ModbusInfo.ReadDiscrete)
				return OperateResult.CreateSuccessResult(inputBuffer.GetBytes(analysis.Content.Address, length).Select(m => m != 0x00).ToArray());
			else
				return new OperateResult<bool[]>(StringResources.Language.NotSupportedDataType);
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool[])"/>
		[HslMqttApi("WriteBoolArray", "")]
		public override OperateResult Write(string address, bool[] value)
		{
			OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, (byte)Station, true, ModbusInfo.ReadCoil);
			if (!analysis.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(analysis);

			if (analysis.Content.Function == ModbusInfo.ReadCoil)
			{
				coilBuffer.SetBytes(value.Select(m => (byte)(m ? 0x01 : 0x00)).ToArray(), analysis.Content.Address);
				return OperateResult.CreateSuccessResult();
			}
			else if (analysis.Content.Function == ModbusInfo.ReadDiscrete)
			{
				inputBuffer.SetBytes(value.Select(m => (byte)(m ? 0x01 : 0x00)).ToArray(), analysis.Content.Address);
				return OperateResult.CreateSuccessResult();
			}
			else
			{
				return new OperateResult<byte[]>(StringResources.Language.NotSupportedDataType);
			}
		}

		/// <inheritdoc cref="ModbusTcpNet.Write(string, bool)"/>
		[HslMqttApi("WriteBool", "")]
		public override OperateResult Write(string address, bool value)
		{
			if (address.IndexOf('.') < 0)
			{
				return base.Write(address, value);
			}
			else
			{
				try
				{
					int bitIndex = Convert.ToInt32(address.Substring(address.IndexOf('.') + 1));
					address = address.Substring(0, address.IndexOf('.'));

					OperateResult<ModbusAddress> analysis = ModbusInfo.AnalysisAddress(address, (byte)Station, true, ModbusInfo.ReadRegister);
					if (!analysis.IsSuccess) return analysis;

					bitIndex = analysis.Content.Address * 16 + bitIndex;
					if (analysis.Content.Function == ModbusInfo.ReadRegister)
					{
						registerBuffer.SetBool(value, bitIndex);
						return OperateResult.CreateSuccessResult();
					}
					else if (analysis.Content.Function == ModbusInfo.ReadInputRegister)
					{
						inputRegisterBuffer.SetBool(value, bitIndex);
						return OperateResult.CreateSuccessResult();
					}
					else
					{
						return new OperateResult(StringResources.Language.NotSupportedDataType);
					}
				}
				catch (Exception ex)
				{
					return new OperateResult(ex.Message);
				}
			}
		}

		/// <summary>
		/// 写入寄存器数据，指定字节数据
		/// </summary>
		/// <param name="address">起始地址，示例："100"，如果是输入寄存器："x=4;100"</param>
		/// <param name="high">高位数据</param>
		/// <param name="low">地位数据</param>
		public void Write(string address, byte high, byte low) => Write(address, new byte[] { high, low });

		#endregion

		#region NetServer Override

		/// <inheritdoc/>
		protected override void ThreadPoolLoginAfterClientCheck(Socket socket, System.Net.IPEndPoint endPoint)
		{
			// 开始接收数据信息
			AppSession appSession = new AppSession();
			appSession.IpEndPoint = endPoint;
			appSession.WorkSocket = socket;

			if (socket.BeginReceiveResult(SocketAsyncCallBack, appSession).IsSuccess)
				AddClient(appSession);
			else
				LogNet?.WriteDebug(ToString(), string.Format(StringResources.Language.ClientOfflineInfo, endPoint));

		}

		private void SocketAsyncCallBack(IAsyncResult ar)
		{
			if (ar.AsyncState is AppSession session)
			{
				if (!session.WorkSocket.EndReceiveResult(ar).IsSuccess) { RemoveClient(session); return; }

				OperateResult<byte[]> read1 = ReceiveByMessage(session.WorkSocket, 2000, new ModbusTcpMessage());
				if (!read1.IsSuccess) { RemoveClient(session); return; };

			

				if (!CheckModbusMessageLegal(read1.Content.RemoveBegin(6))) { RemoveClient(session); return; }

				LogNet?.WriteDebug(ToString(), $"Tcp {StringResources.Language.Receive}：{read1.Content.ToHexString(' ')}");

				ushort id = (ushort)(read1.Content[0] * 256 + read1.Content[1]);
				byte[] back = ModbusInfo.PackCommandToTcp(ReadFromModbusCore(read1.Content.RemoveBegin(6)), id);

				if (back == null) { RemoveClient(session); return; }
				if (!Send(session.WorkSocket, back).IsSuccess) { RemoveClient(session); return; }

				LogNet?.WriteDebug(ToString(), $"Tcp {StringResources.Language.Send}：{back.ToHexString(' ')}");

				session.HeartTime = DateTime.Now;
				RaiseDataReceived(read1.Content);
				if (!session.WorkSocket.BeginReceiveResult(SocketAsyncCallBack, session).IsSuccess) RemoveClient(session);
			}
		}

		#endregion

		#region Function Process Center

		/// <summary>
		/// 创建特殊的功能标识，然后返回该信息<br />
		/// Create a special feature ID and return this information
		/// </summary>
		/// <param name="modbusCore">modbus核心报文</param>
		/// <param name="error">错误码</param>
		/// <returns>携带错误码的modbus报文</returns>
		private byte[] CreateExceptionBack(byte[] modbusCore, byte error) => new byte[] { modbusCore[0], (byte)(modbusCore[1] + 0x80), error };

		/// <summary>
		/// 创建返回消息<br />
		/// Create return message
		/// </summary>
		/// <param name="modbusCore">modbus核心报文</param>
		/// <param name="content">返回的实际数据内容</param>
		/// <returns>携带内容的modbus报文</returns>
		private byte[] CreateReadBack(byte[] modbusCore, byte[] content) => SoftBasic.SpliceByteArray(new byte[] { modbusCore[0], modbusCore[1], (byte)content.Length }, content);

		/// <summary>
		/// 创建写入成功的反馈信号<br />
		/// Create feedback signal for successful write
		/// </summary>
		/// <param name="modbus">modbus核心报文</param>
		/// <returns>携带成功写入的信息</returns>
		private byte[] CreateWriteBack(byte[] modbus) => modbus.SelectBegin(6);

		private byte[] ReadCoilBack(byte[] modbus, string addressHead)
		{
			try
			{
				ushort address = ByteTransform.TransUInt16(modbus, 2);
				ushort length = ByteTransform.TransUInt16(modbus, 4);

				// 越界检测
				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeOverBound);

				// 地址长度检测
				if (length > 2040) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeQuantityOver);

				bool[] read = ReadBool(addressHead + address.ToString(), length).Content;
				return CreateReadBack(modbus, SoftBasic.BoolArrayToByte(read));
			}
			catch (Exception ex)
			{
				LogNet?.WriteException(ToString(), StringResources.Language.ModbusTcpReadCoilException, ex);
				return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeReadWriteException);
			}
		}

		private byte[] ReadRegisterBack(byte[] modbus, string addressHead)
		{
			try
			{
				ushort address = ByteTransform.TransUInt16(modbus, 2);
				ushort length = ByteTransform.TransUInt16(modbus, 4);

				// 越界检测
				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeOverBound);

				// 地址长度检测
				if (length > 127) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeQuantityOver);

				byte[] buffer = Read(addressHead + address.ToString(), length).Content;
				return CreateReadBack(modbus, buffer);
			}
			catch (Exception ex)
			{
				LogNet?.WriteException(ToString(), StringResources.Language.ModbusTcpReadRegisterException, ex);
				return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeReadWriteException);
			}
		}

		private byte[] WriteOneCoilBack(byte[] modbus)
		{
			try
			{
				ushort address = ByteTransform.TransUInt16(modbus, 2);

				if (modbus[4] == 0xFF && modbus[5] == 0x00) Write(address.ToString(), true);
				else if (modbus[4] == 0x00 && modbus[5] == 0x00) Write(address.ToString(), false);

				return CreateWriteBack(modbus);
			}
			catch (Exception ex)
			{
				LogNet?.WriteException(ToString(), StringResources.Language.ModbusTcpWriteCoilException, ex);
				return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeReadWriteException);
			}
		}

		private byte[] WriteOneRegisterBack(byte[] modbus)
		{
			try
			{
				ushort address = ByteTransform.TransUInt16(modbus, 2);
				short ValueOld = ReadInt16(address.ToString()).Content;

				Write(address.ToString(), modbus[4], modbus[5]);
				short ValueNew = ReadInt16(address.ToString()).Content;

				OnRegisterBeforWrite(address, ValueOld, ValueNew);
				return CreateWriteBack(modbus);
			}
			catch (Exception ex)
			{
				LogNet?.WriteException(ToString(), StringResources.Language.ModbusTcpWriteRegisterException, ex);
				return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeReadWriteException);
			}
		}

		private byte[] WriteCoilsBack(byte[] modbus)
		{
			try
			{
				ushort address = ByteTransform.TransUInt16(modbus, 2);
				ushort length = ByteTransform.TransUInt16(modbus, 4);

				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeOverBound);

				if (length > 2040) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeQuantityOver);

				Write(address.ToString(), modbus.RemoveBegin(7).ToBoolArray(length));
				return CreateWriteBack(modbus);
			}
			catch (Exception ex)
			{
				LogNet?.WriteException(ToString(), StringResources.Language.ModbusTcpWriteCoilException, ex);
				return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeReadWriteException);
			}
		}

		private byte[] WriteRegisterBack(byte[] modbus)
		{
			try
			{
				ushort address = ByteTransform.TransUInt16(modbus, 2);
				ushort length = ByteTransform.TransUInt16(modbus, 4);

				if ((address + length) > ushort.MaxValue + 1) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeOverBound);

				if (length > 127) return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeQuantityOver);

				// 为了使服务器的数据订阅更加的准确，决定将设计改为等待所有的数据写入完成后，再统一触发订阅，2018年3月4日 20:56:47
				MonitorAddress[] addresses = new MonitorAddress[length];
				for (ushort i = 0; i < length; i++)
				{
					short ValueOld = ReadInt16((address + i).ToString()).Content;
					Write((address + i).ToString(), modbus[2 * i + 7], modbus[2 * i + 8]);
					short ValueNew = ReadInt16((address + i).ToString()).Content;

					// 触发写入请求
					addresses[i] = new MonitorAddress()
					{
						Address = (ushort)(address + i),
						ValueOrigin = ValueOld,
						ValueNew = ValueNew
					};
				}

				// 所有数据都更改完成后，再触发消息
				for (int i = 0; i < addresses.Length; i++)
				{
					OnRegisterBeforWrite(addresses[i].Address, addresses[i].ValueOrigin, addresses[i].ValueNew);
				}

				return CreateWriteBack(modbus);
			}
			catch (Exception ex)
			{
				LogNet?.WriteException(ToString(), StringResources.Language.ModbusTcpWriteRegisterException, ex);
				return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeReadWriteException);
			}
		}

		private byte[] WriteMaskRegisterBack(byte[] modbus)
		{
			try
			{
				ushort address = ByteTransform.TransUInt16(modbus, 2);
				int and_Mask = ByteTransform.TransUInt16(modbus, 4);
				int or_Mask = ByteTransform.TransUInt16(modbus, 6);

				int ValueOld = ReadInt16(address.ToString()).Content;
				short ValueNew = (short)((ValueOld & and_Mask) | or_Mask);
				Write(address.ToString(), ValueNew);

				// 触发写入请求
				MonitorAddress addresses = new MonitorAddress()
				{
					Address = address,
					ValueOrigin = (short)ValueOld,
					ValueNew = ValueNew
				};

				// 所有数据都更改完成后，再触发消息
				OnRegisterBeforWrite(addresses.Address, addresses.ValueOrigin, addresses.ValueNew);

				return modbus;
			}
			catch (Exception ex)
			{
				LogNet?.WriteException(ToString(), StringResources.Language.ModbusTcpWriteRegisterException, ex);
				return CreateExceptionBack(modbus, ModbusInfo.FunctionCodeReadWriteException);
			}
		}


		#endregion

		#region Subscription Support

		// 本服务器端支持指定地址的数据订阅器，目前仅支持寄存器操作

		private List<ModBusMonitorAddress> subscriptions;     // 数据订阅集合
		private SimpleHybirdLock subcriptionHybirdLock;       // 集合锁

		/// <summary>
		/// 新增一个数据监视的任务，针对的是寄存器地址的数据<br />
		/// Added a data monitoring task for data at register addresses
		/// </summary>
		/// <param name="monitor">监视地址对象</param>
		public void AddSubcription(ModBusMonitorAddress monitor)
		{
			subcriptionHybirdLock.Enter();
			subscriptions.Add(monitor);
			subcriptionHybirdLock.Leave();
		}

		/// <summary>
		/// 移除一个数据监视的任务<br />
		/// Remove a data monitoring task
		/// </summary>
		/// <param name="monitor">监视地址对象</param>
		public void RemoveSubcrption(ModBusMonitorAddress monitor)
		{
			subcriptionHybirdLock.Enter();
			subscriptions.Remove(monitor);
			subcriptionHybirdLock.Leave();
		}

		/// <summary>
		/// 在数据变更后，进行触发是否产生订阅<br />
		/// Whether to generate a subscription after triggering data changes
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="before">修改之前的数</param>
		/// <param name="after">修改之后的数</param>
		private void OnRegisterBeforWrite(ushort address, short before, short after)
		{
			subcriptionHybirdLock.Enter();
			for (int i = 0; i < subscriptions.Count; i++)
			{
				if (subscriptions[i].Address == address)
				{
					subscriptions[i].SetValue(after);
					if (before != after)
					{
						subscriptions[i].SetChangeValue(before, after);
					}
				}
			}
			subcriptionHybirdLock.Leave();
		}

		#endregion

		#region Modbus Core Logic

		/// <summary>
		/// 检测当前的Modbus接收的指定是否是合法的<br />
		/// Check if the current Modbus received designation is valid
		/// </summary>
		/// <param name="buffer">缓存数据</param>
		/// <returns>是否合格</returns>
		private bool CheckModbusMessageLegal(byte[] buffer)
		{
			bool check = false;
			switch (buffer[1])
			{
				case ModbusInfo.ReadCoil:
				case ModbusInfo.ReadDiscrete:
				case ModbusInfo.ReadRegister:
				case ModbusInfo.ReadInputRegister:
				case ModbusInfo.WriteOneCoil:
				case ModbusInfo.WriteOneRegister: check = buffer.Length == 0x06; break;
				case ModbusInfo.WriteCoil:
				case ModbusInfo.WriteRegister: check = buffer.Length > 6 && (buffer[6] == (buffer.Length - 7)); break;
				case ModbusInfo.WriteMaskRegister: check = buffer.Length == 0x08; break;
				default: check = true; break;
			}
			if (check == false) LogNet?.WriteError(ToString(), $"Receive Nosense Modbus-rtu : " + buffer.ToHexString(' '));
			return check;
		}

		/// <summary>
		/// Modbus核心数据交互方法，允许重写自己来实现，报文只剩下核心的Modbus信息，去除了MPAB报头信息<br />
		/// The Modbus core data interaction method allows you to rewrite it to achieve the message. 
		/// Only the core Modbus information is left in the message, and the MPAB header information is removed.
		/// </summary>
		/// <param name="modbusCore">核心的Modbus报文</param>
		/// <returns>进行数据交互之后的结果</returns>
		protected virtual byte[] ReadFromModbusCore(byte[] modbusCore)
		{
			byte[] buffer;
			switch (modbusCore[1])
			{
				case ModbusInfo.ReadCoil: buffer = ReadCoilBack(modbusCore, string.Empty); break;
				case ModbusInfo.ReadDiscrete: buffer = ReadCoilBack(modbusCore, "x=2;"); break;
				case ModbusInfo.ReadRegister: buffer = ReadRegisterBack(modbusCore, string.Empty); break;
				case ModbusInfo.ReadInputRegister: buffer = ReadRegisterBack(modbusCore, "x=4;"); break;
				case ModbusInfo.WriteOneCoil: buffer = WriteOneCoilBack(modbusCore); break;
				case ModbusInfo.WriteOneRegister: buffer = WriteOneRegisterBack(modbusCore); break;
				case ModbusInfo.WriteCoil: buffer = WriteCoilsBack(modbusCore); break;
				case ModbusInfo.WriteRegister: buffer = WriteRegisterBack(modbusCore); break;
				case ModbusInfo.WriteMaskRegister: buffer = WriteMaskRegisterBack(modbusCore); break;
				default: buffer = CreateExceptionBack(modbusCore, ModbusInfo.FunctionCodeNotSupport); break;
			}

			return buffer;
		}

		#endregion

		#region Serial Support

		private SerialPortStream serialPort;            // 核心的串口对象

		/// <summary>
		/// 启动modbus-rtu的从机服务，使用默认的参数进行初始化串口，9600波特率，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of modbus-rtu, initialize the serial port with default parameters, 9600 baud rate, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		public void StartModbusRtu(string com) => StartModbusRtu(com, 9600);

		/// <summary>
		/// 启动modbus-rtu的从机服务，使用默认的参数进行初始化串口，8位数据位，无奇偶校验，1位停止位<br />
		/// Start the slave service of modbus-rtu, initialize the serial port with default parameters, 8 data bits, no parity, 1 stop bit
		/// </summary>
		/// <param name="com">串口信息</param>
		/// <param name="baudRate">波特率</param>
		public void StartModbusRtu(string com, int baudRate)
		{
			StartModbusRtu(sp =>
			{
				sp.PortName = com;
				sp.BaudRate = baudRate;
				sp.DataBits = 8;
				sp.Parity = Parity.None;
				sp.StopBits = StopBits.One;
			});
		}

		/// <summary>
		/// 启动modbus-rtu的从机服务，使用自定义的初始化方法初始化串口的参数<br />
		/// Start the slave service of modbus-rtu and initialize the parameters of the serial port using a custom initialization method
		/// </summary>
		/// <param name="inni">初始化信息的委托</param>
		public void StartModbusRtu(Action<SerialPortStream> inni)
		{
			if (!serialPort.IsOpen)
			{
				inni?.Invoke(serialPort);

				serialPort.ReadBufferSize = 1024;
				serialPort.ReceivedBytesThreshold = 1;
				serialPort.Open();
				serialPort.DataReceived += SerialPort_DataReceived;
			}
		}

		/// <summary>
		/// 关闭modbus-rtu的串口对象<br />
		/// Close the serial port object of modbus-rtu
		/// </summary>
		public void CloseModbusRtu()
		{
			if (serialPort.IsOpen)
			{
				serialPort.Close();
			}
		}

		/// <summary>
		/// 接收到串口数据的时候触发
		/// </summary>
		/// <param name="sender">串口对象</param>
		/// <param name="e">消息</param>
		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			int rCount = 0;
			byte[] buffer = new byte[1024];
			while (true)
			{
				System.Threading.Thread.Sleep(20);            // 此处做个微小的延时，等待数据接收完成
				int count = serialPort.Read(buffer, rCount, serialPort.BytesToRead);
				rCount += count;
				if (count == 0) break;
			}

			if (rCount == 0) return;
			byte[] receive = buffer.SelectBegin(rCount);
			if (receive.Length < 3)
			{
				LogNet?.WriteError(ToString(), $"Uknown Data：{receive.ToHexString(' ')}");
				return;
			}

			if (receive[0] != 0x3A)
			{
				LogNet?.WriteDebug(ToString(), $"Rtu {StringResources.Language.Receive}：{receive.ToHexString(' ')}");

				if (Serial.SoftCRC16.CheckCRC16(receive))
				{
					byte[] modbusCore = receive.RemoveLast(2);

					// 指令长度验证错误，关闭网络连接
					if (!CheckModbusMessageLegal(modbusCore)) return;

					// 验证站号是否一致
					if (station >= 0 && station != modbusCore[0])
					{
						LogNet?.WriteError(ToString(), $"Station not match Modbus-rtu : {receive.ToHexString(' ')}");
						return;
					}

					// 需要回发消息
					byte[] back = ModbusInfo.PackCommandToRtu(ReadFromModbusCore(modbusCore));

					serialPort.Write(back, 0, back.Length);

					LogNet?.WriteDebug(ToString(), $"Rtu {StringResources.Language.Send}：{back.ToHexString(' ')}");
					if (IsStarted) RaiseDataReceived(receive);
				}
				else
				{
					LogNet?.WriteWarn($"CRC Check Failed : {receive.ToHexString(' ')}");
				}
			}
			else
			{
				LogNet?.WriteDebug(ToString(), $"Ascii {StringResources.Language.Receive}：{Encoding.ASCII.GetString(receive.RemoveLast(2))}");

				OperateResult<byte[]> ascii = ModbusInfo.TransAsciiPackCommandToCore(receive);
				if (!ascii.IsSuccess)
				{
					LogNet?.WriteError(ToString(), ascii.Message);
					return;
				}
				byte[] modbusCore = ascii.Content;
				// 指令长度验证错误，关闭网络连接
				if (!CheckModbusMessageLegal(modbusCore)) return;

				// 验证站号是否一致
				if (station >= 0 && station != modbusCore[0])
				{
					LogNet?.WriteError(ToString(), $"Station not match Modbus-Ascii : {Encoding.ASCII.GetString(receive.RemoveLast(2))}");
					return;
				}

				// 需要回发消息
				byte[] back = ModbusInfo.TransModbusCoreToAsciiPackCommand(ReadFromModbusCore(modbusCore));

				serialPort.Write(back, 0, back.Length);

				LogNet?.WriteDebug(ToString(), $"Ascii {StringResources.Language.Send}：{Encoding.ASCII.GetString(back.RemoveLast(2))}");
				if (IsStarted) RaiseDataReceived(receive);
			}
		}

		#endregion

		#region IDisposable Support

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				subcriptionHybirdLock?.Dispose();
				subscriptions?.Clear();
				coilBuffer?.Dispose();
				inputBuffer?.Dispose();
				registerBuffer?.Dispose();
				inputRegisterBuffer?.Dispose();
				serialPort?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Private Member

		private SoftBuffer coilBuffer;                // 线圈的数据池
		private SoftBuffer inputBuffer;               // 离散输入的数据池
		private SoftBuffer registerBuffer;            // 寄存器的数据池
		private SoftBuffer inputRegisterBuffer;       // 输入寄存器的数据池

		private const int DataPoolLength = 65536;     // 数据的长度
		private int station = 1;                      // 服务器的站号数据，对于tcp无效，对于rtu来说，如果小于0，则忽略站号信息

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"ModbusTcpServer[{Port}]";

		#endregion
	}
}
