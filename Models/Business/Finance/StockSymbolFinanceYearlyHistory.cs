using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models.Business
{
    public class StockSymbolFinanceYearlyHistoryModel
    {
        public int Year { get; set; }
        public decimal roic { get; set; }
        public decimal roe { get; set; }
        public decimal saleGrowth { get; set; }
        public decimal epsGrowth { get; set; }
        public decimal bvpsGrowth { get; set; }
        public decimal cashGrowth { get; set; }
        public decimal timeToPayDept { get; set; }
        public decimal PECoBan { get; set; }
        public decimal LNSTGrowth { get; set; }
    }
}

