using System;
using System.Collections.Generic;
using System.Text;
using Communication.BasicFramework;
using System.Net.Sockets;
using System.Linq;
using System.Linq.Expressions;

namespace Communication
{
	/// <summary>
	/// 扩展的辅助类方法
	/// </summary>
	public static class HslExtension
	{
		/// <inheritdoc cref="SoftBasic.ByteToHexString(byte[])"/>
		public static string ToHexString(this byte[] InBytes) => SoftBasic.ByteToHexString(InBytes);

		/// <inheritdoc cref="SoftBasic.ByteToHexString(byte[], char)"/>
		public static string ToHexString(this byte[] InBytes, char segment) => SoftBasic.ByteToHexString(InBytes, segment);

		/// <inheritdoc cref="SoftBasic.ByteToHexString(byte[], char, int)"/>
		public static string ToHexString(this byte[] InBytes, char segment, int newLineCount) => SoftBasic.ByteToHexString(InBytes, segment, newLineCount);

		/// <inheritdoc cref="SoftBasic.HexStringToBytes( string )"/>
		public static byte[] ToHexBytes(this string value) => SoftBasic.HexStringToBytes(value);

		/// <inheritdoc cref="SoftBasic.BoolOnByteIndex"/>
		public static bool GetBoolOnIndex(this byte value, int offset) => SoftBasic.BoolOnByteIndex(value, offset);

		/// <inheritdoc cref="SoftBasic.BoolArrayToByte"/>
		public static byte[] ToByteArray(this bool[] array) => SoftBasic.BoolArrayToByte(array);

		/// <inheritdoc cref="SoftBasic.ByteToBoolArray(byte[],int)"/>
		public static bool[] ToBoolArray(this byte[] InBytes, int length) => SoftBasic.ByteToBoolArray(InBytes, length);

		/// <inheritdoc cref="SoftBasic.ByteToBoolArray(byte[])"/>
		public static bool[] ToBoolArray(this byte[] InBytes) => SoftBasic.ByteToBoolArray(InBytes);

		/// <inheritdoc cref="SoftBasic.ArrayRemoveDouble"/>
		public static T[] RemoveDouble<T>(this T[] value, int leftLength, int rightLength) => SoftBasic.ArrayRemoveDouble(value, leftLength, rightLength);

		/// <inheritdoc cref="SoftBasic.ArrayRemoveBegin"/>
		public static T[] RemoveBegin<T>(this T[] value, int length) => SoftBasic.ArrayRemoveBegin(value, length);

		/// <inheritdoc cref="SoftBasic.ArrayRemoveLast"/>
		public static T[] RemoveLast<T>(this T[] value, int length) => SoftBasic.ArrayRemoveLast(value, length);

		/// <inheritdoc cref="SoftBasic.ArraySelectMiddle"/>
		public static T[] SelectMiddle<T>(this T[] value, int index, int length) => SoftBasic.ArraySelectMiddle(value, index, length);

		/// <inheritdoc cref="SoftBasic.ArraySelectBegin"/>
		public static T[] SelectBegin<T>(this T[] value, int length) => SoftBasic.ArraySelectBegin(value, length);

		/// <inheritdoc cref="SoftBasic.ArraySelectLast"/>
		public static T[] SelectLast<T>(this T[] value, int length) => SoftBasic.ArraySelectLast(value, length);

		/// <summary>
		/// 将指定的数据添加到数组的每个元素上去，使用表达式树的形式实现，将会修改原数组。不适用byte类型
		/// </summary>
		/// <typeparam name="T">数组的类型</typeparam>
		/// <param name="array">原始数据</param>
		/// <param name="value">数据值</param>
		/// <returns>返回的结果信息</returns>
		public static T[] IncreaseBy<T>(this T[] array, T value)
		{
			ParameterExpression firstArg = Expression.Parameter(typeof(T), "first");
			ParameterExpression secondArg = Expression.Parameter(typeof(T), "second");
			Expression body = Expression.Add(firstArg, secondArg);
			Expression<Func<T, T, T>> adder = Expression.Lambda<Func<T, T, T>>(body, firstArg, secondArg);
			Func<T, T, T> addDelegate = adder.Compile();
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = addDelegate(array[i], value);
			}

			return array;
		}

		/// <summary>
		/// 拷贝当前的实例数组，是基于引用层的浅拷贝，如果类型为值类型，那就是深度拷贝，如果类型为引用类型，就是浅拷贝
		/// </summary>
		/// <typeparam name="T">类型对象</typeparam>
		/// <param name="value">数组对象</param>
		/// <returns>拷贝的结果内容</returns>
		public static T[] CopyArray<T>(this T[] value)
		{
			if (value == null) return null;
			T[] buffer = new T[value.Length];
			Array.Copy(value, buffer, value.Length);
			return buffer;
		}

		/// <inheritdoc cref="SoftBasic.ArrayFormat{T}(T[])"/>
		public static string ToArrayString<T>(this T[] value) => SoftBasic.ArrayFormat(value);

		/// <inheritdoc cref="SoftBasic.ArrayFormat{T}(T, string)"/>
		public static string ToArrayString<T>(this T[] value, string format) => SoftBasic.ArrayFormat(value, format);

		/// <summary>
		/// 将字符串数组转换为实际的数据数组。例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象<br />
		/// Converts a string array into an actual data array. For example, the string format [1,2,3,4,5] can be converted into an actual array object
		/// </summary>
		/// <typeparam name="T">类型对象</typeparam>
		/// <param name="value">字符串数据</param>
		/// <param name="selector">转换方法</param>
		/// <returns>实际的数组</returns>
		public static T[] ToStringArray<T>(this string value, Func<string, T> selector)
		{
			if (value.IndexOf('[') >= 0) value = value.Replace("[", "");
			if (value.IndexOf(']') >= 0) value = value.Replace("]", "");

			string[] splits = value.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
			return splits.Select(selector).ToArray();
		}

		/// <summary>
		/// 将字符串数组转换为实际的数据数组。支持byte,sbyte,bool,short,ushort,int,uint,long,ulong,float,double，使用默认的十进制，例如字符串格式[1,2,3,4,5]，可以转成实际的数组对象<br />
		/// Converts a string array into an actual data array. Support byte, sbyte, bool, short, ushort, int, uint, long, ulong, float, double, use the default decimal, 
		/// such as the string format [1,2,3,4,5], which can be converted into an actual array Object
		/// </summary>
		/// <typeparam name="T">类型对象</typeparam>
		/// <param name="value">字符串数据</param>
		/// <returns>实际的数组</returns>
		public static T[] ToStringArray<T>(this string value)
		{
			Type type = typeof(T);
			if (type == typeof(byte)) return (T[])((object)value.ToStringArray(byte.Parse));
			else if (type == typeof(sbyte)) return (T[])((object)value.ToStringArray(sbyte.Parse));
			else if (type == typeof(bool)) return (T[])((object)value.ToStringArray(bool.Parse));
			else if (type == typeof(short)) return (T[])((object)value.ToStringArray(short.Parse));
			else if (type == typeof(ushort)) return (T[])((object)value.ToStringArray(ushort.Parse));
			else if (type == typeof(int)) return (T[])((object)value.ToStringArray(int.Parse));
			else if (type == typeof(uint)) return (T[])((object)value.ToStringArray(uint.Parse));
			else if (type == typeof(long)) return (T[])((object)value.ToStringArray(long.Parse));
			else if (type == typeof(ulong)) return (T[])((object)value.ToStringArray(ulong.Parse));
			else if (type == typeof(float)) return (T[])((object)value.ToStringArray(float.Parse));
			else if (type == typeof(double)) return (T[])((object)value.ToStringArray(double.Parse));
			else throw new Exception("use ToArray<T>(Func<string,T>) method instead");
		}

		/// <summary>
		/// 启动接收数据，需要传入回调方法，传递对象<br />
		/// To start receiving data, you need to pass in a callback method and pass an object
		/// </summary>
		/// <param name="socket">socket对象</param>
		/// <param name="callback">回调方法</param>
		/// <param name="obj">数据对象</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult BeginReceiveResult(this Socket socket, AsyncCallback callback, object obj)
		{
			try
			{
				socket.BeginReceive(new byte[0], 0, 0, SocketFlags.None, callback, obj);
				return OperateResult.CreateSuccessResult();
			}
			catch (Exception ex)
			{
				socket?.Close();
				return new OperateResult(ex.Message);
			}
		}

		/// <summary>
		/// 启动接收数据，需要传入回调方法，传递对象默认为socket本身<br />
		/// To start receiving data, you need to pass in a callback method. The default object is the socket itself.
		/// </summary>
		/// <param name="socket">socket对象</param>
		/// <param name="callback">回调方法</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult BeginReceiveResult(this Socket socket, AsyncCallback callback)
		{
			return BeginReceiveResult(socket, callback, socket);
		}

		/// <summary>
		/// 结束挂起的异步读取，返回读取的字节数，如果成功的情况。<br />
		/// Ends the pending asynchronous read and returns the number of bytes read, if successful.
		/// </summary>
		/// <param name="socket">socket对象</param>
		/// <param name="ar">回调方法</param>
		/// <returns>是否启动成功</returns>
		public static OperateResult<int> EndReceiveResult(this Socket socket, IAsyncResult ar)
		{
			try
			{
				return OperateResult.CreateSuccessResult(socket.EndReceive(ar));
			}
			catch (Exception ex)
			{
				socket?.Close();
				return new OperateResult<int>(ex.Message);
			}
		}
		/// <summary>
		/// 获取当前数组的倒序数组，这是一个新的实例，不改变原来的数组值<br />
		/// Get the reversed array of the current byte array, this is a new instance, does not change the original array value
		/// </summary>
		/// <param name="value">输入的原始数组</param>
		/// <returns>反转之后的数组信息</returns>
		public static T[] ReverseNew<T>(this T[] value)
		{
			T[] array = value.CopyArray();
			Array.Reverse(array);
			return array;
		}
		/// <summary>
		/// 根据英文小数点进行切割字符串，去除空白的字符<br />
		/// Cut the string according to the English decimal point and remove the blank characters
		/// </summary>
		/// <param name="str">字符串本身</param>
		/// <returns>切割好的字符串数组，例如输入 "100.5"，返回 "100", "5"</returns>
		public static string[] SplitDot(this string str)
		{
			return str.Split(new char[1] { '.' }, StringSplitOptions.RemoveEmptyEntries);
		}
		/// <summary>
		/// 获取当前对象的JSON格式表示的字符串。<br />
		/// Gets the string represented by the JSON format of the current object.
		/// </summary>
		/// <returns>字符串对象</returns>
		public static string ToJsonString(this object obj, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.Indented) => Newtonsoft.Json.JsonConvert.SerializeObject(obj, formatting);

	}
}
