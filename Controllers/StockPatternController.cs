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
        public async Task<PatternResponseModel> Pattern(string pattern)
        {
            var result = new PatternResponseModel();// List<PatternDetailsResponseModel>();


            //if (string.IsNullOrWhiteSpace(pattern))
            //{
            //    return null;
            //}

            //var todo = await _context.StockSymbol.FirstOrDefaultAsync(m => m._sc_ == pattern);
            //if (todo == null)
            //{
            //    return null;
            //}

            pattern = "";
            switch (pattern)
            {
                case "":
                    //DK 1: Lowest today < Lowest in the last XXX day && Lowest today < 2nd Lowest in the last XXX day
                    //DK 2: C > L * 0.02
                    //DK3 = C <= O * 0.03;
                    //DK4 = V > MA(V, 21) * 1.5;
                    //Filter: DK1 && DK2 && DK3 && DK4

                    result.PatternName = "aL - Pattern 1";
                    //var symbols = await _context.StockSymbol.Where(s => s._sc_ == "VIC").ToListAsync();

                    var symbols = await _context.StockSymbol.ToListAsync();
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

                            var firstCondition = history.L < lowest.L && history.L < secondLowest.L;
                            var secondCondition = history.C > history.L * 0.02M;
                            var thirdCondition = (history.O - history.O * 0.03M) <= history.C && history.C <= (history.O + (history.O * 0.03M));
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
                default:
                    break;
            }


            var ok = result.Symbols.SelectMany(s => s.Details).Count(d => d.MoreInformation.RealityExpectation == true);
            result.SuccessRate = (decimal)ok / (decimal)result.Symbols.SelectMany(s => s.Details).Count();

            return result;
        }

    }
}
