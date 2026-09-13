using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace AssassinBullet.Services
{
    public class ProxyRotatorService 
    {
        public HttpClientHandler RotateProxyClient()
        {
            if(ProxyStore.Proxies.Count == 0)
            {
                return new HttpClientHandler();

            }
            var proxy = ProxyStore.NextProxy();

            return new HttpClientHandler
            {
                Proxy = new WebProxy(proxy),
                UseProxy = true
            };


        }
 
    }
}
