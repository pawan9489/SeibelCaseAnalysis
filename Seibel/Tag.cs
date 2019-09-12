namespace SeibelCases.Seibel {
    public class Tag {
        private string _name;
        public string name { get => _name; set => _name = value.ToLower(); }
        public string locale { get; set; } = "en-GB";

        override public string ToString() => $"{name}";
    }
}