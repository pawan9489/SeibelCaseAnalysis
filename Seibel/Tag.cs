using System.Collections.Generic;

namespace SeibelCases.Seibel {
    public class Tag {
        public string name { get; set; }
        public string locale { get; set; } = "en-GB";
        public List<Tag> aliases { get; set; } = new List<Tag> ();
    }
}