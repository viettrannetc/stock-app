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

        // GET: StockSymbolHistory/Create
        // Form Data:
        //      code: A32
        //      from: 01-01-1970  (12 AM as default)
        //      to: 15-02-2022    (12 AM as default)
        [HttpPost]
        public async Task<string> Create()
        {
            var restService = new RestServiceHelper();
            //var huyNiemYet = new List<string>();
            //huyNiemYet.Add("KSK");
            //huyNiemYet.Add("TRT");
            //huyNiemYet.Add("ABR");
            //huyNiemYet.Add("GTN");
            //huyNiemYet.Add("FUCTVGF2");

            var allSymbols = await _context.StockSymbol
                .OrderByDescending(s => s._sc_)
                .ToListAsync();

            //allSymbols = allSymbols.Where(s => !huyNiemYet.Contains(s._sc_)).ToList();

            var result = new List<StockSymbolHistory>();
            var t1 = _context.StockSymbolHistory.Where(c => c.StockSymbol == "A32").OrderByDescending(r => r.Date).FirstOrDefault();
            var currentLatestDate = t1 == null ? new DateTime(2000, 1, 1) : t1.Date;
            var from = currentLatestDate;
            var to = DateTime.Now.WithoutHours();

            var service = new Service();
            await service.GetV(result, allSymbols, from, to, from, 0);

            result = result.Where(r => r.Date > currentLatestDate).ToList();

            if (result.Any())
            {
                await _context.StockSymbolHistory.AddRangeAsync(result);
                await _context.SaveChangesAsync();
            }

            return "true";
        }


        // GET: https://localhost:44359/StockSymbolHistory/Details?code=A32
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
            var histories = await _context.StockSymbolHistory.Where(m => codes.Contains(m.StockSymbol)).OrderBy(h => h.Date).ToListAsync();

            var service = new Service();
            //foreach (var symbol in symbols)
            //{
            //var dataOfSymbol = histories.Where(h => h.StockSymbol == symbol._sc_).ToList();
            var checkingDate = from;
            var resultMissingData = new List<StockSymbolHistory>();

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

                            //var date = startDate.ConvertToPhpInt();
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
                await _context.StockSymbolHistory.AddRangeAsync(resultMissingData);
                await _context.SaveChangesAsync();
            }


            return result;
        }


        [HttpPost]
        public async Task<string> Update()
        {
            var allSymbols = await _context.StockSymbol
                .Where(s => s.BiChanGiaoDich == false && s.MA20Vol > 100000)
                .OrderByDescending(s => s._sc_)
                .ToListAsync();
            var stockCodes = allSymbols.Select(s => s._sc_).ToList();
            var historiesStockCode = await _context.StockSymbolHistory.Where(ss => stockCodes.Contains(ss.StockSymbol))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            int rsiPeriod = 14;
            int bandsPeriod = 20;
            int bandsFactor = 2;

            foreach (var symbol in allSymbols)
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

                //double total_average = 0;
                //double total_squares = 0;

                //histories = histories.OrderBy(h => h.Date).ToList();

                var qoutes = MACDConvert(historiesInPeriodOfTime);

                var macd = new MacdResult();
                IEnumerable<Quote> quotes;// = GetHistoryFromFeed("SPY");

                var res = qoutes.GetMacd();


                for (int i = 0; i < historiesInPeriodOfTime.Count; i++)
                {
                    try
                    {
                        ////Update RSI
                        ////if (i < rsiPeriod) continue;
                        ////if (historiesInPeriodOfTime[i].RSI != 0) continue;

                        //if (i >= rsiPeriod)
                        //{
                        //    if (i == rsiPeriod)
                        //    {
                        //        var tuple = historiesInPeriodOfTime[i].RSIDetail(historiesInPeriodOfTime, rsiPeriod);
                        //        historiesInPeriodOfTime[i].RSIAvgG = tuple.Item1;
                        //        historiesInPeriodOfTime[i].RSIAvgL = tuple.Item2;
                        //        historiesInPeriodOfTime[i].RSI = tuple.Item3;
                        //    }
                        //    else
                        //    {
                        //        var gain = historiesInPeriodOfTime[i].C - historiesInPeriodOfTime[i - 1].C;
                        //        var totalG = (historiesInPeriodOfTime[i - 1].RSIAvgG * (rsiPeriod - 1) + (gain < 0 ? 0 : gain)) / rsiPeriod;
                        //        var totalL = (historiesInPeriodOfTime[i - 1].RSIAvgL * (rsiPeriod - 1) + (gain < 0 ? gain * (-1) : 0)) / rsiPeriod;

                        //        historiesInPeriodOfTime[i].RSIAvgG = totalG;
                        //        historiesInPeriodOfTime[i].RSIAvgL = totalL;
                        //        historiesInPeriodOfTime[i].RSI = totalL == 0
                        //            ? 0
                        //            : totalG / totalL == 0
                        //                ? 0
                        //                : 100 - 100 / ((totalG / totalL) + 1);
                        //    }
                        //}
                        //historiesInPeriodOfTime[i].NenBot = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].O : historiesInPeriodOfTime[i].C;
                        //historiesInPeriodOfTime[i].NenTop = historiesInPeriodOfTime[i].TangGia() ? historiesInPeriodOfTime[i].C : historiesInPeriodOfTime[i].O;


                        ////Update Bollingerbands
                        //total_average += (double)historiesInPeriodOfTime[i].C;
                        //total_squares += Math.Pow((double)historiesInPeriodOfTime[i].C, 2);

                        //if (i >= bandsPeriod - 1)
                        //{
                        //    double average = total_average / bandsPeriod;

                        //    double stdev = Math.Sqrt((total_squares - Math.Pow(total_average, 2) / bandsPeriod) / bandsPeriod);

                        //    historiesInPeriodOfTime[i].BandsTop = (decimal)(average + bandsFactor * stdev);
                        //    historiesInPeriodOfTime[i].BandsBot = (decimal)(average - bandsFactor * stdev);

                        //    total_average -= (double)historiesInPeriodOfTime[i - bandsPeriod + 1].C;
                        //    total_squares -= Math.Pow((double)historiesInPeriodOfTime[i - bandsPeriod + 1].C, 2);
                        //}


                        //Update MACD
                        var sameDate = res.Where(r => r.Date == historiesInPeriodOfTime[i].Date).FirstOrDefault();
                        if (sameDate != null)
                        {
                            historiesInPeriodOfTime[i].MACDFast = sameDate.Macd.HasValue ? (decimal)sameDate.Macd.Value : 0;
                            historiesInPeriodOfTime[i].MACDSlow = sameDate.Signal.HasValue ? (decimal)sameDate.Signal.Value : 0;
                            historiesInPeriodOfTime[i].MACDMomentum = sameDate.Histogram.HasValue ? (decimal)sameDate.Histogram.Value : 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    _context.StockSymbolHistory.Update(historiesInPeriodOfTime[i]);
                }


            }

            await _context.SaveChangesAsync();

            return "true";
        }

        public IEnumerable<Quote> MACDConvert(List<StockSymbolHistory> histories)
        {
            var qoutes = new List<Quote>();
            foreach (var h in histories)
            {
                qoutes.Add(new Quote
                {
                    Close = h.C,
                    Date = h.Date
                });
            }

            return qoutes;
        }

        public void AddBollingerBands(List<StockSymbolHistory> histories, int period, int factor)
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
    }
}