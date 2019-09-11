using System;
using System.Collections;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace SeibelCases {
    public class ExcelReader {
        public static void PrintData (string filepath) {
            using (var fs = new FileStream (filepath, FileMode.Open, FileAccess.Read)) {

                IWorkbook workbook = new XSSFWorkbook (fs);
                ISheet sheet = workbook.GetSheet ("Sheet1");
                XSSFFormulaEvaluator evaluator = new XSSFFormulaEvaluator (workbook);
                System.Console.WriteLine (sheet.LastRowNum);
                for (IEnumerator rit = sheet.GetRowEnumerator (); rit.MoveNext ();) {
                    IRow r = (IRow) rit.Current;
                    for (IEnumerator cit = r.GetEnumerator (); cit.MoveNext ();) {
                        ICell c = (ICell) cit.Current;
                        if (c.CellType == NPOI.SS.UserModel.CellType.Numeric) {
                            if (DateUtil.IsCellDateFormatted (c)) {
                                var date = DateUtil.GetJavaDate (c.NumericCellValue);
                                // var dateFormat = c.CellStyle.GetDataFormatString ();
                                Console.WriteLine (date);
                            } else {
                                Console.WriteLine (c.NumericCellValue);
                            }
                        } else {
                            Console.WriteLine (c.StringCellValue);
                        }
                    }
                }
            }
        }
    }
}