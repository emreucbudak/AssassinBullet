using AssassinBullet.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AssassinBullet.Channel
{
    public class ChannelWriter(WorkChannel channel)
    {
        public async Task WriteToChannel(
            string targetUrl,
            Config config,
            Dictionary<string, string> dynamicParameters,CancellationToken cancellationToken)
        {
            await channel.channel.Writer.WriteAsync((targetUrl, config, dynamicParameters,cancellationToken));

        }
    }
}
