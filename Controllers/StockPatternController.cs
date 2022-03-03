﻿using System;
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
using DotNetCoreSqlDb.Models.Business.Report;
using DotNetCoreSqlDb.Models.Business.Report.Implementation;

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
        public async Task<ReportModel> Pattern(string code, DateTime startFrom, int soPhienGd, int trungbinhGd)
        {
            var result = new ReportModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            //var startFrom = "2020-01-02 00:00:00.0000000"

            /* 
             * lay gia C de tinh day
             * lay 30 phien cuoi cung
             * 2 day cach nhau > 5 phien
             * day 1 se cach dinh? cu~ (trong vong 1 thang) > 15% tro len 
             * day 2 >= day 1 
             * day 1 luon luon nam truoc day 2 
             * ngay hien tai C > 2% so voi day 2 (ko bao gio > 7%)
            */

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesInPeriodOfTimeByStockCode = await _context.StockSymbolHistory
                    .Where(ss =>
                        stockCodes.Contains(ss.StockSymbol)
                        && ss.Date >= startFrom
                        )
                    .OrderByDescending(ss => ss.Date)
                    .ToListAsync();

            var dates = await _context.StockSymbolHistory.OrderByDescending(s => s.Date)
                .Where(s => stockCodes.Contains(s.StockSymbol) && s.Date > startFrom.AddDays((30 * 8) * -1))
                .ToListAsync();

            Parallel.ForEach(symbols, symbol =>
            {
                try
                {
                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;

                    var historiesByStockCode = historiesInPeriodOfTimeByStockCode
                        .Where(ss => ss.StockSymbol == symbol._sc_)
                        .ToList();

                    var orderedHistory = historiesByStockCode
                        .OrderBy(s => s.Date)
                        .ToList();

                    for (int i = 0; i < orderedHistory.Count; i++)
                    {
                        var history = orderedHistory[i];
                        if (history.Date < startFrom) continue;

                        var avarageOfLastXXPhien = history.MA(orderedHistory, -soPhienGd);
                        if (avarageOfLastXXPhien < trungbinhGd) continue;

                        //var historiesUntilCheckingDate = orderedHistory.Where(h => h.Date < history.Date).ToList();

                        var formular1TimTrendGiam = new ReportFormular1TimTrendGiam().Calculation(symbol._sc_, history.Date, dates, null);
                        var formular2TimDay2 = new ReportFormular2TimDay2().Calculation(symbol._sc_, history.Date, orderedHistory, null);
                        var formular3KLGDTangDotBien = new ReportFormular3TangDotBien().Calculation(symbol._sc_, history.Date, null, null);
                        var formular4GiaXuongDay = new ReportFormular4GiaXuongDay().Calculation(symbol._sc_, history.Date, orderedHistory, null);

                        ReportStockModel stockModel = new ReportStockModel();
                        stockModel.Date = history.Date;
                        stockModel.StockCode = symbol._sc_;

                        stockModel.Formulars
                            .Plus(formular1TimTrendGiam)
                            .Plus(formular2TimDay2)
                            .Plus(formular3KLGDTangDotBien)
                            .Plus(formular4GiaXuongDay);

                        result.Stocks.Add(stockModel);

                    }
                }
                catch (Exception ex) { }

            });

            return result;
        }


        private StockSymbolHistory LookingForSecondLowest(List<StockSymbolHistory> histories, StockSymbolHistory lowest, StockSymbolHistory currentDateHistory)
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
                    var dk4 = lowest.C * 1.15M >= secondLowest.C;

                    if (dk1 && dk2 && dk3 && dk4) //basically we should start buying
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
                                Text = @$"{history.StockSymbol}: Đáy 1 {lowest.Date.ToShortDateString()}: {lowest.C},
                                        Đáy 2 {secondLowest.Date.ToShortDateString()}: {secondLowest.C},
                                        Giá đóng cửa hum nay ({history.C}) cao hơn giá đóng của đáy 2 {secondLowest.C},
                                        Đỉnh trong vòng 30 ngày ({highest.C}) giảm 15% ({highest.C * 0.85M}) vẫn cao hơn giá đóng cửa của đáy 1 {lowest.C},
                                        Giữa đáy 1 và đáy 2, có giá trị cao hơn đáy 2 và giá đóng cửa ngày hum nay ít nhất 2%",
                                TodayOpening = history.O,
                                TodayClosing = history.C,
                                TodayLowest = history.L,
                                TodayTrading = history.V,
                                Previous1stLowest = lowest.C,
                                Previous1stLowestDate = lowest.Date,
                                Previous2ndLowest = secondLowest.C,
                                Previous2ndLowestDate = secondLowest.Date,
                                AverageNumberOfTradingInPreviousTimes = avarageOfLastXXPhien,
                                RealityExpectation = string.Empty,
                                ShouldBuy = true
                            }
                        });
                    }

                    if (patternOnsymbol.Details.Any())
                    {
                        result.TimDay2.Items.Add(patternOnsymbol);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });

            result.TimDay2.Items = result.TimDay2.Items.OrderBy(s => s.StockCode).ToList();

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
        /// <param name="ngay">Co the 1 ngay bat ki trong qua khu hoac ngay hien tai - neu kiemTraTiLe == true -> ngay bat dau</param>
        /// <param name="soPhienGd"></param>
        /// <param name="trungbinhGd"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> TimTrendGiam(string code, bool kiemTraTiLe, DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var allHistories = await GetStockDataByWeekFromNgay(stockCodes, ngay);
            var historiesInPeriodOfTimeNonDB = kiemTraTiLe
                ? allHistories
                : allHistories.Where(h => h.DateInWeek <= ngay).ToList();

            Parallel.ForEach(symbols, symbol =>
            {
                try
                {
                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;

                    var historiesByStockCode = historiesInPeriodOfTimeNonDB
                        .Where(ss => ss.StockSymbol == symbol._sc_)
                        .ToList();

                    var histories = historiesByStockCode
                        .OrderBy(s => s.Date)
                        .ToList();

                    for (int i = 0; i < histories.Count; i++)
                    {
                        var historyByWeek = histories[i];
                        if (historyByWeek.DateInWeek < ngay) continue;

                        var historiesByStockCodeUntilDate = historiesByStockCode.Where(h => h.DateInWeek < historyByWeek.DateInWeek).OrderByDescending(h => h.DateInWeek).Take(30).ToList();


                        var avarageOfLastXXPhien = historyByWeek.MA(histories, -soPhienGd);
                        if (avarageOfLastXXPhien < trungbinhGd) continue;

                        var dinh1 = GetHigestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek);
                        if (dinh1 == null) continue;

                        var day1 = GetLowestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek, dinh1);
                        if (day1 == null) continue;

                        var dinh2 = GetHigestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek, day1);
                        if (dinh2 == null) continue;

                        var day2 = GetLowestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek, dinh2);
                        if (day2 == null) continue;


                        var indexOfDinh2 = historiesByStockCodeUntilDate.IndexOf(dinh2);
                        var indexOfDinh1 = historiesByStockCodeUntilDate.IndexOf(dinh1);

                        var a = indexOfDinh2 - indexOfDinh1;
                        var b = dinh1.C - dinh2.C;

                        var formula = "y = ax / -b";

                        //var thisWeek = histories.OrderByDescending(h => h.Date).FirstOrDefault();
                        //if (thisWeek == null) continue;

                        var distanceFrom0ToThisWeekX = historiesByStockCodeUntilDate.IndexOf(historyByWeek) - indexOfDinh2;    //we need a positive number
                        var distanceFrom0ToThisWeekY = historyByWeek.C - dinh2.C;                                        //we need a negative number

                        var expectedY = b * distanceFrom0ToThisWeekX / (-a);
                        var dk1 = (historyByWeek.C - dinh2.C) > expectedY;

                        var dk2 = dinh1.C > dinh2.C;
                        var dk3 = dinh2.C > dinh1.C * 0.93M;
                        if (dk1 && dk2 && dk3) //basically we should start buying
                        {
                            var ma5next = historyByWeek.MA(histories, 5);

                            patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                            {
                                ConditionMatchAt = historyByWeek.Date,
                                MoreInformation = new
                                {
                                    TodayOpening = historyByWeek.O,
                                    TodayClosing = historyByWeek.C,
                                    TodayLowest = historyByWeek.C,
                                    TodayTrading = historyByWeek.V,
                                    Day1 = day1.C,
                                    Day1Date = day1.Date,
                                    Day2 = day2.C,
                                    Day2Date = day2.Date,
                                    Dinh1 = dinh1.C,
                                    Dinh1Date = dinh1.Date,
                                    Dinh2 = dinh2.C,
                                    Dinh2Date = dinh2.Date,
                                    AverageNumberOfTradingInPreviousTimes = avarageOfLastXXPhien,
                                    ShouldBuy = true,
                                    RealityExpectation = ma5next == 0
                                        ? string.Empty
                                        : historyByWeek.C < ma5next
                                            ? "true"
                                            : "false",
                                    Ma5WeekNext = ma5next,
                                }
                            });
                        }
                    }

                    if (patternOnsymbol.Details.Any())
                    {
                        result.TimTrendGiam.Items.Add(patternOnsymbol);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });

            result.TimTrendGiam.Items = result.TimTrendGiam.Items.OrderBy(s => s.StockCode).ToList();
            return result;
        }

        public PatternWeekResearchModel GetLowestStockDataByWeek(List<PatternWeekResearchModel> stockDataByWeek, string stockCode, DateTime toDate, PatternWeekResearchModel? dinhTruoc = null)
        {
            var day = new PatternWeekResearchModel();
            if (dinhTruoc != null)
            {
                var newOrder = stockDataByWeek.Where(s => s.StockSymbol == stockCode && s.Date > dinhTruoc.Date).OrderBy(s => s.Date).ToList();

                return newOrder.Where(s => s.Date < toDate.AddDays(-4)).OrderBy(s => s.C).FirstOrDefault();
            }
            else
            {
                return stockDataByWeek.Where(s => s.StockSymbol == stockCode && s.Date < toDate.AddDays(-4)).OrderBy(s => s.C).FirstOrDefault();
            }

            //return stockDataByWeek.Where(s => s.StockSymbol == stockCode && s.Date > fromDate && s.Date < toDate.AddDays(-4)).OrderBy(s => s.L).FirstOrDefault();
        }

        public PatternWeekResearchModel GetHigestStockDataByWeek(List<PatternWeekResearchModel> stockDataByWeek, string stockCode, DateTime toDate, PatternWeekResearchModel? dayTruoc = null)
        {
            var dinh = new PatternWeekResearchModel();
            if (dayTruoc != null)
            {
                var newOrder = stockDataByWeek.Where(s => s.StockSymbol == stockCode && s.Date > dayTruoc.Date).OrderBy(s => s.Date).ToList();

                dinh = newOrder.Where(s => s.Date < toDate.AddDays(-4)).OrderByDescending(s => s.C).FirstOrDefault();
            }
            else
            {
                var ordered = stockDataByWeek.Where(s => s.StockSymbol == stockCode && s.Date < toDate.AddDays(-4)).OrderByDescending(s => s.C).ToList();
                dinh = ordered.FirstOrDefault();
            }

            if (dinh != null && dayTruoc == null) return dinh;

            if (dinh != null && dinh.C > dayTruoc.C * 1.15M) return dinh;
            return null;
        }

        public async Task<List<PatternWeekResearchModel>> GetStockDataByWeekFromNgay(List<string> symbols, DateTime ngayKiemTra, int? weekRange = 30)
        {
            var dates = await _context.StockSymbolHistory.OrderByDescending(s => s.Date)
                .Where(s => symbols.Contains(s.StockSymbol) && s.Date > ngayKiemTra.AddDays((weekRange.Value * 8) * -1))
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

            var currentDate = dates[0].Date;
            var currentWeek = dates[0].Date.GetIso8601WeekOfYear();

            foreach (var history in dates)
            {
                var weekIndex = history.Date.GetIso8601WeekOfYear();

                historyWithWeek.Add(new PatternWeekResearchModel
                {
                    Week = weekIndex,
                    O = history.O,
                    C = history.C,
                    H = history.H,
                    L = history.L,
                    StockSymbol = history.StockSymbol,
                    Date = history.Date,
                    V = history.V,
                    DateInWeek = currentWeek == weekIndex
                        ? currentDate
                        : history.Date
                });

                if (currentWeek != weekIndex)
                {
                    currentDate = history.Date;
                    currentWeek = weekIndex;
                }
            }

            var result = new List<PatternWeekResearchModel>();
            var historyGroupedByStockCodes = historyWithWeek.GroupBy(h => h.StockSymbol).ToDictionary(h => h.Key, h => h.ToList());

            foreach (var historyGroupedByStockCode in historyGroupedByStockCodes)
            {
                var historyGroupedByWeeks = historyGroupedByStockCode.Value.GroupBy(h => h.DateInWeek).ToDictionary(h => h.Key, h => h.ToList());
                foreach (var historyGroupedByWeek in historyGroupedByWeeks)
                {
                    var stockSymbol = historyGroupedByStockCode.Key;
                    var week = historyGroupedByWeek.Key.GetIso8601WeekOfYear();
                    var o = historyGroupedByWeek.Value.OrderBy(h => h.Date).First().O;
                    var l = historyGroupedByWeek.Value.OrderBy(h => h.L).First().L;
                    var h = historyGroupedByWeek.Value.OrderBy(h => h.H).Last().H;
                    var c = historyGroupedByWeek.Value.OrderBy(h => h.Date).Last().C;
                    var v = historyGroupedByWeek.Value.Sum(h => h.V) / historyGroupedByWeek.Value.Count;
                    result.Add(new PatternWeekResearchModel
                    {
                        Week = week,
                        O = o,
                        C = c,
                        H = h,
                        L = l,
                        V = v,
                        StockSymbol = stockSymbol,
                        Date = historyGroupedByWeek.Value.OrderBy(h => h.Date).First().Date,
                        DateInWeek = historyGroupedByWeek.Value.OrderBy(h => h.Date).First().DateInWeek
                    });
                }
            }

            result = result.OrderBy(r => r.StockSymbol).ThenBy(r => r.Date).ToList();

            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="fromDate"></param>
        /// <param name="t">t+1, 2, 3</param>
        /// <param name="soPhienGd"></param>
        /// <param name="trungbinhGd"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> PredictDataByTimDay2(string code, DateTime fromDate, int t, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();
            var historiesInPeriodOfTimeNonDB = await _context.StockSymbolHistory
                    .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= fromDate)
                    .OrderByDescending(ss => ss.Date)
                    .ToListAsync();

            Parallel.ForEach(symbols, symbol =>
            {
                try
                {
                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;

                    var historiesInPeriodOfTime = historiesInPeriodOfTimeNonDB
                        .Where(ss => ss.StockSymbol == symbol._sc_)
                        .ToList();

                    var allHistories = historiesInPeriodOfTime
                        .OrderBy(s => s.Date)
                        .ToList();

                    for (int i = 0; i < allHistories.Count; i++)
                    {
                        if (i <= soPhienGd) continue;

                        var histories = allHistories.Where(h => h.Date <= allHistories[i].Date).ToList();
                        var ngay = allHistories[i].Date;


                        var avarageOfLastXXPhien = histories.OrderByDescending(h => h.Date).Take(soPhienGd).Sum(h => h.V) / soPhienGd;
                        if (avarageOfLastXXPhien < trungbinhGd) continue;

                        var history = histories.FirstOrDefault(h => h.Date == ngay);
                        if (history == null) continue;

                        var currentDateToCheck = history.Date;
                        var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();

                        //TODO: lowest & 2nd lowest can be reused to improve performance
                        var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                        if (lowest == null) continue;

                        var secondLowest = LookingForSecondLowest(histories, lowest, history);
                        if (secondLowest == null) continue;

                        var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();
                        var highest1 = previousDaysFromCurrentDay.OrderByDescending(h => h.C).FirstOrDefault();
                        if (highest1 == null) continue;

                        var dk1 = highest1.C * 0.85M >= lowest.C;
                        var dk2 = history.C > secondLowest.C;/*history.C >= secondLowest.C * 1.02M;*/
                        var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
                        var dk4 = lowest.C * 1.15M >= secondLowest.C;

                        //var dinh1TruocDay1 = allHistories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(30).OrderByDescending(h => h.C).FirstOrDefault();

                        var dinh2SauDay1 = allHistories.Where(h => h.Date > lowest.Date && h.Date < secondLowest.Date).OrderByDescending(h => h.C).FirstOrDefault();
                        var dk5 = dinh2SauDay1.C <= highest1.C * 0.93M;

                        var vtrungBinh20PhienGanNhat = histories.Where(h => h.Date < ngay).OrderByDescending(h => h.Date).Take(20).Sum(h => h.V) / 20;
                        var dk6 = history.V > vtrungBinh20PhienGanNhat;

                        var giatrungBinh20PhienGanNhat = histories.Where(h => h.Date < ngay).OrderByDescending(h => h.Date).Take(20).Sum(h => h.C) / 20;
                        var dk7 = history.C > giatrungBinh20PhienGanNhat;

                        if (dk1 && dk2 && dk3 && dk4 && dk5 && dk6 && dk7) //basically we should start buying
                        {
                            var reality = "false";
                            var historySellingTime = allHistories.Where(h => h.Date > history.Date).OrderBy(h => h.Date).Skip(t).FirstOrDefault();

                            if (historySellingTime == null) continue;

                            if (historySellingTime != null && historySellingTime.C >= history.C)
                                reality = "true";


                            patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                            {
                                ConditionMatchAt = currentDateToCheck,
                                MoreInformation = new
                                {
                                    Text = @$"{history.StockSymbol}: Đáy 1 {lowest.Date.ToShortDateString()}: {lowest.C},
                                        Đáy 2 {secondLowest.Date.ToShortDateString()}: {secondLowest.C},
                                        Giá đóng cửa hum nay ({history.C}) cao hơn giá đóng của đáy 2 {secondLowest.C},
                                        Đỉnh trong vòng 30 ngày ({highest1.C}) giảm 15% ({highest1.C * 0.85M}) vẫn cao hơn giá đóng cửa của đáy 1 {lowest.C},
                                        Giữa đáy 1 và đáy 2, có giá trị cao hơn đáy 2 và giá đóng cửa ngày hum nay ít nhất 2%",
                                    TodayOpening = history.O,
                                    TodayClosing = history.C,
                                    TodayLowest = history.L,
                                    TodayTrading = history.V,
                                    Previous1stLowest = lowest.C,
                                    Previous1stLowestDate = lowest.Date,
                                    Previous2ndLowest = secondLowest.C,
                                    Previous2ndLowestDate = secondLowest.Date,
                                    AverageNumberOfTradingInPreviousTimes = avarageOfLastXXPhien,
                                    RealityExpectation = reality,
                                    SellingPoint = historySellingTime?.Date,
                                    SellingPointClosing = historySellingTime?.C
                                }
                            });
                        }
                    }

                    if (patternOnsymbol.Details.Any())
                    {
                        result.TimDay2.Items.Add(patternOnsymbol);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });

            result.TimDay2.Items = result.TimDay2.Items.OrderByDescending(s => s.SuccessRate).ToList();

            foreach (var item in result.TimDay2.Items)
            {
                var failedItems = item.Details.Where(d => d.MoreInformation.RealityExpectation == "false").ToList();
                foreach (var failedItem in failedItems)
                {
                    result.TimDay2.FailedItems.Add(new PatternIsFailedBySymbolResponseModel
                    {
                        Date = failedItem.ConditionMatchAt,
                        StockCode = item.StockCode
                    });
                }
            }

            return result;
        }


        /// <summary>
        /// /*
        /// * dk1: ma10 cua phien hien tai tang so voi ma10 cua phien truoc
        /// * dk2: ma10 cua phien hien tai > m20 cua phien hien tai
        /// * dk3: ma20 cua phien hien tai  > m50 cua phien hien tai
        /// * dk4: ma20 va m50 cua phien hien tai deu tang so voi m20 va m50 cua phien truoc
        /// * dk5: gia dong cua cua phien hien tai<ma50* 1.07
        /// * dk6: gia dong cua cua phien hien tai> ma50 * 1.15
        /// * Mua: 
        /// *  - Dat dk 1, 2, 3, 4, 5
        /// *  - Xac Nhan: 
        /// *      + True: ma cua 10 phien tiep theo > gia dog cua cua phien hien tai
        /// *      + ELse: False
        /// * Ban: 
        /// *  - dk6
        /// *  - Xac Nhan: 
        /// *      + True: ma cua 10 phien tiep theo<gia dog cua cua phien hien tai
        /// *      + ELse: False
        /// */
        /// </summary>
        /// <param name="code"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> PredictDataMuaVaBan(string code, DateTime fromDate)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();
            var historiesInPeriodOfTimeNonDB = await _context.StockSymbolHistory
                    .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= fromDate.AddDays(-100))
                    .OrderByDescending(ss => ss.Date)
                    .ToListAsync();

            var mua = new PatternSellAndBuyBySymbolDetailResponseModel();
            var ban = new PatternSellAndBuyBySymbolDetailResponseModel();

            result.BuyAndSell.Sell = ban;
            result.BuyAndSell.Buy = mua;

            Parallel.ForEach(symbols, symbol =>
            {
                try
                {
                    var patternOnsymbolOnBuy = new PatternBySymbolResponseModel();
                    var patternOnsymbolOnSell = new PatternBySymbolResponseModel();

                    var historiesInPeriodOfTime = historiesInPeriodOfTimeNonDB
                        .Where(ss => ss.StockSymbol == symbol._sc_)
                        .ToList();
                    var allHistories = historiesInPeriodOfTime
                        .OrderBy(s => s.Date)
                        .ToList();

                    var fromDateHistory = allHistories.FirstOrDefault(h => h.Date.Date >= fromDate);
                    if (fromDateHistory == null) return;

                    var fromDateIndex = allHistories.IndexOf(fromDateHistory);

                    for (int i = 0; i < allHistories.Count; i++)
                    {
                        var history = allHistories[i];
                        if (history.Date < fromDate || i < 1) continue;

                        var vol20 = history.VOL(allHistories, -20);
                        if (vol20 < 100000) continue;

                        var ma10 = history.MA(allHistories, -10);
                        var ma20 = history.MA(allHistories, -20);
                        var ma50 = history.MA(allHistories, -50);


                        var lastTrade = allHistories[i - 1];
                        var ma10Yesterday = lastTrade.MA(allHistories, -10);
                        var ma20Yesterday = lastTrade.MA(allHistories, -20);
                        var ma50Yesterday = lastTrade.MA(allHistories, -50);

                        var ma10Future = history.MA(allHistories, 10);
                        //if (ma10Future == 0) continue; //Consider continue or break....

                        var dk1 = ma10 > ma10Yesterday;
                        var dk2 = ma10 > ma20;
                        var dk3 = ma20 > ma50;
                        var dk4 = ma20 > ma20Yesterday && ma50 > ma50Yesterday;
                        var dk5 = history.C < ma50 * 1.07M;
                        var dk6 = history.C > ma50 * 1.15M;

                        var willBuy = dk1 && dk2 && dk3 && dk4 && dk5;
                        var willSell = dk6;

                        if (!willSell && !willBuy) continue;

                        var reality = string.Empty;

                        if (willBuy)
                        {
                            reality = ma10Future == 0
                                ? string.Empty
                                : ma10Future > history.C ? "true" : "false";
                            patternOnsymbolOnBuy.StockCode = symbol._sc_;
                            patternOnsymbolOnBuy.Details.Add(new PatternDetailsResponseModel
                            {
                                ConditionMatchAt = history.Date,
                                MoreInformation = new
                                {
                                    ma10 = ma10,
                                    ma20 = ma20,
                                    ma50 = ma50,
                                    ma10Last = ma10Yesterday,
                                    ma20Last = ma20Yesterday,
                                    ma50Last = ma50Yesterday,
                                    ma10Next = ma10Future,
                                    TodayClosing = history.C,
                                    RealityExpectation = reality,
                                }
                            });
                        }

                        if (willSell)
                        {
                            reality = ma10Future == 0
                                ? string.Empty
                                : ma10Future < history.C ? "true" : "false";
                            patternOnsymbolOnSell.StockCode = symbol._sc_;
                            patternOnsymbolOnSell.Details.Add(new PatternDetailsResponseModel
                            {
                                ConditionMatchAt = history.Date,
                                MoreInformation = new
                                {
                                    ma10 = ma10,
                                    ma20 = ma10,
                                    ma50 = ma10,
                                    ma10Last = ma10Yesterday,
                                    ma20Last = ma10Yesterday,
                                    ma50Last = ma10Yesterday,
                                    ma10Next = ma10Future,
                                    TodayClosing = history.C,
                                    RealityExpectation = reality,
                                }
                            });
                        }
                    }


                    if (patternOnsymbolOnBuy.Details.Any())
                    {
                        mua.Items.Add(patternOnsymbolOnBuy);
                    }
                    if (patternOnsymbolOnSell.Details.Any())
                    {
                        ban.Items.Add(patternOnsymbolOnSell);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });

            return result;
        }


        public async Task<PatternResponseModel> FollowUpStock()
        {
            /*
             * Thanh khoản tăng, giá giảm: Nguy hiểm
             * Thanh khoản tăng, giá không tăng: Sắp có điều chỉnh
             * Thanh khoản giảm, giá giảm: bán
             * Thanh khoản giản, giá không giảm: mua -> DGC ngày 13 tháng 12 2021
             * Thanh khoản tốt, giá tăng: nắm giữ
             * Tăng đột biến?
             */

            return null;
        }

        public async Task<PatternResponseModel> FollowUpPriceInDayMarkTangDotBien(string code)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            /*
             * Dk1 == true
             *  - send email
             * 
             * Dk1:
             *  + khớp lệnh hiện tại là 1 trong 5 khớp lệnh với số lượng cp mua cao nhất đột biến
             *      - Thế nào là "cao nhất đột biến"
             *          + tính từ lần cuối cùng kiểm tra tới hiện tại, có bất kì giao dịch nào có lượng cp khớp  > trung bình cộng của tất cả lượng cp trung bình trong 5 phiên gần nhất ít nhất 50 lần
             *              - Thế nào là "cp trung bình"
             *                  + cổ phiếu không được tính là "giao dịch đột biến"
             *                  + "giao dịch đột biến": là giao dịch > so với trung bình cộng của cp trung bình trong ngày trong những lần giao dịch trước đó ít nhất 50 lần
             *                  
             * select DATEADD(DAY, -1, '9/1/2011 20:08:20')                 
             *                  
             *                  
             */

            var last5Phien = _context.StockSymbolHistory.Where(s => s.StockSymbol == "CEO").OrderByDescending(s => s.Date).Take(5).Last();

            var historiesInPeriodOfTimeNonDB = await _context.StockSymbolTradingHistory
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date > last5Phien.Date && !ss.IsTangDotBien)
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var lstDotBien = new List<string>();

            Parallel.ForEach(symbols, symbol =>
            {
                try
                {
                    var historiesInPeriodByStockCode = historiesInPeriodOfTimeNonDB
                        .Where(ss => ss.StockSymbol == symbol._sc_)
                        .ToList();
                    var allHistories = historiesInPeriodByStockCode
                        .OrderBy(s => s.Date)
                        .ToList();

                    var lstDays = allHistories.Select(h => h.Date.WithoutHours()).Distinct().ToList();

                    for (int i = 0; i < lstDays.Count; i++)
                    {
                        var day = lstDays[i];

                        var tradingsHistoryInSpecificDate = allHistories.Where(h => h.Date > day && h.Date < day.AddDays(1).WithoutHours())
                            .OrderBy(h => h.Date)
                            .ToList();

                        for (int j = 0; j < tradingsHistoryInSpecificDate.Count; j++)
                        {
                            tradingsHistoryInSpecificDate.TimDiemTangGiaDotBien(tradingsHistoryInSpecificDate[j]);
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });




            return null;
            //var historiesInPeriodOfTimeNonDB = await _context.StockSymbolHistory
            //        .Where(ss => stockCodes.Contains(ss.StockSymbol))
            //        .OrderByDescending(ss => ss.Date)
            //        .ToListAsync();

            //var mua = new PatternSellAndBuyBySymbolDetailResponseModel();
            //var ban = new PatternSellAndBuyBySymbolDetailResponseModel();

            //result.BuyAndSell.Sell = ban;
            //result.BuyAndSell.Buy = mua;

            //Parallel.ForEach(symbols, symbol =>
            //{
            //    try
            //    {
            //        var patternOnsymbolOnBuy = new PatternBySymbolResponseModel();
            //        var patternOnsymbolOnSell = new PatternBySymbolResponseModel();

            //        var historiesInPeriodOfTime = historiesInPeriodOfTimeNonDB
            //            .Where(ss => ss.StockSymbol == symbol._sc_)
            //            .ToList();
            //        var allHistories = historiesInPeriodOfTime
            //            .OrderBy(s => s.Date)
            //            .ToList();

            //        var fromDateHistory = allHistories.FirstOrDefault(h => h.Date.Date >= fromDate);
            //        if (fromDateHistory == null) return;

            //        var fromDateIndex = allHistories.IndexOf(fromDateHistory);

            //        for (int i = 0; i < allHistories.Count; i++)
            //        {
            //            var history = allHistories[i];
            //            if (history.Date < fromDate || i < 1) continue;

            //            var vol20 = history.VOL(allHistories, -20);
            //            if (vol20 < 100000) continue;

            //            var ma10 = history.MA(allHistories, -10);
            //            var ma20 = history.MA(allHistories, -20);
            //            var ma50 = history.MA(allHistories, -50);


            //            var lastTrade = allHistories[i - 1];
            //            var ma10Yesterday = lastTrade.MA(allHistories, -10);
            //            var ma20Yesterday = lastTrade.MA(allHistories, -20);
            //            var ma50Yesterday = lastTrade.MA(allHistories, -50);

            //            var ma10Future = history.MA(allHistories, 10);
            //            //if (ma10Future == 0) continue; //Consider continue or break....

            //            var dk1 = ma10 > ma10Yesterday;
            //            var dk2 = ma10 > ma20;
            //            var dk3 = ma20 > ma50;
            //            var dk4 = ma20 > ma20Yesterday && ma50 > ma50Yesterday;
            //            var dk5 = history.C < ma50 * 1.07M;
            //            var dk6 = history.C > ma50 * 1.15M;

            //            var willBuy = dk1 && dk2 && dk3 && dk4 && dk5;
            //            var willSell = dk6;

            //            if (!willSell && !willBuy) continue;

            //            var reality = string.Empty;

            //            if (willBuy)
            //            {
            //                reality = ma10Future == 0
            //                    ? string.Empty
            //                    : ma10Future > history.C ? "true" : "false";
            //                patternOnsymbolOnBuy.StockCode = symbol._sc_;
            //                patternOnsymbolOnBuy.Details.Add(new PatternDetailsResponseModel
            //                {
            //                    ConditionMatchAt = history.Date,
            //                    MoreInformation = new
            //                    {
            //                        ma10 = ma10,
            //                        ma20 = ma20,
            //                        ma50 = ma50,
            //                        ma10Last = ma10Yesterday,
            //                        ma20Last = ma20Yesterday,
            //                        ma50Last = ma50Yesterday,
            //                        ma10Next = ma10Future,
            //                        TodayClosing = history.C,
            //                        RealityExpectation = reality,
            //                    }
            //                });
            //            }

            //            if (willSell)
            //            {
            //                reality = ma10Future == 0
            //                    ? string.Empty
            //                    : ma10Future < history.C ? "true" : "false";
            //                patternOnsymbolOnSell.StockCode = symbol._sc_;
            //                patternOnsymbolOnSell.Details.Add(new PatternDetailsResponseModel
            //                {
            //                    ConditionMatchAt = history.Date,
            //                    MoreInformation = new
            //                    {
            //                        ma10 = ma10,
            //                        ma20 = ma10,
            //                        ma50 = ma10,
            //                        ma10Last = ma10Yesterday,
            //                        ma20Last = ma10Yesterday,
            //                        ma50Last = ma10Yesterday,
            //                        ma10Next = ma10Future,
            //                        TodayClosing = history.C,
            //                        RealityExpectation = reality,
            //                    }
            //                });
            //            }
            //        }


            //        if (patternOnsymbolOnBuy.Details.Any())
            //        {
            //            mua.Items.Add(patternOnsymbolOnBuy);
            //        }
            //        if (patternOnsymbolOnSell.Details.Any())
            //        {
            //            ban.Items.Add(patternOnsymbolOnSell);
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //        throw;
            //    }
            //});

            //return result;
        }
    }
}
