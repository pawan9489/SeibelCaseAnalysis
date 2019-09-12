using System.Collections.Generic;

namespace SeibelCases.Seibel {
    public class Category {
        private string _umbrellaterm;
        public string umbrellaterm { get => _umbrellaterm; set => _umbrellaterm = value.ToLower(); }
        public List<Tag> aliases { get; set; } = new List<Tag> ();
        public List<Category> subcategories { get; set; } = new List<Category> ();

        override public string ToString() => $"{umbrellaterm}";
    }
}