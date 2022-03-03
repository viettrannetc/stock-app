using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;

namespace DotNetCoreSqlDb.Models.Business
{
    public class VietStockSymbolTradingHistoryResponseModel
    {
        public VietStockSymbolTradingHistoryResponseModel()
        {
            data = new List<VietStockSymbolTradingHistoryModel>();
        }

        public List<VietStockSymbolTradingHistoryModel> data { get; set; }
        public decimal execute_time_ms { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
    public class VietStockSymbolTradingHistoryModel
    {
        /*
		 * "Stockcode": "CEO",
        "Package": 144720,
        "TradingDate": "/Date(1646120841000)/",
        "Price": 71800.00,
        "Vol": 100,
        "TotalVol": 8169767,
        "TotalVal": 565161884700,
        "Change": 6100.00,
        "IsBuy": false,
        "PerChange": 9.28
		 */
        public VietStockSymbolTradingHistoryModel()
        { }

        public string Code { get; set; }
        public int Package { get; set; }
        public string TradingDate { get; set; }
        public decimal Price { get; set; }
        public decimal total_vol { get; set; }
        public decimal match_qtty { get; set; }
        public decimal Change { get; set; }
        public bool IsBuy { get; set; }
        public decimal PerChange { get; set; }
        public string time { get; set; }
        public string side { get; set; }
        public DateTime Date
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TradingDate))
                    return double.Parse(TradingDate.Substring(6, 13)).UnixTimeStampToDateTime();
                else
                    return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day -1, int.Parse(time.Substring(0, 2)), int.Parse(time.Substring(3, 2)), int.Parse(time.Substring(6, 2)));
            }
        }

        //public DateTime Date
        //{
        //    get
        //    {
        //        return DateTime.Parse(time);
        //    }
        //}
    }
}
