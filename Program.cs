using System;
using System.IO;
using System.Collections.Generic;

namespace SeibelCases {
    class Program {
        static void Main (string[] args) {
            var excel = Path.Combine(Directory.GetCurrentDirectory(), @"Excel\lna.xlsx");
            var formatJSON = Path.Combine(Directory.GetCurrentDirectory(), @"format.json");
            var dictionary = AnalyzeSummary.Analyze(excel, formatJSON, "Data From Siebel", "Summary");
            System.Console.WriteLine(dictionary.Count);
            foreach(var item in dictionary) {
                // System.Console.WriteLine($"{item.Key} => ${item.Value.Count} \t\t => {String.Join(", ", item.Value.ToArray())}");
                System.Console.WriteLine($"{item.Key} => {item.Value.Count}");
            }
        }
    }
}