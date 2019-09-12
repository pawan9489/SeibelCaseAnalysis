using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SeibelCases {
    public class ExcelReader {
        public struct DateOrDoubleOrString {
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
            } else if (c.CellType == NPOI.SS.UserModel.CellType.String) {
                // System.Console.WriteLine($"{c.RowIndex} - {c.ColumnIndex}");
                res.str = c.StringCellValue;
            }
            return (c.CellType, res);
        }
        private static List<string> ExtractHeadings (IRow row, int columnCount) {
            var headings = new List<string> (columnCount);
            for (IEnumerator cit = row.GetEnumerator (); cit.MoveNext ();) {
                ICell c = (ICell) cit.Current;
                var (cellType, content) = ExcelReader.ParseCellContent (c);
                if (cellType == NPOI.SS.UserModel.CellType.String) {
                    var columnValue = content.str;
                    headings.Add (columnValue.ToLower ());
                } else
                    throw new Exception ("Excel Header must Contain only String Formatted Cells.");
            }
            return headings;
        }
        private static bool IsEmpty (IRow row) {
            for (IEnumerator cit = row.GetEnumerator (); cit.MoveNext ();) {
                ICell c = (ICell) cit.Current;
                if (c.CellType != NPOI.SS.UserModel.CellType.Blank) {
                    return false;
                }
            }
            return true;
        }
        public static IEnumerable < Dictionary <string, (CellType, DateOrDoubleOrString, bool) >> GetColumnsOfEachRow (string filepath, string sheetname, List<string> columnName) {
            using (var fs = new FileStream (filepath, FileMode.Open, FileAccess.Read)) {
                IWorkbook workbook = new XSSFWorkbook (fs);
                ISheet sheet = workbook.GetSheet (sheetname);
                XSSFFormulaEvaluator evaluator = new XSSFFormulaEvaluator (workbook);
                var rowCount = sheet.LastRowNum;
                var firstRow = sheet.GetRow (0);
                var columnCount = firstRow.LastCellNum;
                var headings = new List<string> ();
                var columnIndexesToCollectData = new List<int> ();
                for (int i = 0; i <= rowCount; i++) {
                    var row = sheet.GetRow (i);
                    if (i == 0) { // Headings
                        headings = ExtractHeadings (row, columnCount);
                        foreach (var colName in columnName) {
                            var ci = headings.IndexOf (colName.ToLower ());
                            columnIndexesToCollectData.Add (ci);
                            if (ci == -1) {
                                throw new Exception ($"There is no Column with Name '{colName}'");
                            }
                        }
                    } else if (!IsEmpty (row)) {
                        var res = new Dictionary <string, (CellType, DateOrDoubleOrString, bool) > ();
                        for (IEnumerator cit = row.GetEnumerator (); cit.MoveNext ();) {
                            ICell c = (ICell) cit.Current;
                            if (columnIndexesToCollectData.Contains (c.ColumnIndex)) {
                                var (cellType, content) = ExcelReader.ParseCellContent (c);
                                if (cellType == NPOI.SS.UserModel.CellType.Numeric) {
                                    if (DateUtil.IsCellDateFormatted (c))
                                        res.Add(headings[c.ColumnIndex], (cellType, content, true));
                                    else
                                        res.Add(headings[c.ColumnIndex], (cellType, content, false));
                                } else if (cellType == NPOI.SS.UserModel.CellType.String) {
                                    res.Add (headings[c.ColumnIndex], (cellType, content, false));
                                }
                            }
                        }
                        yield return res;
                    }
                }
            }
        }

        public static string CellToString ((CellType, DateOrDoubleOrString, bool) tuple) {
            var (cellType, content, isCellDateFormatted) = tuple;
            var cellContent = "";
            if (cellType == NPOI.SS.UserModel.CellType.Numeric) {
                if (isCellDateFormatted)
                    cellContent = content.date.ToString ();
                else
                    cellContent = content.value.ToString ();
            } else {
                cellContent = content.str;
            }
            return cellContent;
        }
    }
}