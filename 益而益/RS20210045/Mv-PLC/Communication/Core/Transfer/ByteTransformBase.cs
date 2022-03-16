using System;
using System.Text;
using Communication.BasicFramework;

namespace Communication.Core
{
	/// <summary>
	/// 数据转换类的基础，提供了一些基础的方法实现.<br />
	/// The basis of the data conversion class provides some basic method implementations.
	/// </summary>
	public class ByteTransformBase : IByteTransform
	{
		/// <inheritdoc cref="P:HslCommunication.Core.IByteTransform.DataFormat" />
		public DataFormat DataFormat { get; set; }

		/// <inheritdoc cref="P:HslCommunication.Core.IByteTransform.IsStringReverseByteWord" />
		public bool IsStringReverseByteWord { get; set; }

		/// <summary>
		/// 实例化一个默认的对象<br />
		/// Instantiate a default object
		/// </summary>
		public ByteTransformBase()
		{
			DataFormat = DataFormat.DCBA;
		}

		/// <summary>
		/// 使用指定的数据解析来实例化对象<br />
		/// Instantiate the object using the specified data parsing
		/// </summary>
		/// <param name="dataFormat">数据规则</param>
		public ByteTransformBase(DataFormat dataFormat)
		{
			DataFormat = dataFormat;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransBool(System.Byte[],System.Int32)" />
		public virtual bool TransBool(byte[] buffer, int index)
		{
			return (buffer[index] & 1) == 1;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransBool(System.Byte[],System.Int32,System.Int32)" />
		public bool[] TransBool(byte[] buffer, int index, int length)
		{
			byte[] array = new byte[length];
			Array.Copy(buffer, index, array, 0, length);
			return SoftBasic.ByteToBoolArray(array, length * 8);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Byte[],System.Int32)" />
		public virtual byte TransByte(byte[] buffer, int index)
		{
			return buffer[index];
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Byte[],System.Int32,System.Int32)" />
		public virtual byte[] TransByte(byte[] buffer, int index, int length)
		{
			byte[] array = new byte[length];
			Array.Copy(buffer, index, array, 0, length);
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransInt16(System.Byte[],System.Int32)" />
		public virtual short TransInt16(byte[] buffer, int index)
		{
			return BitConverter.ToInt16(buffer, index);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransInt16(System.Byte[],System.Int32,System.Int32)" />
		public virtual short[] TransInt16(byte[] buffer, int index, int length)
		{
			short[] array = new short[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransInt16(buffer, index + 2 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransUInt16(System.Byte[],System.Int32)" />
		public virtual ushort TransUInt16(byte[] buffer, int index)
		{
			return BitConverter.ToUInt16(buffer, index);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransUInt16(System.Byte[],System.Int32,System.Int32)" />
		public virtual ushort[] TransUInt16(byte[] buffer, int index, int length)
		{
			ushort[] array = new ushort[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransUInt16(buffer, index + 2 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransInt32(System.Byte[],System.Int32)" />
		public virtual int TransInt32(byte[] buffer, int index)
		{
			return BitConverter.ToInt32(ByteTransDataFormat4(buffer, index), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransInt32(System.Byte[],System.Int32,System.Int32)" />
		public virtual int[] TransInt32(byte[] buffer, int index, int length)
		{
			int[] array = new int[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransInt32(buffer, index + 4 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransUInt32(System.Byte[],System.Int32)" />
		public virtual uint TransUInt32(byte[] buffer, int index)
		{
			return BitConverter.ToUInt32(ByteTransDataFormat4(buffer, index), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransUInt32(System.Byte[],System.Int32,System.Int32)" />
		public virtual uint[] TransUInt32(byte[] buffer, int index, int length)
		{
			uint[] array = new uint[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransUInt32(buffer, index + 4 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransInt64(System.Byte[],System.Int32)" />
		public virtual long TransInt64(byte[] buffer, int index)
		{
			return BitConverter.ToInt64(ByteTransDataFormat8(buffer, index), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransInt64(System.Byte[],System.Int32,System.Int32)" />
		public virtual long[] TransInt64(byte[] buffer, int index, int length)
		{
			long[] array = new long[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransInt64(buffer, index + 8 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransUInt64(System.Byte[],System.Int32)" />
		public virtual ulong TransUInt64(byte[] buffer, int index)
		{
			return BitConverter.ToUInt64(ByteTransDataFormat8(buffer, index), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransUInt64(System.Byte[],System.Int32,System.Int32)" />
		public virtual ulong[] TransUInt64(byte[] buffer, int index, int length)
		{
			ulong[] array = new ulong[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransUInt64(buffer, index + 8 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransSingle(System.Byte[],System.Int32)" />
		public virtual float TransSingle(byte[] buffer, int index)
		{
			return BitConverter.ToSingle(ByteTransDataFormat4(buffer, index), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransSingle(System.Byte[],System.Int32,System.Int32)" />
		public virtual float[] TransSingle(byte[] buffer, int index, int length)
		{
			float[] array = new float[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransSingle(buffer, index + 4 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransDouble(System.Byte[],System.Int32)" />
		public virtual double TransDouble(byte[] buffer, int index)
		{
			return BitConverter.ToDouble(ByteTransDataFormat8(buffer, index), 0);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransDouble(System.Byte[],System.Int32,System.Int32)" />
		public virtual double[] TransDouble(byte[] buffer, int index, int length)
		{
			double[] array = new double[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = TransDouble(buffer, index + 8 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransString(System.Byte[],System.Int32,System.Int32,System.Text.Encoding)" />
		public virtual string TransString(byte[] buffer, int index, int length, Encoding encoding)
		{
			byte[] array = TransByte(buffer, index, length);
			if (IsStringReverseByteWord)
			{
				return encoding.GetString(SoftBasic.BytesReverseByWord(array));
			}
			return encoding.GetString(array);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransString(System.Byte[],System.Text.Encoding)" />
		public virtual string TransString(byte[] buffer, Encoding encoding)
		{
			return encoding.GetString(buffer);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Boolean)" />
		public virtual byte[] TransByte(bool value)
		{
			return TransByte(new bool[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Boolean[])" />
		public virtual byte[] TransByte(bool[] values)
		{
			return (values == null) ? null : SoftBasic.BoolArrayToByte(values);
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Byte)" />
		public virtual byte[] TransByte(byte value)
		{
			return new byte[1] { value };
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Int16)" />
		public virtual byte[] TransByte(short value)
		{
			return TransByte(new short[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Int16[])" />
		public virtual byte[] TransByte(short[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 2];
			for (int i = 0; i < values.Length; i++)
			{
				BitConverter.GetBytes(values[i]).CopyTo(array, 2 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.UInt16)" />
		public virtual byte[] TransByte(ushort value)
		{
			return TransByte(new ushort[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.UInt16[])" />
		public virtual byte[] TransByte(ushort[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 2];
			for (int i = 0; i < values.Length; i++)
			{
				BitConverter.GetBytes(values[i]).CopyTo(array, 2 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Int32)" />
		public virtual byte[] TransByte(int value)
		{
			return TransByte(new int[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Int32[])" />
		public virtual byte[] TransByte(int[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 4];
			for (int i = 0; i < values.Length; i++)
			{
				ByteTransDataFormat4(BitConverter.GetBytes(values[i])).CopyTo(array, 4 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.UInt32)" />
		public virtual byte[] TransByte(uint value)
		{
			return TransByte(new uint[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.UInt32[])" />
		public virtual byte[] TransByte(uint[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 4];
			for (int i = 0; i < values.Length; i++)
			{
				ByteTransDataFormat4(BitConverter.GetBytes(values[i])).CopyTo(array, 4 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Int64)" />
		public virtual byte[] TransByte(long value)
		{
			return TransByte(new long[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Int64[])" />
		public virtual byte[] TransByte(long[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 8];
			for (int i = 0; i < values.Length; i++)
			{
				ByteTransDataFormat8(BitConverter.GetBytes(values[i])).CopyTo(array, 8 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.UInt64)" />
		public virtual byte[] TransByte(ulong value)
		{
			return TransByte(new ulong[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.UInt64[])" />
		public virtual byte[] TransByte(ulong[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 8];
			for (int i = 0; i < values.Length; i++)
			{
				ByteTransDataFormat8(BitConverter.GetBytes(values[i])).CopyTo(array, 8 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Single)" />
		public virtual byte[] TransByte(float value)
		{
			return TransByte(new float[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Single[])" />
		public virtual byte[] TransByte(float[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 4];
			for (int i = 0; i < values.Length; i++)
			{
				ByteTransDataFormat4(BitConverter.GetBytes(values[i])).CopyTo(array, 4 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Double)" />
		public virtual byte[] TransByte(double value)
		{
			return TransByte(new double[1] { value });
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.Double[])" />
		public virtual byte[] TransByte(double[] values)
		{
			if (values == null)
			{
				return null;
			}
			byte[] array = new byte[values.Length * 8];
			for (int i = 0; i < values.Length; i++)
			{
				ByteTransDataFormat8(BitConverter.GetBytes(values[i])).CopyTo(array, 8 * i);
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.String,System.Text.Encoding)" />
		public virtual byte[] TransByte(string value, Encoding encoding)
		{
			if (value == null)
			{
				return null;
			}
			byte[] bytes = encoding.GetBytes(value);
			return IsStringReverseByteWord ? SoftBasic.BytesReverseByWord(bytes) : bytes;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.TransByte(System.String,System.Int32,System.Text.Encoding)" />
		public virtual byte[] TransByte(string value, int length, Encoding encoding)
		{
			if (value == null)
			{
				return null;
			}
			byte[] bytes = encoding.GetBytes(value);
			return IsStringReverseByteWord ? SoftBasic.ArrayExpandToLength(SoftBasic.BytesReverseByWord(bytes), length) : SoftBasic.ArrayExpandToLength(bytes, length);
		}

		/// <summary>
		/// 反转多字节的数据信息
		/// </summary>
		/// <param name="value">数据字节</param>
		/// <param name="index">起始索引，默认值为0</param>
		/// <returns>实际字节信息</returns>
		protected byte[] ByteTransDataFormat4(byte[] value, int index = 0)
		{
			byte[] array = new byte[4];
			switch (DataFormat)
			{
				case DataFormat.ABCD:
					array[0] = value[index + 3];
					array[1] = value[index + 2];
					array[2] = value[index + 1];
					array[3] = value[index];
					break;
				case DataFormat.BADC:
					array[0] = value[index + 2];
					array[1] = value[index + 3];
					array[2] = value[index];
					array[3] = value[index + 1];
					break;
				case DataFormat.CDAB:
					array[0] = value[index + 1];
					array[1] = value[index];
					array[2] = value[index + 3];
					array[3] = value[index + 2];
					break;
				case DataFormat.DCBA:
					array[0] = value[index];
					array[1] = value[index + 1];
					array[2] = value[index + 2];
					array[3] = value[index + 3];
					break;
			}
			return array;
		}

		/// <summary>
		/// 反转多字节的数据信息
		/// </summary>
		/// <param name="value">数据字节</param>
		/// <param name="index">起始索引，默认值为0</param>
		/// <returns>实际字节信息</returns>
		protected byte[] ByteTransDataFormat8(byte[] value, int index = 0)
		{
			byte[] array = new byte[8];
			switch (DataFormat)
			{
				case DataFormat.ABCD:
					array[0] = value[index + 7];
					array[1] = value[index + 6];
					array[2] = value[index + 5];
					array[3] = value[index + 4];
					array[4] = value[index + 3];
					array[5] = value[index + 2];
					array[6] = value[index + 1];
					array[7] = value[index];
					break;
				case DataFormat.BADC:
					array[0] = value[index + 6];
					array[1] = value[index + 7];
					array[2] = value[index + 4];
					array[3] = value[index + 5];
					array[4] = value[index + 2];
					array[5] = value[index + 3];
					array[6] = value[index];
					array[7] = value[index + 1];
					break;
				case DataFormat.CDAB:
					array[0] = value[index + 1];
					array[1] = value[index];
					array[2] = value[index + 3];
					array[3] = value[index + 2];
					array[4] = value[index + 5];
					array[5] = value[index + 4];
					array[6] = value[index + 7];
					array[7] = value[index + 6];
					break;
				case DataFormat.DCBA:
					array[0] = value[index];
					array[1] = value[index + 1];
					array[2] = value[index + 2];
					array[3] = value[index + 3];
					array[4] = value[index + 4];
					array[5] = value[index + 5];
					array[6] = value[index + 6];
					array[7] = value[index + 7];
					break;
			}
			return array;
		}

		/// <inheritdoc cref="M:HslCommunication.Core.IByteTransform.CreateByDateFormat(HslCommunication.Core.DataFormat)" />
		public virtual IByteTransform CreateByDateFormat(DataFormat dataFormat)
		{
			return this;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"ByteTransformBase[{DataFormat}]";
		}
	}
}
