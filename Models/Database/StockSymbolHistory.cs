using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    [Serializable]
    public class StockSymbolHistory
    {
        public int ID { get; set; }
        public decimal O { get; set; }
        public decimal C { get; set; }
        public decimal L { get; set; }
        public decimal H { get; set; }
        public decimal V { get; set; }
        /// <summary>
        /// Time int - perhaps we won't use this - we had Date column
        /// </summary>
        public int T { get; set; }
        public decimal PE { get; set; }
        public decimal RSI { get; set; }
        public decimal RSIAvgG { get; set; }
        public decimal RSIAvgL { get; set; }
        public decimal NenTop { get; set; }
        public decimal NenBot { get; set; }

        public decimal BandsTop { get; set; }
        public decimal BandsBot { get; set; }

        public decimal MACDFast { get; set; }
        public decimal MACDSlow { get; set; }
        public decimal MACDMomentum { get; set; }

        public decimal IchimokuTenKan { get; set; }
        public decimal IchimokuKijun { get; set; }
        public decimal IchimokuCloudTop { get; set; }
        public decimal IchimokuCloudBot { get; set; }

        public string StockSymbol { get; set; }
        public DateTime Date { get; set; }
    }
}

