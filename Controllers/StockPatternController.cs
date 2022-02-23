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
                        try
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

                            for (var i = 0; i < histories.Count; i++)
                            {
                                var history = histories[i];
                                var currentDateToCheck = history.Date;
                                var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();

                                //TODO: lowest & 2nd lowest can be reused to improve performance
                                var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                                if (lowest == null) continue;

                                var secondLowest = LookingForSecondLowest(histories, lowest, history);
                                if (secondLowest == null) continue;

                                var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();
                                var highest = previousDaysFromCurrentDay.OrderByDescending(h => h.C).FirstOrDefault();
                                if (highest == null) continue;


                                var dk1 = highest.C * 0.85M >= lowest.C;
                                var dk2 = history.C >= secondLowest.C * 1.02M;


                                //var periodInLowestAndSecondLowest = historiesInPeriodOfTime.Where(h => h.Date > lowest.Date && h.Date < secondLowest.Date).ToList();
                                //var dk3 = periodInLowestAndSecondLowest.Any(s => s.C > secondLowest.C);


                                if (dk1 && dk2) //basically we should start buying
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
                                if (patternOnsymbol.SuccessRate > 0)
                                    result.Symbols.Add(patternOnsymbol);
                            }
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }
                    });

                    break;
                #endregion
                default:
                    break;
            }

            result.Symbols = result.Symbols.OrderByDescending(s => s.SuccessRate).ToList();

            var ok = result.Symbols.SelectMany(s => s.Details).Count(d => d.MoreInformation.RealityExpectation == true);
            result.SuccessRate = result.Symbols.SelectMany(s => s.Details).Count() <= 0
                ? 0
                : (decimal)ok / (decimal)result.Symbols.SelectMany(s => s.Details).Count();

            return result;
        }

        private static StockSymbolHistory LookingForSecondLowest(List<StockSymbolHistory> histories, StockSymbolHistory lowest, StockSymbolHistory currentDateHistory)
        {
            var theDaysAfterLowest = histories.Where(h => h.Date > lowest.Date && h.Date <= currentDateHistory.Date)
                .OrderBy(h => h.Date)
                //.Skip(4)
                .ToList();

            if (!theDaysAfterLowest.Any()) return null;

            for (int i = 0; i < theDaysAfterLowest.Count(); i++)
            {
                if (i <= 3) continue;
                var secondLowestAssumption = theDaysAfterLowest[i];
                var rangesFromLowestTo2ndLowest = theDaysAfterLowest.Where(d => d.Date > lowest.Date && d.Date < secondLowestAssumption.Date).ToList();

                var dkSub1 = rangesFromLowestTo2ndLowest.Any(r => r.C > secondLowestAssumption.C);//at least 1 day > 2nd lowest
                if (!dkSub1) continue;

                var dkSub2 = false;//at least 1 day > next Phien from 2nd lowest
                if (i < theDaysAfterLowest.Count() - 1)
                {
                    var nextPhien = theDaysAfterLowest[i + 1];
                    dkSub2 = rangesFromLowestTo2ndLowest.Any(r => r.C > nextPhien.C) && nextPhien.C > (secondLowestAssumption.C * 1.02M);
                }

                if (dkSub1 && dkSub2)
                {
                    return secondLowestAssumption;
                }
            }

            return null;
        }

        /// <summary>
        /// Scan from the specific day back to 60 days
        /// - trung binh gd trong so phien gd > 100K
        /// - Tim Day1
        ///     Lowest price in the last 30 days
        /// - Tim Day2
        ///     - Start looking from Day1
        ///     - Skip next 4 phien
        ///     - Start from 5th phien
        ///     - if the checking date matches
        ///         + There is any date from Day1 to checking date has Close price > checking Date. Closing price
        ///         + There is any date from Day1 to checking date has Close price > next Phien from checking Date. Closing price * 1.02 (>2%)
        ///     - Then this is Day2
        /// - Anounce stock code when
        ///     var dk1 = highest.C * 0.85M >= lowest.C; (Day1 thap hon 15% so voi gia cao nhat in the last 30 days)
        ///     var dk2 = history.C >= secondLowest.C * 1.02M;  (Day1 thap hon 15% so voi gia cao nhat in the last 30 days)
        ///     var dk3 = checking Date is yesterday from today
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ngay"></param>
        /// <param name="soPhienGd"></param>
        /// <param name="trungbinhGd"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> TimDay2(string code, DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();// List<PatternDetailsResponseModel>();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var startFrom = ngay.AddDays(-60);

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
                try
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

                    var history = histories.FirstOrDefault(h => h.Date == ngay);
                    if (history == null) return;

                    var currentDateToCheck = history.Date;
                    var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();

                    //TODO: lowest & 2nd lowest can be reused to improve performance
                    var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                    if (lowest == null) return;

                    var secondLowest = LookingForSecondLowest(histories, lowest, history);
                    if (secondLowest == null) return;

                    var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();
                    var highest = previousDaysFromCurrentDay.OrderByDescending(h => h.C).FirstOrDefault();
                    if (highest == null) return;

                    var dk1 = highest.C * 0.85M >= lowest.C;
                    var dk2 = history.C >= secondLowest.C * 1.02M;
                    var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;

                    if (dk1 && dk2 && dk3) //basically we should start buying
                    {
                        //var reality = false;
                        //var historyTomorrow = histories
                        //    .Where(h => h.Date > history.Date)
                        //    .OrderBy(h => h.Date)
                        //    .FirstOrDefault();

                        //if (historyTomorrow == null) return;

                        //if (historyTomorrow != null && historyTomorrow.C >= history.C)
                        //    reality = true;


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
                                AverageNumberOfTradingInPreviousTimes = avarageOfLastXXPhien,
                                ShouldBuy = true,
                                //RealityExpectation = reality,
                                //Tomorrow = historyTomorrow?.Date,
                                //TomorrowClosing = historyTomorrow?.C
                            }
                        });
                    }

                    if (patternOnsymbol.Details.Any())
                    {
                        result.Symbols.Add(patternOnsymbol);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });

            result.Symbols = result.Symbols.OrderBy(s => s.StockCode).ToList();
            return result;
        }

        /// <summary>
        /// Đỉnh 1 là giá cao nhất trong vòng 30 tuần đổ lại
        /// Đỉnh 2 là giá cao thứ 2 trong vòng 30 tuần đổ lại và cách đỉnh 1 ít nhất 5 tuần
        /// Đỉnh sẽ được xác nhận là tồn tại nếu khoảng cách chênh lệch của điểm gọi là đỉnh > điểm gọi là đáy trước đó ít nhất 7%
        /// Cách xác định điểm gọi là đáy trước đó
        /// - tính từ 30 ngày trở về quá khứ từ điểm gọi là đỉnh
        /// - tìm giá thấp nhất
        /// - áp dụng cho cả 2 đỉnh 1 và 2 để tìm đáy gần nhất trước đó
        /// Nối 2 đỉnh và tạo thành 1 đường thẳng có phương trình ax + by = c(x là số ngày tính từ đỉnh 2 trở lại quá khứ, y là giá cao nhất ở đỉnh 1 - giá cao nhất ở đỉnh 2), nếu có bất kì ngày nào mà giá đóng của thỏa phương trình này thì báo gợi ý mua
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ngay"></param>
        /// <param name="soPhienGd"></param>
        /// <param name="trungbinhGd"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> TimTrendGiam(string code, DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesInPeriodOfTimeNonDB = await GetStockDataByWeek();

            Parallel.ForEach(symbols, symbol =>
            {
                try
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
                    
                    var day1 = GetLowestStockDataByWeek(historiesInPeriodOfTime, symbol._sc_, new DateTime(1970, 1, 1), DateTime.Now);
                    if (day1 == null) return;
                    var dinh1 = GetHigestStockDataByWeek(historiesInPeriodOfTime, symbol._sc_, day1.Date, DateTime.Now);
                    if (dinh1 == null) return;

                    var day2 = GetLowestStockDataByWeek(historiesInPeriodOfTime, symbol._sc_, dinh1.Date, DateTime.Now);
                    if (day2 == null) return;
                    var dinh2 = GetHigestStockDataByWeek(historiesInPeriodOfTime, symbol._sc_, day2.Date, DateTime.Now);
                    if (dinh2 == null) return;

                    var indexOfDinh2 = historiesInPeriodOfTime.IndexOf(dinh2);
                    var indexOfDinh1 = historiesInPeriodOfTime.IndexOf(dinh1);
                    
                    var a = indexOfDinh2 - indexOfDinh1;
                    var b = dinh1.H - dinh2.H;

                    var formula = "y = ax / -b";

                    var thisWeek = histories.FirstOrDefault();
                    if (thisWeek == null) return;

                    var distanceFrom0ToThisWeekX = historiesInPeriodOfTime.IndexOf(thisWeek) - indexOfDinh2;    //we need a positive number
                    var distanceFrom0ToThisWeekY = thisWeek.H - dinh2.H;                                        //we need a negative number

                    var expectedY = a * distanceFrom0ToThisWeekX / (-b);
                    var dk1 = thisWeek.L <= expectedY && expectedY <= thisWeek.H;

                    //var currentWeekToCheck = history.Date;



                    if (dk1) //basically we should start buying
                    {
                        patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                        {
                            ConditionMatchAt = thisWeek.Date,
                            MoreInformation = new
                            {
                                TodayOpening = thisWeek.O,
                                TodayClosing = thisWeek.C,
                                TodayLowest = thisWeek.L,
                                TodayTrading = thisWeek.V,
                                Day1 = day1.L,
                                Day1Date = day1.Date,
                                Day2 = day2.L,
                                Day2Date = day2.Date,
                                Dinh1 = dinh1.H,
                                Dinh1Date = dinh1.Date,
                                Dinh2 = dinh2.H,
                                Dinh2Date = dinh2.Date,
                                AverageNumberOfTradingInPreviousTimes = avarageOfLastXXPhien,
                                ShouldBuy = true,
                                //RealityExpectation = reality,
                                //Tomorrow = historyTomorrow?.Date,
                                //TomorrowClosing = historyTomorrow?.C
                            }
                        });
                    }

                    if (patternOnsymbol.Details.Any())
                    {
                        result.Symbols.Add(patternOnsymbol);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });

            result.Symbols = result.Symbols.OrderBy(s => s.StockCode).ToList();
            return result;
        }

        public PatternWeekResearchModel GetLowestStockDataByWeek(List<PatternWeekResearchModel> stockDataByWeek, string stockCode, DateTime fromDate, DateTime toDate)
        {
            return stockDataByWeek.Where(s => s.StockSymbol == stockCode && s.Date > fromDate && s.Date < toDate).OrderBy(s => s.L).FirstOrDefault();
        }

        public PatternWeekResearchModel GetHigestStockDataByWeek(List<PatternWeekResearchModel> stockDataByWeek, string stockCode, DateTime fromDate, DateTime toDate)
        {
            var day1 = GetLowestStockDataByWeek(stockDataByWeek, stockCode, fromDate, toDate); //new DateTime(1970, 1, 1));

            var dinh1 = stockDataByWeek.Where(s => s.StockSymbol == stockCode).OrderByDescending(s => s.H).First();

            if (dinh1 != null && dinh1.H > day1.L * 1.07M) return dinh1;

            return null;
        }

        public PatternWeekResearchModel Get2ndHigestStockDataByWeek(List<PatternWeekResearchModel> stockDataByWeek, string stockCode, DateTime fromDate, DateTime toDate)
        {
            var newData = stockDataByWeek.Where(s => s.Date > fromDate && s.StockSymbol == stockCode).OrderBy(s => s.Date).Skip(4).ToList();

            return GetLowestStockDataByWeek(newData, stockCode, fromDate, toDate);
        }

        public async Task<List<PatternWeekResearchModel>> GetStockDataByWeek(int? weekRange = 30)
        {
            var dates = await _context.StockSymbolHistory.OrderByDescending(s => s.Date)
                .Where(s => s.StockSymbol == "VIC" && s.Date > DateTime.Today.AddDays((weekRange.Value * 7) * -1))
                .ToListAsync();

            var newShorted = dates.OrderBy(h => h.Date).ToList();
            var minWeekIndex = newShorted[0].Date.GetIso8601WeekOfYear();

            var numberOfremovedItems = 0;
            for (int i = 1; i < newShorted.Count; i++)
            {
                var weeKIndex = newShorted[i].Date.GetIso8601WeekOfYear();
                numberOfremovedItems = i;

                if (weeKIndex > minWeekIndex)
                    break;
            }

            var newListOfDates = newShorted.Skip(numberOfremovedItems).ToList();

            var historyWithWeek = new List<PatternWeekResearchModel>();
            foreach (var history in dates)
            {
                var weekIndex = history.Date.GetIso8601WeekOfYear();
                historyWithWeek.Add(new PatternWeekResearchModel { Week = weekIndex, O = history.O, C = history.C, H = history.H, L = history.L, StockSymbol = history.StockSymbol, Date = history.Date });
            }

            var res1 = historyWithWeek.GroupBy(h => h.Week).ToDictionary(h => h.Key, h => h.ToList());
            var res2 = new List<PatternWeekResearchModel>();

            foreach (var historyGroupedByWeek in res1)
            {

                var week = historyGroupedByWeek.Key;
                var o = historyGroupedByWeek.Value.OrderBy(h => h.Date).First().O;
                var l = historyGroupedByWeek.Value.OrderBy(h => h.L).First().L;
                var h = historyGroupedByWeek.Value.OrderBy(h => h.H).Last().H;
                var c = historyGroupedByWeek.Value.OrderBy(h => h.Date).Last().C;
                var v = historyGroupedByWeek.Value.Sum(h => h.V) / historyGroupedByWeek.Value.Count;
                res2.Add(new PatternWeekResearchModel
                {
                    Week = week,
                    O = o,
                    C = c,
                    H = h,
                    L = l,
                    V = v,
                    StockSymbol = historyGroupedByWeek.Value[0].StockSymbol,
                    Date = historyGroupedByWeek.Value[0].Date
                });
            }

            return res2;
        }
    }
}
