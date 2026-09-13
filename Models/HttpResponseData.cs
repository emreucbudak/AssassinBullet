using System.Net.Http.Headers;

namespace AssassinBullet.Models
{
    public class HttpResponseData
    {
        public int statusCode { get; set; }
        public string Body { get; set; }
        public string Url { get; set; }
        public HttpResponseHeaders Headers { get; set; }
    }
}
