using CSharpItertools;
using DotNetCoreSqlDb.Models.Business;
using DotNetCoreSqlDb.Models.Learning;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DotNetCoreSqlDb.Common
{
    public static class ExcelExtension
    {
        /// <summary>
        /// https://riptutorial.com/epplus/example/26422/fill-with-a-datatable
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static bool WriteToExcel(this DataTable dataTable, string filename, bool usingHeader = true)
        {
            if (dataTable == null) return false;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage excelPackage = new ExcelPackage(filename))
            {
                //create a WorkSheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Any()
                    ? excelPackage.Workbook.Worksheets[0]
                    : excelPackage.Workbook.Worksheets.Add("Sheet 1");

                //add all the content from the DataTable, starting at cell A1
                var startCell = usingHeader
                    ? "A1"
                    : $"A{int.Parse(worksheet.Cells.Last(c => c.Start.Column == 1).ToString().Split('A')[1]) + 1}";

                worksheet.Cells[startCell].LoadFromDataTable(dataTable, usingHeader);

                excelPackage.Save();

                //Create excel file on physical disk
                FileStream objFileStrm = File.Create(filename);
                objFileStrm.Close();

                // Write content to excel file 
                File.WriteAllBytes(filename, excelPackage.GetAsByteArray());

                //Close Excel package
                //excelPackage.Dispose();
            }

            return true;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/13396604/excel-to-datatable-using-epplus-excel-locked-for-editing
        /// </summary>
        /// <param name="excelPath"></param>
        /// <param name="hasHeader"></param>
        /// <returns></returns>
        public static DataTable ReadFromExcel(this string excelPath, bool hasHeader = true)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var pck = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(excelPath))
                {
                    pck.Load(stream);
                }
                var ws = pck.Workbook.Worksheets.First();
                DataTable tbl = new DataTable();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    tbl.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                }
                var startRow = hasHeader ? 2 : 1;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    DataRow row = tbl.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }
                return tbl;
            }
        }


        public static void GetCombination(this List<string> str, List<string> possibleOptions, List<string> result)
        {


            //var item = new StringBuilder();

            //for (int i = 1; i <= str.Count; i++)
            //{
            //    foreach (var option in possibleOptions)
            //    {

            //        //item.Append($"{str[i]}-{option}");
            //        GetCombination
            //    }

            //}

            //return null;

            string item = string.Empty;
            var firstItem = str[0];
            foreach (var option in possibleOptions)
            {
                item = $"{firstItem}-{option}";
                var remainingItems = str.Where(x => x != firstItem).ToList();
                GetCombination1(item, remainingItems, possibleOptions, result);
            }

        }

        private static void GetCombination1(string parentItem, List<string> str, List<string> possibleOptions, List<string> result)
        {
            //var expectedIndex;
            var firstItem = str[0];
            var remainingItems = str.Where(x => x != firstItem).ToList();
            foreach (var option in possibleOptions)
            {
                if (remainingItems.Any())
                {
                    //parentItem = ;
                    GetCombination1($"{parentItem},{firstItem}-{option}", remainingItems, possibleOptions, result);
                }
                else
                {
                    result.Add($"{parentItem},{firstItem}-{option}");
                }
            }

            //return parentItem;
        }

        /// <summary>
        /// https://riptutorial.com/epplus/example/26422/fill-with-a-datatable
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static LearningDataResponseModel ExportTo(this DataTable dataTable,
            EnumExcelColumnModel targetColumn,
            LearningDataConditionModel condition,
            params EnumExcelColumnModel[] columnNames)
        {
            if (dataTable == null) return null;

            var result = new LearningDataResponseModel();

            var combinations = new List<string>();
            columnNames.Select(x => x.ToString()).ToList().GetCombination(new List<string>() { "True", "False" }, combinations);

            var data = dataTable.AsEnumerable();

            var gotConditionRows = dataTable
                .AsEnumerable();

            foreach (var item in condition.Condition)
            {
                gotConditionRows = gotConditionRows.Where(myRow => ConstantData.Condition.Contains(myRow.Field<string>((int)item.Key)) == item.Value);
            }

            foreach (var combination in combinations)
            {
                var pattern = gotConditionRows.AsEnumerable();

                foreach (var item in combination.Split(','))
                {
                    var column = item.Split('-')[0];
                    Enum.TryParse(column.Trim(), out EnumExcelColumnModel myColumn);
                    var value = item.Split('-')[1];
                    pattern = pattern.Where(myRow => myRow.Field<string>((int)myColumn) == value);
                }

                var tong = pattern.Count();
                var success = tong == 0
                    ? 0
                    : pattern.Count(myRow => ConstantData.Condition.Contains(myRow.Field<string>((int)targetColumn)));
                var tile = tong == 0
                    ? 0
                    : Math.Round((decimal)success / (decimal)tong, 2) * 100;
                result.Pattern.Add(new LearningDataPatternResponseModel
                {
                    Pattern = $"[{string.Join(", ", combination)}]",
                    Tile = tile,
                    Tong = tong
                });
            }

            result.Pattern = result.Pattern.OrderByDescending(r => r.Tile).ToList();
            return result;
        }

        public static void Merge(this string folderOfExcelFiles)
        {
            DirectoryInfo di = new DirectoryInfo(folderOfExcelFiles);
            FileInfo[] files = di.GetFiles("*.xlsx");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var batchNumber = 50;
            var batch = files.Length / batchNumber;
            batch = batch + 1;

            for (int j = 0; j < batch; j++)
            {
                var skip = j == 0 ? 0 : j * batchNumber;

                var filesInBatch = files.Skip(skip).Take(batchNumber).ToList();
                var name = $"{folderOfExcelFiles}{j}.xlsx";
                for (int i = 0; i < filesInBatch.Count; i++)
                {
                    var hasHeader = i == 0;
                    var file = j == 0 ? files[i] : files[skip + i + 1];
                    var data = file.FullName.ReadFromExcel();
                    data.WriteToExcel(name, hasHeader);
                }
            }
        }

        public static LearningDataResponseModel ExportTo(this DataTable dataTable,
            int minMatchedPattern,
            decimal minExpectedSucceed,
            EnumExcelColumnModel targetColumn,
            LearningDataConditionModel condition,
            params EnumExcelColumnModel[] columnNames)
        {
            if (dataTable == null) return null;

            var result = new LearningDataResponseModel();

            var combinations = new List<string>();
            columnNames.Select(x => x.ToString()).ToList().GetCombination(new List<string>() { "True", "False" }, combinations);

            var data = dataTable.AsEnumerable();
            var codes = data.Select(x => x.Field<string>(1).Trim()).Distinct().ToList();

            var gotConditionRows = dataTable
                .AsEnumerable();

            foreach (var item in condition.Condition)
            {
                gotConditionRows = gotConditionRows.Where(myRow => ConstantData.Condition.Contains(myRow.Field<string>((int)item.Key)) == item.Value);
            }

            foreach (var combination in combinations)
            {
                var rowsMatchedPattern = gotConditionRows.AsEnumerable();

                foreach (var item in combination.Split(','))
                {
                    var column = item.Split('-')[0];
                    Enum.TryParse(column.Trim(), out EnumExcelColumnModel myColumn);
                    var value = item.Split('-')[1];
                    rowsMatchedPattern = rowsMatchedPattern.Where(myRow => myRow.Field<string>((int)myColumn) == value);
                }

                var codeDetails = new LearningDataPatternWithCodeResponseModel
                {
                    Pattern = $"[{string.Join(", ", combination)}]"
                };
                foreach (var ma in codes)
                {
                    var maData = rowsMatchedPattern.Where(myRow => myRow.Field<string>(1) == ma);

                    var tong = maData.Count();
                    var success = tong == 0
                        ? 0
                        : maData.Count(myRow => ConstantData.Condition.Contains(myRow.Field<string>((int)targetColumn)));
                    var tile = tong == 0
                        ? 0
                        : Math.Round((decimal)success / (decimal)tong, 2) * 100;

                    if (tile > minExpectedSucceed)
                        codeDetails.Details.Add(new LearningDataPatternWithCodeDetailResponseModel()
                        {
                            Tile = tile,
                            Tong = tong,
                            Ma = ma
                        });
                }

                codeDetails.Details = codeDetails.Details.OrderByDescending(r => r.Tile).ToList();
                result.PatternWithCodes.Add(codeDetails);
            }

            result.PatternWithCodes = result.PatternWithCodes.OrderByDescending(r => r.Pattern).ToList();
            return result;
        }
    }
}
