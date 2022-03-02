using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormular1TimTrendGiam : IReportFormular
    {
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

        public List<PatternWeekResearchModel> GetStockDataByWeekFromNgay(List<StockSymbolHistory> dataFromLast8Months, string symbol, DateTime ngayKiemTra, int? weekRange = 30)
        {
            //var dates = await _context.StockSymbolHistory.OrderByDescending(s => s.Date)
            //    .Where(s => symbols.Contains(s.StockSymbol) && s.Date > ngayKiemTra.AddDays((weekRange.Value * 8) * -1))
            //    .ToListAsync();

            var newShorted = dataFromLast8Months.OrderBy(h => h.Date).ToList();
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

            var currentDate = dataFromLast8Months[0].Date;
            var currentWeek = dataFromLast8Months[0].Date.GetIso8601WeekOfYear();

            foreach (var history in dataFromLast8Months)
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

        public ReportFormularModel Calculation(string code, DateTime ngay, List<StockSymbolHistory> dataFromLast8Months, List<StockSymbolTradingHistory> tradingHistories)
        {
            var res = new ReportFormularModel();
            res.Name = "Tim Trend Giam";
            var result = new PatternResponseModel();

            var allHistories = GetStockDataByWeekFromNgay(dataFromLast8Months, code, ngay);
            var historiesInPeriodOfTimeNonDB = allHistories.Where(h => h.DateInWeek <= ngay).ToList();

            try
            {
                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = code;

                var historiesByStockCode = historiesInPeriodOfTimeNonDB
                    .Where(ss => ss.StockSymbol == code)
                    .ToList();

                var histories = historiesByStockCode
                    .OrderBy(s => s.Date)
                    .ToList();

                for (int i = 0; i < histories.Count; i++)
                {
                    var historyByWeek = histories[i];
                    if (historyByWeek.DateInWeek < ngay) continue;

                    var historiesByStockCodeUntilDate = historiesByStockCode.Where(h => h.DateInWeek < historyByWeek.DateInWeek).OrderByDescending(h => h.DateInWeek).Take(30).ToList();

                    var dinh1 = GetHigestStockDataByWeek(historiesByStockCodeUntilDate, code, historyByWeek.DateInWeek);
                    if (dinh1 == null) continue;

                    var day1 = GetLowestStockDataByWeek(historiesByStockCodeUntilDate, code, historyByWeek.DateInWeek, dinh1);
                    if (day1 == null) continue;

                    var dinh2 = GetHigestStockDataByWeek(historiesByStockCodeUntilDate, code, historyByWeek.DateInWeek, day1);
                    if (dinh2 == null) continue;

                    var day2 = GetLowestStockDataByWeek(historiesByStockCodeUntilDate, code, historyByWeek.DateInWeek, dinh2);
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
                                AverageNumberOfTradingInPreviousTimes = 0,
                                ShouldBuy = true,
                                RealityExpectation = ma5next == 0
                                    ? string.Empty
                                    : historyByWeek.C < ma5next
                                        ? "true"
                                        : "false",
                                Ma5WeekNext = ma5next,
                            }
                        });

                        res.Date = historyByWeek.Date; //TODO: find the next transaction date -> will be used to calculate T+3
                        res.IsActive = true;
                        res.Price = historyByWeek.C;
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

            result.TimTrendGiam.Items = result.TimTrendGiam.Items.OrderBy(s => s.StockCode).ToList();
            //return result;

            return res.IsActive ? res : null;
        }
    }
}
