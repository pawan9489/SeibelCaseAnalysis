using System.Collections.Generic;

namespace SeibelCases {
    public class CaseFormat {
        public List<string> tags { get; set; } = new List<string>();
        // Summary = "'Sick' 'scheme' with leave to 'payroll' option." ; 
        // categoryAndConnectionType = { Key = "Sick", Value = ["Payment", "Setup"] }
        public Dictionary<string, List<string>> categoryAndConnectionType { get; set; } = new Dictionary<string, List<string>>();
    }
}