using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Skender.Stock.Indicators;

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
            var restService = new RestServiceHelper();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000 && splitStringCode.Contains(s._sc_)).OrderByDescending(s => s._sc_).ToListAsync();
            //var stockCodes = symbols.Select(s => s._sc_).ToList();

            //var allSymbols = await _context.StockSymbol
            //    .Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000)
            //    .OrderByDescending(s => s._sc_)
            //    .ToListAsync();

            var result = new List<History>();
            var latestHistory = _context.History.OrderByDescending(r => r.Date).FirstOrDefault();
            var currentLatestDate = latestHistory == null ? new DateTime(2000, 1, 1) : latestHistory.Date;
            var from = tuNgay < new DateTime(2000, 1, 1) ? currentLatestDate.AddDays(1) : tuNgay;
            var to = DateTime.Now.WithoutHours();

            var service = new Service();
            await service.GetV(result, symbols, from, to, from, 0);

            result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.History.AddRangeAsync(result);
                await _context.SaveChangesAsync();
                await UpdateIndicators(currentLatestDate);
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
                .Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000)
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

        public async Task<string> UpdateIndicators(DateTime fromDate)
        {
            var symbols = await _context.StockSymbol
                .Where(s => s.BiChanGiaoDich == false && s.MA20Vol > 100000)
                .Where(s => s._sc_ == "HMC")
                .OrderByDescending(s => s._sc_)
                .ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();


            //var last60Phien = await _context.History.Where(h => h.StockSymbol == "SSI" && h.Date >= fromDate).OrderByDescending(h => h.Date).Skip(60).Take(1).FirstOrDefaultAsync();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol))
                //.Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= last60Phien.Date)
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

                var qoutes = MACDConvert(historiesInPeriodOfTime);

                var ichimoku = qoutes.GetIchimoku();
                var macd = qoutes.GetMacd();
                var rsis = qoutes.GetRsi();
                var bands = qoutes.GetBollingerBands();
                var ma5 = qoutes.GetSma(5);


                var test = new List<History>();
                for (int i = 0; i < historiesInPeriodOfTime.Count; i++)
                {
                    try
                    {
                        //if (historiesInPeriodOfTime[i].HadAllIndicators()) continue;

                        var sameDateMA5 = ma5.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateMA5 != null)// && !historiesInPeriodOfTime[i].HadMA5())
                        {
                            historiesInPeriodOfTime[i].GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
                        }

                        var sameDateIchi = ichimoku.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateIchi != null)// && !historiesInPeriodOfTime[i].HadIchimoku())
                        {
                            historiesInPeriodOfTime[i].IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
                        }

                        historiesInPeriodOfTime[i].NenBot = Math.Min(historiesInPeriodOfTime[i].O, historiesInPeriodOfTime[i].C);
                        historiesInPeriodOfTime[i].NenTop = Math.Max(historiesInPeriodOfTime[i].O, historiesInPeriodOfTime[i].C);

                        var sameDateRSI = rsis.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateRSI != null)// && !historiesInPeriodOfTime[i].HadRsi())
                        {
                            historiesInPeriodOfTime[i].RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
                        }

                        var sameDateBands = bands.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateBands != null)// && !historiesInPeriodOfTime[i].HadBands())
                        {
                            historiesInPeriodOfTime[i].BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
                        }

                        var sameDateMacd = macd.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateMacd != null)// && !historiesInPeriodOfTime[i].HadMACD())
                        {
                            historiesInPeriodOfTime[i].MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
                            historiesInPeriodOfTime[i].MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
                            historiesInPeriodOfTime[i].MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    _context.History.Update(historiesInPeriodOfTime[i]);
                }
            }

            await _context.SaveChangesAsync();

            return "true";
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
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000).OrderByDescending(s => s._sc_).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > 100000 && splitStringCode.Contains(s._sc_)).OrderByDescending(s => s._sc_).ToListAsync();

            var result = new List<HistoryHour>();

            var latestHistory = _context.HistoryHour.OrderByDescending(r => r.Date).FirstOrDefault();
            var currentLatestDate = latestHistory == null ? new DateTime(2000, 1, 1) : latestHistory.Date;

            //var from = new DateTime(2000, 1, 1);
            //var to = DateTime.Now;

            var from = currentLatestDate.WithoutHours();
            var to = DateTime.Now;

            var service = new Service();
            await service.GetVHours(result, symbols, from, to, from, 0);

            result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.HistoryHour.AddRangeAsync(result);
                await _context.SaveChangesAsync();
                await UpdateIndicators(currentLatestDate);
            }

            return string.Empty;
        }


        public async Task<string> UpdateIndicatorsHour()
        {
            var symbols = await _context.StockSymbol
                .Where(s => s.BiChanGiaoDich == false && s.MA20Vol > 100000)
                .Where(s => s._sc_ == "HMC")
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


                var test = new List<HistoryHour>();
                for (int i = 0; i < historiesInPeriodOfTime.Count; i++)
                {
                    try
                    {
                        //if (historiesInPeriodOfTime[i].HadAllIndicators()) continue;

                        var sameDateMA5 = ma5.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateMA5 != null)// && !historiesInPeriodOfTime[i].HadMA5())
                        {
                            historiesInPeriodOfTime[i].GiaMA05 = sameDateMA5.Sma.HasValue ? (decimal)sameDateMA5.Sma.Value : 0;
                        }

                        var sameDateIchi = ichimoku.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateIchi != null)// && !historiesInPeriodOfTime[i].HadIchimoku())
                        {
                            historiesInPeriodOfTime[i].IchimokuCloudBot = sameDateIchi.SenkouSpanB.HasValue ? (decimal)sameDateIchi.SenkouSpanB.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuCloudTop = sameDateIchi.SenkouSpanA.HasValue ? (decimal)sameDateIchi.SenkouSpanA.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuTenKan = sameDateIchi.TenkanSen.HasValue ? (decimal)sameDateIchi.TenkanSen.Value : 0;
                            historiesInPeriodOfTime[i].IchimokuKijun = sameDateIchi.KijunSen.HasValue ? (decimal)sameDateIchi.KijunSen.Value : 0;
                        }

                        historiesInPeriodOfTime[i].NenBot = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].O : historiesInPeriodOfTime[i].C;
                        historiesInPeriodOfTime[i].NenTop = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].C : historiesInPeriodOfTime[i].O;

                        var sameDateRSI = rsis.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateRSI != null)// && !historiesInPeriodOfTime[i].HadRsi())
                        {
                            historiesInPeriodOfTime[i].RSI = sameDateRSI.Rsi.HasValue ? (decimal)sameDateRSI.Rsi.Value : 0;
                        }

                        var sameDateBands = bands.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateBands != null)// && !historiesInPeriodOfTime[i].HadBands())
                        {
                            historiesInPeriodOfTime[i].BandsTop = sameDateBands.UpperBand.HasValue ? (decimal)sameDateBands.UpperBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsBot = sameDateBands.LowerBand.HasValue ? (decimal)sameDateBands.LowerBand.Value : 0;
                            historiesInPeriodOfTime[i].BandsMid = sameDateBands.Sma.HasValue ? (decimal)sameDateBands.Sma.Value : 0;
                        }

                        var sameDateMacd = macd.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDateMacd != null)// && !historiesInPeriodOfTime[i].HadMACD())
                        {
                            historiesInPeriodOfTime[i].MACD = sameDateMacd.Macd.HasValue ? (decimal)sameDateMacd.Macd.Value : 0;
                            historiesInPeriodOfTime[i].MACDSignal = sameDateMacd.Signal.HasValue ? (decimal)sameDateMacd.Signal.Value : 0;
                            historiesInPeriodOfTime[i].MACDMomentum = sameDateMacd.Histogram.HasValue ? (decimal)sameDateMacd.Histogram.Value : 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    _context.HistoryHour.Update(historiesInPeriodOfTime[i]);
                }
            }

            await _context.SaveChangesAsync();

            return "true";
        }
    }
}