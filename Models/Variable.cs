using Newtonsoft.Json;

namespace AssassinBullet.Models
{
    public  class Variable : Blocks
    {
        public Variable(string VariableName, string VariableValue) : base(BlockType.Variable)
        {
            this.VariableName = VariableName;
            this.VariableValue = VariableValue;
        }
        [JsonRequired] public string VariableName { get; set; } = "";
        [JsonRequired] public string VariableValue { get; set; }
     

    }
}
