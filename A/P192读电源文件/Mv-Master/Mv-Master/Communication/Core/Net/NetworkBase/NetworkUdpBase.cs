using Communication.BasicFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Communication.Reflection;
using System.Threading.Tasks;

namespace Communication.Core.Net
{
	/// <summary>
	/// 基于Udp的应答式通信类<br />
	/// Udp - based responsive communication class
	/// </summary>
	public class NetworkUdpBase : NetworkBase
	{
		#region Constructor

		/// <summary>
		/// 实例化一个默认的方法<br />
		/// Instantiate a default method
		/// </summary>
		public NetworkUdpBase()
		{
			hybirdLock = new SimpleHybirdLock();                                          // 当前交互的数据锁
			ReceiveTimeout = 5000;                                                         // 当前接收的超时时间
			ConnectionId = BasicFramework.SoftBasic.GetUniqueStringByGuidAndRandom();     // 设备的唯一的编号
		}

		#endregion

		#region Public Properties

		/// <inheritdoc cref="NetworkDoubleBase.IpAddress"/>
		public virtual string IpAddress { get; set; }

		/// <inheritdoc cref="NetworkDoubleBase.Port"/>
		public virtual int Port { get; set; }

		/// <inheritdoc cref="NetworkDoubleBase.ReceiveTimeOut"/>
		public int ReceiveTimeout { get; set; }

		/// <inheritdoc cref="NetworkDoubleBase.ConnectionId"/>
		public string ConnectionId { get; set; }

		/// <summary>
		/// 获取或设置一次接收时的数据长度，默认2KB数据长度，特殊情况的时候需要调整<br />
		/// Gets or sets the length of data received at a time. The default length is 2KB
		/// </summary>
		public int ReceiveCacheLength { get; set; } = 2048;

		#endregion

		#region Core Read

		/// <summary>
		/// 核心的数据交互读取，发数据发送到串口上去，然后从串口上接收返回的数据<br />
		/// The core data is read interactively, the data is sent to the serial port, and the returned data is received from the serial port
		/// </summary>
		/// <param name="value">完整的报文内容</param>
		/// <returns>是否成功的结果对象</returns>
		public virtual OperateResult<byte[]> ReadFromCoreServer(byte[] value)
		{
			

			LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + SoftBasic.ByteToHexString(value));
			hybirdLock.Enter();
			try
			{
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(IpAddress), Port);
				Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				server.SendTo(value, value.Length, SocketFlags.None, endPoint);

				if (ReceiveTimeout < 0)
				{
					hybirdLock.Leave();
					return OperateResult.CreateSuccessResult(new byte[0]);
				}

				// 对于不存在的IP地址，加入此行代码后，可以在指定时间内解除阻塞模式限制
				server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeout);
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				EndPoint Remote = (EndPoint)sender;
				byte[] buffer = new byte[ReceiveCacheLength];
				int recv = server.ReceiveFrom(buffer, ref Remote);
				byte[] receive = buffer.Take(recv).ToArray();

				hybirdLock.Leave();

				LogNet?.WriteDebug(ToString(), StringResources.Language.Receive + " : " + SoftBasic.ByteToHexString(receive));
				connectErrorCount = 0;
				return OperateResult.CreateSuccessResult(receive);
			}
			catch (Exception ex)
			{
				hybirdLock.Leave();
				if (connectErrorCount < 1_0000_0000) connectErrorCount++;
				return new OperateResult<byte[]>(-connectErrorCount, ex.Message);
			}
		}

		#endregion

		#region Public Method

		/// <inheritdoc cref="NetworkDoubleBase.IpAddressPing"/>
		public IPStatus IpAddressPing()
		{
			Ping ping = new Ping();
			return ping.Send(IpAddress).Status;
		}

		#endregion

		#region Private Member

		private SimpleHybirdLock hybirdLock = null;       // 数据锁
		private int connectErrorCount = 0;                // 连接错误次数

		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"NetworkUdpBase[{IpAddress}:{Port}]";

		#endregion
	}
}
