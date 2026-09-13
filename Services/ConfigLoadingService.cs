using AssassinBullet.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AssassinBullet.Services
{
    public class ConfigLoadingService
    {
        public Config Load(string json)
        {
            var document = JObject.Parse(json);
            if (document["ConfigName"]?.Type != JTokenType.String)
                throw new JsonException("Config adı eksik veya geçersiz.");
            if (document["Blocks"] is not JArray blocks)
                throw new JsonException("Config blok listesi eksik veya geçersiz.");

            var config = new Config
            {
                ConfigName = document["ConfigName"]!.Value<string>()!
            };

            for (var i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] is not JObject block)
                    throw new JsonException($"{i + 1}. blok geçerli bir JSON nesnesi değil.");

                var typeToken = block["BlockType"];
                if (typeToken is null ||
                    (typeToken.Type != JTokenType.Integer && typeToken.Type != JTokenType.String))
                    throw new JsonException($"{i + 1}. bloğun BlockType değeri eksik veya geçersiz.");

                var type = typeToken.ToObject<BlockType>();
                Blocks? model = type switch
                {
                    BlockType.HttpRequest => block.ToObject<HttpRequest>(),
                    BlockType.KeyCheck => block.ToObject<KeyCheck>(),
                    BlockType.Variable => block.ToObject<Variable>(),
                    BlockType.Parse => block.ToObject<Parse>(),
                    BlockType.FileWrite => block.ToObject<FileWrite>(),
                    _ => throw new JsonException($"{i + 1}. blok türü desteklenmiyor: {typeToken}")
                };

                config.Blocks.Add(model
                    ?? throw new JsonException($"{i + 1}. blok modele dönüştürülemedi."));
            }

            return config;
        }
    }
}
