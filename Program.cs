using System;
using System.Collections.Generic;

namespace SeibelCases {
    class Program {
        static void Main (string[] args) {
            var excel = @"S:\Learnings\C#\SeibelCases\SeibelCaseAnalysis\Excel\Seibel2.xlsx";
            var formatJSON = @"S:\Learnings\C#\SeibelCases\SeibelCaseAnalysis\format.json";
            var seibelJSON = JsonReader.PrintJSON (formatJSON);
            foreach (var row in ExcelReader.GetColumnsOfEachRow (excel, "Sheet1", new List<string> () { "Summary", "Last Opened" })) {
                foreach (var column in row) {
                    System.Console.WriteLine (ExcelReader.CellToString (column));
                }
            }
        }
    }
}