using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Communication.Core
{
	#region 多线程同步协调类
	/// <summary>
	/// 线程的协调逻辑状态
	/// </summary>
	internal enum CoordinationStatus
	{
		/// <summary>
		/// 所有项完成
		/// </summary>
		AllDone,
		/// <summary>
		/// 超时
		/// </summary>
		Timeout,
		/// <summary>
		/// 任务取消
		/// </summary>
		Cancel
	}
	/// <summary>
	/// 一个线程协调逻辑类，详细参考书籍《CLR Via C#》page:681
	/// 这个类可惜没有报告进度的功能
	/// </summary>
	internal sealed class AsyncCoordinator
	{
		private int m_opCount = 1;

		private int m_statusReported = 0;

		private Action<CoordinationStatus> m_callback;

		private Timer m_timer;

		/// <summary>
		/// 每次的操作任务开始前必须调用该方法
		/// </summary>
		/// <param name="opsToAdd"></param>
		public void AboutToBegin(int opsToAdd = 1)
		{
			Interlocked.Add(ref m_opCount, opsToAdd);
		}

		/// <summary>
		/// 在一次任务处理好操作之后，必须调用该方法
		/// </summary>
		public void JustEnded()
		{
			if (Interlocked.Decrement(ref m_opCount) == 0)
			{
				ReportStatus(CoordinationStatus.AllDone);
			}
		}

		/// <summary>
		/// 该方法必须在发起所有的操作之后调用
		/// </summary>
		/// <param name="callback">回调方法</param>
		/// <param name="timeout">超时时间</param>
		public void AllBegun(Action<CoordinationStatus> callback, int timeout = -1)
		{
			m_callback = callback;
			if (timeout != -1)
			{
				m_timer = new Timer(TimeExpired, null, timeout, -1);
			}
			JustEnded();
		}

		/// <summary>
		/// 超时的方法
		/// </summary>
		/// <param name="o"></param>
		private void TimeExpired(object o)
		{
			ReportStatus(CoordinationStatus.Timeout);
		}

		/// <summary>
		/// 取消任务的执行
		/// </summary>
		public void Cancel()
		{
			ReportStatus(CoordinationStatus.Cancel);
		}

		/// <summary>
		/// 生成一次报告
		/// </summary>
		/// <param name="status">报告的状态</param>
		private void ReportStatus(CoordinationStatus status)
		{
			if (Interlocked.Exchange(ref m_statusReported, 1) == 0)
			{
				m_callback(status);
			}
		}

		/// <summary>
		/// 乐观的并发方法模型，具体参照《CLR Via C#》page:686
		/// </summary>
		/// <param name="target">唯一的目标数据</param>
		/// <param name="change">修改数据的算法</param>
		/// <returns></returns>
		public static int Maxinum(ref int target, Func<int, int> change)
		{
			int num = target;
			int num2;
			int num3;
			do
			{
				num2 = num;
				num3 = change(num2);
				num = Interlocked.CompareExchange(ref target, num3, num2);
			}
			while (num2 != num);
			return num3;
		}
	}
	#endregion
	#region 乐观并发模型的协调类
	/// <summary>
	/// 一个用于高性能，乐观并发模型控制操作的类，允许一个方法(隔离方法)的安全单次执行
	/// </summary>
	public sealed class HslAsyncCoordinator
	{
		private Action action = null;

		private int OperaterStatus = 0;

		private long Target = 0L;

		/// <summary>
		/// 实例化一个对象，需要传入隔离执行的方法
		/// </summary>
		/// <param name="operater">隔离执行的方法</param>
		public HslAsyncCoordinator(Action operater)
		{
			action = operater;
		}

		/// <summary>
		/// 启动线程池执行隔离方法
		/// </summary>
		public void StartOperaterInfomation()
		{
			Interlocked.Increment(ref Target);
			if (Interlocked.CompareExchange(ref OperaterStatus, 1, 0) == 0)
			{
				ThreadPool.QueueUserWorkItem(ThreadPoolOperater, null);
			}
		}

		private void ThreadPoolOperater(object obj)
		{
			long num = Target;
			long num2 = 0L;
			long num3;
			do
			{
				num3 = num;
				action?.Invoke();
				num = Interlocked.CompareExchange(ref Target, num2, num3);
			}
			while (num3 != num);
			Interlocked.Exchange(ref OperaterStatus, 0);
			if (Target != num2)
			{
				StartOperaterInfomation();
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return "HslAsyncCoordinator";
		}
	}

	#endregion
	#region 高性能的读写锁
	/// <summary>
	/// 一个高性能的读写锁，支持写锁定，读灵活，读时写锁定，写时读锁定
	/// </summary>
	public sealed class HslReadWriteLock : IDisposable
	{
		private enum OneManyLockStates
		{
			Free,
			OwnedByWriter,
			OwnedByReaders,
			OwnedByReadersAndWriterPending,
			ReservedForWriter
		}

		private const int c_lsStateStartBit = 0;

		private const int c_lsReadersReadingStartBit = 3;

		private const int c_lsReadersWaitingStartBit = 12;

		private const int c_lsWritersWaitingStartBit = 21;

		private const int c_lsStateMask = 7;

		private const int c_lsReadersReadingMask = 4088;

		private const int c_lsReadersWaitingMask = 2093056;

		private const int c_lsWritersWaitingMask = 1071644672;

		private const int c_lsAnyWaitingMask = 1073737728;

		private const int c_ls1ReaderReading = 8;

		private const int c_ls1ReaderWaiting = 4096;

		private const int c_ls1WriterWaiting = 2097152;

		private int m_LockState = 0;

		private Semaphore m_ReadersLock = new Semaphore(0, int.MaxValue);

		private Semaphore m_WritersLock = new Semaphore(0, int.MaxValue);

		private bool disposedValue = false;

		private bool m_exclusive;

		private static OneManyLockStates State(int ls)
		{
			return (OneManyLockStates)(ls & 7);
		}

		private static void SetState(ref int ls, OneManyLockStates newState)
		{
			ls = ((ls & -8) | (int)newState);
		}

		private static int NumReadersReading(int ls)
		{
			return (ls & 0xFF8) >> 3;
		}

		private static void AddReadersReading(ref int ls, int amount)
		{
			ls += 8 * amount;
		}

		private static int NumReadersWaiting(int ls)
		{
			return (ls & 0x1FF000) >> 12;
		}

		private static void AddReadersWaiting(ref int ls, int amount)
		{
			ls += 4096 * amount;
		}

		private static int NumWritersWaiting(int ls)
		{
			return (ls & 0x3FE00000) >> 21;
		}

		private static void AddWritersWaiting(ref int ls, int amount)
		{
			ls += 2097152 * amount;
		}

		private static bool AnyWaiters(int ls)
		{
			return (ls & 0x3FFFF000) != 0;
		}

		private static string DebugState(int ls)
		{
			return string.Format(CultureInfo.InvariantCulture, "State={0}, RR={1}, RW={2}, WW={3}", State(ls), NumReadersReading(ls), NumReadersWaiting(ls), NumWritersWaiting(ls));
		}

		/// <summary>
		/// 返回本对象的描述字符串
		/// </summary>
		/// <returns>对象的描述字符串</returns>
		public override string ToString()
		{
			return DebugState(m_LockState);
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				m_WritersLock.Close();
				m_WritersLock = null;
				m_ReadersLock.Close();
				m_ReadersLock = null;
				disposedValue = true;
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
		}

		/// <summary>
		/// 根据读写情况请求锁
		/// </summary>
		/// <param name="exclusive">True为写请求，False为读请求</param>
		public void Enter(bool exclusive)
		{
			if (exclusive)
			{
				while (WaitToWrite(ref m_LockState))
				{
					m_WritersLock.WaitOne();
				}
			}
			else
			{
				while (WaitToRead(ref m_LockState))
				{
					m_ReadersLock.WaitOne();
				}
			}
			m_exclusive = exclusive;
		}

		private static bool WaitToWrite(ref int target)
		{
			int num = target;
			int num2;
			bool result;
			do
			{
				num2 = num;
				int ls = num2;
				result = false;
				switch (State(ls))
				{
					case OneManyLockStates.Free:
					case OneManyLockStates.ReservedForWriter:
						SetState(ref ls, OneManyLockStates.OwnedByWriter);
						break;
					case OneManyLockStates.OwnedByWriter:
						AddWritersWaiting(ref ls, 1);
						result = true;
						break;
					case OneManyLockStates.OwnedByReaders:
					case OneManyLockStates.OwnedByReadersAndWriterPending:
						SetState(ref ls, OneManyLockStates.OwnedByReadersAndWriterPending);
						AddWritersWaiting(ref ls, 1);
						result = true;
						break;
					default:
						Debug.Assert(condition: false, "Invalid Lock state");
						break;
				}
				num = Interlocked.CompareExchange(ref target, ls, num2);
			}
			while (num2 != num);
			return result;
		}

		/// <summary>
		/// 释放锁，将根据锁状态自动区分读写锁
		/// </summary>
		public void Leave()
		{
			int num;
			if (m_exclusive)
			{
				Debug.Assert(State(m_LockState) == OneManyLockStates.OwnedByWriter && NumReadersReading(m_LockState) == 0);
				num = DoneWriting(ref m_LockState);
			}
			else
			{
				OneManyLockStates oneManyLockStates = State(m_LockState);
				Debug.Assert(State(m_LockState) == OneManyLockStates.OwnedByReaders || State(m_LockState) == OneManyLockStates.OwnedByReadersAndWriterPending);
				num = DoneReading(ref m_LockState);
			}
			if (num == -1)
			{
				m_WritersLock.Release();
			}
			else if (num > 0)
			{
				m_ReadersLock.Release(num);
			}
		}

		private static int DoneWriting(ref int target)
		{
			int num = target;
			int num2 = 0;
			int num3;
			do
			{
				int ls = num3 = num;
				if (!AnyWaiters(ls))
				{
					SetState(ref ls, OneManyLockStates.Free);
					num2 = 0;
				}
				else if (NumWritersWaiting(ls) > 0)
				{
					SetState(ref ls, OneManyLockStates.ReservedForWriter);
					AddWritersWaiting(ref ls, -1);
					num2 = -1;
				}
				else
				{
					num2 = NumReadersWaiting(ls);
					Debug.Assert(num2 > 0);
					SetState(ref ls, OneManyLockStates.OwnedByReaders);
					AddReadersWaiting(ref ls, -num2);
				}
				num = Interlocked.CompareExchange(ref target, ls, num3);
			}
			while (num3 != num);
			return num2;
		}

		private static bool WaitToRead(ref int target)
		{
			int num = target;
			int num2;
			bool result;
			do
			{
				int ls = num2 = num;
				result = false;
				switch (State(ls))
				{
					case OneManyLockStates.Free:
						SetState(ref ls, OneManyLockStates.OwnedByReaders);
						AddReadersReading(ref ls, 1);
						break;
					case OneManyLockStates.OwnedByReaders:
						AddReadersReading(ref ls, 1);
						break;
					case OneManyLockStates.OwnedByWriter:
					case OneManyLockStates.OwnedByReadersAndWriterPending:
					case OneManyLockStates.ReservedForWriter:
						AddReadersWaiting(ref ls, 1);
						result = true;
						break;
					default:
						Debug.Assert(condition: false, "Invalid Lock state");
						break;
				}
				num = Interlocked.CompareExchange(ref target, ls, num2);
			}
			while (num2 != num);
			return result;
		}

		private static int DoneReading(ref int target)
		{
			int num = target;
			int num2;
			int result;
			do
			{
				int ls = num2 = num;
				AddReadersReading(ref ls, -1);
				if (NumReadersReading(ls) > 0)
				{
					result = 0;
				}
				else if (!AnyWaiters(ls))
				{
					SetState(ref ls, OneManyLockStates.Free);
					result = 0;
				}
				else
				{
					Debug.Assert(NumWritersWaiting(ls) > 0);
					SetState(ref ls, OneManyLockStates.ReservedForWriter);
					AddWritersWaiting(ref ls, -1);
					result = -1;
				}
				num = Interlocked.CompareExchange(ref target, ls, num2);
			}
			while (num2 != num);
			return result;
		}
	}
	#endregion
	#region 简单的混合锁
	/// <summary>
	/// 一个简单的混合线程同步锁，采用了基元用户加基元内核同步构造实现<br />
	/// A simple hybrid thread editing lock, implemented by the base user plus the element kernel synchronization.
	/// </summary>
	/// <remarks>
	/// 当前的锁适用于，竞争频率比较低，锁部分的代码运行时间比较久的情况，当前的简单混合锁可以达到最大性能。
	/// </remarks>
	/// <example>
	/// 以下演示常用的锁的使用方式，还包含了如何优雅的处理异常锁
	/// <code lang="cs" source="HslCommunication_Net45.Test\Documentation\Samples\Core\ThreadLock.cs" region="SimpleHybirdLockExample1" title="SimpleHybirdLock示例" />
	/// </example>
	public sealed class SimpleHybirdLock : IDisposable
	{
		private bool disposedValue = false;

		/// <summary>
		/// 基元用户模式构造同步锁
		/// </summary>
		private int m_waiters = 0;

		/// <summary>
		/// 基元内核模式构造同步锁
		/// </summary>
		private readonly Lazy<AutoResetEvent> m_waiterLock = new Lazy<AutoResetEvent>(() => new AutoResetEvent(initialState: false));

		private static long simpleHybirdLockCount;

		private static long simpleHybirdLockWaitCount;

		/// <summary>
		/// 获取当前锁是否在等待当中
		/// </summary>
		public bool IsWaitting => m_waiters != 0;

		/// <summary>
		/// 获取当前总的所有进入锁的信息<br />
		/// Get the current total information of all access locks
		/// </summary>
		public static long SimpleHybirdLockCount => simpleHybirdLockCount;

		/// <summary>
		/// 当前正在等待的锁的统计信息，此时已经发生了竞争了
		/// </summary>
		public static long SimpleHybirdLockWaitCount => simpleHybirdLockWaitCount;

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				m_waiterLock.Value.Close();
				disposedValue = true;
			}
		}

		/// <inheritdoc cref="M:System.IDisposable.Dispose" />
		public void Dispose()
		{
			Dispose(disposing: true);
		}

		/// <summary>
		/// 获取锁
		/// </summary>
		public void Enter()
		{
			Interlocked.Increment(ref simpleHybirdLockCount);
			if (Interlocked.Increment(ref m_waiters) != 1)
			{
				Interlocked.Increment(ref simpleHybirdLockWaitCount);
				m_waiterLock.Value.WaitOne();
			}
		}

		/// <summary>
		/// 离开锁
		/// </summary>
		public void Leave()
		{
			Interlocked.Decrement(ref simpleHybirdLockCount);
			if (Interlocked.Decrement(ref m_waiters) != 0)
			{
				Interlocked.Decrement(ref simpleHybirdLockWaitCount);
				m_waiterLock.Value.Set();
			}
		}
	}
	#endregion
	#region 多线程并发处理数据的类
	/// <summary>
	/// 一个双检锁的示例，适合一些占内存的静态数据对象，获取的时候才实例化真正的对象
	/// </summary>
	internal sealed class Singleton
	{
		private static object m_lock = new object();

		private static Singleton SValue = null;

		public static Singleton GetSingleton()
		{
			if (SValue != null)
			{
				return SValue;
			}
			Monitor.Enter(m_lock);
			if (SValue == null)
			{
				Singleton value = new Singleton();
				Volatile.Write(ref SValue, value);
				SValue = new Singleton();
			}
			Monitor.Exit(m_lock);
			return SValue;
		}
	}
	/// <summary>
	/// 一个用于多线程并发处理数据的模型类，适用于处理数据量非常庞大的情况
	/// </summary>
	/// <typeparam name="T">等待处理的数据类型</typeparam>
	public sealed class SoftMultiTask<T>
	{
		/// <summary>
		/// 一个双参数委托
		/// </summary>
		/// <param name="item"></param>
		/// <param name="ex"></param>
		public delegate void MultiInfo(T item, Exception ex);

		/// <summary>
		/// 用于报告进度的委托，当finish等于count时，任务完成
		/// </summary>
		/// <param name="finish">已完成操作数量</param>
		/// <param name="count">总数量</param>
		/// <param name="success">成功数量</param>
		/// <param name="failed">失败数量</param>
		public delegate void MultiInfoTwo(int finish, int count, int success, int failed);

		/// <summary>
		/// 操作总数，判定操作是否完成
		/// </summary>
		private int m_opCount = 0;

		/// <summary>
		/// 判断是否所有的线程是否处理完成
		/// </summary>
		private int m_opThreadCount = 1;

		/// <summary>
		/// 准备启动的处理数据的线程数量
		/// </summary>
		private int m_threadCount = 10;

		/// <summary>
		/// 指示多线程处理是否在运行中，防止冗余调用
		/// </summary>
		private int m_runStatus = 0;

		/// <summary>
		/// 列表数据
		/// </summary>
		private T[] m_dataList = null;

		/// <summary>
		/// 需要操作的方法
		/// </summary>
		private Func<T, bool> m_operater = null;

		/// <summary>
		/// 已处理完成数量，无论是否异常
		/// </summary>
		private int m_finishCount = 0;

		/// <summary>
		/// 处理完成并实现操作数量
		/// </summary>
		private int m_successCount = 0;

		/// <summary>
		/// 处理过程中异常数量
		/// </summary>
		private int m_failedCount = 0;

		/// <summary>
		/// 用于触发事件的混合线程锁
		/// </summary>
		private SimpleHybirdLock HybirdLock = new SimpleHybirdLock();

		/// <summary>
		/// 指示处理状态是否为暂停状态
		/// </summary>
		private bool m_isRunningStop = false;

		/// <summary>
		/// 指示系统是否需要强制退出
		/// </summary>
		private bool m_isQuit = false;

		/// <summary>
		/// 在发生错误的时候是否强制退出后续的操作
		/// </summary>
		private bool m_isQuitAfterException = false;

		/// <summary>
		/// 在发生错误的时候是否强制退出后续的操作
		/// </summary>
		public bool IsQuitAfterException
		{
			get
			{
				return m_isQuitAfterException;
			}
			set
			{
				m_isQuitAfterException = value;
			}
		}

		/// <summary>
		/// 异常发生时事件
		/// </summary>
		public event MultiInfo OnExceptionOccur;

		/// <summary>
		/// 报告处理进度时发生
		/// </summary>
		public event MultiInfoTwo OnReportProgress;

		/// <summary>
		/// 实例化一个数据处理对象
		/// </summary>
		/// <param name="dataList">数据处理列表</param>
		/// <param name="operater">数据操作方法，应该是相对耗时的任务</param>
		/// <param name="threadCount">需要使用的线程数</param>
		public SoftMultiTask(T[] dataList, Func<T, bool> operater, int threadCount = 10)
		{
			m_dataList = (dataList ?? throw new ArgumentNullException("dataList"));
			m_operater = (operater ?? throw new ArgumentNullException("operater"));
			if (threadCount < 1)
			{
				throw new ArgumentException("threadCount can not less than 1", "threadCount");
			}
			m_threadCount = threadCount;
			Interlocked.Add(ref m_opCount, dataList.Length);
			Interlocked.Add(ref m_opThreadCount, threadCount);
		}

		/// <summary>
		/// 启动多线程进行数据处理
		/// </summary>
		public void StartOperater()
		{
			if (Interlocked.CompareExchange(ref m_runStatus, 0, 1) == 0)
			{
				for (int i = 0; i < m_threadCount; i++)
				{
					Thread thread = new Thread(ThreadBackground);
					thread.IsBackground = true;
					thread.Start();
				}
				JustEnded();
			}
		}

		/// <summary>
		/// 暂停当前的操作
		/// </summary>
		public void StopOperater()
		{
			if (m_runStatus == 1)
			{
				m_isRunningStop = true;
			}
		}

		/// <summary>
		/// 恢复暂停的操作
		/// </summary>
		public void ResumeOperater()
		{
			m_isRunningStop = false;
		}

		/// <summary>
		/// 直接手动强制结束操作
		/// </summary>
		public void EndedOperater()
		{
			if (m_runStatus == 1)
			{
				m_isQuit = true;
			}
		}

		private void ThreadBackground()
		{
			while (true)
			{
				bool flag = true;
				while (m_isRunningStop)
				{
				}
				int num = Interlocked.Decrement(ref m_opCount);
				if (num < 0)
				{
					break;
				}
				T val = m_dataList[num];
				bool flag2 = false;
				bool flag3 = false;
				try
				{
					if (!m_isQuit)
					{
						flag2 = m_operater(val);
					}
				}
				catch (Exception ex)
				{
					flag3 = true;
					this.OnExceptionOccur?.Invoke(val, ex);
					if (m_isQuitAfterException)
					{
						EndedOperater();
					}
				}
				finally
				{
					HybirdLock.Enter();
					if (flag2)
					{
						m_successCount++;
					}
					if (flag3)
					{
						m_failedCount++;
					}
					m_finishCount++;
					this.OnReportProgress?.Invoke(m_finishCount, m_dataList.Length, m_successCount, m_failedCount);
					HybirdLock.Leave();
				}
			}
			JustEnded();
		}

		private void JustEnded()
		{
			if (Interlocked.Decrement(ref m_opThreadCount) == 0)
			{
				m_finishCount = 0;
				m_failedCount = 0;
				m_successCount = 0;
				Interlocked.Exchange(ref m_opCount, m_dataList.Length);
				Interlocked.Exchange(ref m_opThreadCount, m_threadCount + 1);
				Interlocked.Exchange(ref m_runStatus, 0);
				m_isRunningStop = false;
				m_isQuit = false;
			}
		}
	}
	#endregion
	#region 高级混合锁

	/// <summary>
	/// 一个高级的混合线程同步锁，采用了基元用户加基元内核同步构造实现，并包含了自旋和线程所有权
	/// </summary>
	/// <remarks>
	/// 当竞争的频率很高的时候，锁的时间很短的时候，当前的锁可以获得最大性能。
	/// </remarks>
	internal sealed class AdvancedHybirdLock : IDisposable
	{
		private bool disposedValue = false;

		private int m_waiters = 0;

		private readonly Lazy<AutoResetEvent> m_waiterLock = new Lazy<AutoResetEvent>(() => new AutoResetEvent(initialState: false));

		private int m_spincount = 1000;

		private int m_owningThreadId = 0;

		private int m_recursion = 0;

		/// <summary>
		/// 自旋锁的自旋周期，当竞争频率小，就要设置小，当竞争频率大，就要设置大，锁时间长就设置小，锁时间短就设置大，这样才能达到真正的高性能，默认为1000
		/// </summary>
		public int SpinCount
		{
			get
			{
				return m_spincount;
			}
			set
			{
				m_spincount = value;
			}
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				m_waiterLock.Value.Close();
				disposedValue = true;
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
		}

		/// <summary>
		/// 获取锁
		/// </summary>
		public void Enter()
		{
			int managedThreadId = Thread.CurrentThread.ManagedThreadId;
			if (managedThreadId == m_owningThreadId)
			{
				m_recursion++;
				return;
			}
			SpinWait spinWait = default(SpinWait);
			for (int i = 0; i < m_spincount; i++)
			{
				if (Interlocked.CompareExchange(ref m_waiters, 1, 0) == 0)
				{
					m_owningThreadId = Thread.CurrentThread.ManagedThreadId;
					m_recursion = 1;
					return;
				}
				spinWait.SpinOnce();
			}
			if (Interlocked.Increment(ref m_waiters) > 1)
			{
				m_waiterLock.Value.WaitOne();
			}
			m_owningThreadId = Thread.CurrentThread.ManagedThreadId;
			m_recursion = 1;
		}

		/// <summary>
		/// 离开锁
		/// </summary>
		public void Leave()
		{
			if (Thread.CurrentThread.ManagedThreadId != m_owningThreadId)
			{
				throw new SynchronizationLockException("Current Thread have not the owning thread.");
			}
			if (--m_recursion <= 0)
			{
				m_owningThreadId = 0;
				if (Interlocked.Decrement(ref m_waiters) != 0)
				{
					m_waiterLock.Value.Set();
				}
			}
		}
	}
	#endregion
}
