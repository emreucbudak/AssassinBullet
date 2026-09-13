namespace AssassinBullet.Models
{
    public class Parse : Blocks
    {
        public Parse() : base(BlockType.Parse)
        {
        }
        public string ParseBlockName { get; set; }
        public string LeftParse { get; set; }
        public string RightParse { get; set; }
        public string VariableName { get; set; }
    }
}
