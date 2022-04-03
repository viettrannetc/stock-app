using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;
using Flurl.Http;

namespace DotNetCoreSqlDb.Controllers
{
    public class StockFinanceController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockFinanceController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: Finance
        public async Task<List<string>> Index()
        {
            return await _context.StockSymbol.Select(s => s._sc_).ToListAsync();
        }


        public async Task<bool> Get(string code)
        {
            //CDKT
            //CSTC
            //CTKH
            //KQKD
            //LCTT

            return false;
        }


        public async Task<dynamic> Pull(string code)
        {
            var restService = new RestServiceHelper();

            //return await restService.HexecuteVietStockPostman();

            //return await restService.HexecuteVietStockPostman("DGW", FinanceType.KQKD, "9");


            var symbols = await _context.StockSymbol
                .Where(c => c._sc_ == "DGW")
                .ToListAsync();
            List<StockSymbolFinanceHistory> result = new List<StockSymbolFinanceHistory>();

            await GetV(result, symbols, 3);

            return result;
        }

        public async Task GetV(List<StockSymbolFinanceHistory> result, List<StockSymbol> allSymbols, int count)
        {
            var restService = new RestServiceHelper();
            List<Task> TaskList = new List<Task>();
            foreach (var item in allSymbols)
            {
                var LastTask = GetFinanceData(item, restService, result);
                TaskList.Add(LastTask);
            }

            await Task.WhenAll(TaskList.ToArray());

            var updated = result.Select(r => r.StockSymbol).ToList();

            var notFetchedSymbols = allSymbols.Where(s => !updated.Contains(s._sc_)).ToList();

            if (notFetchedSymbols.Any() && count <= 3)
            {
                if (notFetchedSymbols.Count() == allSymbols.Count())
                    count++;
                await GetV(result, notFetchedSymbols, count);
            }
        }

        public const string VietStock_GetDetailsBySymbolCode = "https://api.vietstock.vn/ta/history?symbol={0}&resolution={1}&from={2}&to={3}";

        private async Task GetFinanceData(StockSymbol item, RestServiceHelper restService, List<StockSymbolFinanceHistory> result)
        {
            for (int i = 1; i <= 20; i++)
            {
                var data1 = await restService.HexecuteVietStockPostman(item._sc_, FinanceType.KQKD, i.ToString());
                var data2 = await restService.HexecuteVietStockPostman(item._sc_, FinanceType.CSTC, i.ToString());
                var data3 = await restService.HexecuteVietStockPostman(item._sc_, FinanceType.CDKT, i.ToString());
                var data4 = await restService.HexecuteVietStockPostman(item._sc_, FinanceType.LCTT, i.ToString());
                var data5 = await restService.HexecuteVietStockPostman(item._sc_, FinanceType.CTKH, i.ToString());
                result.AddRange(data1);
                result.AddRange(data2);
                result.AddRange(data3);
                result.AddRange(data4);
                result.AddRange(data5);
            }
        }

    }
}
