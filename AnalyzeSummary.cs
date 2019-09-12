using System.Collections.Generic;
using System.Linq;
using SeibelCases.Seibel;

namespace SeibelCases { 
    public class AnalyzeSummary {
        public static Dictionary<string, string> SeibelSummary = new Dictionary<string, string>();
        private static void TagASummary(Dictionary<string, List<CaseFormat>> dict, string seibelNumber, string summary, Product product) {
            // A Summary can belong to multiple Areas & multiple Categories
            foreach(var area in product.areas) {
                var categoriesBelongedTo = TagASummaryToArea(summary, area);
                // ex: summary = "Sick Leave payment for working days"; 
                // categoriesBelongedTo = [ CaseFormat([...tags], {"Cat": [...subcats]), CaseFormat([...tags], {"Cat": [...subcats] ]
                foreach(var cat in categoriesBelongedTo) {
                    if (dict.ContainsKey(seibelNumber)) {
                        dict[seibelNumber].Add(cat);
                    } else {
                        dict[seibelNumber] = new List<CaseFormat>() { cat };
                    }
                }
            }
        }
        private static CaseFormat TagASummaryToCategory(string summary, Category category) {
            var ls = summary.ToLower();
            var caseFormat = new CaseFormat();
            var fitsToCategory = false;

            var hasSubCat = false;
            var subCatList = new List<string>();
            foreach (var alias in category.aliases)
            {
                if (ls.Contains(alias.name)){
                    fitsToCategory = true;
                    caseFormat.tags.Add(alias.name);
                }
            }
            if (fitsToCategory) {
                foreach(var subcat in category.subcategories) {
                    foreach(var subAlias in subcat.aliases) {
                        if (ls.Contains(subAlias.name)) {
                            hasSubCat = true;
                            caseFormat.tags.Add(subAlias.name);
                        }
                    }
                    if (hasSubCat) {
                        subCatList.Add(subcat.umbrellaterm);
                    }
                    hasSubCat = false;
                }
                caseFormat.categoryAndConnectionType.Add(category.umbrellaterm, 
                    subCatList.Count == 0 ? new List<string>() { "DIRECT" } : subCatList);
            }
            return caseFormat;
        }
        private static IEnumerable<CaseFormat> TagASummaryToArea(string summary, ProductArea productArea) 
            => productArea.categories
                .Select(category => TagASummaryToCategory(summary, category))
                .Where(caseFormat => caseFormat.categoryAndConnectionType.Count != 0);
        public static Dictionary<string, List<CaseFormat>> Analyze(string excel, string formatJSON, string sheetName, List<string> columnNames) {
            var seibelJSON = JsonReader.ParseJsonToProduct (formatJSON);
            var dict = new Dictionary<string, List<CaseFormat>>(); 
            foreach (var row in ExcelReader.GetColumnsOfEachRow (excel, sheetName, columnNames)) {
                var summary = "";
                var seibelNumber = "";
                foreach (var item in row) {
                    var columnName = item.Key.ToLower();
                    if (columnName == "summary") {
                        summary = ExcelReader.CellToString (item.Value);
                    } else if (columnName == "sr#") {
                        seibelNumber = ExcelReader.CellToString (item.Value);
                    }
                }
                SeibelSummary.Add(seibelNumber, summary); // Optional
                TagASummary(dict, seibelNumber, summary, seibelJSON);
            }
            return dict;
        }
    }
}