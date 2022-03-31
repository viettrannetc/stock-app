using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    [Serializable]
    public class StockSymbolFinanceHistory
    {
        public int ID { get; set; }
        public string StockSymbol { get; set; }
    }


    [Serializable]
    public class StockSymbolFinanceYearHistory
    {
        public int ID { get; set; }
        public int StockSymbolFinanceHistoryId { get; set; }
        public int Year { get; set; }

    }

    [Serializable]
    public class StockSymbolFinanceQuarterHistory
    {
        public int ID { get; set; }
        public int StockSymbolFinanceHistoryId { get; set; }
        public int Quarter { get; set; }

    }


    [Serializable]
    public class StockSymbolBCTT
    {
        public int ID { get; set; }
        public int QuarterHistoryId { get; set; }
        public int YearHistoryId { get; set; }
    }

    [Serializable]
    public class StockSymbolKQKD
    {
        public int ID { get; set; }
        public int StockSymbolBCTTId { get; set; }
    }

    [Serializable]
    public class StockSymbolCDKT
    {
        public int ID { get; set; }
        public int StockSymbolBCTTId { get; set; }
    }

    [Serializable]
    public class StockSymbolCSTC
    {
        public int ID { get; set; }
        public int StockSymbolBCTTId { get; set; }
    }

    [Serializable]
    public class StockSymbolLCTT
    {
        public int ID { get; set; }
        public int StockSymbolBCTTId { get; set; }
    }

    [Serializable]
    public class StockSymbolCTKH
    {
        public int ID { get; set; }
        public int StockSymbolBCTTId { get; set; }
    }
}

