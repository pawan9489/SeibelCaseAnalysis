using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SeibelCases {
    public class ExcelReader {
        private struct DateOrDoubleOrString {
            public DateTime date;
            public string str;
            public double value;
        }
        private static (CellType, DateOrDoubleOrString) ParseCellContent (ICell c) {
            var res = new DateOrDoubleOrString ();
            if (c.CellType == NPOI.SS.UserModel.CellType.Numeric) {
                if (DateUtil.IsCellDateFormatted (c)) {
                    var date = DateUtil.GetJavaDate (c.NumericCellValue);
                    // var dateFormat = c.CellStyle.GetDataFormatString ();
                    res.date = date;
                } else {
                    res.value = c.NumericCellValue;
                }
            } else {
                res.str = c.StringCellValue;
            }
            return (c.CellType, res);
        }
        public static IEnumerable<string> GetSummariesOfEachRow (string filepath, string sheetname) {
            using (var fs = new FileStream (filepath, FileMode.Open, FileAccess.Read)) {
                IWorkbook workbook = new XSSFWorkbook (fs);
                ISheet sheet = workbook.GetSheet (sheetname);
                XSSFFormulaEvaluator evaluator = new XSSFFormulaEvaluator (workbook);
                var rowCount = sheet.LastRowNum;
                var firstRow = sheet.GetRow (0);
                var columnCount = firstRow.LastCellNum;
                var headings = new List<string> (columnCount);
                var summaryColumn = -1;

                for (int i = 0; i <= rowCount; i++) {
                    var row = sheet.GetRow (i);
                    for (IEnumerator cit = row.GetEnumerator (); cit.MoveNext ();) {
                        ICell c = (ICell) cit.Current;
                        var (cellType, content) = ExcelReader.ParseCellContent (c);
                        if (i == 0) { // Headings
                            if (cellType == NPOI.SS.UserModel.CellType.String) {
                                var columnValue = content.str;
                                headings.Add (columnValue);
                                if (columnValue.ToLower () == "summary") {
                                    summaryColumn = c.ColumnIndex;
                                }
                            } else
                                throw new Exception ("Excel Header must Contain only String Formatted Cells.");
                            continue;
                        }
                        if (c.ColumnIndex == summaryColumn) {
                            var cellContent = "";
                            if (cellType == NPOI.SS.UserModel.CellType.Numeric) {
                                if (DateUtil.IsCellDateFormatted (c))
                                    cellContent = content.date.ToString ();
                                else
                                    cellContent = content.value.ToString ();
                            } else {
                                cellContent = content.str;
                            }
                            yield return cellContent;
                        }
                    }
                }
            }
        }
    }
}