using AssassinBullet.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;

namespace AssassinBullet.Channel
{
    public class WorkChannel
    {
        public Channel<(string,Config,Dictionary<string,string>,CancellationToken)> channel { get; }
        public WorkChannel(int capacity)
        {
            channel = System.Threading.Channels.Channel.CreateBounded<(string, Config, Dictionary<string, string>,CancellationToken)>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
        }
    }
}
