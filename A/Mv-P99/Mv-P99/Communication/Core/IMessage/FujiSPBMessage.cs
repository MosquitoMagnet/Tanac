﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Core.IMessage
{
	/// <summary>
	/// 富士SPB的消息内容
	/// </summary>
	public class FujiSPBMessage : INetMessage
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

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			if (HeadBytes == null)
			{
				return 0;
			}
			return Convert.ToInt32(Encoding.ASCII.GetString(HeadBytes, 3, 2), 16) * 2 + 2;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.CheckHeadBytesLegal(System.Byte[])" />
		public bool CheckHeadBytesLegal(byte[] token)
		{
			return true;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
		public int GetHeadBytesIdentity()
		{
			return 0;
		}
	}
}
