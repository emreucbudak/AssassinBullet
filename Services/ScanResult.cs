using System.Collections.Concurrent;

namespace AssassinBullet.Services
{
    public static class ScanResult
    {
        public static ConcurrentBag<string> Success { get; set; } 
        public static ConcurrentBag<string> Failed { get; set; }
        public static ConcurrentBag<string> Retry { get; set; }
    }
}
