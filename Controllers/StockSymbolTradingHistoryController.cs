using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using DotNetCoreSqlDb.Common;
using System.Collections.Concurrent;

namespace DotNetCoreSqlDb.Controllers
{
    public class StockSymbolTradingHistoryController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockSymbolTradingHistoryController(MyDatabaseContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<string> Create()
        {
            /*
             *  https://www.w3schools.com/php/phptryit.asp?filename=tryphp_compiler
                echo date("Y-m-d H:i:s", 1388516401);
                echo strtotime("2022-02-22 00:00:01");
                echo "-";
                echo strtotime("2022-02-24 00:00:01");
             */
            var restService = new RestServiceHelper();

            var huyNiemYet = new List<string>();
            huyNiemYet.Add("KSK");
            huyNiemYet.Add("TRT");
            huyNiemYet.Add("ABR");
            huyNiemYet.Add("FUCTVGF2");

            var allSymbols = await _context.StockSymbol
                //.Where(s => s._sc_ == "CEO")
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            allSymbols = allSymbols.Where(s => !huyNiemYet.Contains(s._sc_)).ToList();

            var result = new List<StockSymbolTradingHistory>();
            
            await GetV(result, allSymbols);

            if (result.Any())
            {
                await _context.StockSymbolTradingHistory.AddRangeAsync(result);
                await _context.SaveChangesAsync();
            }

            return "true";
        }

        public async Task GetV(List<StockSymbolTradingHistory> result, List<StockSymbol> allSymbols)
        {
            var restService = new RestServiceHelper();
            List<Task> TaskList = new List<Task>();
            foreach (var item in allSymbols)
            {
                var LastTask = GetStockDataByDay(item, restService, result);
                //LastTask.Start();
                TaskList.Add(LastTask);
            }

            await Task.WhenAll(TaskList.ToArray());

            //result = result.Where(r => r.Date > currentLatestDate).ToList();

            //var updated = result.Select(r => r.StockSymbol).ToList();
            //var notIn = allSymbols.Where(s => !updated.Contains(s._sc_)).ToList();

            //if (notIn.Any())
            //    await GetV(result, notIn);
        }

        /// <summary>
		/// Example: "https://api.vietstock.vn/ta/history?symbol=VIC&resolution=D&from=1609459200&to=1644796800";
		/// </summary>
		/// <params>{0}: symbol code</params>
		/// <params>{1}: resolution = D</params>
		/// <params>{2}: from: int from php code</params>
		/// <params>{3}: to: int from php code</params>
		public const string VietStock_GetTradingHistoryBySymbolCode = "https://api-finance-t19.24hmoney.vn/v1/web/stock/transaction-list-ssi?device_id=web&device_name=INVALID&device_model=Windows+10&network_carrier=INVALID&connection_type=INVALID&os=Chrome&os_version=98.0.4758.102&app_version=INVALID&access_token=INVALID&push_token=INVALID&locale=vi&symbol={0}&page=1&per_page=21600";

        private async Task GetStockDataByDay(StockSymbol item, RestServiceHelper restService, List<StockSymbolTradingHistory> result)
        {
            var url = string.Format(VietStock_GetTradingHistoryBySymbolCode, item._sc_);
            var allSharePointsObjects = await restService.Get<VietStockSymbolTradingHistoryResponseModel>(url);

            var numberOfT = allSharePointsObjects.data.Count();

            for (int i = 0; i < numberOfT; i++)
            {
                var obj = allSharePointsObjects.data[i];
                var history = new StockSymbolTradingHistory();

                history.StockSymbol = item._sc_;
                history.Date = obj.Date;
                history.Price = obj.Price;
                history.TotalVol = obj.total_vol;
                history.MatchQtty = obj.match_qtty;
                history.IsBuy = obj.IsBuy;
                history.Change = obj.Change;

                //var isExisting = _context.StockSymbolHistory.Any(e => e.StockSymbol == history.StockSymbol && e.T == history.T);
                //if (!isExisting)
                result.Add(history);
            }
        }
    }
}
