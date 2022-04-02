using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Communication.Core.Net
{
	/// <summary>
	/// 读写网络的辅助类
	/// </summary>
	public class ReadWriteNetHelper
	{
#pragma warning disable CS1573 // 参数在 XML 注释中没有匹配的 param 标记(但其他参数有)
		#region Wait Support

		/// <inheritdoc cref="IReadWriteNet.Wait(string, bool, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static OperateResult<TimeSpan> Wait(IReadWriteNet readWriteNet, string address, bool waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<bool> read = readWriteNet.ReadBool(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				Thread.Sleep(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, short, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static OperateResult<TimeSpan> Wait(IReadWriteNet readWriteNet, string address, short waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<short> read = readWriteNet.ReadInt16(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				Thread.Sleep(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ushort, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static OperateResult<TimeSpan> Wait(IReadWriteNet readWriteNet, string address, ushort waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<ushort> read = readWriteNet.ReadUInt16(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				Thread.Sleep(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, int, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static OperateResult<TimeSpan> Wait(IReadWriteNet readWriteNet, string address, int waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<int> read = readWriteNet.ReadInt32(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				Thread.Sleep(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, uint, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static OperateResult<TimeSpan> Wait(IReadWriteNet readWriteNet, string address, uint waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<uint> read = readWriteNet.ReadUInt32(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				Thread.Sleep(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, long, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static OperateResult<TimeSpan> Wait(IReadWriteNet readWriteNet, string address, long waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<long> read = readWriteNet.ReadInt64(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				Thread.Sleep(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ulong, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static OperateResult<TimeSpan> Wait(IReadWriteNet readWriteNet, string address, ulong waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<ulong> read = readWriteNet.ReadUInt64(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				Thread.Sleep(readInterval);
			}
		}

		#endregion

		#region Wait Support
#if !NET20 && !NET35
		/// <inheritdoc cref="IReadWriteNet.Wait(string, bool, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static async Task<OperateResult<TimeSpan>> WaitAsync(IReadWriteNet readWriteNet, string address, bool waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<bool> read = await readWriteNet.ReadBoolAsync(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				await Task.Delay(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, short, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static async Task<OperateResult<TimeSpan>> WaitAsync(IReadWriteNet readWriteNet, string address, short waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<short> read = await readWriteNet.ReadInt16Async(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				await Task.Delay(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ushort, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static async Task<OperateResult<TimeSpan>> WaitAsync(IReadWriteNet readWriteNet, string address, ushort waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<ushort> read = await readWriteNet.ReadUInt16Async(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				await Task.Delay(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, int, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static async Task<OperateResult<TimeSpan>> WaitAsync(IReadWriteNet readWriteNet, string address, int waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<int> read = await readWriteNet.ReadInt32Async(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				await Task.Delay(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, uint, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static async Task<OperateResult<TimeSpan>> WaitAsync(IReadWriteNet readWriteNet, string address, uint waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<uint> read = readWriteNet.ReadUInt32(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				await Task.Delay(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, long, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static async Task<OperateResult<TimeSpan>> WaitAsync(IReadWriteNet readWriteNet, string address, long waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<long> read = readWriteNet.ReadInt64(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				await Task.Delay(readInterval);
			}
		}

		/// <inheritdoc cref="IReadWriteNet.Wait(string, ulong, int, int)"/>
		/// <param name="readWriteNet">通信对象</param>
		public static async Task<OperateResult<TimeSpan>> WaitAsync(IReadWriteNet readWriteNet, string address, ulong waitValue, int readInterval, int waitTimeout)
		{
			DateTime start = DateTime.Now;
			while (true)
			{
				OperateResult<ulong> read = readWriteNet.ReadUInt64(address);
				if (!read.IsSuccess) return OperateResult.CreateFailedResult<TimeSpan>(read);

				if (read.Content == waitValue) return OperateResult.CreateSuccessResult(DateTime.Now - start);
				if (waitTimeout > 0 && (DateTime.Now - start).TotalMilliseconds > waitTimeout)
				{
					return new OperateResult<TimeSpan>(StringResources.Language.CheckDataTimeout + waitTimeout);
				}

				await Task.Delay(readInterval);
			}
		}
#endif
		#endregion
#pragma warning restore CS1573 // 参数在 XML 注释中没有匹配的 param 标记(但其他参数有)

	}
}
