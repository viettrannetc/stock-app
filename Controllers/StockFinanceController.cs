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
using DotNetCoreSqlDb.Models.Database.Finance;

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


        public async Task<bool> Pull(string code)
        {
            var restService = new RestServiceHelper();

            var symbols = await _context.StockSymbol.ToListAsync();
            List<StockSymbolFinanceHistory> result = new List<StockSymbolFinanceHistory>();

            foreach (var symbol in symbols)
            {
                await GetFinanceData(symbol, new RestServiceHelper(), result);
            }

            await _context.StockSymbolFinanceHistory.AddRangeAsync(result);
            await _context.SaveChangesAsync();

            return true;
        }

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


        public async Task<bool> KLGD()
        {
            var restService = new RestServiceHelper();

            var symbols = await _context.StockSymbol.ToListAsync();
            List<KLGDMuaBan> result = new List<KLGDMuaBan>();

            foreach (var symbol in symbols)
            {
                await GetFinanceData(symbol, new RestServiceHelper(), result);
            }

            await _context.KLGDMuaBan.AddRangeAsync(result);
            await _context.SaveChangesAsync();

            return true;
        }



        public const string KLGDMBFinance = "https://api-finance-t19.24hmoney.vn/v2/web/stock/transaction-detail-by-price?symbol={0}";

        private async Task GetFinanceData(StockSymbol item, RestServiceHelper restService, List<KLGDMuaBan> result)
        {
            var url = string.Format(KLGDMBFinance, item._sc_);
            var allSharePointsObject = await restService.Get<KLGDMuaBanModel>(url);

            if (allSharePointsObject == null || allSharePointsObject.status != "200") return;

            var obj = allSharePointsObject.data;
            var history = new KLGDMuaBan()
            {
                StockSymbol = item._sc_,
                Date = DateTime.UtcNow,
                TotalBuy = obj.total_buy,
                TotalVol = obj.total_vol,
                TotalSell = obj.total_sell,
                TotalUnknow = obj.total_unknow
            };

            result.Add(history);
        }
    }
}
