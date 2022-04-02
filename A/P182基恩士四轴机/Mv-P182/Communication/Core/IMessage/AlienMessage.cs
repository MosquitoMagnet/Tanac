﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Core.IMessage
{
	/// <summary>
	/// 异形消息对象，用于异形客户端的注册包接收以及验证使用
	/// </summary>
	public class AlienMessage : INetMessage
	{
		/// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 5;

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

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
		public bool CheckHeadBytesLegal(byte[] token)
		{
			if (HeadBytes == null)
			{
				return false;
			}
			if (HeadBytes[0] == 72 && HeadBytes[1] == 115 && HeadBytes[2] == 110)
			{
				return true;
			}
			return false;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			return HeadBytes[4];
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
		public int GetHeadBytesIdentity()
		{
			return 0;
		}
	}
}
