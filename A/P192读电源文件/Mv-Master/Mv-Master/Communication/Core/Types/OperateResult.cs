using System;
namespace Communication
{
	/// <summary>
	/// 操作结果的类，只带有成功标志和错误信息<br />
	/// The class that operates the result, with only success flags and error messages
	/// </summary>
	/// <remarks>
	/// 当 <see cref="P:HslCommunication.OperateResult.IsSuccess" /> 为 True 时，忽略 <see cref="P:HslCommunication.OperateResult.Message" /> 及 <see cref="P:HslCommunication.OperateResult.ErrorCode" /> 的值
	/// </remarks>
	public class OperateResult
	{
		public bool IsSuccess
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		} = StringResources.Language.UnknownError;


		public int ErrorCode
		{
			get;
			set;
		} = 10000;


		public OperateResult()
		{
		}

		public OperateResult(string msg)
		{
			Message = msg;
		}

		public OperateResult(int err, string msg)
		{
			ErrorCode = err;
			Message = msg;
		}

		public string ToMessageShowString()
		{
			return $"{StringResources.Language.ErrorCode}:{ErrorCode}{Environment.NewLine}{StringResources.Language.TextDescription}:{Message}";
		}

		public void CopyErrorFromOther<TResult>(TResult result) where TResult : OperateResult
		{
			if (result != null)
			{
				ErrorCode = result.ErrorCode;
				Message = result.Message;
			}
		}

		public static OperateResult<T> CreateFailedResult<T>(OperateResult result)
		{
			return new OperateResult<T>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2> CreateFailedResult<T1, T2>(OperateResult result)
		{
			return new OperateResult<T1, T2>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3> CreateFailedResult<T1, T2, T3>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3, T4> CreateFailedResult<T1, T2, T3, T4>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3, T4>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5> CreateFailedResult<T1, T2, T3, T4, T5>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3, T4, T5>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6> CreateFailedResult<T1, T2, T3, T4, T5, T6>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7> CreateFailedResult<T1, T2, T3, T4, T5, T6, T7>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7, T8> CreateFailedResult<T1, T2, T3, T4, T5, T6, T7, T8>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateFailedResult<T1, T2, T3, T4, T5, T6, T7, T8, T9>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateFailedResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(OperateResult result)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
			{
				ErrorCode = result.ErrorCode,
				Message = result.Message
			};
		}

		public static OperateResult CreateSuccessResult()
		{
			return new OperateResult
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText
			};
		}

		public static OperateResult<T> CreateSuccessResult<T>(T value)
		{
			return new OperateResult<T>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content = value
			};
		}

		public static OperateResult<dynamic> CreateSuccessDynamic(dynamic value)
		{
			return new OperateResult<object>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content = value
			};
		}

		public static OperateResult<T1, T2> CreateSuccessResult<T1, T2>(T1 value1, T2 value2)
		{
			return new OperateResult<T1, T2>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2
			};
		}

		public static OperateResult<T1, T2, T3> CreateSuccessResult<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
		{
			return new OperateResult<T1, T2, T3>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3
			};
		}

		public static OperateResult<T1, T2, T3, T4> CreateSuccessResult<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
		{
			return new OperateResult<T1, T2, T3, T4>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3,
				Content4 = value4
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5> CreateSuccessResult<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
		{
			return new OperateResult<T1, T2, T3, T4, T5>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3,
				Content4 = value4,
				Content5 = value5
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6> CreateSuccessResult<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3,
				Content4 = value4,
				Content5 = value5,
				Content6 = value6
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7> CreateSuccessResult<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3,
				Content4 = value4,
				Content5 = value5,
				Content6 = value6,
				Content7 = value7
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7, T8> CreateSuccessResult<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3,
				Content4 = value4,
				Content5 = value5,
				Content6 = value6,
				Content7 = value7,
				Content8 = value8
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9> CreateSuccessResult<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3,
				Content4 = value4,
				Content5 = value5,
				Content6 = value6,
				Content7 = value7,
				Content8 = value8,
				Content9 = value9
			};
		}

		public static OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> CreateSuccessResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8, T9 value9, T10 value10)
		{
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
			{
				IsSuccess = true,
				ErrorCode = 0,
				Message = StringResources.Language.SuccessText,
				Content1 = value1,
				Content2 = value2,
				Content3 = value3,
				Content4 = value4,
				Content5 = value5,
				Content6 = value6,
				Content7 = value7,
				Content8 = value8,
				Content9 = value9,
				Content10 = value10
			};
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带一个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T">泛型类</typeparam>
	public class OperateResult<T> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据
		/// </summary>
		public T Content
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T> Check(Func<T, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content))
			{
				return this;
			}
			return new OperateResult<T>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T> Check(Func<T, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T, OperateResult> func)
		{
			return base.IsSuccess ? func(Content) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带两个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	public class OperateResult<T1, T2> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">可以自由指定的错误信息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2> Check(Func<T1, T2, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2))
			{
				return this;
			}
			return new OperateResult<T1, T2>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2> Check(Func<T1, T2, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带三个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	public class OperateResult<T1, T2, T3> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3> Check(Func<T1, T2, T3, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3> Check(Func<T1, T2, T3, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带四个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	/// <typeparam name="T4">泛型类</typeparam>
	public class OperateResult<T1, T2, T3, T4> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据4
		/// </summary>
		public T4 Content4
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4> Check(Func<T1, T2, T3, T4, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3, Content4))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3, T4>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4> Check(Func<T1, T2, T3, T4, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3, Content4);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3, T4>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, T4, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, T4, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带五个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	/// <typeparam name="T4">泛型类</typeparam>
	/// <typeparam name="T5">泛型类</typeparam>
	public class OperateResult<T1, T2, T3, T4, T5> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据4
		/// </summary>
		public T4 Content4
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据5
		/// </summary>
		public T5 Content5
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5> Check(Func<T1, T2, T3, T4, T5, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3, Content4, Content5))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3, T4, T5>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5> Check(Func<T1, T2, T3, T4, T5, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3, Content4, Content5);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3, T4, T5>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, T4, T5, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, T4, T5, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带六个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	/// <typeparam name="T4">泛型类</typeparam>
	/// <typeparam name="T5">泛型类</typeparam>
	/// <typeparam name="T6">泛型类</typeparam>
	public class OperateResult<T1, T2, T3, T4, T5, T6> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据4
		/// </summary>
		public T4 Content4
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据5
		/// </summary>
		public T5 Content5
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据5
		/// </summary>
		public T6 Content6
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6> Check(Func<T1, T2, T3, T4, T5, T6, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3, Content4, Content5, Content6))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3, T4, T5, T6>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6> Check(Func<T1, T2, T3, T4, T5, T6, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3, Content4, Content5, Content6);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3, T4, T5, T6>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, T4, T5, T6, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, T4, T5, T6, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带七个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	/// <typeparam name="T4">泛型类</typeparam>
	/// <typeparam name="T5">泛型类</typeparam>
	/// <typeparam name="T6">泛型类</typeparam>
	/// <typeparam name="T7">泛型类</typeparam>
	public class OperateResult<T1, T2, T3, T4, T5, T6, T7> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据4
		/// </summary>
		public T4 Content4
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据5
		/// </summary>
		public T5 Content5
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据6
		/// </summary>
		public T6 Content6
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据7
		/// </summary>
		public T7 Content7
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7> Check(Func<T1, T2, T3, T4, T5, T6, T7, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3, Content4, Content5, Content6, Content7))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7> Check(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3, Content4, Content5, Content6, Content7);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3, T4, T5, T6, T7>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, T4, T5, T6, T7, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带八个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	/// <typeparam name="T4">泛型类</typeparam>
	/// <typeparam name="T5">泛型类</typeparam>
	/// <typeparam name="T6">泛型类</typeparam>
	/// <typeparam name="T7">泛型类</typeparam>
	/// <typeparam name="T8">泛型类</typeparam>
	public class OperateResult<T1, T2, T3, T4, T5, T6, T7, T8> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据4
		/// </summary>
		public T4 Content4
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据5
		/// </summary>
		public T5 Content5
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据6
		/// </summary>
		public T6 Content6
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据7
		/// </summary>
		public T7 Content7
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据8
		/// </summary>
		public T8 Content8
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7, T8> Check(Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7, T8> Check(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3, T4, T5, T6, T7, T8>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带九个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	/// <typeparam name="T4">泛型类</typeparam>
	/// <typeparam name="T5">泛型类</typeparam>
	/// <typeparam name="T6">泛型类</typeparam>
	/// <typeparam name="T7">泛型类</typeparam>
	/// <typeparam name="T8">泛型类</typeparam>
	/// <typeparam name="T9">泛型类</typeparam>
	public class OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据4
		/// </summary>
		public T4 Content4
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据5
		/// </summary>
		public T5 Content5
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据6
		/// </summary>
		public T6 Content6
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据7
		/// </summary>
		public T7 Content7
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据8
		/// </summary>
		public T8 Content8
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据9
		/// </summary>
		public T9 Content9
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9> Check(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9> Check(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3, T4, T5, T6, T7, T8, T9>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
	/// <summary>
	/// 操作结果的泛型类，允许带十个用户自定义的泛型对象，推荐使用这个类
	/// </summary>
	/// <typeparam name="T1">泛型类</typeparam>
	/// <typeparam name="T2">泛型类</typeparam>
	/// <typeparam name="T3">泛型类</typeparam>
	/// <typeparam name="T4">泛型类</typeparam>
	/// <typeparam name="T5">泛型类</typeparam>
	/// <typeparam name="T6">泛型类</typeparam>
	/// <typeparam name="T7">泛型类</typeparam>
	/// <typeparam name="T8">泛型类</typeparam>
	/// <typeparam name="T9">泛型类</typeparam>
	/// <typeparam name="T10">泛型类</typeparam>
	public class OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : OperateResult
	{
		/// <summary>
		/// 用户自定义的泛型数据1
		/// </summary>
		public T1 Content1
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据2
		/// </summary>
		public T2 Content2
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据3
		/// </summary>
		public T3 Content3
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据4
		/// </summary>
		public T4 Content4
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据5
		/// </summary>
		public T5 Content5
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据6
		/// </summary>
		public T6 Content6
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据7
		/// </summary>
		public T7 Content7
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据8
		/// </summary>
		public T8 Content8
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据9
		/// </summary>
		public T9 Content9
		{
			get;
			set;
		}

		/// <summary>
		/// 用户自定义的泛型数据10
		/// </summary>
		public T10 Content10
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的结果对象
		/// </summary>
		public OperateResult()
		{
		}

		/// <summary>
		/// 使用指定的消息实例化一个默认的结果对象
		/// </summary>
		/// <param name="msg">错误消息</param>
		public OperateResult(string msg)
			: base(msg)
		{
		}

		/// <summary>
		/// 使用错误代码，消息文本来实例化对象
		/// </summary>
		/// <param name="err">错误代码</param>
		/// <param name="msg">错误消息</param>
		public OperateResult(int err, string msg)
			: base(err, msg)
		{
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <param name="message">检查失败的错误消息</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Check(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool> check, string message = "All content data check failed")
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			if (check(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10))
			{
				return this;
			}
			return new OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(message);
		}

		/// <summary>
		/// 返回一个检查结果对象，可以进行自定义的数据检查。<br />
		/// Returns a check result object that allows you to perform custom data checks.
		/// </summary>
		/// <param name="check">检查的委托方法</param>
		/// <returns>如果检查成功，则返回对象本身，如果失败，返回错误信息。</returns>
		public OperateResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Check(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult> check)
		{
			if (!base.IsSuccess)
			{
				return this;
			}
			OperateResult operateResult = check(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(operateResult);
			}
			return this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult Then(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : this;
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。
		/// </summary>
		/// <typeparam name="TResult">泛型参数</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult> Then<TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2> Then<TResult1, TResult2>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3> Then<TResult1, TResult2, TResult3>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4> Then<TResult1, TResult2, TResult3, TResult4>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3, TResult4>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5> Then<TResult1, TResult2, TResult3, TResult4, TResult5>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9>(this);
		}

		/// <summary>
		/// 指定接下来要做的是内容，当前对象如果成功，就返回接下来的执行结果，如果失败，就返回当前对象本身。<br />
		/// Specify what you want to do next, return the result of the execution of the current object if it succeeds, and return the current object itself if it fails.
		/// </summary>
		/// <typeparam name="TResult1">泛型参数一</typeparam>
		/// <typeparam name="TResult2">泛型参数二</typeparam>
		/// <typeparam name="TResult3">泛型参数三</typeparam>
		/// <typeparam name="TResult4">泛型参数四</typeparam>
		/// <typeparam name="TResult5">泛型参数五</typeparam>
		/// <typeparam name="TResult6">泛型参数六</typeparam>
		/// <typeparam name="TResult7">泛型参数七</typeparam>
		/// <typeparam name="TResult8">泛型参数八</typeparam>
		/// <typeparam name="TResult9">泛型参数九</typeparam>
		/// <typeparam name="TResult10">泛型参数十</typeparam>
		/// <param name="func">等待当前对象成功后执行的内容</param>
		/// <returns>返回整个方法链最终的成功失败结果</returns>
		public OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10> Then<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, OperateResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>> func)
		{
			return base.IsSuccess ? func(Content1, Content2, Content3, Content4, Content5, Content6, Content7, Content8, Content9, Content10) : OperateResult.CreateFailedResult<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6, TResult7, TResult8, TResult9, TResult10>(this);
		}
	}
}
