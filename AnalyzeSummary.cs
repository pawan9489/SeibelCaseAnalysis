using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SeibelCases.Seibel;
using System;

namespace SeibelCases { 
    public class AnalyzeSummary {
        public static Dictionary<string, Dictionary<string, string>> SeibelSummary = new Dictionary<string, Dictionary<string, string>>();
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
                string pattern = @"\b" + Regex.Escape(alias.name) + @"\b";
                Regex regex = new Regex(pattern,RegexOptions.IgnoreCase);
                if (regex.IsMatch(ls)){
                    fitsToCategory = true;
                    caseFormat.tags.Add(alias.name);
                }
            }
            if (fitsToCategory) {
                foreach(var subcat in category.subcategories) {
                    foreach(var subAlias in subcat.aliases) {
                        string pattern = @"\b" + Regex.Escape(subAlias.name) + @"\b";
                        Regex regex = new Regex(pattern,RegexOptions.IgnoreCase);
                        if (regex.IsMatch(ls)) {
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
        public static Dictionary<string, List<CaseFormat>> Analyze(string excel, Product seibelJSON, string sheetName, List<string> columnNames) {
            var dict = new Dictionary<string, List<CaseFormat>>(); 
            foreach (var row in ExcelReader.GetColumnsOfEachRow (excel, sheetName, columnNames)) {
                var summary = "";
                var status = "";
                var priority = "";
                var date_created = "";
                var seibelNumber = "";
                var group = "";
                var product_name = "";
                var tempDict = new Dictionary<string, string>() {};
                foreach (var item in row) {
                    var columnName = item.Key.ToLower().Trim();
                    if (columnName == "summary") {
                        summary = ExcelReader.CellToString (item.Value);
                        tempDict.Add("summary", new string(summary.Where(c => !char.IsPunctuation(c)).ToArray()));
                    } else if (columnName == "sr#") {
                        seibelNumber = ExcelReader.CellToString (item.Value);
                        tempDict.Add("sr#", seibelNumber);
                    } else if (columnName == "date created") {
                        date_created = DateTime.Parse(ExcelReader.CellToString (item.Value)).ToString("yyyy-MM-ddTHH:mm:ss.fff");
                        tempDict.Add("date created", date_created);
                    } else if (columnName == "status") {
                        status = ExcelReader.CellToString (item.Value);
                        tempDict.Add("status", status);
                    } else if (columnName == "priority") {
                        priority = ExcelReader.CellToString (item.Value);
                        tempDict.Add("priority", priority);
                    } else if (columnName == "group") {
                        group = ExcelReader.CellToString (item.Value);
                        tempDict.Add("group", group);
                    } else if (columnName == "product name") {
                        product_name = ExcelReader.CellToString (item.Value);
                        tempDict.Add("original product name", product_name);
                        if (product_name.IndexOf("ihcm2", StringComparison.OrdinalIgnoreCase) >= 0) 
                        {
                            tempDict.Add("product name", "iHCM2");
                        } 
                        else if (product_name.IndexOf("ihcm", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            tempDict.Add("product name", "iHCM-UK");
                        } 
                        else 
                        {
                            tempDict.Add("product name", "");
                        }
                    }
                }
                SeibelSummary.Add(seibelNumber, tempDict); // Optional
                TagASummary(dict, seibelNumber, summary, seibelJSON);
            }
            return dict;
        }
    }
}