using System.Collections.Generic;
using System.Threading;

namespace AssassinBullet.Services
{
    public static class ProxyStore
    {
        public static int currentIndex = 0;
        public static List<string> Proxies { get; set; } = new List<string>();
        public static string NextProxy()
        {
            if (Proxies.Count == 0)
            {
                throw new System.Exception("Proxy listesi boş");
            }
            var currentIndex = Interlocked.Increment(ref ProxyStore.currentIndex);
            var proxyIndex = currentIndex % Proxies.Count;
            var proxy = Proxies[proxyIndex];
            return proxy;
        }
    }
}
