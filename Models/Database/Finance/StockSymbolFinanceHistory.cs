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
        public string Name { get; set; }
        public string NameEn { get; set; }
        public decimal? Value { get; set; }

        /// <summary>
        /// 1: KQKD
        /// 2: CDKT
        /// 3: CSTC
        /// 4: LCTT
        /// 5: CTKH
        /// </summary>
        public int Type { get; set; }
    }

    public enum FinanceType
    {
        KQKD = 1,
        CDKT = 2,
        CSTC = 3,
        LCTT = 4,
        CTKH = 5
    }

    //[Serializable]
    //public class StockSymbolKQKD : StockSymbolDetail
    //{
    //}

    //[Serializable]
    //public class StockSymbolCDKT : StockSymbolDetail
    //{
    //}

    //[Serializable]
    //public class StockSymbolCSTC : StockSymbolDetail
    //{
    //}

    //[Serializable]
    //public class StockSymbolLCTT : StockSymbolDetail
    //{
    //}

    //[Serializable]
    //public class StockSymbolCTKH : StockSymbolDetail
    //{
    //}
}

