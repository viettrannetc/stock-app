using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreSqlDb.Common
{
    public class Service
    {
        public async Task GetV(List<History> result, List<StockSymbol> allSymbols, DateTime from, DateTime to, DateTime currentLatestDate, int count)
        {
            //try
            //{
            //    var restService = new RestServiceHelper();
            //    List<Task> TaskList = new List<Task>();
            //    foreach (var item in allSymbols)
            //    {
            //        var LastTask = GetStockDataByDay(item, restService, result, from, to);
            //        TaskList.Add(LastTask);
            //    }

            //    await Task.WhenAll(TaskList.ToArray());

            //    result = result.Where(r => r.Date >= currentLatestDate).ToList();   //TODO: consider whether it should be ">=" or ">"

            //    var updated = result.Select(r => r.StockSymbol).ToList();

            //    var notFetchedSymbols = allSymbols.Where(s => !updated.Contains(s._sc_)).ToList();

            //    if (notFetchedSymbols.Any() && count <= 3)
            //    {
            //        if (notFetchedSymbols.Count() == allSymbols.Count())
            //            count++;
            //        await GetV(result, notFetchedSymbols, from, to, currentLatestDate, count);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    result = new List<History>();
            //    await GetV(result, allSymbols, from, to, currentLatestDate, count);
            //}

            var restService = new RestServiceHelper();

            foreach (var item in allSymbols)
            {
                await GetStockDataByDay(item, restService, result, from, to);
            }

            result = result.Where(r => r.Date >= currentLatestDate).ToList();   //TODO: consider whether it should be ">=" or ">"

            var updated = result.Select(r => r.StockSymbol).ToList();

            var notFetchedSymbols = allSymbols.Where(s => !updated.Contains(s._sc_)).ToList();

            if (notFetchedSymbols.Any() && count <= 3)
            {
                if (notFetchedSymbols.Count() == allSymbols.Count())
                    count++;
                await GetV(result, notFetchedSymbols, from, to, currentLatestDate, count);
            }
        }

        public const string VietStock_GetDetailsBySymbolCode = "https://api.vietstock.vn/ta/history?symbol={0}&resolution={1}&from={2}&to={3}";

        private async Task GetStockDataByDay(StockSymbol item, RestServiceHelper restService, List<History> result, DateTime from, DateTime to)
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
            if (allSharePointsObjects == null) return;

            var numberOfT = allSharePointsObjects.t.Count();

            var histories = new List<History>();
            for (int i = 0; i < numberOfT; i++)
            {
                var history = new History();
                history.T = allSharePointsObjects.t[i];
                history.Date = allSharePointsObjects.t[i].PhpIntConvertToDateTime();
                history.O = allSharePointsObjects.o[i];
                history.C = allSharePointsObjects.c[i];
                history.H = allSharePointsObjects.h[i];
                history.L = allSharePointsObjects.l[i];
                history.V = allSharePointsObjects.v[i];
                history.StockSymbol = requestModel.code;

                histories.Add(history);
            }

            result.AddRange(histories);

            if (numberOfT >= 5000)
            {
                await GetStockDataByDay(item, restService, result, histories.OrderByDescending(h => h.Date).First().Date, to);
            }
        }


        public async Task<History> GetStockDataByDay(string stockCode, RestServiceHelper restService, DateTime missingDate)
        {
            var requestModel = new VietStockSymbolHistoryResquestModel();
            requestModel.code = stockCode;
            requestModel.from = missingDate;
            requestModel.to = missingDate;

            var url = string.Format(VietStock_GetDetailsBySymbolCode,
                            requestModel.code,
                            "D",
                            requestModel.from.ConvertToPhpInt(),
                            requestModel.to.ConvertToPhpInt()
                            );
            var allSharePointsObjects = await restService.Get<VietStockSymbolHistoryResponseModel>(url, true);
            if (allSharePointsObjects == null || !allSharePointsObjects.t.Any()) return null;

            var history = new History();
            history.T = allSharePointsObjects.t[0];
            history.Date = allSharePointsObjects.t[0].PhpIntConvertToDateTime();
            history.O = allSharePointsObjects.o[0];
            history.C = allSharePointsObjects.c[0];
            history.H = allSharePointsObjects.h[0];
            history.L = allSharePointsObjects.l[0];
            history.V = allSharePointsObjects.v[0];
            history.StockSymbol = requestModel.code;

            return history;
        }






        public async Task GetVHours(List<HistoryHour> result, List<StockSymbol> allSymbols, DateTime from, DateTime to, DateTime currentLatestDate, int count)
        {
            var restService = new RestServiceHelper();

            foreach (var item in allSymbols)
            {
                await GetStockDataByHour(item, restService, result, from, to);
            }

            result = result.Where(r => r.Date >= currentLatestDate).ToList();   //TODO: consider whether it should be ">=" or ">"

            var updated = result.Select(r => r.StockSymbol).ToList();

            var notFetchedSymbols = allSymbols.Where(s => !updated.Contains(s._sc_)).ToList();

            if (notFetchedSymbols.Any() && count <= 3)
            {
                if (notFetchedSymbols.Count() == allSymbols.Count())
                    count++;
                await GetVHours(result, notFetchedSymbols, from, to, currentLatestDate, count);
            }
        }

        private async Task GetStockDataByHour(StockSymbol item, RestServiceHelper restService, List<HistoryHour> result, DateTime from, DateTime to)
        {
            var requestModel = new VietStockSymbolHistoryResquestModel();
            requestModel.code = item._sc_;
            requestModel.from = from;
            requestModel.to = to;

            var url = string.Format(VietStock_GetDetailsBySymbolCode,
                            requestModel.code,
                            "60",
                            requestModel.from.ConvertToPhpInt(),
                            requestModel.to.ConvertToPhpInt()
                            );
            var allSharePointsObjects = await restService.Get<VietStockSymbolHistoryResponseModel>(url, true);
            if (allSharePointsObjects == null) return;

            var numberOfT = allSharePointsObjects.t.Count();

            var histories = new List<HistoryHour>();
            for (int i = 0; i < numberOfT; i++)
            {
                var history = new HistoryHour();
                history.T = allSharePointsObjects.t[i];
                history.Date = allSharePointsObjects.t[i].PhpIntConvertToDateTime();
                history.O = allSharePointsObjects.o[i];
                history.C = allSharePointsObjects.c[i];
                history.H = allSharePointsObjects.h[i];
                history.L = allSharePointsObjects.l[i];
                history.V = allSharePointsObjects.v[i];
                history.StockSymbol = requestModel.code;

                histories.Add(history);
            }
            histories = histories.OrderBy(h => h.Date).ToList();

            result.AddRange(histories);

            if (numberOfT > 0)
            {
                to = histories.OrderBy(h => h.Date).First().Date.AddDays(-1).WithoutHours();
                await GetStockDataByHour(item, restService, result, from, to);
            }
        }


        //public async Task GetChanges(List<History> result, List<StockSymbol> allSymbols, DateTime from, DateTime to, DateTime currentLatestDate, int count)
        //{
        //    var restService = new RestServiceHelper();

        //    foreach (var item in allSymbols)
        //    {
        //        await GetStockDataByDay(item, restService, result, from, to);
        //    }

        //    result = result.Where(r => r.Date >= currentLatestDate).ToList();   //TODO: consider whether it should be ">=" or ">"

        //    var updated = result.Select(r => r.StockSymbol).ToList();

        //    var notFetchedSymbols = allSymbols.Where(s => !updated.Contains(s._sc_)).ToList();

        //    if (notFetchedSymbols.Any() && count <= 3)
        //    {
        //        if (notFetchedSymbols.Count() == allSymbols.Count())
        //            count++;
        //        await GetChanges(result, notFetchedSymbols, from, to, currentLatestDate, count);
        //    }
        //}
    }
}
