using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnd.Core.ConsoleApp
{
    static class Program
    {
        static async Task Main()
        {
            var entries = new List<JObject>();
            var sr = new StreamReader(@"C:\Code\Data\entities.json");
            var reader = new JsonTextReader(sr);
            var serialiser = new JsonSerializer();

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.StartObject)
                    continue;

                var obj = serialiser.Deserialize<JObject>(reader);
                entries.Add(obj);

                if (entries.Count != 1000) continue;

                File.WriteAllText($@"C:\Code\Data\Entities\dynamo-db-data-{Guid.NewGuid()}.json", JsonConvert.SerializeObject(entries));
                entries.Clear();
            }
        }
    }
}
