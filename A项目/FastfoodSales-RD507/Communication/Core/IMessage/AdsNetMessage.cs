using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Core.IMessage
{
	/// <summary>
	/// 倍福的ADS协议的信息
	/// </summary>
	public class AdsNetMessage : INetMessage
	{
		/// <inheritdoc cref="P:HslCommunication.Core.IMessage.INetMessage.ProtocolHeadBytesLength" />
		public int ProtocolHeadBytesLength => 6;

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

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.AdsNetMessage.CheckHeadBytesLegal(System.Byte[])" />
		public bool CheckHeadBytesLegal(byte[] token)
		{
			if (HeadBytes == null)
			{
				return false;
			}
			return true;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetContentLengthByHeadBytes" />
		public int GetContentLengthByHeadBytes()
		{
			byte[] headBytes = HeadBytes;
			if (headBytes != null && headBytes.Length >= 6)
			{
				return BitConverter.ToInt32(HeadBytes, 2);
			}
			return 0;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IMessage.INetMessage.GetHeadBytesIdentity" />
		public int GetHeadBytesIdentity()
		{
			return 0;
		}
	}
}
