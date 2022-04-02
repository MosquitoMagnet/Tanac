using System;
using System.Collections.Generic;
using System.Text;
using Communication.BasicFramework;

namespace Communication.Core
{
	/// <summary>
	/// 按照字节错位的数据转换类<br />
	/// Data conversion class according to byte misalignment
	/// </summary>
	public class ReverseWordTransform : ByteTransformBase
	{
		/// <inheritdoc cref="M:HslCommunication.Core.ByteTransformBase.#ctor" />
		public ReverseWordTransform()
		{
			base.DataFormat = DataFormat.ABCD;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.ByteTransformBase.#ctor(HslCommunication.Core.DataFormat)" />
		public ReverseWordTransform(DataFormat dataFormat)
			: base(dataFormat)
		{
		}

		/// <summary>
		/// 按照字节错位的方法
		/// </summary>
		/// <param name="buffer">实际的字节数据</param>
		/// <param name="index">起始字节位置</param>
		/// <param name="length">数据长度</param>
		/// <returns>处理过的数据信息</returns>
		private byte[] ReverseBytesByWord(byte[] buffer, int index, int length)
		{
			if (buffer == null)
			{
				return null;
			}
			return SoftBasic.BytesReverseByWord(buffer.SelectMiddle(index, length));
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransInt16(System.Byte[],System.Int32)" />
		public override short TransInt16(byte[] buffer, int index)
		{
			return base.TransInt16(ReverseBytesByWord(buffer, index, 2), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransUInt16(System.Byte[],System.Int32)" />
		public override ushort TransUInt16(byte[] buffer, int index)
		{
			return base.TransUInt16(ReverseBytesByWord(buffer, index, 2), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Int16[])" />
		public override byte[] TransByte(short[] values)
		{
			byte[] inBytes = base.TransByte(values);
			return SoftBasic.BytesReverseByWord(inBytes);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.UInt16[])" />
		public override byte[] TransByte(ushort[] values)
		{
			byte[] inBytes = base.TransByte(values);
			return SoftBasic.BytesReverseByWord(inBytes);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"ReverseWordTransform[{base.DataFormat}]";
		}
	}
}
