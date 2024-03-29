﻿using DotNetCoreSqlDb.Models;
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
            var restService = new RestServiceHelper();

            foreach (var item in allSymbols)
            {
                await GetStockDataByDay(item, restService, result, from, to);
            }

            //result = result.Where(r => r.Date >= currentLatestDate).ToList();   //TODO: consider whether it should be ">=" or ">"

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
        //public const string FireAnt_GetDetailsBySymbolCode = "https://svr6.fireant.vn/api/Data/Markets/Bars?symbol=SBT&resolution=60&startDate=2010-1-20%2023:26:23&endDate=2030-1-20%2023:26:23";
        public const string FireAnt_GetDetailsBySymbolCode = "https://svr6.fireant.vn/api/Data/Markets/Bars?symbol={0}&resolution={1}&startDate={2}%20{3}&endDate=2030-8-24%2012:11:50";


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

            result = result
                .OrderBy(h => h.StockSymbol)
                .ThenBy(h => h.Date)
                .ToList();
        }


        public async Task GetVHoursFireAnt(List<HistoryHour> result, List<string> allSymbols, DateTime from, string token)
        {
            var restService = new RestServiceHelper();
            from = new DateTime(2022, 3, 20);
            var t = 3;

            foreach (var item in allSymbols)
            {
                var i = 1;
                var counted = new List<int>();
                var expectedAdded = new Dictionary<int, List<HistoryHour>>();
                while (i <= t && counted.Count() < 2 || counted.Last() != counted[counted.Count - 1])
                {
                    var addedData = await GetStockDataByHourFireant(item, restService, from, token);
                    counted.Add(addedData.Count());
                    expectedAdded.Add(i, addedData);
                    i++;
                }

                result.AddRange(expectedAdded.Last().Value);
            }
        }

        public async Task<List<string>> GetDataMinutesFireAnt(List<string> allSymbols, DateTime from, string token)
        {
            var restService = new RestServiceHelper();
            var result = new List<string>();
            foreach (var item in allSymbols)
            {
                var code = await GetStockDataByMinutesFireant(item, restService, from, token);
                if (!string.IsNullOrEmpty(code))
                    result.Add(code);
            }


            var vnindex = await GetStockDataByMinutesFireant("HOSTC", restService, from, token);
            if (!string.IsNullOrEmpty(vnindex))
                result.Add(vnindex);

            return result;
        }

        private async Task<string> GetStockDataByMinutesFireant(string code, RestServiceHelper restService, DateTime from, string token)
        {
            var requestModel = new VietStockSymbolHistoryResquestModel();
            requestModel.code = code;
            var fr = from.ToString("yyyy-M-dd");
            var hour = DateTime.Now.ToString("HH:mm:ss");
            //var hour = "13:10:05";// DateTime.Now.ToString("HH:mm:ss");
            var url = string.Format(FireAnt_GetDetailsBySymbolCode,
                            requestModel.code, "1", fr, hour);

            var allSharePointsObjects = await restService.Get<FireAntSymbolHistoryResponseModel>(url, true, token);
            if (allSharePointsObjects == null || !allSharePointsObjects.Bars.Any()) return string.Empty;

            var last30sessions = allSharePointsObjects.Bars.OrderByDescending(b => b.Date).Take(60).ToList();

            var histories = new List<HistoryHour>();
            for (int i = 0; i < last30sessions.Count(); i++)
            {
                var history = new HistoryHour();
                history.Date = last30sessions[i].Date;
                history.O = last30sessions[i].Open;
                history.C = last30sessions[i].Close;
                history.H = last30sessions[i].High;
                history.L = last30sessions[i].Low;
                history.V = last30sessions[i].Volume;
                history.StockSymbol = requestModel.code;

                histories.Add(history);
            }

            var session0V = histories[0].VOL(histories, -20);
            var session0C = histories[0].C;
            var session1V = histories[1].VOL(histories, -20);
            var session1C = histories[1].C;

            var giaTang = histories[0].C > histories[1].C && histories[1].C > histories[2].C;
            var volTang = session0V > session1V;
            var volTo = histories[0].V >= session0V || histories[1].V >= session1V;

            //if (giaTang && (volTang || volTo))
            if (giaTang)
                return $"{code} - Tang GIA va VOL liên tục: {histories[2].C.ToString("N2")}/{histories[1].C.ToString("N2")}/{histories[0].C.ToString("N2")} ----- {histories[2].V}/{histories[1].V}/{histories[0].V} ----- {histories[0].Date.ToString("HH:mm")}";

            return string.Empty;
        }

        //private async Task GetStockDataByHourFireant(string code, RestServiceHelper restService, List<HistoryHour> result, DateTime from, string token)
        private async Task<List<HistoryHour>> GetStockDataByHourFireant(string code, RestServiceHelper restService, DateTime from, string token)
        {
            List<HistoryHour> result = new List<HistoryHour>();
            var requestModel = new VietStockSymbolHistoryResquestModel();
            requestModel.code = code;
            var fr = from.ToString("yyyy-M-dd");
            var hour = DateTime.Now.ToString("HH:mm:ss");
            var url = string.Format(FireAnt_GetDetailsBySymbolCode,
                            requestModel.code,
                            "60",
                            fr,
                            hour
                            );

            var allSharePointsObjects = await restService.Get<FireAntSymbolHistoryResponseModel>(url, true, token);
            if (allSharePointsObjects == null || !allSharePointsObjects.Bars.Any())
                return null;

            var lastData = allSharePointsObjects.Bars.OrderByDescending(b => b.Date).First();

            var historyData = allSharePointsObjects.Bars.Where(b => b.Date < lastData.Date.Date).ToList();

            var histories = new List<HistoryHour>();
            for (int i = 0; i < historyData.Count(); i++)
            {
                if (allSharePointsObjects.Bars[i].Date.Hour >= 8) continue; //Có 1 số mã CP sẽ có giao dịch phát sinh tại 8, nhưng thường là SL CP rất ít, đã kiểm tra với Fireant và cũng ko thấy họ lưu trong chart H1, nên mình cũng ko lưu

                var history = new HistoryHour();
                history.Date = allSharePointsObjects.Bars[i].Date;
                history.O = allSharePointsObjects.Bars[i].Open;
                history.C = allSharePointsObjects.Bars[i].Close;
                history.H = allSharePointsObjects.Bars[i].High;
                history.L = allSharePointsObjects.Bars[i].Low;
                history.V = allSharePointsObjects.Bars[i].Volume;
                history.StockSymbol = requestModel.code;

                histories.Add(history);//trong 1 ds dữ liệu, dữ liệu quá khứ sẽ được lưu sẵn theo dạng giờ, nhưng dữ liệu real time trong ngày cuối cùng sẽ được lưu dạng phút (đang trong phiên), nên phải cộng phút như bên dưới để ra dữ liệu
            }

            var todayDataInMinutes = allSharePointsObjects.Bars.Except(historyData).ToList();
            var todayDataInMinuteAt1stHour = todayDataInMinutes.Where(b => b.Date.Hour >= 2 && b.Date.Hour < 3).ToList();
            var todayDataInMinuteAt2ndHour = todayDataInMinutes.Where(b => b.Date.Hour >= 3 && b.Date.Hour < 4).ToList();
            var todayDataInMinuteAt3rdHour = todayDataInMinutes.Where(b => b.Date.Hour >= 4 && b.Date.Hour < 6).ToList();
            var todayDataInMinuteAt4thHour = todayDataInMinutes.Where(b => b.Date.Hour >= 6 && b.Date.Hour < 7).ToList();
            var todayDataInMinuteAt5thHour = todayDataInMinutes.Where(b => b.Date.Hour >= 7 && b.Date.Hour < 8).ToList(); //Có 1 số mã CP sẽ có giao dịch phát sinh tại 8, nhưng thường là SL CP rất ít, đã kiểm tra với Fireant và cũng ko thấy họ lưu trong chart H1, nên mình cũng ko lưu
            var data1 = CombineMinutesToHour(todayDataInMinuteAt1stHour, requestModel.code);
            var data2 = CombineMinutesToHour(todayDataInMinuteAt2ndHour, requestModel.code);
            var data3 = CombineMinutesToHour(todayDataInMinuteAt3rdHour, requestModel.code);
            var data4 = CombineMinutesToHour(todayDataInMinuteAt4thHour, requestModel.code);
            var data5 = CombineMinutesToHour(todayDataInMinuteAt5thHour, requestModel.code);

            if (data1 != null) histories.Add(data1);
            if (data2 != null) histories.Add(data2);
            if (data3 != null) histories.Add(data3);
            if (data4 != null) histories.Add(data4);
            if (data5 != null) histories.Add(data5);

            histories = histories.OrderBy(h => h.Date).ToList();

            result.AddRange(histories);
            return result;
        }

        private HistoryHour CombineMinutesToHour(List<FireAntBarSymbolHistoryResponseModel> dataInMinutes, string symbol)
        {
            if (dataInMinutes.Any())
            {
                var history = new HistoryHour();
                history.Date = dataInMinutes[0].Date;
                history.O = dataInMinutes.OrderBy(h => h.Date).First().Open;
                history.C = dataInMinutes.OrderBy(h => h.Date).Last().Close;
                history.H = dataInMinutes.OrderBy(h => h.High).Last().High;
                history.L = dataInMinutes.OrderBy(h => h.Low).First().Low;
                history.V = dataInMinutes.Sum(d => d.Volume);
                history.StockSymbol = symbol;

                return history;
            }

            return null;
        }
    }
}
