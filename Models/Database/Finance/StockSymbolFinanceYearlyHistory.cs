using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    [Serializable]
    public class StockSymbolFinanceYearlyHistory
    {
        public int ID { get; set; }
        public string StockSymbol { get; set; }
        public int YearPeriod { get; set; }
        public string NameEn { get; set; }
        public decimal? Value { get; set; }

        /// <summary>
        /// 1: KQKD
        /// 2: CDKT
        /// 3: CSTC
        /// 4: LCTT
        /// 5: CTKH
        /// 6: BCTQ
        /// </summary>
        public int Type { get; set; }
    }
}

