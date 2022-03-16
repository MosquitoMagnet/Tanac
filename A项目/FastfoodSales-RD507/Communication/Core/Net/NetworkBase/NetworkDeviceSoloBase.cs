using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Communication.Core.IMessage;
using System.Threading;
using Communication.BasicFramework;
using System.Threading.Tasks;

namespace Communication.Core.Net
{
	/// <summary>
	/// 基于单次无协议的网络交互的基类，通常是串口协议扩展成网口协议的基类<br />
	/// Base class based on a single non-protocol network interaction, usually the base class that the serial port protocol is extended to the network port protocol
	/// </summary>
	public class NetworkDeviceSoloBase : NetworkDeviceBase
	{
		#region Constrcutor

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public NetworkDeviceSoloBase()
		{
			ReceiveTimeOut = 5000;
			SleepTime = 20;
		}

		#endregion

		#region Receive Bytes

		/// <summary>
		/// 从串口接收一串数据信息，可以指定是否一定要接收到数据<br />
		/// Receive a string of data information from the serial port, you can specify whether you must receive data
		/// </summary>
		/// <param name="socket">串口对象</param>
		/// <param name="awaitData">是否必须要等待数据返回</param>
		/// <returns>结果数据对象</returns>
		protected OperateResult<byte[]> ReceiveSolo(Socket socket, bool awaitData)
		{
			
			byte[] buffer = new byte[1024];
			System.IO.MemoryStream ms = new System.IO.MemoryStream();

			HslTimeOut hslTimeOut = new HslTimeOut()
			{
				DelayTime = ReceiveTimeOut,
				WorkSocket = socket,
			};
			if (ReceiveTimeOut > 0) ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolSocketCheckTimeOut), hslTimeOut);

			try
			{
				Thread.Sleep(SleepTime);
				int receiveCount = socket.Receive(buffer);
				hslTimeOut.IsSuccessful = true;
				ms.Write(buffer, 0, receiveCount);
			}
			catch (Exception ex)
			{
				hslTimeOut.IsSuccessful = true;
				ms.Dispose();
				if (hslTimeOut.IsTimeout)
					return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + " " + ReceiveTimeOut + "ms");
				else
					return new OperateResult<byte[]>(ex.Message);
			}
			byte[] result = ms.ToArray();
			ms.Dispose();
			return OperateResult.CreateSuccessResult(result);
		}

#if !NET35 && !NET20
		/// <inheritdoc cref="ReceiveSolo(Socket, bool)"/>
		protected async Task<OperateResult<byte[]>> ReceiveSoloAsync(Socket socket, bool awaitData)
		{
			
			byte[] buffer = new byte[1024];
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			HslTimeOut hslTimeOut = new HslTimeOut()
			{
				DelayTime = ReceiveTimeOut,
				WorkSocket = socket,
			};
			if (ReceiveTimeOut > 0) ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadPoolSocketCheckTimeOut), hslTimeOut);

			try
			{
				Thread.Sleep(SleepTime);
				int receiveCount = await Task.Factory.FromAsync(socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, socket), socket.EndReceive);
				hslTimeOut.IsSuccessful = true;
				ms.Write(buffer, 0, receiveCount);
			}
			catch (Exception ex)
			{
				hslTimeOut.IsSuccessful = true;
				ms.Dispose();
				if (hslTimeOut.IsTimeout)
					return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + " " + ReceiveTimeOut + "ms");
				else
					return new OperateResult<byte[]>(ex.Message);
			}
			byte[] result = ms.ToArray();
			ms.Dispose();
			return OperateResult.CreateSuccessResult(result);
		}
#endif
		#endregion

		#region Override NetworkDeviceBase

		/// <inheritdoc/>
		public override OperateResult<byte[]> ReadFromCoreServer(Socket socket, byte[] send, bool hasResponseData = true)
		{

			LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + SoftBasic.ByteToHexString(send, ' '));

			// send
			OperateResult sendResult = Send(socket, send);
			if (!sendResult.IsSuccess)
			{
				socket?.Close();
				return OperateResult.CreateFailedResult<byte[]>(sendResult);
			}

			if (receiveTimeOut < 0) return OperateResult.CreateSuccessResult(new byte[0]);

			// receive msg
			OperateResult<byte[]> resultReceive = ReceiveSolo(socket, false);
			if (!resultReceive.IsSuccess)
			{
				socket?.Close();
				return new OperateResult<byte[]>(resultReceive.Message);
			}

			LogNet?.WriteDebug(ToString(), StringResources.Language.Receive + " : " + SoftBasic.ByteToHexString(resultReceive.Content, ' '));

			// Success
			return OperateResult.CreateSuccessResult(resultReceive.Content);
		}

#if !NET35 && !NET20
		/// <inheritdoc/>
		public override async Task<OperateResult<byte[]>> ReadFromCoreServerAsync(Socket socket, byte[] send, bool hasResponseData = true)
		{
			LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + SoftBasic.ByteToHexString(send, ' '));

			// send
			OperateResult sendResult = await SendAsync(socket, send);
			if (!sendResult.IsSuccess)
			{
				socket?.Close();
				return OperateResult.CreateFailedResult<byte[]>(sendResult);
			}

			if (receiveTimeOut < 0) return OperateResult.CreateSuccessResult(new byte[0]);

			// receive msg
			OperateResult<byte[]> resultReceive = await ReceiveSoloAsync(socket, false);
			if (!resultReceive.IsSuccess)
			{
				socket?.Close();
				return new OperateResult<byte[]>(resultReceive.Message);
			}

			LogNet?.WriteDebug(ToString(), StringResources.Language.Receive + " : " + SoftBasic.ByteToHexString(resultReceive.Content, ' '));

			// Success
			return OperateResult.CreateSuccessResult(resultReceive.Content);
		}
#endif
		#endregion

		#region Object Override

		/// <inheritdoc/>
		public override string ToString() => $"NetworkDeviceSoloBase<{ByteTransform.GetType()}>[{IpAddress}:{Port}]";

		#endregion
	}
}
