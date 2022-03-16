using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Communication.Core;
using Communication.LogNet;

namespace Communication.Serial
{
	/// <summary>
	/// 所有串行通信类的基类，提供了一些基础的服务，核心的通信实现<br />
	/// The base class of all serial communication classes provides some basic services for the core communication implementation
	/// </summary>
	public class SerialBase : IDisposable
	{
		#region Constructor

		/// <summary>
		/// 实例化一个无参的构造方法<br />
		/// Instantiate a parameterless constructor
		/// </summary>
		public SerialBase()
		{
			sP_ReadData = new SerialPort();
			hybirdLock = new SimpleHybirdLock();
		}

		#endregion

		#region Public Method

		/// <summary>
		/// 初始化串口信息，9600波特率，8位数据位，1位停止位，无奇偶校验<br />
		/// Initial serial port information, 9600 baud rate, 8 data bits, 1 stop bit, no parity
		/// </summary>
		/// <param name="portName">端口号信息，例如"COM3"</param>
		public void SerialPortInni(string portName) => SerialPortInni(portName, 9600);

		/// <summary>
		/// 初始化串口信息，波特率，8位数据位，1位停止位，无奇偶校验<br />
		/// Initializes serial port information, baud rate, 8-bit data bit, 1-bit stop bit, no parity
		/// </summary>
		/// <param name="portName">端口号信息，例如"COM3"</param>
		/// <param name="baudRate">波特率</param>
		public void SerialPortInni(string portName, int baudRate) => SerialPortInni(portName, baudRate, 8, StopBits.One, Parity.None);

		/// <summary>
		/// 初始化串口信息，波特率，数据位，停止位，奇偶校验需要全部自己来指定<br />
		/// Start serial port information, baud rate, data bit, stop bit, parity all need to be specified
		/// </summary>
		/// <param name="portName">端口号信息，例如"COM3"</param>
		/// <param name="baudRate">波特率</param>
		/// <param name="dataBits">数据位</param>
		/// <param name="stopBits">停止位</param>
		/// <param name="parity">奇偶校验</param>
		public void SerialPortInni(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
		{
			if (sP_ReadData.IsOpen) return;
			sP_ReadData.PortName = portName;    // 串口
			sP_ReadData.BaudRate = baudRate;    // 波特率
			sP_ReadData.DataBits = dataBits;    // 数据位
			sP_ReadData.StopBits = stopBits;    // 停止位
			sP_ReadData.Parity = parity;      // 奇偶校验
			PortName = sP_ReadData.PortName;
			BaudRate = sP_ReadData.BaudRate;
		}

		/// <summary>
		/// 根据自定义初始化方法进行初始化串口信息<br />
		/// Initialize the serial port information according to the custom initialization method
		/// </summary>
		/// <param name="initi">初始化的委托方法</param>
		public void SerialPortInni(Action<SerialPort> initi)
		{
			if (sP_ReadData.IsOpen) return;
			sP_ReadData.PortName = "COM5";
			sP_ReadData.BaudRate = 9600;
			sP_ReadData.DataBits = 8;
			sP_ReadData.StopBits = StopBits.One;
			sP_ReadData.Parity = Parity.None;

			initi.Invoke(sP_ReadData);
			PortName = sP_ReadData.PortName;
			BaudRate = sP_ReadData.BaudRate;
		}

		/// <summary>
		/// 打开一个新的串行端口连接<br />
		/// Open a new serial port connection
		/// </summary>
		public OperateResult Open()
		{
			try
			{
				if (!sP_ReadData.IsOpen)
				{
					sP_ReadData.Open();
					return InitializationOnOpen();
				}
				return OperateResult.CreateSuccessResult();
			}
			catch (Exception ex)
			{
				if (connectErrorCount < 1_0000_0000) connectErrorCount++;
				return new OperateResult(-connectErrorCount, ex.Message);
			}
		}

		/// <summary>
		/// 获取一个值，指示串口是否处于打开状态<br />
		/// Gets a value indicating whether the serial port is open
		/// </summary>
		/// <returns>是或否</returns>
		public bool IsOpen() => sP_ReadData.IsOpen;

		/// <summary>
		/// 关闭当前的串口连接<br />
		/// Close the current serial connection
		/// </summary>
		public void Close()
		{
			if (sP_ReadData.IsOpen)
			{
				ExtraOnClose();
				sP_ReadData.Close();
			}
		}

		/// <summary>
		/// 将原始的字节数据发送到串口，然后从串口接收一条数据。<br />
		/// The raw byte data is sent to the serial port, and then a piece of data is received from the serial port.
		/// </summary>
		/// <param name="send">发送的原始字节数据</param>
		/// <returns>带接收字节的结果对象</returns>
		public OperateResult<byte[]> ReadBase(byte[] send)
		{
			return ReadBase(send, false);
		}

		/// <summary>
		/// 将原始的字节数据发送到串口，然后从串口接收一条数据。<br />
		/// The raw byte data is sent to the serial port, and then a piece of data is received from the serial port.
		/// </summary>
		/// <param name="send">发送的原始字节数据</param>
		/// <param name="sendOnly">是否只是发送，如果为true, 不需要等待数据返回，如果为false, 需要等待数据返回</param>
		/// <returns>带接收字节的结果对象</returns>
		public OperateResult<byte[]> ReadBase(byte[] send, bool sendOnly)
		{
			LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + (LogMsgFormatBinary ? send.ToHexString(' ') : Encoding.ASCII.GetString(send)));

			hybirdLock.Enter();

			OperateResult open = Open();
			if (!open.IsSuccess)
			{
				hybirdLock.Leave();
				return OperateResult.CreateFailedResult<byte[]>(open);
			}

			if (IsClearCacheBeforeRead) ClearSerialCache();

			OperateResult sendResult = SPSend(sP_ReadData, send);
			if (!sendResult.IsSuccess)
			{
				hybirdLock.Leave();
				return OperateResult.CreateFailedResult<byte[]>(sendResult);
			}

			if (sendOnly)
			{
				hybirdLock.Leave();
				return OperateResult.CreateSuccessResult(new byte[0]);
			}
			else
			{
				OperateResult<byte[]> receiveResult = SPReceived(sP_ReadData, true);
				hybirdLock.Leave();

				LogNet?.WriteDebug(ToString(), StringResources.Language.Receive + " : " + (LogMsgFormatBinary ? receiveResult.Content.ToHexString(' ') : Encoding.ASCII.GetString(receiveResult.Content)));
				return receiveResult;
			}
		}

		/// <summary>
		/// 清除串口缓冲区的数据，并返回该数据，如果缓冲区没有数据，返回的字节数组长度为0<br />
		/// The number sent clears the data in the serial port buffer and returns that data, or if there is no data in the buffer, the length of the byte array returned is 0
		/// </summary>
		/// <returns>是否操作成功的方法</returns>
		public OperateResult<byte[]> ClearSerialCache() => SPReceived(sP_ReadData, false);

		#endregion

		#region Initialization And Extra

		/// <inheritdoc cref="Core.Net.NetworkDoubleBase.InitializationOnConnect(System.Net.Sockets.Socket)"/>
		protected virtual OperateResult InitializationOnOpen() => OperateResult.CreateSuccessResult();

		/// <inheritdoc cref="Core.Net.NetworkDoubleBase.ExtraOnDisconnect(System.Net.Sockets.Socket)"/>
		protected virtual OperateResult ExtraOnClose() => OperateResult.CreateSuccessResult();

		/// <inheritdoc cref="Core.Net.NetworkDoubleBase.LogMsgFormatBinary"/>
		protected bool LogMsgFormatBinary = true;

		#endregion

		#region Private Method

		/// <summary>
		/// 发送数据到串口去。<br />
		/// Send data to serial port.
		/// </summary>
		/// <param name="serialPort">串口对象</param>
		/// <param name="data">字节数据</param>
		/// <returns>是否发送成功</returns>
		protected virtual OperateResult SPSend(SerialPort serialPort, byte[] data)
		{
			if (data != null && data.Length > 0)
			{
				
				try
				{
					serialPort.Write(data, 0, data.Length);
					return OperateResult.CreateSuccessResult();
				}
				catch (Exception ex)
				{
					if (connectErrorCount < 1_0000_0000) connectErrorCount++;
					return new OperateResult(-connectErrorCount, ex.Message);
				}
			}
			else
			{
				return OperateResult.CreateSuccessResult();
			}
		}

		/// <summary>
		/// 从串口接收一串字节数据信息，直到没有数据为止，如果参数awaitData为false, 第一轮接收没有数据则返回<br />
		/// Receives a string of bytes of data information from the serial port until there is no data, and returns if the parameter awaitData is false
		/// </summary>
		/// <param name="serialPort">串口对象</param>
		/// <param name="awaitData">是否必须要等待数据返回</param>
		/// <returns>结果数据对象</returns>
		protected virtual OperateResult<byte[]> SPReceived(SerialPort serialPort, bool awaitData)
		{			
			byte[] buffer = new byte[1024];
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			DateTime start = DateTime.Now;                                  // 开始时间，用于确认是否超时的信息
			while (true)
			{
				Thread.Sleep(sleepTime);
				try
				{
					if (serialPort.BytesToRead < 1)
					{
						if ((DateTime.Now - start).TotalMilliseconds > ReceiveTimeout)
						{
							ms.Dispose();
							if (connectErrorCount < 1_0000_0000) connectErrorCount++;
							return new OperateResult<byte[]>(-connectErrorCount, $"Time out: {ReceiveTimeout}");
						}
						else if (ms.Length > 0)
						{
							break;
						}
						else if (awaitData)
						{
							continue;
						}
						else
						{
							break;
						}
					}

					// 继续接收数据
					int sp_receive = serialPort.Read(buffer, 0, buffer.Length);
					ms.Write(buffer, 0, sp_receive);
				}
				catch (Exception ex)
				{
					ms.Dispose();
					if (connectErrorCount < 1_0000_0000) connectErrorCount++;
					return new OperateResult<byte[]>(-connectErrorCount, ex.Message);
				}
			}

			// resetEvent.Set( );
			byte[] result = ms.ToArray();
			ms.Dispose();
			connectErrorCount = 0;
			return OperateResult.CreateSuccessResult(result);
		}

		#endregion

		#region Public Properties

		/// <inheritdoc cref="Core.Net.NetworkBase.LogNet"/>
		public ILogNet LogNet
		{
			get { return logNet; }
			set { logNet = value; }
		}

		/// <summary>
		/// 获取或设置一个值，该值指示在串行通信中是否启用请求发送 (RTS) 信号。<br />
		/// Gets or sets a value indicating whether the request sending (RTS) signal is enabled in serial communication.
		/// </summary>
		public bool RtsEnable
		{
			get => sP_ReadData.RtsEnable;
			set => sP_ReadData.RtsEnable = value;
		}

		/// <summary>
		/// 接收数据的超时时间，默认5000ms<br />
		/// Timeout for receiving data, default is 5000ms
		/// </summary>
		public int ReceiveTimeout
		{
			get { return receiveTimeout; }
			set { receiveTimeout = value; }
		}

		/// <summary>
		/// 连续串口缓冲数据检测的间隔时间，默认20ms，该值越小，通信速度越快，但是越不稳定。<br />
		/// Continuous serial port buffer data detection interval, the default 20ms, the smaller the value, the faster the communication, but the more unstable.
		/// </summary>
		public int SleepTime
		{
			get { return sleepTime; }
			set { if (value > 0) sleepTime = value; }
		}

		/// <summary>
		/// 是否在发送数据前清空缓冲数据，默认是false<br />
		/// Whether to empty the buffer before sending data, the default is false
		/// </summary>
		public bool IsClearCacheBeforeRead
		{
			get { return isClearCacheBeforeRead; }
			set { isClearCacheBeforeRead = value; }
		}

		/// <summary>
		/// 当前连接串口信息的端口号名称<br />
		/// The port name of the current connection serial port information
		/// </summary>
		public string PortName { get; private set; }

		/// <summary>
		/// 当前连接串口信息的波特率<br />
		/// Baud rate of current connection serial port information
		/// </summary>
		public int BaudRate { get; private set; }

		#endregion

		#region IDisposable Support

		private bool disposedValue = false; // 要检测冗余调用

		/// <summary>
		/// 释放当前的对象
		/// </summary>
		/// <param name="disposing">是否在</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)。
					hybirdLock?.Dispose();
					sP_ReadData?.Dispose();
				}

				// TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
				// TODO: 将大型字段设置为 null。

				disposedValue = true;
			}
		}

		// TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
		// ~SerialBase()
		// {
		//   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
		//   Dispose(false);
		// }

		// 添加此代码以正确实现可处置模式。
		/// <summary>
		/// 释放当前的对象
		/// </summary>
		public void Dispose()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			Dispose(true);
			// TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
			// GC.SuppressFinalize(this);
		}
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"SerialBase[{sP_ReadData.PortName},{sP_ReadData.BaudRate},{sP_ReadData.DataBits},{sP_ReadData.StopBits},{sP_ReadData.Parity}]";

		#endregion

		#region Private Member

		/// <summary>
		/// 串口交互的核心
		/// </summary>
		protected SerialPort sP_ReadData = null;                  // 串口交互的核心
		private SimpleHybirdLock hybirdLock;                      // 数据交互的锁
		private ILogNet logNet;                                   // 日志存储
		private int receiveTimeout = 5000;                        // 接收数据的超时时间
		private int sleepTime = 20;                               // 睡眠的时间
		private bool isClearCacheBeforeRead = false;              // 是否在发送前清除缓冲
		private int connectErrorCount = 0;                        // 连接错误次数

		#endregion
	}
}
