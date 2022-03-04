using OfficeOpenXml;
using System;
using System.Data;
using System.IO;
using System.Linq;

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
        public static bool WriteToExcel(this DataTable dataTable, string filename)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (ExcelPackage excelPackage = new ExcelPackage(filename))
                {
                    //create a WorkSheet
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet 1");

                    //add all the content from the DataTable, starting at cell A1
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                    excelPackage.Save();

                    //Create excel file on physical disk
                    FileStream objFileStrm = File.Create(filename);
                    objFileStrm.Close();

                    // Write content to excel file 
                    File.WriteAllBytes(filename, excelPackage.GetAsByteArray());

                    //Close Excel package
                    //excelPackage.Dispose();
                }

                //if (!File.Exists(filename))                 // Write to a file.
                //{
                //	using (ExcelPackage excelPackage = new ExcelPackage(filename))
                //	{
                //		excelPackage.Workbook.Worksheets.Add("Work sheet 1");

                //		var worksheet = excelPackage.Workbook.Worksheets[0];

                //		worksheet.Cells[1, 1].Value = "FeatureId";
                //		worksheet.Cells[1, 2].Value = "Feature";
                //		worksheet.Cells[1, 3].Value = "USId";
                //		worksheet.Cells[1, 4].Value = "USTitle";
                //		worksheet.Cells[1, 5].Value = "WPId";
                //		worksheet.Cells[1, 6].Value = "WPTitle";
                //		worksheet.Cells[1, 7].Value = "WPType";
                //		worksheet.Cells[1, 8].Value = "WPStart";
                //		worksheet.Cells[1, 9].Value = "WPDuedate";
                //		worksheet.Cells[1, 10].Value = "WPStatus";
                //		worksheet.Cells[1, 11].Value = "WPTeam";
                //		worksheet.Cells[1, 12].Value = "WPAssignee";
                //		worksheet.Cells[1, 13].Value = "WPEstimate";
                //		worksheet.Cells[1, 14].Value = "WPRemainingHour";
                //		worksheet.Cells[1, 15].Value = "WPSpentHour";
                //		worksheet.Cells[1, 16].Value = "WPIterationId";
                //		worksheet.Cells[1, 17].Value = "WPIterationName";
                //		worksheet.Cells[1, 18].Value = "WPDependOn";
                //		worksheet.Cells[1, 19].Value = "WPPriority";
                //		worksheet.Cells[1, 20].Value = "Version";
                //		worksheet.Cells[1, 21].Value = "VersionDate";

                //		// Create excel file on physical disk 
                //		FileStream objFileStrm = File.Create(filename);
                //		objFileStrm.Close();

                //		// Write content to excel file 
                //		File.WriteAllBytes(filename, excelPackage.GetAsByteArray());
                //		//Close Excel package
                //		excelPackage.Dispose();
                //	}
                //}

                ////var file = new FileInfo(filename);
                //using (ExcelPackage excelPackage = new ExcelPackage(filename))
                //{
                //	var worksheet = excelPackage.Workbook.Worksheets[0]; ////Get a WorkSheet by index. Note that EPPlus indexes are base 1, not base 0!

                //	var lastRowCell1 = worksheet.Cells.Last(c => c.Start.Column == 1);

                //	var lastRowIndex = worksheet.Dimension.Rows;
                //	lastRowIndex += 1;
                //	for (int i = 0; i < records.Count(); i++)
                //	{
                //		worksheet.Cells[lastRowIndex + i, 1].Value = records[i].FeatureId;
                //		worksheet.Cells[lastRowIndex + i, 2].Value = records[i].Feature;
                //		worksheet.Cells[lastRowIndex + i, 3].Value = records[i].USId;
                //		worksheet.Cells[lastRowIndex + i, 4].Value = records[i].USTitle;
                //		worksheet.Cells[lastRowIndex + i, 5].Value = records[i].WPId;
                //		worksheet.Cells[lastRowIndex + i, 6].Value = records[i].WPTitle;
                //		worksheet.Cells[lastRowIndex + i, 7].Value = records[i].WPType;

                //		worksheet.Cells[lastRowIndex + i, 8].Value = records[i].WPStart;
                //		worksheet.Cells[lastRowIndex + i, 8].Style.Numberformat.Format = "dd/mm/yyyy";
                //		//worksheet.Cells[lastRowIndex + i, 8].Formula = "=DATE(2014,10,5)";

                //		worksheet.Cells[lastRowIndex + i, 9].Value = records[i].WPDueDate;
                //		worksheet.Cells[lastRowIndex + i, 9].Style.Numberformat.Format = "dd/mm/yyyy";
                //		//worksheet.Cells[lastRowIndex + i, 9].Formula = "=DATE(2014,10,5)";

                //		worksheet.Cells[lastRowIndex + i, 10].Value = records[i].WPStatus;
                //		worksheet.Cells[lastRowIndex + i, 11].Value = records[i].WPTeam;
                //		worksheet.Cells[lastRowIndex + i, 12].Value = records[i].WPAssignee;
                //		worksheet.Cells[lastRowIndex + i, 13].Value = records[i].WPEstimate;
                //		worksheet.Cells[lastRowIndex + i, 14].Value = records[i].WPRemainingHour;
                //		worksheet.Cells[lastRowIndex + i, 15].Value = records[i].WPSpentHour;
                //		worksheet.Cells[lastRowIndex + i, 16].Value = records[i].WPIterationId;
                //		worksheet.Cells[lastRowIndex + i, 17].Value = records[i].WPIterationName;
                //		worksheet.Cells[lastRowIndex + i, 18].Value = records[i].WPDependOn;
                //		worksheet.Cells[lastRowIndex + i, 19].Value = records[i].WPPriority;
                //		worksheet.Cells[lastRowIndex + i, 20].Value = records[i].Version;

                //		worksheet.Cells[lastRowIndex + i, 21].Value = records[i].VersionDate;
                //		worksheet.Cells[lastRowIndex + i, 21].Style.Numberformat.Format = "dd/mm/yyyy";
                //		//worksheet.Cells[lastRowIndex + i, 21].Formula = "=DATE(2014,10,5)";
                //	}

                //	excelPackage.Save();
                //}

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
