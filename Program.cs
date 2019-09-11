using System;

namespace SeibelCases {
    class Program {
        static void Main (string[] args) {
            // var excel = @"S:\Learnings\C#\SeibelCases\Excel\Seibel.xlsx";
            var formatJSON = @"S:\Learnings\C#\SeibelCases\format.json";
            JsonReader.PrintJSON (formatJSON);
            // ExcelReader.PrintData (excel);
        }
    }
}