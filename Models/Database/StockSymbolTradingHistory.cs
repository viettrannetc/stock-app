using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    [Serializable]
    public class StockSymbolTradingHistory
    {
        public int ID { get; set; }
        /// <summary>
        /// Price for this transaction
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Compare with the 1st price in the day (open price)
        /// </summary>
        public decimal Change { get; set; }
        /// <summary>
        /// Vol for each transaction
        /// </summary>
        public decimal MatchQtty { get; set; }
        /// <summary>
        /// Total vol from begining of the day until this transaction - it's a sum of all Match_qtty of previous transactions in the day
        /// </summary>
        public decimal TotalVol { get; set; }
        /// <summary>
        /// sd is sell, bu is buy, unknown
        /// </summary>
        public bool IsBuy { get; set; }
        public string StockSymbol { get; set; }
        /// <summary>
        /// combination between date & TIME
        /// </summary>
        public DateTime Date { get; set; }

        public bool IsTangDotBien { get; set; }
    }
}

