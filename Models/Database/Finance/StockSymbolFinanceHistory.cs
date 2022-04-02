using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    [Serializable]
    public class StockSymbolFinanceHistory
    {
        public int ID { get; set; }
        public string StockSymbol { get; set; }
        public int YearPeriod { get; set; }
        public int Quarter { get; set; }
        public string TermCode { get; set; }
        public string PeriodBegin { get; set; }

    }

    [Serializable]
    public class StockSymbolDetail
    {
        public int ID { get; set; }
        public int StockSymbolFinanceHistoryId { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string ReportComponentName { get; set; }
        public string ReportComponentNameEn { get; set; }
        public decimal Value { get; set; }
    }

    [Serializable]
    public class StockSymbolKQKD : StockSymbolDetail
    {
    }

    [Serializable]
    public class StockSymbolCDKT : StockSymbolDetail
    {
    }

    [Serializable]
    public class StockSymbolCSTC : StockSymbolDetail
    {
    }

    [Serializable]
    public class StockSymbolLCTT : StockSymbolDetail
    {
    }

    [Serializable]
    public class StockSymbolCTKH : StockSymbolDetail
    {
    }
}

