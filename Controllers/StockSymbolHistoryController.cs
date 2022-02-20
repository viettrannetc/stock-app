﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;
using DotNetCoreSqlDb.Models.Business;

namespace DotNetCoreSqlDb.Controllers
{
    public class StockSymbolHistoryController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockSymbolHistoryController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: StockSymbolHistory
        public async Task<List<StockSymbolHistory>> Index()
        {
            return await _context.StockSymbolHistory.ToListAsync();
        }

        // GET: https://localhost:44359/StockSymbolHistory/Details?code=A32
        [HttpGet]
        public async Task<List<StockSymbolHistory>> Details(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var result = await _context.StockSymbolHistory.Where(m => m.StockSymbol == code).ToListAsync();
            if (result == null)
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Example: "https://api.vietstock.vn/ta/history?symbol=VIC&resolution=D&from=1609459200&to=1644796800";
        /// </summary>
        /// <params>{0}: symbol code</params>
        /// <params>{1}: resolution = D</params>
        /// <params>{2}: from: int from php code</params>
        /// <params>{3}: to: int from php code</params>
        public const string VietStock_GetDetailsBySymbolCode = "https://api.vietstock.vn/ta/history?symbol={0}&resolution={1}&from={2}&to={3}";


        // GET: StockSymbolHistory/Create
        // Form Data:
        //      code: A32
        //      from: 01-01-1970  (12 AM as default)
        //      to: 15-02-2022    (12 AM as default)
        [HttpPost]
        public async Task<string> Create()
        {
            var restService = new RestServiceHelper();

            var allSymbols = await _context.StockSymbol
                .Where(ss => ss._sc_ != "TCI")
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            var result = new List<StockSymbolHistory>();
            var currentLatestDate = _context.StockSymbolHistory.Where(c => c.StockSymbol == "A32").OrderByDescending(r => r.Date).First().Date;
            var from = currentLatestDate.AddDays(1);
            var to = from == DateTime.Now.WithoutHours()
                    ? from.AddDays(1)
                    : DateTime.Now.WithoutHours();

            await Parallel.ForEachAsync(allSymbols, async (item, cancellationToken) =>
            {
                var requestModel = new VietStockSymbolHistoryResquestModel();
                requestModel.code = item._sc_;
                requestModel.from = from;
                requestModel.to = to;

                var url = string.Format(VietStock_GetDetailsBySymbolCode,
                                requestModel.code,
                                "D",
                                requestModel.from.ConvertToPhpInt(),
                                requestModel.to.ConvertToPhpInt()
                                );
                var allSharePointsObjects = await restService.Get<VietStockSymbolHistoryResponseModel>(url, true);

                var numberOfT = allSharePointsObjects.t.Count();
                var numberOfO = allSharePointsObjects.t.Count();
                var numberOfC = allSharePointsObjects.t.Count();
                var numberOfH = allSharePointsObjects.t.Count();
                var numberOfL = allSharePointsObjects.t.Count();
                var numberOfV = allSharePointsObjects.t.Count();

                for (int i = 0; i < numberOfT; i++)
                {
                    var history = new StockSymbolHistory();
                    history.T = allSharePointsObjects.t[i];
                    history.Date = allSharePointsObjects.t[i].PhpIntConvertToDateTime();
                    history.O = allSharePointsObjects.o[i];
                    history.C = allSharePointsObjects.c[i];
                    history.H = allSharePointsObjects.h[i];
                    history.L = allSharePointsObjects.l[i];
                    history.V = allSharePointsObjects.v[i];
                    history.StockSymbol = requestModel.code;

                    //var isExisting = _context.StockSymbolHistory.Any(e => e.StockSymbol == history.StockSymbol && e.T == history.T);
                    //if (!isExisting)
                    result.Add(history);
                }
            });

            //foreach (var item in allSymbols)
            //{
            //    try
            //    {
            //        var requestModel = new VietStockSymbolHistoryResquestModel();
            //        requestModel.code = item._sc_;
            //        requestModel.from = new DateTime(2022, 2, 16);
            //        requestModel.to = new DateTime(2022, 2, 19);

            //        var url = string.Format(VietStock_GetDetailsBySymbolCode,
            //                        requestModel.code,
            //                        "D",
            //                        requestModel.from.ConvertToPhpInt(),
            //                        requestModel.to.ConvertToPhpInt()
            //                        );
            //        var allSharePointsObjects = await restService.Get<VietStockSymbolHistoryResponseModel>(url, true);

            //        var numberOfT = allSharePointsObjects.t.Count();
            //        var numberOfO = allSharePointsObjects.t.Count();
            //        var numberOfC = allSharePointsObjects.t.Count();
            //        var numberOfH = allSharePointsObjects.t.Count();
            //        var numberOfL = allSharePointsObjects.t.Count();
            //        var numberOfV = allSharePointsObjects.t.Count();

            //        if (numberOfT != numberOfO || numberOfT != numberOfC || numberOfT != numberOfH || numberOfT != numberOfL || numberOfT != numberOfV)
            //        {
            //            return "false";
            //        }

            //        for (int i = 0; i < numberOfT; i++)
            //        {
            //            var history = new StockSymbolHistory();
            //            history.T = allSharePointsObjects.t[i];
            //            history.Date = allSharePointsObjects.t[i].PhpIntConvertToDateTime();
            //            history.O = allSharePointsObjects.o[i];
            //            history.C = allSharePointsObjects.c[i];
            //            history.H = allSharePointsObjects.h[i];
            //            history.L = allSharePointsObjects.l[i];
            //            history.V = allSharePointsObjects.v[i];
            //            history.StockSymbol = requestModel.code;

            //            //var isExisting = _context.StockSymbolHistory.Any(e => e.StockSymbol == history.StockSymbol && e.T == history.T);
            //            //if (!isExisting)
            //            result.Add(history);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        return item._sc_;
            //    }
            //}

            if (result.Any())
            {
                List<StockSymbolHistory> newH = new List<StockSymbolHistory>();
                result.ForEach(history =>
                {
                    var newObject = history.Copy();
                    //var newObject = history.DeepClone();
                    if (newH.Any(h => h.StockSymbol == newObject.StockSymbol
                    && h.Date == newObject.Date))
                    { }
                    else
                        newH.Add(newObject);
                });

                var t1 = newH.GroupBy(g => g.StockSymbol).ToDictionary(x => x.Key, g => g.ToList());
                var t2 = t1.Where(t => t.Value.Count() < 3).ToList();

                foreach (var item in t2)
                {
                    //var tt1 = result.Where(r => r.StockSymbol == item.Key).ToList();
                    await GetVet(newH, item.Key, from, to);
                }

                var t3 = t1.Where(t => t.Value.Count() == 3).ToList();
                var t4 = t1.Where(t => t.Value.Count() > 3).ToList();

                newH = newH.OrderBy(h => h.Date).ToList();

                await _context.StockSymbolHistory.AddRangeAsync(newH);
                await _context.SaveChangesAsync();
            }

            return "true";
        }

        public async Task GetVet(List<StockSymbolHistory> currentResults, string code, DateTime from, DateTime to)
        {
            var requestModel = new VietStockSymbolHistoryResquestModel();
            requestModel.code = code;
            requestModel.from = from;// currentLatestDate.AddDays(1);
            requestModel.to = requestModel.from == DateTime.Now.WithoutHours()
                ? requestModel.from.AddDays(1)
                : DateTime.Now.WithoutHours();

            var url = string.Format(VietStock_GetDetailsBySymbolCode,
                            requestModel.code,
                            "D",
                            requestModel.from.ConvertToPhpInt(),
                            requestModel.to.ConvertToPhpInt()
                            );

            var restService = new RestServiceHelper();
            var allSharePointsObjects = await restService.Get<VietStockSymbolHistoryResponseModel>(url, true);

            var numberOfT = allSharePointsObjects.t.Count();
            var numberOfO = allSharePointsObjects.t.Count();
            var numberOfC = allSharePointsObjects.t.Count();
            var numberOfH = allSharePointsObjects.t.Count();
            var numberOfL = allSharePointsObjects.t.Count();
            var numberOfV = allSharePointsObjects.t.Count();

            for (int i = 0; i < numberOfT; i++)
            {
                var history = new StockSymbolHistory();
                history.T = allSharePointsObjects.t[i];
                history.Date = allSharePointsObjects.t[i].PhpIntConvertToDateTime();
                history.O = allSharePointsObjects.o[i];
                history.C = allSharePointsObjects.c[i];
                history.H = allSharePointsObjects.h[i];
                history.L = allSharePointsObjects.l[i];
                history.V = allSharePointsObjects.v[i];
                history.StockSymbol = requestModel.code;

                var isExisting = currentResults.Any(r => r.StockSymbol == history.StockSymbol && r.Date == history.Date);
                if (!isExisting)
                    currentResults.Add(history);
            }
        }
    }
}