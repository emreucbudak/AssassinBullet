using System;
using System.Threading;

namespace AssassinBullet.Models
{
    public class ScanCounter
    {
        public static int AllCount = 0;
        public static int SuccessCount = 0;
        public static int FailureCount = 0;
        public static int RetryCount = 0;

        public static event Action? CountersUpdated;

        public static void NotifyCountersUpdated() => CountersUpdated?.Invoke();

        public static void Reset()
        {
            Interlocked.Exchange(ref AllCount, 0);
            Interlocked.Exchange(ref SuccessCount, 0);
            Interlocked.Exchange(ref FailureCount, 0);
            Interlocked.Exchange(ref RetryCount, 0);
            NotifyCountersUpdated();
        }
    }
}
