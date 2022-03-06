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
            if (dataTable == null) return false;

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

            return true;
        }
    }
}
