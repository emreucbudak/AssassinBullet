using AssassinBullet.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace AssassinBullet.Services
{
    public class BlockExecutorService
    {
        public async Task<HttpResponseData> HttpExecutor(HttpRequest req,IReadOnlyDictionary<string, string> dynamicParameters,CancellationToken cancel,Context context,string? homePageUrl = null)
        {
            ProxyRotatorService service = new();
            var httpClient = service.RotateProxyClient();

            using var httpCli = new HttpClient(httpClient);
            var targetUrl = req.UseHomePageUrl
            ? homePageUrl ?? throw new ArgumentException("Ana sayfa URL'si eksik.")
            : req.TargetUrl;

            if (homePageUrl is not null && req.UseHomePageUrl is true) {
                foreach (var param in dynamicParameters)
                {
                    targetUrl = targetUrl.Replace($"<{param.Key}>", param.Value);
                    Uri uri = new Uri(homePageUrl);
                    string baseUrl = uri.GetLeftPart(UriPartial.Authority);
                    if (!context.Variables.ContainsKey("BaseUrl"))
                    {
                        context.Variables.Add("BaseUrl", baseUrl);
                    }
                }
                context.Variables.Add("TargetUrl", targetUrl);
            }
            if (targetUrl.StartsWith("<"))
            {
                var secondIndex = targetUrl.IndexOf(">");
                var key = targetUrl.Substring(1, secondIndex-1);
                if(context.Variables.TryGetValue(key,out var value))
                {
                    targetUrl = targetUrl.Replace($"<{key}>", (string)value);
                }
                foreach( var param in dynamicParameters)
                {
                    targetUrl = targetUrl.Replace($"<{param.Key}>", param.Value);
                }
                
            }
            foreach(var param in dynamicParameters)
            {
                if (context.Variables.ContainsKey(param.Key)) {
                    continue;
                }
                else
                {
                    context.Variables.Add(param.Key, param.Value);
                }
            }


            switch (req.Method.ToUpper())
            {
                case "GET":
                    foreach(var header in req.Headers)
                    {
                        var headerValue = header.Value;
                        var firstIndex = headerValue.IndexOf("<");
                        var secondIndex = headerValue.IndexOf(">");
                        if (firstIndex >= 0 && secondIndex > firstIndex)
                        {
                            var key = headerValue.Substring(firstIndex + 1, secondIndex - firstIndex - 1);
                            if (context.Variables.TryGetValue(key, out var value))
                                headerValue = (string)value;
                        }
                        httpCli.DefaultRequestHeaders.Add(header.Key, headerValue);
                    }
                    var getResponse = await httpCli.GetAsync(targetUrl,cancel);
                    httpCli.Dispose();
                    HttpResponseData data   = new HttpResponseData();
                    data.Url = targetUrl;
                    data.statusCode = (int)getResponse.StatusCode;
                    data.Body = await getResponse.Content.ReadAsStringAsync();
                    data.Headers = getResponse.Headers;
                    return data;
                case "POST":
                    var body = req.Body;
                    for (int i = 0; i < body.Length; i++)
                    {
                        var firstIndex = body.IndexOf("<", i);
                        var secondIndex = body.IndexOf(">", i);
                        if (firstIndex == -1 || secondIndex == -1)
                        {
                            continue;
                        }
                        var key = body.Substring(firstIndex + 1, secondIndex - firstIndex - 1);
                        if (context.Variables.TryGetValue(key, out var value))
                        {
                            var change = body.Substring(firstIndex, secondIndex - firstIndex + 1);
                            body = body.Replace(change, (string)value);
                        }
                    }
                    var postContent = new StringContent(body, Encoding.UTF8, "application/json");
                    foreach (var header in req.Headers)
                    {
                        var firstIndex = header.Value.IndexOf("<");
                        var secondIndex = header.Value.IndexOf(">");
                        if(firstIndex == -1 || secondIndex == -1)
                        {
                            postContent.Headers.Add(header.Key, header.Value);
                            continue; 
                        }
                        var key = header.Value.Substring(firstIndex+1, secondIndex - firstIndex-1);
                        if(context.Variables.TryGetValue(key, out var value))
                        {
                            postContent.Headers.Add(header.Key, (string)value);
                        }
                        else
                        {
                            postContent.Headers.Add(header.Key, header.Value);
                        }
                        
                         
                    }

                    var postResponse = await httpCli.PostAsync(targetUrl, postContent,cancel);
                    httpCli.Dispose();
                    HttpResponseData responseData = new HttpResponseData();
                    responseData.Url = targetUrl;
                    responseData.Body = await postResponse.Content.ReadAsStringAsync();
                    responseData.statusCode = (int)postResponse.StatusCode;
                    responseData.Headers = postResponse.Headers;
                    return responseData;
                default:
                    throw new NotSupportedException($"İstenen {req.Method} methodlar içinde bulunamadı");
            }
        }
        public async Task<Dictionary<string,string>> ParseExecutor( string leftParse,string RightParse,string variableName,string parseBlockName,Context context)
        {
            if (parseBlockName == "<Source>")
            {
                var bodyContent = context.LastResponseData.Body;
                var leftIndex = bodyContent.IndexOf(leftParse) + leftParse.Length;
                var rightIndex = bodyContent.IndexOf(RightParse, leftIndex);
                if (leftIndex < leftParse.Length || rightIndex < 0)
                {
                    throw new Exception("Parse edilmek istenen yer bulunamadı!");
                }
                var parsedValue = bodyContent.Substring(leftIndex, rightIndex - leftIndex);
                return new Dictionary<string, string> { { variableName, parsedValue } };
            }
            var blockName = parseBlockName.Substring(0, parseBlockName.Length);
            if (context.Variables.TryGetValue(blockName, out var value))
            {
                var val = value.ToString();
                var leftIndex = val!.IndexOf(leftParse) + leftParse.Length;
                var rightIndex = val.IndexOf(RightParse, leftIndex);
                if(leftIndex < leftParse.Length || rightIndex < 0)
                {
                    throw new Exception("Parse edilmek istenen yer bulunamadı!");
                }
                var parsedValue = val.Substring(leftIndex, rightIndex - leftIndex);
                return new Dictionary<string, string> { { variableName, parsedValue } };
            }
            throw new Exception("Parse edilmek istenen yer bulunamadı!");
        }
        public async Task KeyCheck(HttpResponseData response,KeyCheck check)
        { 
            if(check.WhereCheck == "Header")
            {
                var headers = response.Headers.ToString();
                foreach (var message in check.FailedMessage)
                {
                    if (headers.Contains(message))
                    {
                        Interlocked.Increment(ref ScanCounter.FailureCount);
                        ScanCounter.NotifyCountersUpdated();
                        throw new Exception("Başarısız");
                    }
                }
                foreach(var successMesage in check.SuccessMessage)
                {
                    if (headers.Contains(successMesage))
                    {

                        Interlocked.Increment(ref ScanCounter.SuccessCount);
                        ScanCounter.NotifyCountersUpdated();
                        
                    }
                }
                foreach(var retryMessage in check.RetryMessage)
                {
                    if (headers.Contains(retryMessage))
                    {
                        Interlocked.Increment(ref ScanCounter.RetryCount);
                        ScanCounter.NotifyCountersUpdated();
                        throw new Exception("Yeniden Dene");
                    }
                }
            }
            else if (check.WhereCheck == "Body")
            {
                var body = response.Body;
                foreach (var message in check.FailedMessage)
                {
                    if (body.Contains(message))
                    {
                        Interlocked.Increment(ref ScanCounter.FailureCount);
                        ScanCounter.NotifyCountersUpdated();
                        throw new Exception("Başarısız");
                    }
                }
                foreach (var successMesage in check.SuccessMessage)
                {
                    if (body.Contains(successMesage))
                    {

                        Interlocked.Increment(ref ScanCounter.SuccessCount);
                        ScanCounter.NotifyCountersUpdated();

                    }
                }
                foreach (var retryMessage in check.RetryMessage)
                {
                    if (body.Contains(retryMessage))
                    {
                        Interlocked.Increment(ref ScanCounter.RetryCount);
                        ScanCounter.NotifyCountersUpdated();
                        throw new Exception("Yeniden Dene");
                    }
                }
            }


        }
        public async Task<Dictionary<string, string>> MakeHttpRequest(
            HttpRequest req, System.Threading.CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(req);
            if (!Uri.TryCreate(req.TargetUrl, UriKind.Absolute, out var url) ||
                (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Geçerli bir HTTP veya HTTPS adresi gir.", nameof(req));

            var method = req.Method.Trim().ToUpperInvariant() switch
            {
                "GET" => HttpMethod.Get,
                "POST" => HttpMethod.Post,
                _ => throw new NotSupportedException($"HTTP metodu desteklenmiyor: {req.Method}")
            };

            using var client = new HttpClient(new HttpClientHandler { })
            {
         
            };
            using var message = new HttpRequestMessage(method, url);
            if (method == HttpMethod.Post || !string.IsNullOrEmpty(req.Body))
                message.Content = new StringContent(req.Body ?? "", Encoding.UTF8, "application/json");

            foreach (var header in req.Headers)
            {
                if (message.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    continue;

                message.Content ??= new StringContent("");
                message.Content.Headers.Remove(header.Key);
                if (!message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    throw new ArgumentException($"Geçersiz HTTP başlığı: {header.Key}", nameof(req));
            }

            using var response = await client.SendAsync(message, cancellationToken);
            return new Dictionary<string, string>
            {
                ["Body"] = await response.Content.ReadAsStringAsync(cancellationToken),
                ["Headers"] = response.Headers.ToString() + response.Content.Headers.ToString()
            };
        }
        public Task<string> FileWrite(string content, Context context)
        {
            int index = 0;

            while (index < content.Length)
            {
                int firstItem = content.IndexOf('<', index);
                if (firstItem == -1)
                    break;

                int secondItem = content.IndexOf('>', firstItem + 1);
                if (secondItem == -1)
                    break;

                string key = content.Substring(
                    firstItem + 1, secondItem - firstItem - 1);

                if (context.Variables.TryGetValue(key, out var value))
                {
                    string replacement = value?.ToString() ?? "";

                    content = content
                        .Remove(firstItem, secondItem - firstItem + 1)
                        .Insert(firstItem, replacement);

                    index = firstItem + replacement.Length;
                }
                else
                {
                    index = secondItem + 1;
                }
            }

            return Task.FromResult(content);
        }

    }
}

