using System;
using System.Net.Sockets;

namespace Communication
{
	/****************************************************************************
	 * 
	 *    应用于一些操作超时请求的判断功能 
	 * 
	 *    When applied to a network connection request timeouts
	 * 
	 ****************************************************************************/

	/// <summary>
	/// 超时操作的类<br />
	/// a class use to indicate the time-out of the connection
	/// </summary>
	public class HslTimeOut
	{
		/// <summary>
		/// 实例化对象
		/// </summary>
		public HslTimeOut()
		{
			StartTime = DateTime.Now;
			IsSuccessful = false;
			IsTimeout = false;
		}

		/// <summary>
		/// 操作的开始时间
		/// </summary>
		public DateTime StartTime { get; set; }

		/// <summary>
		/// 操作是否成功
		/// </summary>
		public bool IsSuccessful { get; set; }

		/// <summary>
		/// 延时的时间，单位毫秒
		/// </summary>
		public int DelayTime { get; set; }

		/// <summary>
		/// 连接超时用的Socket
		/// </summary>
		public Socket WorkSocket { get; set; }

		/// <summary>
		/// 是否发生了超时的操作
		/// </summary>
		public bool IsTimeout { get; set; }

		/// <summary>
		/// 获取到目前为止所花费的时间
		/// </summary>
		/// <returns>时间信息</returns>
		public TimeSpan GetConsumeTime() => DateTime.Now - StartTime;

		/// <inheritdoc/>
		public override string ToString() => $"HslTimeOut[{DelayTime}]";
	}
}
