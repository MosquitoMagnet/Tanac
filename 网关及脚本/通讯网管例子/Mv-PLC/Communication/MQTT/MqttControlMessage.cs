using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.MQTT
{
	/// <summary>
	/// 定义了Mqtt的相关的控制报文的信息
	/// </summary>
	public class MqttControlMessage
	{
		/// <summary>
		/// 操作失败的信息返回
		/// </summary>
		public const byte FAILED = 0x00;

		/// <summary>
		/// 连接标识
		/// </summary>
		public const byte CONNECT = 0x01;

		/// <summary>
		/// 连接返回的标识
		/// </summary>
		public const byte CONNACK = 0x02;

		/// <summary>
		/// 发布消息
		/// </summary>
		public const byte PUBLISH = 0x03;

		/// <summary>
		/// QoS 1消息发布收到确认
		/// </summary>
		public const byte PUBACK = 0x04;

		/// <summary>
		/// 发布收到（保证交付第一步）
		/// </summary>
		public const byte PUBREC = 0x05;

		/// <summary>
		/// 发布释放（保证交付第二步）
		/// </summary>
		public const byte PUBREL = 0x06;

		/// <summary>
		/// QoS 2消息发布完成（保证交互第三步）
		/// </summary>
		public const byte PUBCOMP = 0x07;

		/// <summary>
		/// 客户端订阅请求
		/// </summary>
		public const byte SUBSCRIBE = 0x08;

		/// <summary>
		/// 订阅请求报文确认
		/// </summary>
		public const byte SUBACK = 0x09;

		/// <summary>
		/// 客户端取消订阅请求
		/// </summary>
		public const byte UNSUBSCRIBE = 0x0A;

		/// <summary>
		/// 取消订阅报文确认
		/// </summary>
		public const byte UNSUBACK = 0x0B;

		/// <summary>
		/// 心跳请求
		/// </summary>
		public const byte PINGREQ = 0x0C;

		/// <summary>
		/// 心跳响应
		/// </summary>
		public const byte PINGRESP = 0x0D;

		/// <summary>
		/// 客户端断开连接
		/// </summary>
		public const byte DISCONNECT = 0x0E;

		/// <summary>
		/// 报告进度
		/// </summary>
		public const byte REPORTPROGRESS = 0x0F;
	}
}
