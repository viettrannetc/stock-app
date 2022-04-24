using System;

namespace DotNetCoreSqlDb.Models.Database.Finance
{
    [Serializable]
    public class KLGDMuaBan
    {
        public int ID { get; set; }
        /// <summary>
        /// Mua chủ động
        /// </summary>
        public decimal TotalBuy { get; set; }
        /// <summary>
        /// Bán chủ động
        /// </summary>
        public decimal TotalSell { get; set; }
        /// <summary>
        /// Chưa xác định
        /// </summary>
        public decimal TotalUnknow { get; set; }
        /// <summary>
        /// Tổng
        /// </summary>
        public decimal TotalVol { get; set; }
        public string StockSymbol { get; set; }
        /// <summary>
        /// combination between date & TIME
        /// </summary>
        public DateTime Date { get; set; }
    }
}
