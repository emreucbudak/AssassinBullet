using Newtonsoft.Json;

namespace AssassinBullet.Models
{
    public class KeyCheck : Blocks
    {
        public KeyCheck() : base(BlockType.KeyCheck)
        {
        }
        public string WhereCheck { get; set; }
        public string[] SuccessMessage { get; set; }
        public string[] FailedMessage { get; set; }
        public string[] RetryMessage { get; set; }
    }
}
