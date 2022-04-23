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
using System.IO;
using Newtonsoft.Json;

namespace DotNetCoreSqlDb.Controllers
{
    /// <summary>
    /// Cái này không xài nữa vì lượng data mỗi ngày quá lớn, ko chứa nổi
    /// </summary>
    public class StockSymbolTradingHistoryController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockSymbolTradingHistoryController(MyDatabaseContext context)
        {
            _context = context;
        }


        public async Task<object> Index()
        {
            var allTradingHistories = await _context.StockSymbolTradingHistory
                .Select(h => new
                {
                    price = h.Price,
                    change = h.Change,
                    match_qtty = h.MatchQtty,
                    total_vol = h.TotalVol,
                    time = h.Date,
                    side = h.IsBuy ? "bu" : "sd",
                    code = h.StockSymbol
                })
                .OrderBy(s => s.time)
                .ThenBy(s => s.code)
                .ToListAsync();

            return allTradingHistories;
        }

        public async Task<bool> ConvertData()
        {
            return false;
            //Exp: we dont need this function anymore because the data is already pushed into database

            try
            {
                string path = @"C:\Users\Viet\Documents\GitHub\stock-app\Data\Json\Transaction\1-2-March\2022-03-02.json";
                var allSharePointsObjects = new VietStockSymbolTradingHistoryResponseModel();
                using (StreamReader r = new StreamReader(path))
                {
                    string json = await r.ReadToEndAsync();
                    allSharePointsObjects = JsonConvert.DeserializeObject<VietStockSymbolTradingHistoryResponseModel>(json);
                }

                var dataFromHost = new List<StockSymbolTradingHistory>();
                var result = new List<StockSymbolTradingHistory>();
                var numberOfT = allSharePointsObjects.data.Count();

                for (int i = 0; i < numberOfT; i++)
                {
                    var obj = allSharePointsObjects.data[i];
                    var history = new StockSymbolTradingHistory();

                    history.StockSymbol = obj.Code;
                    history.Date = obj.Date;
                    history.Price = obj.Price;
                    history.TotalVol = obj.total_vol;
                    history.MatchQtty = obj.match_qtty;
                    history.IsBuy = obj.IsBuy;
                    history.Change = obj.Change;

                    dataFromHost.Add(history);
                }

                if (dataFromHost.Any())
                {
                    var allSymbols = dataFromHost.Select(r => r.StockSymbol).Distinct().ToList();

                    //List<Task> TaskList = new List<Task>();
                    //foreach (var item in allSymbols)
                    //{
                    //    var LastTask = BuildTradingHistory(item, result, dataFromHost);
                    //    TaskList.Add(LastTask);
                    //}

                    //await Task.WhenAll(TaskList.ToArray());



                    Parallel.ForEach(allSymbols, symbol =>
                    {
                        var historiesInPeriodByStockCode = dataFromHost.Where(ss => ss.StockSymbol == symbol)
                            .OrderBy(s => s.Date)
                            .ToList();

                        if (historiesInPeriodByStockCode.Any())
                        {
                            var newList = historiesInPeriodByStockCode.GroupBy(h => h.Date).ToDictionary(h => h.Key, h => new StockSymbolTradingHistory
                            {
                                Date = h.Key,
                                Change = h.OrderByDescending(i => i.Change).First().Change,
                                IsBuy = h.First().IsBuy,
                                MatchQtty = h.Sum(i => i.MatchQtty),
                                Price = h.OrderByDescending(i => i.Price).First().Change,
                                StockSymbol = symbol,
                                TotalVol = h.OrderByDescending(i => i.TotalVol).First().Change
                            });

                            var newlst = newList.Select(nl => nl.Value).ToList();

                            foreach (var item in newlst)
                            {
                                newlst.TimDiemTangGiaDotBien(item);
                            }

                            lock (result)
                                result.AddRange(newlst);
                        }
                    });

                    var t1 = result.Select(r => r.StockSymbol).Distinct().Count();
                    var t2 = dataFromHost.Select(r => r.StockSymbol).Distinct().Count();

                    var verify = t1 == t2;

                    if (result.Any() && verify)
                    {
                        await _context.StockSymbolTradingHistory.AddRangeAsync(result);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
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
                //.Where(s => s._sc_ == "CEO" || s._sc_ == "VIC")
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            allSymbols = allSymbols.Where(s => !huyNiemYet.Contains(s._sc_)).ToList();

            var dataFromHost = new List<StockSymbolTradingHistory>();

            await GetV(dataFromHost, allSymbols);

            var result = new List<StockSymbolTradingHistory>();

            if (dataFromHost.Any())
            {
                Parallel.ForEach(allSymbols, symbol =>
                {
                    var historiesInPeriodByStockCode = dataFromHost.Where(ss => ss.StockSymbol == symbol._sc_)
                        .OrderBy(s => s.Date)
                        .ToList();

                    if (historiesInPeriodByStockCode.Any())
                    {
                        var newList = historiesInPeriodByStockCode.GroupBy(h => h.Date).ToDictionary(h => h.Key, h => new StockSymbolTradingHistory
                        {
                            Date = h.Key,
                            Change = h.OrderByDescending(i => i.Change).First().Change,
                            IsBuy = h.First().IsBuy,
                            MatchQtty = h.Sum(i => i.MatchQtty),
                            Price = h.OrderByDescending(i => i.Price).First().Change,
                            StockSymbol = symbol._sc_,
                            TotalVol = h.OrderByDescending(i => i.TotalVol).First().Change
                        });

                        var newlst = newList.Select(nl => nl.Value).ToList();

                        foreach (var item in newlst)
                        {
                            newlst.TimDiemTangGiaDotBien(item);
                        }

                        lock (result)
                            result.AddRange(newlst);
                    }
                });

                var t1 = result.Select(r => r.StockSymbol).Distinct().Count();
                var t2 = dataFromHost.Select(r => r.StockSymbol).Distinct().Count();

                var verify = t1 == t2;


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
                TaskList.Add(LastTask);
            }

            await Task.WhenAll(TaskList.ToArray());
        }

        /// <summary>
        /// Example: "https://api.vietstock.vn/ta/history?symbol=VIC&resolution=D&from=1609459200&to=1644796800";
        /// </summary>
        /// <params>{0}: symbol code</params>
        /// <params>{1}: resolution = D</params>
        /// <params>{2}: from: int from php code</params>
        /// <params>{3}: to: int from php code</params>
        public const string VietStock_GetTradingHistoryBySymbolCode = "https://api-finance-t19.24hmoney.vn/v1/web/stock/transaction-list-ssi?device_id=web&device_name=INVALID&device_model=Windows+10&network_carrier=INVALID&connection_type=INVALID&os=Chrome&os_version=98.0.4758.102&app_version=INVALID&access_token=INVALID&push_token=INVALID&locale=vi&symbol={0}&page=1&per_page=21600";


        private async Task BuildTradingHistory(string code, List<StockSymbolTradingHistory> dataFromHost, List<StockSymbolTradingHistory> result)
        {
            var historiesInPeriodByStockCode = dataFromHost.Where(ss => ss.StockSymbol == code)
                .OrderBy(s => s.Date)
                .ToList();

            var newList = historiesInPeriodByStockCode.GroupBy(h => h.Date).ToDictionary(h => h.Key, h => new StockSymbolTradingHistory
            {
                Date = h.Key,
                Change = h.OrderByDescending(i => i.Change).First().Change,
                IsBuy = h.First().IsBuy,
                MatchQtty = h.Sum(i => i.MatchQtty),
                Price = h.OrderByDescending(i => i.Price).First().Change,
                StockSymbol = code,
                TotalVol = h.OrderByDescending(i => i.TotalVol).First().Change
            });

            var newlst = newList.Select(nl => nl.Value).ToList();

            foreach (var item in newlst)
            {
                newlst.TimDiemTangGiaDotBien(item);
            }
            result.AddRange(newlst);

            //var t1 = result.Select(r => r.StockSymbol).Count();
            //var t2 = dataFromHost.Select(r => r.StockSymbol).Count();

            //var verify = t1 == t2;
        }


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

                result.Add(history);
            }
        }
    }
}
