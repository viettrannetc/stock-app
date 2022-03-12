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
            dataTable.Columns.Add("Gia HP T4-T10", typeof(string));

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


            dataTable.Columns.Add(ConstantData.CT13, typeof(string));
            dataTable.Columns.Add(ConstantData.CT14, typeof(string));
            dataTable.Columns.Add(ConstantData.CT15, typeof(string));
            dataTable.Columns.Add(ConstantData.CT16, typeof(string));
            dataTable.Columns.Add(ConstantData.CT17, typeof(string));
            dataTable.Columns.Add(ConstantData.CT18, typeof(string));
            dataTable.Columns.Add(ConstantData.CT19, typeof(string));
            dataTable.Columns.Add(ConstantData.CT20, typeof(string));
            dataTable.Columns.Add(ConstantData.CT21, typeof(string));
            dataTable.Columns.Add(ConstantData.CT22, typeof(string));



            //add some rows
            foreach (var stock in reportData.Stocks)
            {
                var date = stock.Date.ToShortDateString();
                var code = stock.Code.ToString();
                var gia = stock.Price.ToString();
                var giaT3 = stock.PriceT3.ToString();
                var hpT4T10 = stock.HPriceT4T10.ToString();
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

                var f13Active = stock.Formulars.Any(f => f.Name == ConstantData.CT13).ToString();
                var f14Active = stock.Formulars.Any(f => f.Name == ConstantData.CT14).ToString();
                var f15Active = stock.Formulars.Any(f => f.Name == ConstantData.CT15).ToString();
                var f16Active = stock.Formulars.Any(f => f.Name == ConstantData.CT16).ToString();
                var f17Active = stock.Formulars.Any(f => f.Name == ConstantData.CT17).ToString();
                var f18Active = stock.Formulars.Any(f => f.Name == ConstantData.CT18).ToString();
                var f19Active = stock.Formulars.Any(f => f.Name == ConstantData.CT19).ToString();
                var f20Active = stock.Formulars.Any(f => f.Name == ConstantData.CT20).ToString();
                var f21Active = stock.Formulars.Any(f => f.Name == ConstantData.CT21).ToString();
                var f22Active = stock.Formulars.Any(f => f.Name == ConstantData.CT22).ToString();

                dataTable.Rows.Add(date, code, gia, vol, giaT3, hpT4T10,
                    f1Active, f2Active, f3Active, f4Active, f5Active, f6Active, f7Active, f8Active, f9Active,
                    f10Active, f11Active, f12Active, f13Active, f14Active, f15Active, f16Active, f17Active, f18Active, f19Active,
                    f20Active, f21Active, f22Active
                    );
            }

            return dataTable;
        }
    }

}
