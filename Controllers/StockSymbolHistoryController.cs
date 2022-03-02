using System;
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
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            allSymbols = allSymbols.Where(s => !huyNiemYet.Contains(s._sc_)).ToList();

            var result = new List<StockSymbolHistory>();
            var currentLatestDate = _context.StockSymbolHistory.Where(c => c.StockSymbol == "A32").OrderByDescending(r => r.Date).First().Date;
            var from = currentLatestDate;
            var to = DateTime.Now.WithoutHours().AddDays(1);

            await GetV(result, allSymbols, from, to , currentLatestDate);
            
            result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.StockSymbolHistory.AddRangeAsync(result);
                await _context.SaveChangesAsync();
            }

            return "true";
        }

        public async Task GetV(List<StockSymbolHistory> result, List<StockSymbol> allSymbols, DateTime from, DateTime to, DateTime currentLatestDate)
        {
            var restService = new RestServiceHelper();
            List<Task> TaskList = new List<Task>();
            foreach (var item in allSymbols)
            {
                var LastTask = GetStockDataByDay(item, restService, result, from, to);
                //LastTask.Start();
                TaskList.Add(LastTask);
            }

            await Task.WhenAll(TaskList.ToArray());

            result = result.Where(r => r.Date > currentLatestDate).ToList();

            var updated = result.Select(r => r.StockSymbol).ToList();

            var notIn = allSymbols.Where(s => !updated.Contains(s._sc_)).ToList();

            if (notIn.Any())
                await GetV(result, notIn, from, to, currentLatestDate);
        }

        private async Task GetStockDataByDay(StockSymbol item, RestServiceHelper restService, List<StockSymbolHistory> result, DateTime from, DateTime to)
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
