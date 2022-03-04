using DotNetCoreSqlDb.Common.ArrayExtensions;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using DotNetCoreSqlDb.Models.Business.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DotNetCoreSqlDb.Common
{
    public static class ReportModellExtension
    {
        public static List<ReportFormularModel> Plus(this List<ReportFormularModel> formulars, ReportFormularModel newObject)
        {
            if (newObject != null)
                formulars.Add(newObject);

            return formulars;
        }


        public static DataTable ConvertToDataTable(this ReportModel reportData)
        {
            //create a datatable
            DataTable dataTable = new DataTable();

            //add three colums to the datatable
            dataTable.Columns.Add("Ngay", typeof(string));
            dataTable.Columns.Add("Ma", typeof(string));
            dataTable.Columns.Add("CT1 - Tim Trend Giam", typeof(string));
            dataTable.Columns.Add("CT2 - Tim Đáy", typeof(string));
            dataTable.Columns.Add("CT3 - Tim Sideway", typeof(string));
            dataTable.Columns.Add("CT4 - Tim Gia Giam", typeof(string));

            //add some rows
            foreach (var stock in reportData.Stocks)
            {
                var date = stock.Date.ToShortDateString();
                var code = stock.Code.ToString();
                var day1Active = stock.Formulars.Any(f => f.Name == "Tìm Trend Giảm").ToString();
                var day2Active = stock.Formulars.Any(f => f.Name == "Tim Day 2").ToString();
                var day3Active = stock.Formulars.Any(f => f.Name == "Tăng mạnh sau sideway").ToString();
                var day4Active = stock.Formulars.Any(f => f.Name == "Giá đang giảm mạnh").ToString();

                dataTable.Rows.Add(date, code, day1Active, day2Active, day3Active, day4Active);
            }

            return dataTable;
        }
    }

}
