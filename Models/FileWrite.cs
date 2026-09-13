namespace AssassinBullet.Models
{
    public class FileWrite : Blocks
    {
        public FileWrite() : base(BlockType.FileWrite)
        {
        }
        public string FileContent { get; set; }
    }
}
