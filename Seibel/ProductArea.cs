using System.Collections.Generic;
namespace SeibelCases.Seibel {
    public class ProductArea {
        private string _area;
        public string area { get => _area; set => _area = value.ToLower(); }
        public List<Category> categories { get; set; }

        override public string ToString() => $"{area}";
    }
}