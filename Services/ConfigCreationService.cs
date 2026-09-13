using AssassinBullet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AssassinBullet.Services
{
    public class ConfigCreationService
    {
        public async Task<string> CreateConfig(string configName, List<Blocks> blocks)
        {
            Config cfg = new Config();
            cfg.ConfigName = configName;
            cfg.Blocks = blocks;
            var path = $"{AppContext.BaseDirectory}Configs/{configName}.json";
            await File.WriteAllLinesAsync(path, new string[] { JsonConvert.SerializeObject(cfg,Formatting.Indented) });
           
            return path;
        }
    }
}
