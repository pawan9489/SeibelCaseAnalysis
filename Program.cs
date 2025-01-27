﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace SeibelCases {
    class Program {
        static void Main (string[] args) {
            var excel = Path.Combine(Directory.GetCurrentDirectory(), @"Excel\Cases_List_from_Apr_2019.xlsx");
            var formatJSON = Path.Combine(Directory.GetCurrentDirectory(), @"format.json");
            var seibelJSON = JsonReader.ParseJsonToProduct (formatJSON);
            var dictionary = AnalyzeSummary.Analyze(
                    excel,
                    seibelJSON,
                    "Data From Siebel",
                    new List<string>() { 
                        "SR#",
                        "Summary",
                        "Date Created",
                        "Status",
                        "Priority",
                        "Group",
                        "Product Name"
                    }
                );
            GraphQuery.InsertData("bolt://localhost:7687", "neo4j", "test", dictionary, seibelJSON);
        }
    }
}