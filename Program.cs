using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SeibelCases {
    class Program {
        static void Main (string[] args) {
            var excel = Path.Combine(Directory.GetCurrentDirectory(), @"Excel\Seibel3.xlsx");
            var formatJSON = Path.Combine(Directory.GetCurrentDirectory(), @"format.json");
            var dictionary = AnalyzeSummary.Analyze(excel, formatJSON, "Data From Siebel", new List<string>() { "SR#", "Summary" });
            System.Console.WriteLine(dictionary.Count);
            foreach(var item in dictionary) {
                // System.Console.WriteLine($"{item.Key} => ${item.Value.Count} \t\t => {String.Join(", ", item.Value.ToArray())}");
                System.Console.WriteLine($"{item.Key} :: {AnalyzeSummary.SeibelSummary[item.Key]}");
                foreach (var csf in item.Value) {
                    System.Console.WriteLine($"\t Tags: {String.Join(", ", csf.tags)}");
                    foreach(var cat in csf.categoryAndConnectionType) {
                        System.Console.WriteLine($"\t\t Category: {String.Join(", ", cat.Key)}");
                        if (cat.Value.First() != "DIRECT") {
                            System.Console.WriteLine($"\t\t SubCategories: {String.Join(", ", String.Join(", ", cat.Value))}");
                        }
                    }
                }
                System.Console.WriteLine("\n--------------------------------------------------------------\n");
            }
        }
    }
}