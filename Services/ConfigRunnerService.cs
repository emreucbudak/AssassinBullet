using AssassinBullet.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AssassinBullet.Services
{
    public class ConfigRunnerService
    {
        public async Task Executor(
            Config cfg,
            string? homePageUrl,
            IReadOnlyDictionary<string, string> dynamicParameters , CancellationToken cancel)
        {
            BlockExecutorService requestService = new BlockExecutorService();
            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();
            Dictionary<string,string> scanSuccess = new Dictionary<string,string>();
            Interlocked.Increment(ref ScanCounter.AllCount);
            ScanCounter.NotifyCountersUpdated();
            var context = new Context();
            
            foreach (var block in cfg.Blocks)
            {

                switch (block)
                {
                    case HttpRequest req:
                        cancel.ThrowIfCancellationRequested();
                        var response = await requestService.HttpExecutor(req, dynamicParameters, cancel, context,homePageUrl);
                        context.LastResponseData = response;
                        continue;
                    case KeyCheck check:
                        cancel.ThrowIfCancellationRequested();
                        await requestService.KeyCheck(context.LastResponseData, check);
                        continue;
                    case Parse parse:
                        cancel.ThrowIfCancellationRequested();
                        var parseResp = await requestService.ParseExecutor(parse.LeftParse,parse.RightParse,parse.VariableName,parse.ParseBlockName,context);
                        context.Variables[parse.VariableName] = parseResp[parse.VariableName];
                        continue;
                    case Variable variable:
                        cancel.ThrowIfCancellationRequested();
                        context.Variables.Add(variable.VariableName, variable.VariableValue);
                        continue;
                    case FileWrite fileWrite:
                        cancel.ThrowIfCancellationRequested();
                        try
                        {
                            var res =  requestService.FileWrite(fileWrite.FileContent, context);
                            await FileWriteService.FileWrite(res);
                        }
                        catch
                        {
                            Interlocked.Increment(ref ScanCounter.FailureCount);
                            ScanCounter.NotifyCountersUpdated();
                        }
                        break;
                        
                        

                }
            }
            
        }
    }
}
