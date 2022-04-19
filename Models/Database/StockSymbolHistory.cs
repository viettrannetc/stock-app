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
        public string StockSymbol { get; set; }

        public DateTime Date { get; set; }
    }
}

