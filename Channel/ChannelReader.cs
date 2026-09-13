using AssassinBullet.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssassinBullet.Channel
{
    public class ChannelReader(WorkChannel channel)
    {
        public async Task CreateBot(int BotCount)
        {
            var bot = new Task[BotCount]; 
            for (int i = 0; i < BotCount; i++)
            {
     
                    bot[i] = ReadFromChannel();


            }
            await Task.WhenAll(bot);
        }
        public async Task ReadFromChannel()
        {
            var configRunnerService = new ConfigRunnerService();

                await foreach (var item in channel.channel.Reader.ReadAllAsync())
                {
                var (targetUrl, config, dynamicParameters, cancellationToken) = item;
                try
                {
                    
                    await configRunnerService.Executor(config, targetUrl, dynamicParameters, cancellationToken);
           
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch
                {

                    continue;
                }
                
                   
                }


        }


    }
}
