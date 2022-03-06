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
            if (!reportData.Stocks.Any()) return null;

            //create a datatable
            DataTable dataTable = new DataTable();

            //add three colums to the datatable
            dataTable.Columns.Add("Ngay", typeof(string));
            dataTable.Columns.Add("Ma", typeof(string));
            dataTable.Columns.Add("Gia T0", typeof(string));
            dataTable.Columns.Add("KLGD", typeof(string));
            dataTable.Columns.Add("Gia T3", typeof(string));
            dataTable.Columns.Add(ConstantData.CT1, typeof(string));
            dataTable.Columns.Add(ConstantData.CT2, typeof(string));
            dataTable.Columns.Add(ConstantData.CT3, typeof(string));
            dataTable.Columns.Add(ConstantData.CT4, typeof(string));
            dataTable.Columns.Add(ConstantData.CT5, typeof(string));
            dataTable.Columns.Add(ConstantData.CT6, typeof(string));
            dataTable.Columns.Add(ConstantData.CT7, typeof(string));
            dataTable.Columns.Add(ConstantData.CT8, typeof(string));
            dataTable.Columns.Add(ConstantData.CT9, typeof(string));
            dataTable.Columns.Add(ConstantData.CT10, typeof(string));
            dataTable.Columns.Add(ConstantData.CT11, typeof(string));
            dataTable.Columns.Add(ConstantData.CT12, typeof(string));



            //add some rows
            foreach (var stock in reportData.Stocks)
            {
                var date = stock.Date.ToShortDateString();
                var code = stock.Code.ToString();
                var gia = stock.Price.ToString();
                var giaT3 = stock.PriceT3.ToString();
                var vol = stock.Vol.ToString();

                var f1Active = stock.Formulars.Any(f => f.Name == ConstantData.CT1).ToString();
                var f2Active = stock.Formulars.Any(f => f.Name == ConstantData.CT2).ToString();
                var f3Active = stock.Formulars.Any(f => f.Name == ConstantData.CT3).ToString();
                var f4Active = stock.Formulars.Any(f => f.Name == ConstantData.CT4).ToString();
                var f5Active = stock.Formulars.Any(f => f.Name == ConstantData.CT5).ToString();
                var f6Active = stock.Formulars.Any(f => f.Name == ConstantData.CT6).ToString();
                var f7Active = stock.Formulars.Any(f => f.Name == ConstantData.CT7).ToString();
                var f8Active = stock.Formulars.Any(f => f.Name == ConstantData.CT8).ToString();
                var f9Active = stock.Formulars.Any(f => f.Name == ConstantData.CT9).ToString();
                var f10Active = stock.Formulars.Any(f => f.Name == ConstantData.CT10).ToString();
                var f11Active = stock.Formulars.Any(f => f.Name == ConstantData.CT11).ToString();
                var f12Active = stock.Formulars.Any(f => f.Name == ConstantData.CT12).ToString();

                dataTable.Rows.Add(date, code, gia, vol, giaT3,
                    f1Active, f2Active, f3Active, f4Active,
                    f5Active, f6Active, f7Active, f8Active,
                    f9Active, f10Active, f11Active, f12Active
                    );
            }

            return dataTable;
        }
    }

}
