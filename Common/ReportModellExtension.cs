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

            dataTable.Columns.Add(ConstantData.CT01, typeof(string));
            dataTable.Columns.Add(ConstantData.CT02, typeof(string));
            dataTable.Columns.Add(ConstantData.CT03, typeof(string));
            dataTable.Columns.Add(ConstantData.CT04, typeof(string));
            dataTable.Columns.Add(ConstantData.CT05, typeof(string));
            dataTable.Columns.Add(ConstantData.CT06, typeof(string));
            dataTable.Columns.Add(ConstantData.CT07, typeof(string));
            dataTable.Columns.Add(ConstantData.CT08, typeof(string));
            dataTable.Columns.Add(ConstantData.CT09, typeof(string));
            dataTable.Columns.Add(ConstantData.CT10, typeof(string));
            dataTable.Columns.Add(ConstantData.CT11, typeof(string));
            dataTable.Columns.Add(ConstantData.CT12, typeof(string));


            dataTable.Columns.Add(ConstantData.CT13, typeof(string));
            dataTable.Columns.Add(ConstantData.CT14, typeof(string));
            dataTable.Columns.Add(ConstantData.CT15, typeof(string));
            //dataTable.Columns.Add(ConstantData.CT16, typeof(string));
            dataTable.Columns.Add(ConstantData.CT17, typeof(string));
            dataTable.Columns.Add(ConstantData.CT18, typeof(string));
            //dataTable.Columns.Add(ConstantData.CT19, typeof(string));
            dataTable.Columns.Add(ConstantData.CT20, typeof(string));
            dataTable.Columns.Add(ConstantData.CT21, typeof(string));
            dataTable.Columns.Add(ConstantData.CT22, typeof(string));

            //dataTable.Columns.Add(ConstantData.CT23, typeof(string));
            //dataTable.Columns.Add(ConstantData.CT24, typeof(string));



            //add some rows
            foreach (var stock in reportData.Stocks)
            {
                var date = stock.Date.ToShortDateString();
                var code = stock.Code.ToString();
                var gia = stock.Price.ToString();
                var giaT3 = stock.PriceT3.ToString();
                var hpT4T10 = stock.HPriceT4T10.ToString();
                var vol = stock.Vol.ToString();

                var f01 = stock.Formulars.Any(f => f.Name == ConstantData.CT01).ToString();
                var f02 = stock.Formulars.Any(f => f.Name == ConstantData.CT02).ToString();
                var f03 = stock.Formulars.Any(f => f.Name == ConstantData.CT03).ToString();
                var f04 = stock.Formulars.Any(f => f.Name == ConstantData.CT04).ToString();
                var f05 = stock.Formulars.Any(f => f.Name == ConstantData.CT05).ToString();
                var f06 = stock.Formulars.Any(f => f.Name == ConstantData.CT06).ToString();
                var f07 = stock.Formulars.Any(f => f.Name == ConstantData.CT07).ToString();
                var f08 = stock.Formulars.Any(f => f.Name == ConstantData.CT08).ToString();
                var f09 = stock.Formulars.Any(f => f.Name == ConstantData.CT09).ToString();
                var f10 = stock.Formulars.Any(f => f.Name == ConstantData.CT10).ToString();
                var f11 = stock.Formulars.Any(f => f.Name == ConstantData.CT11).ToString();
                var f12 = stock.Formulars.Any(f => f.Name == ConstantData.CT12).ToString();
                var f13 = stock.Formulars.Any(f => f.Name == ConstantData.CT13).ToString();
                var f14 = stock.Formulars.Any(f => f.Name == ConstantData.CT14).ToString();
                var f15 = stock.Formulars.Any(f => f.Name == ConstantData.CT15).ToString();
                //var f16 = stock.Formulars.Any(f => f.Name == ConstantData.CT16).ToString();
                var f17 = stock.Formulars.Any(f => f.Name == ConstantData.CT17).ToString();
                var f18 = stock.Formulars.Any(f => f.Name == ConstantData.CT18).ToString();
                //var f19 = stock.Formulars.Any(f => f.Name == ConstantData.CT19).ToString();
                var f20 = stock.Formulars.Any(f => f.Name == ConstantData.CT20).ToString();
                var f21 = stock.Formulars.Any(f => f.Name == ConstantData.CT21).ToString();
                var f22 = stock.Formulars.Any(f => f.Name == ConstantData.CT22).ToString();
                //var f23 = stock.Formulars.Any(f => f.Name == ConstantData.CT23).ToString();
                //var f24 = stock.Formulars.Any(f => f.Name == ConstantData.CT24).ToString();

                dataTable.Rows.Add(date, code, gia, vol, giaT3, hpT4T10,
                    f01, f02, f03, f04, f05, f06, f07, f08, f09,
                    f10, f11, f12, f13, f14, f15, /*f16,*/ f17, f18, /*f19,*/
                    f20, f21, f22/*, f23, f24*/

                    );
            }

            return dataTable;
        }
    }

}
