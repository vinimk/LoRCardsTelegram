using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace LoRCards.JsonImporter
{
    public class JsonImporter
    {
        public static List<Card> ReadCardsFromJson()
        {
            List<Card> result = new List<Card>();
            foreach (string file in Directory.EnumerateFiles("ExternalResources", "*.*"))
            {
                string jsonString = File.ReadAllText(file);
                result.AddRange(JsonSerializer.Deserialize<List<Card>>(jsonString));
            }
            return result;
        }
    }
}
