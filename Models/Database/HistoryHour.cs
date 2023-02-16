using DotNetCoreSqlDb.Models.Business;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetCoreSqlDb.Models
{
    public class HistoryHour
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
        public decimal RSI { get; set; }
        public decimal NenTop { get; set; }
        public decimal NenBot { get; set; }
        public decimal BandsTop { get; set; }
        public decimal BandsBot { get; set; }
        /// <summary>
        /// defined as price MA 20
        /// </summary>
        public decimal BandsMid { get; set; }
        /// <summary>
        /// 12/26
        /// </summary>
        public decimal MACD { get; set; }
        /// <summary>
        /// 9 of 12/26
        /// </summary>
        public decimal MACDSignal { get; set; }
        /// <summary>
        /// slow - fast
        /// </summary>
        public decimal MACDMomentum { get; set; }

        public decimal IchimokuTenKan { get; set; }
        public decimal IchimokuKijun { get; set; }
        public decimal IchimokuCloudTop { get; set; }
        public decimal IchimokuCloudBot { get; set; }
        public decimal GiaMA05 { get; set; }
        public string StockSymbol { get; set; }
        public DateTime Date { get; set; }

        [NotMapped]
        public decimal IchimokuTop
        {
            get
            {
                return Math.Max(IchimokuCloudTop, IchimokuCloudBot);
            }
        }

        [NotMapped]
        public decimal IchimokuBot
        {
            get
            {
                return Math.Min(IchimokuCloudTop, IchimokuCloudBot);
            }
        }

        public EnumPhanKi RSIPhanKi { get; set; }
        public EnumPhanKi MACDPhanKi { get; set; }
    }
}

