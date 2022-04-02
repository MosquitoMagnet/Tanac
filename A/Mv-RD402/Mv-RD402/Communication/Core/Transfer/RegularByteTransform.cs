using System;
using System.Collections.Generic;
using System.Text;

namespace Communication.Core
{
	/// <summary>
	/// 常规的字节转换类<br />
	/// Regular byte conversion class
	/// </summary>
	public class RegularByteTransform : ByteTransformBase
	{
		/// <inheritdoc cref="M:HslCommunication.Core.ByteTransformBase.#ctor" />
		public RegularByteTransform()
		{
		}

		/// <inheritdoc cref="M:HslCommunication.Core.ByteTransformBase.#ctor(HslCommunication.Core.DataFormat)" />
		public RegularByteTransform(DataFormat dataFormat)
			: base(dataFormat)
		{
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"RegularByteTransform[{base.DataFormat}]";
		}
	}
}
