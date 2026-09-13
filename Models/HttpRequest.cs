using Newtonsoft.Json;
using System.Collections.Generic;

namespace AssassinBullet.Models
{
    public sealed class HttpRequest : Blocks
    {
        

        [JsonRequired] public string TargetUrl { get; set; }

        public HttpRequest(string targetUrl, string method, string body, Dictionary<string, string> headers, bool useHomePageUrl) : base(BlockType.HttpRequest)
        {
            TargetUrl = targetUrl;
            Method = method;
            Body = body;
            Headers = headers;
            UseHomePageUrl = useHomePageUrl;
        }

        [JsonRequired] public string Method { get; set; } = "GET";
        [JsonRequired] public string Body { get; set; } = "";
        [JsonRequired] public Dictionary<string, string> Headers { get; set; } = new();
        [JsonRequired] public bool UseHomePageUrl { get; set; }
        





    }
}
