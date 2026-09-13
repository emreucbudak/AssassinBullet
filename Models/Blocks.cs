using Newtonsoft.Json;

namespace AssassinBullet.Models
{
    public abstract class Blocks
    {
        [JsonRequired]public string BlockName { get; set; } = "";
        [JsonRequired] public BlockType BlockType { get; set; }

        protected Blocks(BlockType blockType)
        {
            BlockType = blockType;

        }
        public void SetBlockName(string name)
        {
            BlockName = name;
        }
    }
}
