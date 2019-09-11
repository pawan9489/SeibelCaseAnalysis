using System.Collections.Generic;
using SeibelCases.Seibel;

namespace SeibelCases { 
    public class AnalyzeSummary {
        private static void TagASummary(Dictionary<string, List<string>> dict, string summary, SeibelFormat sf) {
            // if (! dict.ContainsKey(summary)) {
            //     foreach(var area in sf.areas) {
            //         var categoriesBelongedTo = TagASummaryToArea(summary, area);
            //         if (categoriesBelongedTo.Count > 0) {
            //             dict.Add(summary, categoriesBelongedTo);
            //         }
            //     }
            // }
            foreach(var area in sf.areas) {
                var categoriesBelongedTo = TagASummaryToArea(summary, area); 
                // ex: summary = "Sick Leave payment for working days";  categoriesBelongedTo = ["Sick - Payment", "Leave"]
                foreach(var cat in categoriesBelongedTo) {
                    if (dict.ContainsKey(cat)) {
                        dict[cat].Add(summary);
                    } else {
                        dict[cat] = new List<string>() { summary };
                    }
                }
            }
        }
        private static List<string> TagASummaryToArea(string summary, ProductArea productArea) {
            var ls = summary.ToLower();
            var cats = productArea.categories;
            var subcats = productArea.subcategories;
            var categoriesBelongedTo = new List<string>();
            foreach (var cat in cats) {
                if (ls.Contains(cat.name.ToLower())) {
                    foreach(var subcat in subcats) {
                        if (ls.Contains(subcat.name.ToLower()))
                            categoriesBelongedTo.Add($"{cat.name} - {subcat.name}");
                        else
                            categoriesBelongedTo.Add($"{cat.name}");
                    }
                }
            }
            return categoriesBelongedTo;
        }
        public static Dictionary<string, List<string>> Analyze(string excel, string formatJSON, string sheetName, string summaryColumnName) {
            var seibelJSON = JsonReader.ParseJsonToSeibel (formatJSON);
            var dict = new Dictionary<string, List<string>>(); 
            foreach (var row in ExcelReader.GetColumnsOfEachRow (excel, sheetName, new List<string> () { summaryColumnName })) {
                foreach (var column in row) {
                    var summary = ExcelReader.CellToString (column);
                    TagASummary(dict, summary, seibelJSON);
                }
            }
            return dict;
        }
    }
}