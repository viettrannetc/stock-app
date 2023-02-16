using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Skender.Stock.Indicators;
using System.Text;

namespace DotNetCoreSqlDb.Controllers
{
    public partial class StockSymbolHistoryController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockSymbolHistoryController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: History
        public async Task<List<History>> Index()
        {
            return await _context.History.ToListAsync();
        }

        // GET: https://localhost:44359/History/Details?code=A32
        [HttpGet]
        public async Task<List<History>> Details(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var result = await _context.History.Where(m => m.StockSymbol == code).ToListAsync();
            if (result == null)
            {
                return null;
            }

            return result;
        }

        // GET: History/Create
        // Form Data:
        //      code: A32
        //      from: 01-01-1970  (12 AM as default)
        //      to: 15-02-2022    (12 AM as default)
        [HttpPost]
        public async Task<string> Create(string code, DateTime tuNgay)
        {
            //await UpdateIndicators();
            //return String.Empty;

            var restService = new RestServiceHelper();
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_ == "VNINDEX" || (s._sc_.Length == 3 && s.BiChanGiaoDich == false)).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_ == "VNINDEX" || (s._sc_.Length == 3 && s.BiChanGiaoDich == false && splitStringCode.Contains(s._sc_))).OrderByDescending(s => s._sc_).ToListAsync();

            var result = new List<History>();
            var latestHistory = _context.History.OrderByDescending(r => r.Date).FirstOrDefault();
            var currentLatestDate = latestHistory == null ? new DateTime(2000, 1, 1) : latestHistory.Date;
            var from = tuNgay < new DateTime(2000, 1, 1) ? currentLatestDate.AddDays(1) : tuNgay;
            var to = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 0, 0);

            var service = new Service();
            await service.GetV(result, symbols, from, to, from, 0);

            result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.History.AddRangeAsync(result);
                await _context.SaveChangesAsync();
                //await UpdateIndicators();
            }

            return string.Empty;
        }

        /// <summary>
        /// Update data symbols for trash codes
        /// </summary>
        /// <param name="code"></param>
        /// <param name="tuNgay"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> CreateTrashOnes(string code, DateTime tuNgay)
        {
            var restService = new RestServiceHelper();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol <= ConstantData.minMA20VolDaily).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol <= ConstantData.minMA20VolDaily && splitStringCode.Contains(s._sc_)).OrderByDescending(s => s._sc_).ToListAsync();

            var result = new List<History>();
            var latestHistory = _context.History.OrderByDescending(r => r.Date).FirstOrDefault();
            var currentLatestDate = new DateTime(2000, 1, 1);
            var from = new DateTime(2000, 1, 1);
            //var to = DateTime.Now.WithoutHours();
            var to = new DateTime(2022, 8, 1, 23, 59, 59);

            var service = new Service();
            await service.GetV(result, symbols, from, to, from, 0);

            result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.History.AddRangeAsync(result);
                await _context.SaveChangesAsync();
                await UpdateIndicators();
            }

            return string.Empty;
        }

        [HttpPost]
        public async Task<string> CreateWrongOnes(string code, DateTime tuNgay)
        {
            var restService = new RestServiceHelper();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily && splitStringCode.Contains(s._sc_)).OrderByDescending(s => s._sc_).ToListAsync();
            //var stockCodes = symbols.Select(s => s._sc_).ToList();

            //var allSymbols = await _context.StockSymbol
            //    .Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000)
            //    .OrderByDescending(s => s._sc_)
            //    .ToListAsync();

            var result = new List<History>();
            var latestHistory = _context.History.OrderByDescending(r => r.Date).FirstOrDefault();
            var currentLatestDate = latestHistory == null ? new DateTime(2000, 1, 1) : latestHistory.Date;
            var from = new DateTime(2000, 1, 1);
            var to = DateTime.Now.WithoutHours();

            var service = new Service();
            await service.GetV(result, symbols, from, to, from, 0);

            //result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.History.AddRangeAsync(result);
                await _context.SaveChangesAsync();
                await UpdateIndicators();
            }

            return string.Empty;
        }


        // GET: https://localhost:44359/History/Details?code=A32
        [HttpGet]
        public async Task<List<string>> DetectedMissing(DateTime from)
        {
            var restService = new RestServiceHelper();
            var result = new List<string>();
            var symbols = await _context.StockSymbol
                .Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily)
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            var codes = symbols.Select(h => h._sc_).ToList();
            var histories = await _context.History.Where(m => codes.Contains(m.StockSymbol)).OrderBy(h => h.Date).ToListAsync();

            var service = new Service();

            var checkingDate = from;
            var resultMissingData = new List<History>();

            var minDates = histories.GroupBy(h => h.StockSymbol).ToDictionary(g => g.Key, g => g.OrderBy(h => h.Date).First());

            while (checkingDate < DateTime.Today)
            {
                var hasData = histories.Any(h => h.Date == checkingDate);
                if (hasData)
                {
                    var missingData = codes.Except(histories.Where(h => h.Date == checkingDate).Select(h => h.StockSymbol).Distinct().ToList());
                    if (missingData.Any())
                    {
                        foreach (var missingCode in missingData)
                        {
                            if (minDates.ContainsKey(missingCode) && checkingDate < minDates[missingCode].Date) continue;
                            var missingHistory = await service.GetStockDataByDay(missingCode, restService, checkingDate);
                            if (missingHistory != null)
                                resultMissingData.Add(missingHistory);
                        }
                    }
                }

                checkingDate = checkingDate.AddDays(1);
            }

            if (resultMissingData.Any())
            {
                await _context.History.AddRangeAsync(resultMissingData);
                await _context.SaveChangesAsync();
            }


            return result;
        }


        public async Task<List<string>> DetectedChanges()
        {
            //await UpdateIndicators();
            //return null;

            var restService = new RestServiceHelper();
            var resultText = new List<string>();
            var symbols = await _context.StockSymbol
                .Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily)
                //.Where(s => s._sc_ == "HCM")
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            var codes = symbols.Select(h => h._sc_).ToList();
            var histories = await _context.History.Where(m => codes.Contains(m.StockSymbol)).OrderBy(h => h.Date).ToListAsync();

            var result = new List<History>();
            var latestHistory = _context.History.OrderByDescending(r => r.Date).FirstOrDefault();
            var from = DateTime.Now.AddDays(-50000).WithoutHours();
            var to = DateTime.Now.WithoutHours();

            var service = new Service();
            await service.GetV(result, symbols, from, to, from, 0);

            //result = result.Where(r => r.Date > currentLatestDate).ToList();
            foreach (var code in symbols)
            {
                var databaseHistories = histories.Where(h => h.StockSymbol == code._sc_ && h.Date >= from).ToList();
                var websiteHistories = result.Where(h => h.StockSymbol == code._sc_ && h.Date >= from).ToList();

                foreach (var dbHistory in databaseHistories)
                {
                    var compareData = websiteHistories.FirstOrDefault(w => w.Date == dbHistory.Date);
                    if (compareData != null
                        && ((Math.Abs(compareData.C - dbHistory.C) >= 0.01M)
                            || Math.Abs(compareData.V - dbHistory.V) >= 0.01M
                            || Math.Abs(compareData.O - dbHistory.O) >= 0.01M
                            || Math.Abs(compareData.H - dbHistory.H) >= 0.01M
                            || Math.Abs(compareData.L - dbHistory.L) >= 0.01M
                            ))
                    {
                        dbHistory.C = compareData.C;
                        dbHistory.V = compareData.V;
                        dbHistory.O = compareData.O;
                        dbHistory.H = compareData.H;
                        dbHistory.L = compareData.L;
                        resultText.Add($"{code._sc_} - {dbHistory.Date}");
                        //break;
                    }
                }
            }

            if (resultText.Any())
            {
                await _context.SaveChangesAsync();
                await UpdateIndicators();
            }

            return resultText;
        }

        public async Task<string> UpdateIndicators(bool? forceUpdate = false)
        {
            var symbols = await _context.StockSymbol
                .Where(s => s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily)
                .OrderByDescending(s => s._sc_)
                .ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            foreach (var symbol in symbols)
            {
                //Update stock
                var historyOfStockCode = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderByDescending(h => h.Date)
                    .ToList();

                var latestDate = historyOfStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(historyOfStockCode);
                symbol.BiChanGiaoDich = biCanhCao;

                var avarageOfLastXXPhien = historyOfStockCode.Take(20).Sum(h => h.V) / 20;
                symbol.MA20Vol = avarageOfLastXXPhien;
                _context.StockSymbol.Update(symbol);

                //Only Update indicators if the code isn't "BiCanhCao"
                if (biCanhCao || avarageOfLastXXPhien <= ConstantData.minMA20VolDaily) continue;

                var qoutes = MACDConvert(historyOfStockCode);

                var ichimoku = qoutes.GetIchimoku();
                var macd = qoutes.GetMacd();
                var rsis = qoutes.GetRsi();
                var bands = qoutes.GetBollingerBands();
                var ma5 = qoutes.GetSma(5);

                var historiesWithNoIndicate = forceUpdate == false
                    ? historyOfStockCode.Where(h => !h.HadAllIndicators()).ToList()
                    : historyOfStockCode.ToList();

                for (int i = 0; i < historiesWithNoIndicate.Count(); i++)
                {
                    var updatedItem = historiesWithNoIndicate[i];
                    try
                    {
                        //special
                        updatedItem.RSIPhanKi = historyOfStockCode.XacDinhPhanKi(updatedItem);
                        updatedItem.MACDPhanKi = historyOfStockCode.XacDinhPhanKi(updatedItem, "MACD");

                        var sameDateMA5 = ma5.Where(r => r.Date == updatedItem.Date).FirstOrDefault();
                        if (sameDateMA5 != null)
                        {
                            updatedItem.GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
                        }

                        var sameDateIchi = ichimoku.Where(r => r.Date == updatedItem.Date).FirstOrDefault();
                        if (sameDateIchi != null)
                        {
                            updatedItem.IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
                            updatedItem.IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
                            updatedItem.IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
                            updatedItem.IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
                        }

                        updatedItem.NenBot = Math.Min(updatedItem.O, updatedItem.C);
                        updatedItem.NenTop = Math.Max(updatedItem.O, updatedItem.C);

                        var sameDateRSI = rsis.Where(r => r.Date == updatedItem.Date).FirstOrDefault();
                        if (sameDateRSI != null)
                        {
                            updatedItem.RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
                        }

                        var sameDateBands = bands.Where(r => r.Date == updatedItem.Date).FirstOrDefault();
                        if (sameDateBands != null)
                        {
                            updatedItem.BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
                            updatedItem.BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
                            updatedItem.BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
                        }

                        var sameDateMacd = macd.Where(r => r.Date == updatedItem.Date).FirstOrDefault();
                        if (sameDateMacd != null)
                        {
                            updatedItem.MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
                            updatedItem.MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
                            updatedItem.MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    _context.History.Update(updatedItem);
                }

                //for (int i = 0; i < historyOfStockCode.Count(); i++)
                //{
                //    try
                //    {
                //        //special
                //        historyOfStockCode[i].RSIPhanKi = historyOfStockCode.XacDinhPhanKi(historyOfStockCode[i]);
                //        historyOfStockCode[i].MACDPhanKi = historyOfStockCode.XacDinhPhanKi(historyOfStockCode[i], "MACD");

                //        var sameDateMA5 = ma5.Where(r => r.Date == historyOfStockCode[i].Date).FirstOrDefault();
                //        if (sameDateMA5 != null)
                //        {
                //            historyOfStockCode[i].GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
                //        }

                //        var sameDateIchi = ichimoku.Where(r => r.Date == historyOfStockCode[i].Date).FirstOrDefault();
                //        if (sameDateIchi != null)
                //        {
                //            historyOfStockCode[i].IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
                //            historyOfStockCode[i].IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
                //            historyOfStockCode[i].IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
                //            historyOfStockCode[i].IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
                //        }

                //        historyOfStockCode[i].NenBot = Math.Min(historyOfStockCode[i].O, historyOfStockCode[i].C);
                //        historyOfStockCode[i].NenTop = Math.Max(historyOfStockCode[i].O, historyOfStockCode[i].C);

                //        var sameDateRSI = rsis.Where(r => r.Date == historyOfStockCode[i].Date).FirstOrDefault();
                //        if (sameDateRSI != null)
                //        {
                //            historyOfStockCode[i].RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
                //        }

                //        var sameDateBands = bands.Where(r => r.Date == historyOfStockCode[i].Date).FirstOrDefault();
                //        if (sameDateBands != null)
                //        {
                //            historyOfStockCode[i].BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
                //            historyOfStockCode[i].BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
                //            historyOfStockCode[i].BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
                //        }

                //        var sameDateMacd = macd.Where(r => r.Date == historyOfStockCode[i].Date).FirstOrDefault();
                //        if (sameDateMacd != null)
                //        {
                //            historyOfStockCode[i].MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
                //            historyOfStockCode[i].MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
                //            historyOfStockCode[i].MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        continue;
                //    }

                //    _context.History.Update(historyOfStockCode[i]);
                //}
            }

            await _context.SaveChangesAsync();

            return "true";
        }

        //public async Task<string> UpdateIndicators()
        //{
        //    var symbols = await _context.StockSymbol
        //        .Where(s => s.BiChanGiaoDich == false)
        //        .OrderByDescending(s => s._sc_)
        //        .ToListAsync();
        //    var stockCodes = symbols.Select(s => s._sc_).ToList();

        //    var historiesStockCode = await _context.History
        //        .Where(ss => stockCodes.Contains(ss.StockSymbol))
        //        .OrderByDescending(ss => ss.Date)
        //        .ToListAsync();

        //    foreach (var symbol in symbols)
        //    {
        //        //Update stock
        //        var historyOfStockCode = historiesStockCode
        //            .Where(ss => ss.StockSymbol == symbol._sc_)
        //            .OrderByDescending(h => h.Date)
        //            .ToList();

        //        var latestDate = historyOfStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
        //        var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(historyOfStockCode);
        //        symbol.BiChanGiaoDich = biCanhCao;

        //        var avarageOfLastXXPhien = historyOfStockCode.Take(20).Sum(h => h.V) / 20;
        //        symbol.MA20Vol = avarageOfLastXXPhien;
        //        _context.StockSymbol.Update(symbol);

        //        //Only Update indicators if the code isn't "BiCanhCao"
        //        if (biCanhCao || avarageOfLastXXPhien <= ConstantData.minMA20VolDaily) continue;

        //        var qoutes = MACDConvert(historyOfStockCode);

        //        var ichimoku = qoutes.GetIchimoku();
        //        var macd = qoutes.GetMacd();
        //        var rsis = qoutes.GetRsi();
        //        var bands = qoutes.GetBollingerBands();
        //        var ma5 = qoutes.GetSma(5);

        //        //var historiesWithNoIndicate = historyOfStockCode.Where(h => !h.HadAllIndicators()).ToList();
        //        var rebootHistoryIndicates = historyOfStockCode;

        //        var test = new List<History>();
        //        for (int i = 0; i < rebootHistoryIndicates.Count(); i++)
        //        {
        //            try
        //            {
        //                //special
        //                //rebootHistoryIndicates[i].RSIPhanKi = rebootHistoryIndicates.XacDinhPhanKi(rebootHistoryIndicates[i]);
        //                //rebootHistoryIndicates[i].MACDPhanKi = rebootHistoryIndicates.XacDinhPhanKi(rebootHistoryIndicates[i], "MACD");

        //                //if (historiesWithNoIndicate[i].HadAllIndicators()) continue;

        //                var sameDateMA5 = ma5.Where(r => r.Date == rebootHistoryIndicates[i].Date).FirstOrDefault();
        //                if (sameDateMA5 != null && !rebootHistoryIndicates[i].HadMA5())
        //                {
        //                    rebootHistoryIndicates[i].GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
        //                }

        //                var sameDateIchi = ichimoku.Where(r => r.Date == rebootHistoryIndicates[i].Date).FirstOrDefault();
        //                if (sameDateIchi != null && !rebootHistoryIndicates[i].HadIchimoku())
        //                {
        //                    rebootHistoryIndicates[i].IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
        //                    rebootHistoryIndicates[i].IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
        //                    rebootHistoryIndicates[i].IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
        //                    rebootHistoryIndicates[i].IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
        //                }

        //                rebootHistoryIndicates[i].NenBot = Math.Min(rebootHistoryIndicates[i].O, rebootHistoryIndicates[i].C);
        //                rebootHistoryIndicates[i].NenTop = Math.Max(rebootHistoryIndicates[i].O, rebootHistoryIndicates[i].C);

        //                var sameDateRSI = rsis.Where(r => r.Date == rebootHistoryIndicates[i].Date).FirstOrDefault();
        //                if (sameDateRSI != null && !rebootHistoryIndicates[i].HadRsi())
        //                {
        //                    rebootHistoryIndicates[i].RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
        //                }

        //                var sameDateBands = bands.Where(r => r.Date == rebootHistoryIndicates[i].Date).FirstOrDefault();
        //                if (sameDateBands != null && !rebootHistoryIndicates[i].HadBands())
        //                {
        //                    rebootHistoryIndicates[i].BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
        //                    rebootHistoryIndicates[i].BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
        //                    rebootHistoryIndicates[i].BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
        //                }

        //                var sameDateMacd = macd.Where(r => r.Date == rebootHistoryIndicates[i].Date).FirstOrDefault();
        //                if (sameDateMacd != null && !rebootHistoryIndicates[i].HadMACD())
        //                {
        //                    rebootHistoryIndicates[i].MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
        //                    rebootHistoryIndicates[i].MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
        //                    rebootHistoryIndicates[i].MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                continue;
        //            }

        //            _context.History.Update(rebootHistoryIndicates[i]);
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    return "true";
        //}


        public async Task<string> UpdateIndicators1()
        {
            var date = new DateTime(2022, 12, 15);
            var symbols = await _context.StockSymbol
                .Where(s => s._sc_ == "VNINDEX")
                .OrderByDescending(s => s._sc_)
                .ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            foreach (var symbol in symbols)
            {
                //Update stock
                var historiesWithNoIndicate = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderByDescending(h => h.Date)
                    .ToList();

                //var latestDate = historyOfStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
                //var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(historyOfStockCode);
                //symbol.BiChanGiaoDich = biCanhCao;

                //var avarageOfLastXXPhien = historyOfStockCode.Take(20).Sum(h => h.V) / 20;
                //symbol.MA20Vol = avarageOfLastXXPhien;
                //_context.StockSymbol.Update(symbol);

                ////Only Update indicators if the code isn't "BiCanhCao"
                //if (biCanhCao || avarageOfLastXXPhien <= ConstantData.minMA20VolDaily) continue;

                //var qoutes = MACDConvert(historyOfStockCode);

                //var ichimoku = qoutes.GetIchimoku();
                //var macd = qoutes.GetMacd();
                //var rsis = qoutes.GetRsi();
                //var bands = qoutes.GetBollingerBands();
                //var ma5 = qoutes.GetSma(5);

                //var historiesWithNoIndicate = historyOfStockCode.Where(h => !h.HadAllIndicators()).ToList();


                //special
                var i = historiesWithNoIndicate.Where(h => h.Date == date).FirstOrDefault();
                i.RSIPhanKi = historiesWithNoIndicate.XacDinhPhanKi(i);
                i.MACDPhanKi = historiesWithNoIndicate.XacDinhPhanKi(i, "MACD");
                //for (int i = 0; i < historiesWithNoIndicate.Count(); i++)
                //{
                //    try
                //    {
                //        //special
                //        historiesWithNoIndicate[i].RSIPhanKi = historiesWithNoIndicate.XacDinhPhanKi(historiesWithNoIndicate[i]);
                //        historiesWithNoIndicate[i].MACDPhanKi = historiesWithNoIndicate.XacDinhPhanKi(historiesWithNoIndicate[i], "MACD");
                //    }
                //    catch (Exception ex)
                //    {
                //        continue;
                //    }

                //    _context.History.Update(historiesWithNoIndicate[i]);
                //}
            }

            //await _context.SaveChangesAsync();

            return "true";
        }

        public async Task<string> UpdateIndicators2()
        {
            var symbols = await _context.StockSymbol
                .Where(s => s._sc_.Length == 3 && s.MA20Vol >= ConstantData.minMA20VolDaily)
                .OrderByDescending(s => s._sc_)
                .ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var result = new StringBuilder();
            //Stock A in Phase 1 has decreased B and then increase C in phase 2 and then currently it is decreasing D in Phase 3, expected to increase E in Phase 4
            var details = new List<Tuple<string, decimal, decimal, decimal>>();
            foreach (var symbol in symbols)
            {
                try
                {
                    var histories = historiesStockCode
                                .Where(ss => ss.StockSymbol == symbol._sc_)
                                .ToList();

                    var checkingRank = histories.Where(h => h.Date >= new DateTime(2022, 7, 15) && h.Date <= new DateTime(2022, 11, 8)).ToList();
                    var minValue = checkingRank.Min(h => h.L);
                    var maxValue = checkingRank.Max(h => h.H);
                    var decreaseNumber = 1 - (minValue / maxValue);

                    var checkingRank2 = histories.Where(h => h.Date >= new DateTime(2022, 11, 11) && h.Date <= new DateTime(2022, 12, 30)).ToList();
                    var minValue2 = checkingRank2.Min(h => h.L);
                    var maxValue2 = checkingRank2.Max(h => h.H);
                    var increaseNumber = (maxValue2 / minValue2) - 1;

                    var checkingRank3 = histories.Where(h => h.Date >= new DateTime(2022, 12, 5) && h.Date <= new DateTime(2022, 12, 30)).ToList();
                    var currentValue = checkingRank3.OrderByDescending(h => h.Date).First().C;
                    var maxValue3 = checkingRank3.Max(h => h.H);
                    var decreaseNumber3 = 1 - (currentValue / maxValue3);

                    var detailData = new Tuple<string, decimal, decimal, decimal>(symbol._sc_, decreaseNumber, increaseNumber, decreaseNumber3);
                    details.Add(detailData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{symbol._sc_} - {ex.Message.ToString()}");
                    continue;
                }
            }

            //details = details.OrderByDescending(d => d.Item2).ThenByDescending(d => d.Item3).ThenByDescending(d => d.Item4).ToList();
            details = details.OrderByDescending(d => d.Item4).ThenByDescending(d => d.Item2).ThenByDescending(d => d.Item3).ToList();
            foreach (var item in details)
            {
                result.AppendLine($"{item.Item1} in Phase 1 has decreased {item.Item2.ToString("N2")} and then increase {item.Item3.ToString("N2")} in phase 2 and then currently it is decreasing {item.Item4.ToString("N2")} in Phase 3");
            }

            return result.ToString();
        }

        //[HttpPost]
        //public async Task<string> Update()
        //{
        //    return await UpdateIndicators();
        //}

        public IEnumerable<Quote> MACDConvert(List<History> histories)
        {
            var qoutes = new List<Quote>();
            foreach (var h in histories)
            {
                qoutes.Add(new Quote
                {
                    Date = h.Date,
                    Close = h.C,
                    High = h.H,
                    Low = h.L,
                    Open = h.O,
                    Volume = h.V,
                });
            }

            return qoutes;
        }

        public IEnumerable<Quote> MACDConvertHistory(List<HistoryHour> histories)
        {
            var qoutes = new List<Quote>();
            foreach (var h in histories)
            {
                qoutes.Add(new Quote
                {
                    Date = h.Date,
                    Close = h.C,
                    High = h.H,
                    Low = h.L,
                    Open = h.O,
                    Volume = h.V,
                });
            }

            return qoutes;
        }

        public void AddBollingerBands(List<History> histories, int period, int factor)
        {
            double total_average = 0;
            double total_squares = 0;

            for (int i = 0; i < histories.Count(); i++)
            {
                total_average += (double)histories[i].C;
                total_squares += Math.Pow((double)histories[i].C, 2);

                if (i >= period - 1)
                {
                    double total_bollinger = 0;
                    double average = total_average / period;

                    double stdev = Math.Sqrt((total_squares - Math.Pow(total_average, 2) / period) / period);
                    //histories[i]..Values[i]["bollinger_average"] = average;
                    histories[i].BandsTop = (decimal)(average + factor * stdev);
                    histories[i].BandsBot = (decimal)(average - factor * stdev);

                    //total_average -= data.Values[i - period + 1]["close"];
                    //total_squares -= Math.Pow(data.Values[i - period + 1]["close"], 2);

                    total_average -= (double)histories[i - period + 1].C;
                    total_squares -= Math.Pow((double)histories[i - period + 1].C, 2);
                }
            }
        }




        // GET: History/Create
        // Form Data:
        //      code: A32
        //      from: 01-01-1970  (12 AM as default)
        //      to: 15-02-2022    (12 AM as default)
        [HttpPost]
        public async Task<string> CreateByHour(string code)
        {
            var restService = new RestServiceHelper();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 300000).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 300000 && splitStringCode.Contains(s._sc_)).OrderByDescending(s => s._sc_).ToListAsync();

            var result = new List<HistoryHour>();

            var latestHistory = _context.HistoryHour.OrderByDescending(r => r.Date).FirstOrDefault();
            //var currentLatestDate = latestHistory == null ? new DateTime(2000, 1, 1) : latestHistory.Date;
            var currentLatestDate = new DateTime(2000, 1, 1);

            var from = currentLatestDate.WithoutHours();
            var to = DateTime.Now;

            var service = new Service();
            //await service.GetVHours(result, symbols, from, to, from, 0);
            //result = result.Where(r => r.Date > currentLatestDate).ToList();

            //var fireAntDataForHourFromDate = new DateTime(2022, 1, 1);
            var selectedCodes = symbols.Select(s => s._sc_).Distinct().ToList();
            var fireAntCookies = "_gid=GA1.2.180306469.1674532778; _gat=1; _ga_ZJ4G3SW582=GS1.1.1674539348.87.0.1674539348.0.0.0; _ga=GA1.2.805353658.1649312480; FireAnt.Authentication.v3=5uCwF4NDvTmz-zZ-KbRmLjo7DlqALfKvltmim0hOl9MK0jM1IAN42oCKY_98Iusdz_5nHeOYl_mTndaFWEG1HiH5ClY-b5N1B4J8VsUWnQkJ2FyS8WNYUmZ3LUGLbzSBxgD2kFafFhXjsMWrFz6I4VifOAHB6DacXTG55NO0Q8RIqvLa3l7m4Y7xJFJYWW_Ft6meXpOBNbIMCfSrLDytm8Mn9KrPfj9lEnxYYoDE0h3J0mKmAFcI-5mlIR2szM0X6a64y4TQ2jkqnML8txhJmqB-xDJn4z-F7TAH4oLANF9wJw4-H-G-jX4R0GhrSHC5lMPwKDpRmT3kEYlPLYyBHloeTmUtuvYt0JrQd2pitJXBsh4uF3ImkwpcKyJ9hv39iZ9zrKKLMu3kh_acHZhBPn7Osq1F8MCwLFw0o3XnayS-jWZknBbHG4Oxv2QBEsxr68aza3jWZu6O3Q2zTtMzYJhDYTBMiACzC_GHU1y6hPKGpfmGD_PiCjbNlK1fiAGPMu2ojN9K5kpz-BMfXViTHzm7Fn9ZkYEy6psBaSVc_g9S0XSXV1NbvwfDlY3Krkz786pIM-ns7C-MpHLUk0UyWrvKZvChzVHHRwQ5ufk5F9aNcI9lCL5LRqsCtwe6jTK_UNoKbOc82qBOLd1Qk2kPnzXABCXLt44iSYpYzF5c57bxUsB0g39XRNEWUF2pGbfMp4boiBfE9aTH9h3elqBSw4o_dVbzupAs1nS0qg4Wx2jbOyHj5UAHpGKqUtkNRMADYXLBSlf24d4NK9Bj2AvfOMNljrXjYR8M13Zd0jqqDYbwVXo0C6XnmZG8icCubP-b6KY8psOtdBsl9UNrT_g7lfXN2r1--KDxJamT-dYYd0S9ap4XN8nNDVNTnOSEVD5boR-onHwNpajV33W09TfPpqxZr8jBYItdrHBq_0oGQeNlpDX0MUgclhJT8HlaYtEsVsZmx_6iPl1FSnaaae_TPQTJHVM7DwS5Mw5g7PhBkcTw-fYmZjwU7q8SiQ5VSXlDHE51gEGrPtFe7W15TZ_LWEsjGL842EOUan67lu3zP-HKPvR_jw4i-EOhzRf-tJkG9cUSTnirptsRD_P85kgnJx_ds_gjQtQuPuoQjveSu3iot6bLWN9tAcetZZp6ikrcIq9CltuRFR6la_EdNmiMkjdUiu79FVlGZ2Af6I6_W7FJxWu4Jd52rp8m4c4_l6YFKvJ8VVZPkyR-i8pH24DDoQC63eCaX7owI7Zxm9t_JxdtDheGT0vNnVr0n4AWkh2AbszAfCk0eRwdMGDVJ8PxrmGAAXsPo0Y0rtvPlwzfhsJVPV2a1f3wbyrQVBuHgZ_inOyQLQREWx4W0xSWUtpzynILpbx2Ilc21eorJSffF-VFR-Uxajarx1DmlHfGI7r1oFcsDcBEQ0V29QOf8TkLdprr12hToqrerq2yyPPUeLlbhaVTl8R0SzarQtwd5YmU2mUPvwAbuUjnfRK7g_2raYYgNXpbK3CK4jGboQYI8w62oVo2Htuf6TZ3khrs1haFaVh6KC4xDu0GGgfcwIgrmzeNOmAAx__IHRlcY0QDrBs9cemyx4WDOuTc0hFzLjDT_A27u37pxUbAIJor8UNkqBpraTfJR-BTSa9oKw0hIkq13HnyQrNM96vV2jffHOCQkteb7oHIbU2-hyIMTHFiblU3pxJ9StsY8xwrZDEzZp58YA8eX4KL06quWkO0Yx9I241HC7GVbLF06Ibtimzg2uE5QaJUbVIGQr6FfCPY93xnQ2-WCNbZ7xp6xewUstgM_miGRpe3AqN1nWx6pNg5TRDS2c3yCYTzSeYaQC2ZWdaSn7d_4rd0JrQrWqBR698ysX0i2QC1sPedWsacMr4mo3S1VzIPrIpHaEfS9gAzPKxrzBEKrhvHxk8llbqrePUH9F2zX7ro1LpZcjYzwHM5qhCwAzqev-qM4YKy15mL5U6nTwqIthiDGxk1jgQgNJeF6Rw8RTf7jdXmOkJ9eUbap3NIwTRdw6MoZHqvuGZ6PjBVs4z8o-bw8WwRGU8E-E9ELNrogdtMF985cimasB3LubKBCHXA6JQCD63CEbXUn6rt958URo9xShU09-lBkMlaCs7H9BuK4qXOQLRQyxuVWrZSPGQx3bgrEvPykYIdFyK37Kqgu2Elicun09iMDLZ2EaSQKj-uSsfz2xcLFXJaujEx2JfHkysNsrTPQTu9pySkt8UE_HKKnvpi0S2XfkFL3tL-MA0HN8xA1S07eXPrjNxdWaVyktwmNY0S7K_yJJuM3MSRbD5Vt0B5uLEZL_SHcGAwfW2Ae4FmFLcCZt_Hj5T2Tgcnxfd7XJqCWk7_9vwNuAMylg7eWC-oaOD4J_1Hqybl4kBvb95XrdFEKsNpnsqIMeR-HLswQd059cmfSlTSQsbeZCxc1ATRV42I5quU6xAd5PqCaD7AE0i-5R5tzzEbTN7iQNdedbpIF5OpkqRDPkBe4QOe2vGAFrmp1fQyzePjpf25FnQMOqO3OWN6V3-xzVCoYTT-kf-SwuXyioe-k3gFWD2TQVuRj25dTPHA_IaX0rBNAfqSll_GtuCX3CewlSy5RgResIflLLK0yeHYR8gP-fokjIH_C--O4AxVjgct6YdgixXL_Eu7jx3eHB20-Y_0eEYQ7TIC_2BsdxPfCKz0cPkLLbIx_Vjw-zvMl9BkhodXPI5K1M5VCJOWRSmjt2hmFVDS-cBIRV_NHtcsTN3wud3N4FDi5HiwmxWH_XD58VDiopkji3-XFq8TVx_XIuFnyz8OXNu8dGVVjE-XlPaxW4G-R1BZ1UiQ778ZRULDN1DNxQAmXhid3zoJVFEmx85wYJvzhJF7PQC97aGt0vGXWVLAqPQlW7PClFw9AqG8AyakSroNOBHKJySZKL_xLah4GK1Whi8o_5r4qTCkTIfoB1A3XwEIPr7QKUi-ZJ7YVhx0ILVxTfe2ANVm0cQ7h2Q; ASP.NET_SessionId=2j0yr34x30talkyewlt5h0yd";
            await service.GetVHoursFireAnt(result, selectedCodes, currentLatestDate, fireAntCookies);

            if (result.Any())
            {
                var a = result.Select(r => r.StockSymbol).Distinct().ToList();

                await _context.HistoryHour.AddRangeAsync(result);
                await _context.SaveChangesAsync();
                await UpdateIndicatorsHour();
            }

            return string.Empty;
        }


        public async Task<string> UpdateIndicatorsHour()
        {
            var symbols = await _context.StockSymbol
                .Where(s => s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily)
                //.Where(s => s._sc_ == "HMC")
                .OrderByDescending(s => s._sc_)
                .ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.HistoryHour
                .Where(ss => stockCodes.Contains(ss.StockSymbol))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            int rsiPeriod = 14;
            int bandsPeriod = 20;
            int bandsFactor = 2;

            foreach (var symbol in symbols)
            {
                //Update stock
                //var histories = historiesStockCode
                //    .Where(ss => ss.StockSymbol == symbol._sc_)
                //    .OrderByDescending(h => h.Date)
                //    .ToList();

                //var latestDate = histories.OrderByDescending(h => h.Date).FirstOrDefault();
                //var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(histories);
                //symbol.BiChanGiaoDich = biCanhCao;

                //var avarageOfLastXXPhien = histories.Take(20).Sum(h => h.V) / 20;
                //symbol.MA20Vol = avarageOfLastXXPhien;
                //_context.StockSymbol.Update(symbol);



                //Update indicators - Done
                var historiesInPeriodOfTime = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();

                if (!historiesInPeriodOfTime.Any()) continue;

                var qoutes = MACDConvertHistory(historiesInPeriodOfTime);

                var ichimoku = qoutes.GetIchimoku();
                var macd = qoutes.GetMacd();
                var rsis = qoutes.GetRsi();
                var bands = qoutes.GetBollingerBands();
                var ma5 = qoutes.GetSma(5);


                //var test = new List<HistoryHour>();
                //for (int i = 0; i < historiesInPeriodOfTime.Count; i++)
                //{
                //    try
                var historiesWithNoIndicate = historiesInPeriodOfTime.Where(h => !h.HadAllIndicators()).ToList();
                for (int i = 0; i < historiesWithNoIndicate.Count(); i++)
                {
                    var updatedItem = historiesWithNoIndicate[i];
                    {
                        if (historiesInPeriodOfTime[i].HadAllIndicators()) continue;
                        updatedItem.RSIPhanKi = historiesInPeriodOfTime.XacDinhPhanKi(updatedItem);
                        updatedItem.MACDPhanKi = historiesInPeriodOfTime.XacDinhPhanKi(updatedItem, "MACD");

                        var sameDateMA5 = ma5.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateMA5 != null && !historiesInPeriodOfTime[i].HadMA5())
                        {
                            historiesInPeriodOfTime[i].GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
                        }

                        var sameDateIchi = ichimoku.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateIchi != null && !historiesInPeriodOfTime[i].HadIchimoku())
                        {
                            historiesInPeriodOfTime[i].IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
                        }

                        historiesInPeriodOfTime[i].NenBot = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].O : historiesInPeriodOfTime[i].C;
                        historiesInPeriodOfTime[i].NenTop = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].C : historiesInPeriodOfTime[i].O;

                        var sameDateRSI = rsis.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateRSI != null && !historiesInPeriodOfTime[i].HadRsi())
                        {
                            historiesInPeriodOfTime[i].RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
                        }

                        var sameDateBands = bands.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateBands != null && !historiesInPeriodOfTime[i].HadBands())
                        {
                            historiesInPeriodOfTime[i].BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
                        }

                        var sameDateMacd = macd.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateMacd != null && !historiesInPeriodOfTime[i].HadMACD())
                        {
                            historiesInPeriodOfTime[i].MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
                            historiesInPeriodOfTime[i].MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
                            historiesInPeriodOfTime[i].MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
                        }
                    }
                    //catch (Exception ex)
                    //{
                    //    continue;
                    //}

                    _context.HistoryHour.Update(historiesInPeriodOfTime[i]);
                }
            }

            await _context.SaveChangesAsync();

            return "true";
        }


        // GET: https://localhost:44359/History/Details?code=A32
        [HttpGet]
        public async Task<List<string>> DeleteDuplicatedOnes()
        {
            var restService = new RestServiceHelper();
            var result = new List<string>();
            var symbols = await _context.StockSymbol.ToListAsync();
            var codes = symbols.Select(h => h._sc_).ToList();

            var dbData = await _context.History.ToListAsync();
            var duplicatedItems = dbData.GroupBy(c => new { c.StockSymbol, c.Date })
                .Where(grp => grp.Count() > 1)
                .Select(gr => gr)
                .ToList();

            var deleteItems = new List<History>();

            for (int i = 0; i < duplicatedItems.Count; i++)
            {
                var duplicatedItem = duplicatedItems[i].First();
                var deleteItem = dbData.First(d => d.Date == duplicatedItem.Date && d.StockSymbol == duplicatedItem.StockSymbol);
                deleteItems.Add(deleteItem);
            }

            if (deleteItems.Any())
            {
                _context.History.RemoveRange(deleteItems);
                await _context.SaveChangesAsync();
            }
            return result;
        }

    }
}