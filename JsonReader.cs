using System;
using System.IO;
using Newtonsoft.Json;
using SeibelCases.Seibel;

namespace SeibelCases {
    public class JsonReader {
        public static SeibelFormat ParseJsonToSeibel (string filepath) {
            using (var fs = new FileStream (filepath, FileMode.Open, FileAccess.Read)) {
                using (StreamReader reader = new StreamReader (fs)) {
                    var data = reader.ReadToEnd ();
                    // var options = new JsonSerializerOptions {
                    //     PropertyNameCaseInsensitive = true,
                    //     AllowTrailingCommas = true
                    // };
                    // return JsonSerializer.Deserialize<SeibelFormat> (data, options);
                    return JsonConvert.DeserializeObject<SeibelFormat> (data);
                }
            }
        }
    }
}