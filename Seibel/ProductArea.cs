using System.Collections.Generic;
namespace SeibelCases.Seibel {
    public class ProductArea {
        public string area { get; set; }
        public List<Tag> categories { get; set; }
        public List<Tag> subcategories { get; set; }
    }
}