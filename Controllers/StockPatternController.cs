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

namespace DotNetCoreSqlDb.Controllers
{
    public class StockPatternController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockPatternController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: Stock
        public async Task<IActionResult> Index()
        {
            return View(await _context.StockSymbol.ToListAsync());
        }

        // GET: Stock/Pattern/5
        public async Task<PatternResponseModel> Pattern(string pattern, string code, DateTime startFrom, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();// List<PatternDetailsResponseModel>();

            pattern = "aL - Pattern 2";

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            //var startFrom = "2020-01-02 00:00:00.0000000"

            switch (pattern)
            {
                #region case 1
                case "":
                    //DK 1: Lowest today < Lowest in the last XXX day && Lowest today < 2nd Lowest in the last XXX day
                    //DK 2: C > L * 0.02
                    //DK3 = C <= O * 0.03;
                    //DK4 = V > MA(V, 21) * 1.5;
                    //Filter: DK1 && DK2 && DK3 && DK4

                    result.PatternName = "aL - Pattern 1";

                    //var symbols = await _context.StockSymbol.ToListAsync();
                    foreach (var symbol in symbols)
                    {
                        var patternOnsymbol = new PatternBySymbolResponseModel();
                        result.Symbols.Add(patternOnsymbol);
                        patternOnsymbol.StockCode = symbol._sc_;

                        var histories = await _context.StockSymbolHistory.Where(ss => ss.StockSymbol == symbol._sc_)
                            .OrderBy(ss => ss.Date)
                            .ToListAsync();

                        var latestDate = histories.LastOrDefault().Date;
                        foreach (var history in histories)
                        {
                            var today = history.Date;
                            var previousDays = histories.Where(h => h.Date < today).OrderByDescending(h => h.Date).Take(300).ToList();

                            if (previousDays.Count < 20) continue;

                            var lowest = previousDays.OrderBy(h => h.L).First();
                            var secondLowest = previousDays.OrderBy(h => h.L).ToList()[1];

                            var firstCondition = history.L < lowest.L && history.L < secondLowest.L
                                && lowest.L.Difference(secondLowest.L, 0.03M);
                            var secondCondition = history.C > history.L * 0.02M;
                            var thirdCondition = history.O.Difference(history.C, 0.03M);
                            //(history.O - history.O * 0.03M) <= history.C && history.C <= (history.O + (history.O * 0.03M));
                            var previous20Times = (previousDays.OrderByDescending(d => d.Date).Take(20).Sum(h => h.V) / 20);
                            var fourthCondition = history.V > previous20Times * 1.5M;

                            if (firstCondition && secondCondition && thirdCondition && fourthCondition)
                            {
                                var reality = false;
                                var tomorrowDate = history.Date;
                                tomorrowDate = tomorrowDate.AddDays(1);
                                var historyTomorrow = histories.FirstOrDefault(h => h.Date == tomorrowDate);
                                while (historyTomorrow == null && tomorrowDate < latestDate.Date)
                                {
                                    tomorrowDate = tomorrowDate.AddDays(1);
                                    historyTomorrow = histories.FirstOrDefault(h => h.Date == tomorrowDate);
                                }

                                if (historyTomorrow != null && historyTomorrow.C > history.C)
                                    reality = true;


                                patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                                {
                                    ConditionMatchAt = today,
                                    MoreInformation = new
                                    {
                                        TodayOpening = history.O,
                                        TodayClosing = history.C,
                                        TodayLowest = history.L,
                                        TodayTrading = history.V,
                                        Previous1stLowest = lowest.L,
                                        Previous1stLowestDate = lowest.Date,
                                        Previous2ndLowest = secondLowest.L,
                                        Previous2ndLowestDate = secondLowest.Date,
                                        AverageNumberOfTradingInPrevious20Times = previous20Times,
                                        ShouldBuy = true,
                                        RealityExpectation = reality,
                                        Tomorrow = historyTomorrow?.Date,
                                        TomorrowClosing = historyTomorrow?.C
                                    }
                                });
                            }
                        }

                    }
                    break;
                #endregion
                #region case 2
                case "aL - Pattern 2":
                    /* 
                     * lay gia C de tinh day
                     * lay 30 phien cuoi cung
                     * 2 day cach nhau > 5 phien
                     * day 1 se cach dinh? cu~ (trong vong 1 thang) > 15% tro len 
                     * day 2 >= day 1 
                     * day 1 luon luon nam truoc day 2 
                     * ngay hien tai C > 2% so voi day 2 (ko bao gio > 7%)
                    */

                    result.PatternName = pattern;
                    var stockCodes = symbols.Select(s => s._sc_).ToList();

                    var historiesInPeriodOfTimeNonDB = await _context.StockSymbolHistory
                            .Where(ss =>
                                stockCodes.Contains(ss.StockSymbol)
                                && ss.Date >= startFrom
                                )
                            .OrderByDescending(ss => ss.Date)
                            .ToListAsync();

                    var bag = new ConcurrentBag<object>();
                    Parallel.ForEach(symbols, symbol =>
                    {
                        var patternOnsymbol = new PatternBySymbolResponseModel();
                        patternOnsymbol.StockCode = symbol._sc_;

                        var historiesInPeriodOfTime = historiesInPeriodOfTimeNonDB
                            .Where(ss => ss.StockSymbol == symbol._sc_)
                            .ToList();

                        var histories = historiesInPeriodOfTime
                            .OrderBy(s => s.Date)
                            .ToList();

                        var avarageOfLastXXPhien = histories.Take(soPhienGd).Sum(h => h.V) / soPhienGd;
                        if (avarageOfLastXXPhien < trungbinhGd) return;

                        for (var i = 59; i < histories.Count; i++)
                        {
                            var history = histories[i];
                            var currentDateToCheck = history.Date;
                            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

                            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                            if (lowest == null) continue;


                            var secondLowest = new StockSymbolHistory();
                            var theDaysAfterLowest = histories.Where(h => h.Date > lowest.Date)
                                .OrderBy(h => h.Date)
                                .Skip(5)
                                .ToList();

                            for (int j = 0; j < theDaysAfterLowest.Count(); j++)
                            {
                                var rangesFromLowest = theDaysAfterLowest.Where(d => d.Date > lowest.Date && d.Date == theDaysAfterLowest[j].Date).ToList();

                                var dkSub1 = rangesFromLowest.Any(r => r.C > theDaysAfterLowest[j].C);

                            }

                            foreach (var item in theDaysAfterLowest)
                            {

                            }




                            //var secondLowest = previousDaysFromCurrentDay

                            //    .OrderBy(h => h.C)
                            //    .FirstOrDefault();

                            if (secondLowest == null) continue;

                            var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(30).ToList();
                            var highest = previousDaysFromCurrentDay.OrderByDescending(h => h.C).FirstOrDefault();
                            if (highest == null) continue;


                            var dk1 = highest.C * 0.85M >= lowest.C;
                            var dk2 = history.C >= secondLowest.C * 1.02M;


                            var periodInLowestAndSecondLowest = historiesInPeriodOfTime.Where(h => h.Date > lowest.Date && h.Date < secondLowest.Date).ToList();
                            var dk3 = periodInLowestAndSecondLowest.Any(s => s.C > secondLowest.C);


                            if (dk1 && dk2 && dk3) //basically we should start buying
                            {
                                var reality = false;

                                var historyTomorrow = histories
                                    .Where(h => h.Date > history.Date)
                                    .OrderBy(h => h.Date)
                                    .FirstOrDefault();

                                if (historyTomorrow == null) continue;

                                if (historyTomorrow != null && historyTomorrow.C >= history.C)
                                    reality = true;


                                patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                                {
                                    ConditionMatchAt = currentDateToCheck,
                                    MoreInformation = new
                                    {
                                        TodayOpening = history.O,
                                        TodayClosing = history.C,
                                        TodayLowest = history.L,
                                        TodayTrading = history.V,
                                        Previous1stLowest = lowest.L,
                                        Previous1stLowestDate = lowest.Date,
                                        Previous2ndLowest = secondLowest.L,
                                        Previous2ndLowestDate = secondLowest.Date,
                                        AverageNumberOfTradingInPrevious20Times = 0,
                                        ShouldBuy = true,
                                        RealityExpectation = reality,
                                        Tomorrow = historyTomorrow?.Date,
                                        TomorrowClosing = historyTomorrow?.C
                                    }
                                });
                            }
                        }
                        if (patternOnsymbol.Details.Any())
                        {
                            var successedNumber = patternOnsymbol.Details.Count(d => d.MoreInformation.RealityExpectation == true);
                            patternOnsymbol.SuccessRate = (decimal)successedNumber / (decimal)patternOnsymbol.Details.Count();
                            result.Symbols.Add(patternOnsymbol);
                        }
                    });

                    break;
                #endregion
                default:
                    break;
            }


            var ok = result.Symbols.SelectMany(s => s.Details).Count(d => d.MoreInformation.RealityExpectation == true);
            result.SuccessRate = (decimal)ok / (decimal)result.Symbols.SelectMany(s => s.Details).Count();

            return result;
        }

    }
}
