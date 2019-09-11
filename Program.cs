using System;

namespace SeibelCases {
    class Program {
        static void Main (string[] args) {
            var excel = @"S:\Learnings\C#\SeibelCases\SeibelCaseAnalysis\Excel\Seibel.xlsx";
            var formatJSON = @"S:\Learnings\C#\SeibelCases\SeibelCaseAnalysis\format.json";
            var seibelJSON = JsonReader.PrintJSON (formatJSON);
            foreach ( var i in ExcelReader.GetSummariesOfEachRow (excel, "Sheet1")) {
                System.Console.WriteLine(i);
            }
        }
    }
}