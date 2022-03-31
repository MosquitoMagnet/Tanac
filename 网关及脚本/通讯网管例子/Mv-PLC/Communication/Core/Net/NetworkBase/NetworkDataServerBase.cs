using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.BasicFramework;
using Communication.Core;
using Communication.Core.Net;
using Communication.Reflection;

namespace Communication.Core.Net
{
	/// <summary>
	/// 所有虚拟的数据服务器的基类，提供了基本的数据读写，存储加载的功能方法，具体的字节读写需要继承重写。<br />
	/// The base class of all virtual data servers provides basic methods for reading and writing data and storing and loading. 
	/// Specific byte reads and writes need to be inherited and override.
	/// </summary>
	public class NetworkDataServerBase : NetworkAuthenticationServerBase, IDisposable, IReadWriteNet
	{
		/// <summary>
		/// 当接收到来自客户的数据信息时触发的对象，该数据可能来自tcp或是串口<br />
		/// The object that is triggered when receiving data information from the customer, the data may come from tcp or serial port
		/// </summary>
		/// <param name="sender">触发的服务器对象</param>
		/// <param name="source">消息的来源对象</param>
		/// <param name="data">实际的数据信息</param>
		public delegate void DataReceivedDelegate(object sender, object source, byte[] data);

		/// <summary>
		/// 数据发送的时候委托<br />
		/// Show DataSend To PLC
		/// </summary>
		/// <param name="sender">数据发送对象</param>
		/// <param name="data">数据内容</param>
		public delegate void DataSendDelegate(object sender, byte[] data);

		private List<string> TrustedClients = null;

		private bool IsTrustedClientsOnly = false;

		private SimpleHybirdLock lock_trusted_clients;

		private List<AppSession> listsOnlineClient;

		private object lockOnlineClient;

		private int onlineCount = 0;

		private Timer timerHeart;

		/// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDoubleBase.ByteTransform" />
		public IByteTransform ByteTransform { get; set; }

		/// <inheritdoc cref="P:HslCommunication.Core.IReadWriteNet.ConnectionId" />
		public string ConnectionId { get; set; }

		/// <summary>
		/// 获取或设置当前的服务器是否允许远程客户端进行写入数据操作，默认为<c>True</c><br />
		/// Gets or sets whether the current server allows remote clients to write data, the default is <c>True</c>
		/// </summary>
		/// <remarks>
		/// 如果设置为<c>False</c>，那么所有远程客户端的操作都会失败，直接返回错误码或是关闭连接。
		/// </remarks>
		public bool EnableWrite { get; set; } = true;


		/// <summary>
		/// 获取或设置两次数据交互时的最小时间间隔，默认为24小时。<br />
		/// Get or set the minimum time interval between two data interactions, the default is 24 hours.
		/// </summary>
		public TimeSpan ActiveTimeSpan { get; set; }

		/// <inheritdoc cref="P:HslCommunication.Core.Net.NetworkDeviceBase.WordLength" />
		protected ushort WordLength { get; set; } = 1;


		/// <summary>
		/// 获取在线的客户端的数量<br />
		/// Get the number of clients online
		/// </summary>
		public int OnlineCount => onlineCount;

		/// <summary>
		/// 获取当前所有在线的客户端信息，包括IP地址和端口号信息<br />
		/// Get all current online client information, including IP address and port number information
		/// </summary>
		public AppSession[] GetOnlineSessions
		{
			get
			{
				lock (lockOnlineClient)
				{
					return listsOnlineClient.ToArray();
				}
			}
		}

		/// <summary>
		/// 接收到数据的时候就触发的事件，示例详细参考API文档信息<br />
		/// An event that is triggered when data is received
		/// </summary>
		/// <remarks>
		/// 事件共有三个参数，sender指服务器本地的对象，例如 <see cref="T:HslCommunication.ModBus.ModbusTcpServer" /> 对象，source 指会话对象，网口对象为 <see cref="T:HslCommunication.Core.Net.AppSession" />，
		/// 串口为<see cref="T:System.IO.Ports.SerialPort" /> 对象，需要根据实际判断，data 为收到的原始数据 byte[] 对象
		/// </remarks>
		/// <example>
		/// 我们以Modbus的Server为例子，其他的虚拟服务器同理，因为都集成自本服务器对象
		/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Core\NetworkDataServerBaseSample.cs" region="OnDataReceivedSample" title="数据接收触发的示例" />
		/// </example>
		public event DataReceivedDelegate OnDataReceived;

		/// <summary>
		/// 数据发送的时候就触发的事件<br />
		/// Events that are triggered when data is sent
		/// </summary>
		public event DataSendDelegate OnDataSend;

		/// <summary>
		/// 实例化一个默认的数据服务器的对象<br />
		/// Instantiate an object of the default data server
		/// </summary>
		public NetworkDataServerBase()
		{
			ActiveTimeSpan = TimeSpan.FromHours(24.0);
			lock_trusted_clients = new SimpleHybirdLock();
			ConnectionId = SoftBasic.GetUniqueStringByGuidAndRandom();
			lockOnlineClient = new object();
			listsOnlineClient = new List<AppSession>();
			timerHeart = new Timer(ThreadTimerHeartCheck, null, 2000, 10000);
		}

		/// <summary>
		/// 将本系统的数据池数据存储到指定的文件<br />
		/// Store the data pool data of this system to the specified file
		/// </summary>
		/// <param name="path">指定文件的路径</param>
		/// <exception cref="T:System.ArgumentException"></exception>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		/// <exception cref="T:System.IO.PathTooLongException"></exception>
		/// <exception cref="T:System.IO.DirectoryNotFoundException"></exception>
		/// <exception cref="T:System.IO.IOException"></exception>
		/// <exception cref="T:System.UnauthorizedAccessException"></exception>
		/// <exception cref="T:System.NotSupportedException"></exception>
		/// <exception cref="T:System.Security.SecurityException"></exception>
		public void SaveDataPool(string path)
		{
			byte[] bytes = SaveToBytes();
			File.WriteAllBytes(path, bytes);
		}

		/// <summary>
		/// 从文件加载数据池信息<br />
		/// Load datapool information from a file
		/// </summary>
		/// <param name="path">文件路径</param>
		/// <exception cref="T:System.ArgumentException"></exception>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		/// <exception cref="T:System.IO.PathTooLongException"></exception>
		/// <exception cref="T:System.IO.DirectoryNotFoundException"></exception>
		/// <exception cref="T:System.IO.IOException"></exception>
		/// <exception cref="T:System.UnauthorizedAccessException"></exception>
		/// <exception cref="T:System.NotSupportedException"></exception>
		/// <exception cref="T:System.Security.SecurityException"></exception>
		/// <exception cref="T:System.IO.FileNotFoundException"></exception>
		public void LoadDataPool(string path)
		{
			if (File.Exists(path))
			{
				byte[] content = File.ReadAllBytes(path);
				LoadFromBytes(content);
			}
		}

		/// <summary>
		/// 从字节数据加载数据信息，需要进行重写方法<br />
		/// Loading data information from byte data requires rewriting method
		/// </summary>
		/// <param name="content">字节数据</param>
		protected virtual void LoadFromBytes(byte[] content)
		{
		}

		/// <summary>
		/// 将数据信息存储到字节数组去，需要进行重写方法<br />
		/// To store data information into a byte array, a rewrite method is required
		/// </summary>
		/// <returns>所有的内容</returns>
		protected virtual byte[] SaveToBytes()
		{
			return new byte[0];
		}

		/// <summary>
		/// 触发一个数据接收的事件信息<br />
		/// Event information that triggers a data reception
		/// </summary>
		/// <param name="source">数据的发送方</param>
		/// <param name="receive">接收数据信息</param>
		protected void RaiseDataReceived(object source, byte[] receive)
		{
			this.OnDataReceived?.Invoke(this, source, receive);
		}

		/// <summary>
		/// 触发一个数据发送的事件信息<br />
		/// Event information that triggers a data transmission
		/// </summary>
		/// <param name="send">数据内容</param>
		protected void RaiseDataSend(byte[] send)
		{
			this.OnDataSend?.Invoke(this, send);
		}

		/// <summary>
		/// 当客户端登录后，在Ip信息的过滤后，然后触发本方法，进行后续的数据接收，处理，并返回相关的数据信息<br />
		/// When the client logs in, after filtering the IP information, this method is then triggered to perform subsequent data reception, 
		/// processing, and return related data information
		/// </summary>
		/// <param name="socket">网络套接字</param>
		/// <param name="endPoint">终端节点</param>
		protected virtual void ThreadPoolLoginAfterClientCheck(Socket socket, IPEndPoint endPoint)
		{
		}

		/// <summary>
		/// 当接收到了新的请求的时候执行的操作，此处进行账户的安全验证<br />
		/// The operation performed when a new request is received, and the account security verification is performed here
		/// </summary>
		/// <param name="socket">异步对象</param>
		/// <param name="endPoint">终结点</param>
		protected override void ThreadPoolLogin(Socket socket, IPEndPoint endPoint)
		{
			string ipAddress = endPoint.Address.ToString();
			if (IsTrustedClientsOnly && !CheckIpAddressTrusted(ipAddress))
			{
				base.LogNet?.WriteDebug(ToString(), string.Format(StringResources.Language.ClientDisableLogin, endPoint));
				socket.Close();
				return;
			}
			if (!base.IsUseAccountCertificate)
			{
				base.LogNet?.WriteDebug(ToString(), string.Format(StringResources.Language.ClientOnlineInfo, endPoint));
			}
			ThreadPoolLoginAfterClientCheck(socket, endPoint);
		}

		/// <summary>
		/// 设置并启动受信任的客户端登录并读写，如果为null，将关闭对客户端的ip验证<br />
		/// Set and start the trusted client login and read and write, if it is null, the client's IP verification will be turned off
		/// </summary>
		/// <param name="clients">受信任的客户端列表</param>
		public void SetTrustedIpAddress(List<string> clients)
		{
			lock_trusted_clients.Enter();
			if (clients != null)
			{
				TrustedClients = clients.Select(delegate (string m)
				{
					IPAddress iPAddress = IPAddress.Parse(m);
					return iPAddress.ToString();
				}).ToList();
				IsTrustedClientsOnly = true;
			}
			else
			{
				TrustedClients = new List<string>();
				IsTrustedClientsOnly = false;
			}
			lock_trusted_clients.Leave();
		}

		/// <summary>
		/// 检查该Ip地址是否是受信任的<br />
		/// Check if the IP address is trusted
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <returns>是受信任的返回<c>True</c>，否则返回<c>False</c></returns>
		private bool CheckIpAddressTrusted(string ipAddress)
		{
			if (IsTrustedClientsOnly)
			{
				bool result = false;
				lock_trusted_clients.Enter();
				for (int i = 0; i < TrustedClients.Count; i++)
				{
					if (TrustedClients[i] == ipAddress)
					{
						result = true;
						break;
					}
				}
				lock_trusted_clients.Leave();
				return result;
			}
			return false;
		}

		/// <summary>
		/// 获取受信任的客户端列表<br />
		/// Get a list of trusted clients
		/// </summary>
		/// <returns>字符串数据信息</returns>
		public string[] GetTrustedClients()
		{
			string[] result = new string[0];
			lock_trusted_clients.Enter();
			if (TrustedClients != null)
			{
				result = TrustedClients.ToArray();
			}
			lock_trusted_clients.Leave();
			return result;
		}

		/// <summary>
		/// 新增一个在线的客户端信息<br />
		/// Add an online client information
		/// </summary>
		/// <param name="session">会话内容</param>
		protected void AddClient(AppSession session)
		{
			lock (lockOnlineClient)
			{
				listsOnlineClient.Add(session);
				onlineCount++;
			}
		}

		/// <summary>
		/// 移除一个在线的客户端信息<br />
		/// Remove an online client message
		/// </summary>
		/// <param name="session">会话内容</param>
		/// <param name="reason">下线的原因</param>
		protected void RemoveClient(AppSession session, string reason = "")
		{
			lock (lockOnlineClient)
			{
				if (listsOnlineClient.Remove(session))
				{
					base.LogNet?.WriteDebug(ToString(), string.Format(StringResources.Language.ClientOfflineInfo, session.IpEndPoint) + " " + reason);
					session.WorkSocket?.Close();
					onlineCount--;
				}
			}
		}

		/// <summary>
		/// 关闭之后进行的操作
		/// </summary>
		protected override void CloseAction()
		{
			base.CloseAction();
			lock (lockOnlineClient)
			{
				for (int i = 0; i < listsOnlineClient.Count; i++)
				{
					listsOnlineClient[i]?.WorkSocket?.Close();
				}
				listsOnlineClient.Clear();
			}
		}

		private void ThreadTimerHeartCheck(object obj)
		{
			AppSession[] array = null;
			lock (lockOnlineClient)
			{
				array = listsOnlineClient.ToArray();
			}
			if (array == null || array.Length == 0)
			{
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (DateTime.Now - array[i].HeartTime > ActiveTimeSpan)
				{
					RemoveClient(array[i]);
				}
			}
		}

		/// <summary>
		/// 释放当前的对象
		/// </summary>
		/// <param name="disposing">是否托管对象</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock_trusted_clients?.Dispose();
			}
			base.Dispose(disposing);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"NetworkDataServerBase[{base.Port}]";
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Read(System.String,System.UInt16)" />
		[HslMqttApi("ReadByteArray", "")]
		public virtual OperateResult<byte[]> Read(string address, ushort length)
		{
			return new OperateResult<byte[]>(StringResources.Language.NotSupportedFunction);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Byte[])" />
		[HslMqttApi("WriteByteArray", "")]
		public virtual OperateResult Write(string address, byte[] value)
		{
			return new OperateResult(StringResources.Language.NotSupportedFunction);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadBool(System.String,System.UInt16)" />
		[HslMqttApi("ReadBoolArray", "")]
		public virtual OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return new OperateResult<bool[]>(StringResources.Language.NotSupportedFunction);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadBool(System.String)" />
		[HslMqttApi("ReadBool", "")]
		public virtual OperateResult<bool> ReadBool(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadBool(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Boolean[])" />
		[HslMqttApi("WriteBoolArray", "")]
		public virtual OperateResult Write(string address, bool[] value)
		{
			return new OperateResult(StringResources.Language.NotSupportedFunction);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Boolean)" />
		[HslMqttApi("WriteBool", "")]
		public virtual OperateResult Write(string address, bool value)
		{
			return Write(address, new bool[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadCustomer``1(System.String)" />
		public OperateResult<T> ReadCustomer<T>(string address) where T : IDataTransfer, new()
		{
			OperateResult<T> operateResult = new OperateResult<T>();
			T content = new T();
			OperateResult<byte[]> operateResult2 = Read(address, content.ReadCount);
			if (operateResult2.IsSuccess)
			{
				content.ParseSource(operateResult2.Content);
				operateResult.Content = content;
				operateResult.IsSuccess = true;
			}
			else
			{
				operateResult.ErrorCode = operateResult2.ErrorCode;
				operateResult.Message = operateResult2.Message;
			}
			return operateResult;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteCustomer``1(System.String,``0)" />
		public OperateResult WriteCustomer<T>(string address, T data) where T : IDataTransfer, new()
		{
			return Write(address, data.ToSource());
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Read``1" />
		public virtual OperateResult<T> Read<T>() where T : class, new()
		{
			return HslReflectionHelper.Read<T>(this);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write``1(``0)" />
		public virtual OperateResult Write<T>(T data) where T : class, new()
		{
			return HslReflectionHelper.Write(data, this);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt16(System.String)" />
		[HslMqttApi("ReadInt16", "")]
		public OperateResult<short> ReadInt16(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadInt16(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt16(System.String,System.UInt16)" />
		[HslMqttApi("ReadInt16Array", "")]
		public virtual OperateResult<short[]> ReadInt16(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransInt16(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt16(System.String)" />
		[HslMqttApi("ReadUInt16", "")]
		public OperateResult<ushort> ReadUInt16(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadUInt16(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt16(System.String,System.UInt16)" />
		[HslMqttApi("ReadUInt16Array", "")]
		public virtual OperateResult<ushort[]> ReadUInt16(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransUInt16(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt32(System.String)" />
		[HslMqttApi("ReadInt32", "")]
		public OperateResult<int> ReadInt32(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadInt32(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt32(System.String,System.UInt16)" />
		[HslMqttApi("ReadInt32Array", "")]
		public virtual OperateResult<int[]> ReadInt32(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt32(System.String)" />
		[HslMqttApi("ReadUInt32", "")]
		public OperateResult<uint> ReadUInt32(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadUInt32(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt32(System.String,System.UInt16)" />
		[HslMqttApi("ReadUInt32Array", "")]
		public virtual OperateResult<uint[]> ReadUInt32(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransUInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadFloat(System.String)" />
		[HslMqttApi("ReadFloat", "")]
		public OperateResult<float> ReadFloat(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadFloat(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadFloat(System.String,System.UInt16)" />
		[HslMqttApi("ReadFloatArray", "")]
		public virtual OperateResult<float[]> ReadFloat(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransSingle(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt64(System.String)" />
		[HslMqttApi("ReadInt64", "")]
		public OperateResult<long> ReadInt64(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadInt64(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt64(System.String,System.UInt16)" />
		[HslMqttApi("ReadInt64Array", "")]
		public virtual OperateResult<long[]> ReadInt64(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt64(System.String)" />
		[HslMqttApi("ReadUInt64", "")]
		public OperateResult<ulong> ReadUInt64(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadUInt64(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt64(System.String,System.UInt16)" />
		[HslMqttApi("ReadUInt64Array", "")]
		public virtual OperateResult<ulong[]> ReadUInt64(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransUInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadDouble(System.String)" />
		[HslMqttApi("ReadDouble", "")]
		public OperateResult<double> ReadDouble(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadDouble(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadDouble(System.String,System.UInt16)" />
		[HslMqttApi("ReadDoubleArray", "")]
		public virtual OperateResult<double[]> ReadDouble(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransDouble(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadString(System.String,System.UInt16)" />
		[HslMqttApi("ReadString", "")]
		public OperateResult<string> ReadString(string address, ushort length)
		{
			return ReadString(address, length, Encoding.ASCII);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadString(System.String,System.UInt16,System.Text.Encoding)" />
		public virtual OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, length), (byte[] m) => ByteTransform.TransString(m, 0, m.Length, encoding));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int16[])" />
		[HslMqttApi("WriteInt16Array", "")]
		public virtual OperateResult Write(string address, short[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int16)" />
		[HslMqttApi("WriteInt16", "")]
		public virtual OperateResult Write(string address, short value)
		{
			return Write(address, new short[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt16[])" />
		[HslMqttApi("WriteUInt16Array", "")]
		public virtual OperateResult Write(string address, ushort[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt16)" />
		[HslMqttApi("WriteUInt16", "")]
		public virtual OperateResult Write(string address, ushort value)
		{
			return Write(address, new ushort[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int32[])" />
		[HslMqttApi("WriteInt32Array", "")]
		public virtual OperateResult Write(string address, int[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int32)" />
		[HslMqttApi("WriteInt32", "")]
		public OperateResult Write(string address, int value)
		{
			return Write(address, new int[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt32[])" />
		[HslMqttApi("WriteUInt32Array", "")]
		public virtual OperateResult Write(string address, uint[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt32)" />
		[HslMqttApi("WriteUInt32", "")]
		public OperateResult Write(string address, uint value)
		{
			return Write(address, new uint[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Single[])" />
		[HslMqttApi("WriteFloatArray", "")]
		public virtual OperateResult Write(string address, float[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Single)" />
		[HslMqttApi("WriteFloat", "")]
		public OperateResult Write(string address, float value)
		{
			return Write(address, new float[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int64[])" />
		[HslMqttApi("WriteInt64Array", "")]
		public virtual OperateResult Write(string address, long[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Int64)" />
		[HslMqttApi("WriteInt64", "")]
		public OperateResult Write(string address, long value)
		{
			return Write(address, new long[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt64[])" />
		[HslMqttApi("WriteUInt64Array", "")]
		public virtual OperateResult Write(string address, ulong[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.UInt64)" />
		[HslMqttApi("WriteUInt64", "")]
		public OperateResult Write(string address, ulong value)
		{
			return Write(address, new ulong[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Double[])" />
		[HslMqttApi("WriteDoubleArray", "")]
		public virtual OperateResult Write(string address, double[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.Double)" />
		[HslMqttApi("WriteDouble", "")]
		public OperateResult Write(string address, double value)
		{
			return Write(address, new double[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.String)" />
		[HslMqttApi("WriteString", "")]
		public virtual OperateResult Write(string address, string value)
		{
			return Write(address, value, Encoding.ASCII);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.String,System.Int32)" />
		public virtual OperateResult Write(string address, string value, int length)
		{
			return Write(address, value, length, Encoding.ASCII);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.String,System.Text.Encoding)" />
		public virtual OperateResult Write(string address, string value, Encoding encoding)
		{
			byte[] array = ByteTransform.TransByte(value, encoding);
			if (WordLength == 1)
			{
				array = SoftBasic.ArrayExpandToLengthEven(array);
			}
			return Write(address, array);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Write(System.String,System.String,System.Int32,System.Text.Encoding)" />
		public virtual OperateResult Write(string address, string value, int length, Encoding encoding)
		{
			byte[] data = ByteTransform.TransByte(value, encoding);
			if (WordLength == 1)
			{
				data = SoftBasic.ArrayExpandToLengthEven(data);
			}
			data = SoftBasic.ArrayExpandToLength(data, length);
			return Write(address, data);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Boolean,System.Int32,System.Int32)" />
		[HslMqttApi("WaitBool", "")]
		public OperateResult<TimeSpan> Wait(string address, bool waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return ReadWriteNetHelper.Wait(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Int16,System.Int32,System.Int32)" />
		[HslMqttApi("WaitInt16", "")]
		public OperateResult<TimeSpan> Wait(string address, short waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return ReadWriteNetHelper.Wait(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.UInt16,System.Int32,System.Int32)" />
		[HslMqttApi("WaitUInt16", "")]
		public OperateResult<TimeSpan> Wait(string address, ushort waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return ReadWriteNetHelper.Wait(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Int32,System.Int32,System.Int32)" />
		[HslMqttApi("WaitInt32", "")]
		public OperateResult<TimeSpan> Wait(string address, int waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return ReadWriteNetHelper.Wait(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.UInt32,System.Int32,System.Int32)" />
		[HslMqttApi("WaitUInt32", "")]
		public OperateResult<TimeSpan> Wait(string address, uint waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return ReadWriteNetHelper.Wait(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Int64,System.Int32,System.Int32)" />
		[HslMqttApi("WaitInt64", "")]
		public OperateResult<TimeSpan> Wait(string address, long waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return ReadWriteNetHelper.Wait(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.UInt64,System.Int32,System.Int32)" />
		[HslMqttApi("WaitUInt64", "")]
		public OperateResult<TimeSpan> Wait(string address, ulong waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return ReadWriteNetHelper.Wait(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Boolean,System.Int32,System.Int32)" />
		public async Task<OperateResult<TimeSpan>> WaitAsync(string address, bool waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return await ReadWriteNetHelper.WaitAsync(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Int16,System.Int32,System.Int32)" />
		public async Task<OperateResult<TimeSpan>> WaitAsync(string address, short waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return await ReadWriteNetHelper.WaitAsync(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.UInt16,System.Int32,System.Int32)" />
		public async Task<OperateResult<TimeSpan>> WaitAsync(string address, ushort waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return await ReadWriteNetHelper.WaitAsync(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Int32,System.Int32,System.Int32)" />
		public async Task<OperateResult<TimeSpan>> WaitAsync(string address, int waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return await ReadWriteNetHelper.WaitAsync(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.UInt32,System.Int32,System.Int32)" />
		public async Task<OperateResult<TimeSpan>> WaitAsync(string address, uint waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return await ReadWriteNetHelper.WaitAsync(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.Int64,System.Int32,System.Int32)" />
		public async Task<OperateResult<TimeSpan>> WaitAsync(string address, long waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return await ReadWriteNetHelper.WaitAsync(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.Wait(System.String,System.UInt64,System.Int32,System.Int32)" />
		public async Task<OperateResult<TimeSpan>> WaitAsync(string address, ulong waitValue, int readInterval = 100, int waitTimeout = -1)
		{
			return await ReadWriteNetHelper.WaitAsync(this, address, waitValue, readInterval, waitTimeout);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadAsync(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			return await Task.Run(() => Read(address, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Byte[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			return await Task.Run(() => Write(address, value));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadBoolAsync(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			return await Task.Run(() => ReadBool(address, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadBoolAsync(System.String)" />
		public virtual async Task<OperateResult<bool>> ReadBoolAsync(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadBoolAsync(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Boolean[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, bool[] value)
		{
			return await Task.Run(() => Write(address, value));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Boolean)" />
		public virtual async Task<OperateResult> WriteAsync(string address, bool value)
		{
			return await WriteAsync(address, new bool[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteCustomerAsync``1(System.String,``0)" />
		public async Task<OperateResult<T>> ReadCustomerAsync<T>(string address) where T : IDataTransfer, new()
		{
			OperateResult<T> result = new OperateResult<T>();
			T Content = new T();
			OperateResult<byte[]> read = await ReadAsync(address, Content.ReadCount);
			if (read.IsSuccess)
			{
				Content.ParseSource(read.Content);
				result.Content = Content;
				result.IsSuccess = true;
			}
			else
			{
				result.ErrorCode = read.ErrorCode;
				result.Message = read.Message;
			}
			return result;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteCustomerAsync``1(System.String,``0)" />
		public async Task<OperateResult> WriteCustomerAsync<T>(string address, T data) where T : IDataTransfer, new()
		{
			return await WriteAsync(address, data.ToSource());
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadAsync``1" />
		public virtual async Task<OperateResult<T>> ReadAsync<T>() where T : class, new()
		{
			return await HslReflectionHelper.ReadAsync<T>(this);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync``1(``0)" />
		public virtual async Task<OperateResult> WriteAsync<T>(T data) where T : class, new()
		{
			return await HslReflectionHelper.WriteAsync(data, this);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt16Async(System.String)" />
		public async Task<OperateResult<short>> ReadInt16Async(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadInt16Async(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt16Async(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransInt16(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt16Async(System.String)" />
		public async Task<OperateResult<ushort>> ReadUInt16Async(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadUInt16Async(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt16Async(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransUInt16(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt32Async(System.String)" />
		public async Task<OperateResult<int>> ReadInt32Async(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadInt32Async(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt32Async(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt32Async(System.String)" />
		public async Task<OperateResult<uint>> ReadUInt32Async(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadUInt32Async(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt32Async(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransUInt32(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadFloatAsync(System.String)" />
		public async Task<OperateResult<float>> ReadFloatAsync(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadFloatAsync(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadFloatAsync(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransSingle(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt64Async(System.String)" />
		public async Task<OperateResult<long>> ReadInt64Async(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadInt64Async(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadInt64Async(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt64Async(System.String)" />
		public async Task<OperateResult<ulong>> ReadUInt64Async(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadUInt64Async(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadUInt64Async(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransUInt64(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadDoubleAsync(System.String)" />
		public async Task<OperateResult<double>> ReadDoubleAsync(string address)
		{
			return ByteTransformHelper.GetResultFromArray(await ReadDoubleAsync(address, 1));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadDoubleAsync(System.String,System.UInt16)" />
		public virtual async Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransDouble(m, 0, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadStringAsync(System.String,System.UInt16)" />
		public async Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
		{
			return await ReadStringAsync(address, length, Encoding.ASCII);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.ReadStringAsync(System.String,System.UInt16,System.Text.Encoding)" />
		public virtual async Task<OperateResult<string>> ReadStringAsync(string address, ushort length, Encoding encoding)
		{
			return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, length), (byte[] m) => ByteTransform.TransString(m, 0, m.Length, encoding));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int16[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, short[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int16)" />
		public virtual async Task<OperateResult> WriteAsync(string address, short value)
		{
			return await WriteAsync(address, new short[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt16[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, ushort[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt16)" />
		public virtual async Task<OperateResult> WriteAsync(string address, ushort value)
		{
			return await WriteAsync(address, new ushort[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int32[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, int[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int32)" />
		public async Task<OperateResult> WriteAsync(string address, int value)
		{
			return await WriteAsync(address, new int[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt32[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, uint[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt32)" />
		public async Task<OperateResult> WriteAsync(string address, uint value)
		{
			return await WriteAsync(address, new uint[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Single[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, float[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Single)" />
		public async Task<OperateResult> WriteAsync(string address, float value)
		{
			return await WriteAsync(address, new float[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int64[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, long[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Int64)" />
		public async Task<OperateResult> WriteAsync(string address, long value)
		{
			return await WriteAsync(address, new long[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt64[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, ulong[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.UInt64)" />
		public async Task<OperateResult> WriteAsync(string address, ulong value)
		{
			return await WriteAsync(address, new ulong[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Double[])" />
		public virtual async Task<OperateResult> WriteAsync(string address, double[] values)
		{
			return await WriteAsync(address, ByteTransform.TransByte(values));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.Double)" />
		public async Task<OperateResult> WriteAsync(string address, double value)
		{
			return await WriteAsync(address, new double[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.String)" />
		public virtual async Task<OperateResult> WriteAsync(string address, string value)
		{
			return await WriteAsync(address, value, Encoding.ASCII);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.String,System.Text.Encoding)" />
		public virtual async Task<OperateResult> WriteAsync(string address, string value, Encoding encoding)
		{
			byte[] temp = ByteTransform.TransByte(value, encoding);
			if (WordLength == 1)
			{
				temp = SoftBasic.ArrayExpandToLengthEven(temp);
			}
			return await WriteAsync(address, temp);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.String,System.Int32)" />
		public virtual async Task<OperateResult> WriteAsync(string address, string value, int length)
		{
			return await WriteAsync(address, value, length, Encoding.ASCII);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IReadWriteNet.WriteAsync(System.String,System.String,System.Int32,System.Text.Encoding)" />
		public virtual async Task<OperateResult> WriteAsync(string address, string value, int length, Encoding encoding)
		{
			byte[] temp2 = ByteTransform.TransByte(value, encoding);
			if (WordLength == 1)
			{
				temp2 = SoftBasic.ArrayExpandToLengthEven(temp2);
			}
			temp2 = SoftBasic.ArrayExpandToLength(temp2, length);
			return await WriteAsync(address, temp2);
		}
	}
}
