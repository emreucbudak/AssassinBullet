using Newtonsoft.Json;
using System.Collections.Generic;

namespace AssassinBullet.Models
{
    public class Config
    {
        [JsonRequired]public string ConfigName { get; set; } = "";
        [JsonRequired]public List<Blocks> Blocks { get; set; } = [];

    }
}
