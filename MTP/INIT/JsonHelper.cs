using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ACO2.INIT
{
    public static class JsonHelper<T> where T : class
    {
        public static List<T> LoadData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<T>();
            }

            var jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(jsonData) ?? new List<T>();
        }

        public static void SaveData(List<T> data,string filePath)
        {
            var jsonData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }
    }
}
