using System;
using System.Threading.Tasks;

namespace ModbusDriver
{
    public static class TaskHelper
    {
        public static Task WithTimeout(this Task task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout).ContinueWith(_ => TaskContinuationOptions.ExecuteSynchronously);
            return Task.WhenAny(task, timeoutTask).Unwrap();
        }
    }
}
