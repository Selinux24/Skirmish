using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace Engine.Content.OnePageDungeon
{
    public static class DungeonFile
    {
        public static Dungeon Load(string fileName)
        {
            var obj = JsonConvert.DeserializeObject<Dungeon>(File.ReadAllText(fileName));

            return obj;
        }

        public static void Save(string fileName, Dungeon dungeon)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            string json = JsonConvert.SerializeObject(dungeon, settings);

            File.WriteAllText(fileName, json);
        }
    }
}
