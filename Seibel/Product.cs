using System.Collections.Generic;
namespace SeibelCases.Seibel {
    public class Product {
        private string _productName;
        public string productName { get => _productName; set => _productName = value.ToLower(); }
        public List<ProductArea> areas { get; set; }

        override public string ToString() => $"{productName}";
    }
}