using System;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceIOControlLib.Wrapper
{
    public static class WaitHandleExtensions
    {
        public static Task AsTask(this WaitHandle handle, CancellationToken cancellationToken)
        {
            return AsTask(handle, cancellationToken, TimeSpan.Zero);
        }

        public static Task AsTask(this WaitHandle handle, CancellationToken cancellationToken, TimeSpan timeout)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            RegisteredWaitHandle registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
            {
                var localTcs = (TaskCompletionSource<object>)state;
                if (timedOut)
                    localTcs.TrySetCanceled();
                else
                    localTcs.TrySetResult(null);
            }, tcs, timeout, true);

            tcs.Task.ContinueWith(_ => registration.Unregister(null), cancellationToken);

            return tcs.Task;
        }
    }
}