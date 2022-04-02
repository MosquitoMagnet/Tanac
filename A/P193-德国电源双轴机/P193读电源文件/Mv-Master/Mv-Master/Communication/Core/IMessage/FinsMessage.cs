﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Core.IMessage
{
	/// <summary>
	/// 用于欧姆龙通信的Fins协议的消息解析规则
	/// </summary>
	public class FinsMessage : INetMessage
	{
		/// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 8;

		/// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.HeadBytes" />
		public byte[] HeadBytes
		{
			get;
			set;
		}

		/// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ContentBytes" />
		public byte[] ContentBytes
		{
			get;
			set;
		}

		/// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.SendBytes" />
		public byte[] SendBytes
		{
			get;
			set;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return BitConverter.ToInt32(new byte[4]
			{
			HeadBytes[7],
			HeadBytes[6],
			HeadBytes[5],
			HeadBytes[4]
			}, 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
		public bool CheckHeadBytesLegal(byte[] token)
		{
			if (HeadBytes == null)
			{
				return false;
			}
			if (HeadBytes[0] == 70 && HeadBytes[1] == 73 && HeadBytes[2] == 78 && HeadBytes[3] == 83)
			{
				return true;
			}
			return false;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
		public int GetHeadBytesIdentity()
		{
			return 0;
		}
	}
}
