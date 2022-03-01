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
        public decimal Match_qtty { get; set; }
        /// <summary>
        /// Total vol from begining of the day until this transaction - it's a sum of all Match_qtty of previous transactions in the day
        /// </summary>
        public decimal Total_vol { get; set; }
        /// <summary>
        /// sd is sell, bu is buy, unknown
        /// </summary>
        public string Side { get; set; }
        public string StockSymbol { get; set; }
        /// <summary>
        /// combination between date & TIME
        /// </summary>
        public DateTime Date { get; set; }
    }
}

