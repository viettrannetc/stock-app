using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report
{
    public class ReportModel
    {
        public ReportModel()
        {
            Stocks = new List<ReportStockModel>();
        }

        public List<ReportStockModel> Stocks { get; set; }
    }

    public class ReportStockModel
    {
        public ReportStockModel()
        {
            Formulars = new List<ReportFormularModel>();
        }
        public DateTime Date { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
        public decimal PriceT3 { get; set; }
        public decimal HPriceT4T10 { get; set; }
        public decimal Vol { get; set; }

        public List<ReportFormularModel> Formulars { get; set; }
    }

    public class ReportFormularModel
    {
        public string Name { get; set; }
        public decimal Vol { get { return 100; } }
        public decimal Price { get; set; }
    }

    public interface IReportFormular
    {
        ReportFormularModel Calculation(string stockCode, DateTime checkingDate, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories);

    }



    /*
     * Fomula in the list:
     * ATR: https://rtmath.net/assets/docs/finanalysis/html/30265e11-f334-4809-b9fd-2e7120aa0acd.htm
     */

}
