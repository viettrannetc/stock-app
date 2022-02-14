using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Models
{
    public class StockSymbolHistory
    {
        public int ID { get; set; }
        public string O { get; set; }
        public string C { get; set; }
        public string L { get; set; }
        public string H { get; set; }
        public string V { get; set; }

        public string StockSymbol { get; set; }

        public DateTime Date { get; set; }
    }
}

