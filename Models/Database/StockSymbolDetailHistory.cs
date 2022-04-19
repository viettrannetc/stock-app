using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    public class StockSymbolDetailHistory
    {
        public int ID { get; set; }

        public decimal PE { get; set; }
        
        /// <summary>
        /// "CTCP 32",
        /// </summary>
        public string stockName { get; set; }

        public DateTime StockDate { get; set; }

    }
}

