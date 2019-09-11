using System;
using System.IO;
using System.Text.Json;
using SeibelCases.Seibel;

namespace SeibelCases {
    public class JsonReader {
        public static void PrintJSON (string filepath) {
            using (var fs = new FileStream (filepath, FileMode.Open, FileAccess.Read)) {
                using (StreamReader reader = new StreamReader (fs)) {
                    var data = reader.ReadToEnd ();
                    var options = new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true
                    };
                    SeibelFormat obj = JsonSerializer.Deserialize<SeibelFormat>(data, options);
                }
            }
        }
    }
}