using System;
using System.IO;
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
using System.Text;
using DotNetCoreSqlDb.Models.Prediction;
using Newtonsoft.Json;
using Skender.Stock.Indicators;
using DotNetCoreSqlDb.Models.Learning.RealData;
using DotNetCoreSqlDb.Models.Business.Patterns.LocCoPhieu;
using LinqKit;

namespace DotNetCoreSqlDb.Controllers
{
    //[ApiController]
    //[Route("[controller]")]
    public partial class StockPatternController : Controller
    {
        private readonly MyDatabaseContext _context;
                
        public StockPatternController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: StockPattern
        public async Task<IActionResult> Index()
        {
            return View(await _context.StockSymbol.ToListAsync());
        }

        public async Task<PatternResponseModel> All(string code, DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();

            var td2 = await TimDay2(code, ngay, soPhienGd, trungbinhGd);
            result.TimDay2 = td2.TimDay2;

            var td2m = await TimDay2V1(code, ngay, soPhienGd, trungbinhGd);
            result.TimDay2Moi = td2m.TimDay2;

            result.TimTrendGiam = (await TimTrendGiam(code, false, ngay, soPhienGd, trungbinhGd)).TimTrendGiam;
            result.GiamSau = (await FollowUpSymbolsGoingDown(code, ngay)).TimDay2;

            //result.Canslim = (await Canslim(code, 2022, 1)).Canslim;
            result.TangDotBien = await FollowUpPriceInDayMarkTangDotBien(code, 0.7M, ngay, soPhienGd, trungbinhGd);
            return result;
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
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var startFrom = ngay.AddDays(-60);

            var stockCodes = symbols.Select(s => s._sc_).ToList();
            var historiesInPeriodOfTimeByStockCode = await _context.History
                    .Where(ss =>
                        stockCodes.Contains(ss.StockSymbol)
                        && ss.Date >= startFrom
                        )
                    .OrderByDescending(ss => ss.Date)
                    .ToListAsync();

            if (historiesInPeriodOfTimeByStockCode.FirstOrDefault() != null && historiesInPeriodOfTimeByStockCode.First().Date < ngay && ngay.Date == DateTime.Today.WithoutHours())
            {
                var newPackages = new List<History>();
                var from = DateTime.Now.WithoutHours();
                var to = DateTime.Now.WithoutHours().AddDays(1);

                var service = new Service();
                await service.GetV(newPackages, symbols, from, to, from, 0);

                historiesInPeriodOfTimeByStockCode.AddRange(newPackages);
            }

            Parallel.ForEach(symbols, symbol =>
            {
                var orderedHistoryByStockCode = historiesInPeriodOfTimeByStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.Date)
                    .ToList();

                var latestDate = orderedHistoryByStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(orderedHistoryByStockCode);

                if (biCanhCao) return;

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var historiesInPeriodOfTime = historiesInPeriodOfTimeByStockCode
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

                var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                if (lowest == null) return;

                var secondLowest = lowest.LookingForSecondLowest(histories, history);
                if (secondLowest == null) return;

                var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();
                var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
                if (highest == null) return;

                var dk1 = highest.C * 0.85M >= lowest.C;
                //var dk2 = history.C >= secondLowest.C * 1.02M;//default value when we have 2nd lowest
                var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
                var dk4 = lowest.C * 1.15M >= secondLowest.C;

                if (dk1 /*&& dk2*/ && dk3 && dk4) //basically we should start buying
                {
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
            var ct1 = new ReportFormularCT1();
            var dates = await _context.History.OrderByDescending(s => s.Date)
                .Where(s => stockCodes.Contains(s.StockSymbol) && s.Date > ngay.AddDays((30 * 8) * -1))
                .ToListAsync();

            var allHistories = ct1.GetStockDataByWeekFromNgay(dates);

            var historiesInPeriodOfTimeByStockCode = kiemTraTiLe
                ? allHistories
                : allHistories.Where(h => h.DateInWeek <= ngay).ToList();

            Parallel.ForEach(symbols, symbol =>
            {
                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var historiesByStockCode = historiesInPeriodOfTimeByStockCode
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

                    var dinh1 = ct1.GetHigestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek);
                    if (dinh1 == null) continue;

                    var day1 = ct1.GetLowestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek, dinh1);
                    if (day1 == null) continue;

                    var dinh2 = ct1.GetHigestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek, day1);
                    if (dinh2 == null) continue;

                    var day2 = ct1.GetLowestStockDataByWeek(historiesByStockCodeUntilDate, symbol._sc_, historyByWeek.DateInWeek, dinh2);
                    if (day2 == null) continue;


                    var indexOfDinh2 = historiesByStockCodeUntilDate.IndexOf(dinh2);
                    var indexOfDinh1 = historiesByStockCodeUntilDate.IndexOf(dinh1);

                    var a = indexOfDinh2 - indexOfDinh1;
                    var b = dinh1.C - dinh2.C;

                    var formula = "y = ax / -b";

                    var distanceFrom0ToThisWeekX = historiesByStockCodeUntilDate.IndexOf(historyByWeek) - indexOfDinh2;    //we need a positive number
                    var distanceFrom0ToThisWeekY = historyByWeek.C - dinh2.C;                                              //we need a negative number

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
            });

            result.TimTrendGiam.Items = result.TimTrendGiam.Items.OrderBy(s => s.StockCode).ToList();
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
            var historiesInPeriodOfTimeNonDB = await _context.History
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

                        var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                        if (lowest == null) continue;

                        var secondLowest = lowest.LookingForSecondLowest(histories, history);
                        if (secondLowest == null) continue;

                        var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();
                        var highest1 = previousDaysFromCurrentDay.OrderByDescending(h => h.C).FirstOrDefault();
                        if (highest1 == null) continue;

                        var dk1 = highest1.C * 0.85M >= lowest.C;
                        var dk2 = history.C > secondLowest.C;/*history.C >= secondLowest.C * 1.02M;*/
                        var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
                        var dk4 = lowest.C * 1.15M >= secondLowest.C;

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
            var historiesInPeriodOfTimeNonDB = await _context.History
                    .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= fromDate.AddDays(-100))
                    .OrderByDescending(ss => ss.Date)
                    .ToListAsync();

            var mua = new PatternSellAndBuyBySymbolDetailResponseModel();
            var ban = new PatternSellAndBuyBySymbolDetailResponseModel();

            //result.BuyAndSell.Sell = ban;
            //result.BuyAndSell.Buy = mua;

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
                        if (vol20 < ConstantData.minMA20VolDaily) continue;

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

        /// <summary>
        /// Mua chủ động > bán chủ động * 1.6
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<List<string>> FollowUpPriceInDayMarkTangDotBien(string code, decimal minRate, DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesInPeriodOfTimeNonDB = await _context.KLGDMuaBan
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date.Year == ngay.Year && ss.Date.Month == ngay.Month && ss.Date.Day == ngay.Day)
                .OrderByDescending(ss => ss.StockSymbol)
                .ThenByDescending(s => s.Date)
                .ToListAsync();


            var historiesInPeriodOfTimeByStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= ngay.AddDays(-10))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var lstDotBien = new List<string>();

            Parallel.ForEach(symbols, symbol =>
            {
                var historiesInPeriodOfTime = historiesInPeriodOfTimeByStockCode
                   .Where(ss => ss.StockSymbol == symbol._sc_)
                   .OrderBy(s => s.Date)
                   .ToList();

                var latestDate = historiesInPeriodOfTime.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(historiesInPeriodOfTime);

                if (biCanhCao) return;

                var avarageOfLastXXPhien = historiesInPeriodOfTime.Take(soPhienGd).Sum(h => h.V) / soPhienGd;
                if (avarageOfLastXXPhien < trungbinhGd) return;

                var historiesInPeriodByStockCode = historiesInPeriodOfTimeNonDB
                    .Where(ss => ss.StockSymbol == symbol._sc_ && ss.TotalVol > 0)
                    .FirstOrDefault();

                if (historiesInPeriodByStockCode != null)
                {
                    var realVol = Math.Round((decimal)historiesInPeriodByStockCode.TotalBuy / (decimal)historiesInPeriodByStockCode.TotalVol, 2);
                    if (realVol > minRate)
                        lstDotBien.Add($"{symbol._sc_} - {realVol}");
                }
            });

            return lstDotBien;

        }

        public async Task<PatternResponseModel> FollowUpSymbolsGoingDown(string code, DateTime dateTime, int expectedLowerPercentage = 40)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesInPeriodOfTimeNonDB = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= dateTime.AddDays(-60))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            //int expectedLowerPercentage = 40;

            Parallel.ForEach(symbols, symbol =>
            {
                var orderedHistoryByStockCode = historiesInPeriodOfTimeNonDB
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.Date)
                    .ToList();

                var latestDate = orderedHistoryByStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(orderedHistoryByStockCode);

                if (!biCanhCao)
                {

                    var histories = historiesInPeriodOfTimeNonDB
                            .Where(ss => ss.StockSymbol == symbol._sc_)
                            .OrderByDescending(ss => ss.Date)
                            .ToList();

                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;
                    histories = histories.OrderBy(s => s.Date).ToList();

                    var history = histories.FirstOrDefault(h => h.Date == dateTime);
                    if (history == null) return;

                    if (history.VOL(histories, -20) < ConstantData.minMA20VolDaily) return;

                    var currentDateToCheck = history.Date;
                    var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(2).ToList();

                    var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                    if (lowest == null) return;

                    var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(20).ToList();
                    var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
                    if (highest == null) return;

                    var dk1 = highest.C * (100 - expectedLowerPercentage) / 100 >= lowest.C;
                    var dk2 = history.C > history.O;

                    if (dk1) //Start following
                    {
                        patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                        {
                            ConditionMatchAt = currentDateToCheck,
                            MoreInformation = new
                            {
                                Lowest = lowest.C,
                                LowestDate = lowest.Date,
                                Highest = highest.C,
                                HighestDate = highest.Date,
                                RealityExpectation = string.Empty,
                                ShouldBuy = true
                            }
                        });
                    }

                    if (patternOnsymbol.Details.Any())
                    {
                        result.TimDay2.Items.Add(patternOnsymbol);
                        result.TimDay2.Items = result.TimDay2.Items.OrderBy(s => s.StockCode).ToList();
                    }
                }
            });

            return result;
        }

        public async Task<PatternResponseModel> TimDay2V1(string code, DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var startFrom = ngay.AddDays(-180);

            var stockCodes = symbols.Select(s => s._sc_).ToList();
            var historiesInPeriodOfTimeByStockCode = await _context.History
                    .Where(ss =>
                        stockCodes.Contains(ss.StockSymbol)
                        && ss.Date >= startFrom
                        )
                    .OrderByDescending(ss => ss.Date)
                    .ToListAsync();

            if (historiesInPeriodOfTimeByStockCode.FirstOrDefault() != null && historiesInPeriodOfTimeByStockCode.First().Date < ngay && ngay.Date == DateTime.Today.WithoutHours())
            {
                var newPackages = new List<History>();
                var from = DateTime.Now.WithoutHours();
                var to = DateTime.Now.WithoutHours().AddDays(1);

                var service = new Service();
                await service.GetV(newPackages, symbols, from, to, from, 0);

                historiesInPeriodOfTimeByStockCode.AddRange(newPackages);
            }

            Parallel.ForEach(symbols, symbol =>
            {
                var orderedHistoryByStockCode = historiesInPeriodOfTimeByStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.Date)
                    .ToList();

                var latestDate = orderedHistoryByStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(orderedHistoryByStockCode);

                if (biCanhCao) return;

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var historiesInPeriodOfTime = historiesInPeriodOfTimeByStockCode
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

                var dinh1 = new History();
                var day1 = new History();
                var day2 = new History();

                var t1 = histories.DidDay2ShowYesterdayStartWithDay2(history, out dinh1, out day1, out day2);


                //var lowest = histories.LookingForLowest(history);
                //if (lowest == null) return;

                //var secondLowest = lowest.LookingForSecondLowestWithCheckingDate(histories, history);
                //if (secondLowest == null) return;

                ////var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();
                ////var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
                ////if (highest == null) return;

                ////var dk1 = highest.C * 0.85M >= lowest.C;
                ////var dk2 = history.C >= secondLowest.C * 1.02M;//default value when we have 2nd lowest
                //var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
                //var dk4 = lowest.C * 1.15M >= secondLowest.C;

                //if (/*dk1 && dk2 &&*/ dk3 && dk4) //basically we should start buying
                if (t1)
                {
                    patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                    {
                        ConditionMatchAt = currentDateToCheck,
                        MoreInformation = new
                        {
                            Text = @$"{history.StockSymbol}: Đỉnh 1 {dinh1.Date.ToShortDateString()}: {dinh1.C}, Đáy 1 {day1.Date.ToShortDateString()}: {day1.C},
                                        Đáy 2 {day2.Date.ToShortDateString()}: {day2.C},
                                        Giá đóng cửa hum nay ({history.C}) cao hơn giá đóng của đáy 2 {day2.C}",
                            TodayOpening = history.O,
                            TodayClosing = history.C,
                            TodayLowest = history.L,
                            TodayTrading = history.V,
                            Previous1stLowest = day1.C,
                            Previous1stLowestDate = day1.Date,
                            Previous2ndLowest = day2.C,
                            Previous2ndLowestDate = day2.Date,
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
            });

            result.TimDay2.Items = result.TimDay2.Items.OrderBy(s => s.StockCode).ToList();

            return result;
        }

        public async Task<PatternResponseModel> Canslim(string code, int year, int quarter)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var histories = await _context.StockSymbolFinanceHistory
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod >= (year - 4))
                .ToListAsync();

            Parallel.ForEach(symbols, symbol =>
            {
                var orderedHistoryByStockCode = histories
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderByDescending(s => s.YearPeriod)
                    .ThenByDescending(s => s.Quarter)
                    .ToList();

                var C = string.Empty; //Current quartery earning per share => EPS (Tỉ suất lợi nhuận trên cổ phần) của quý hiện tại so với những quý cùng kì của ngoái hoặc 4 quý gần nhất
                var A = string.Empty; //Annual earnings gorwth  - tăng trường lợi nhuận hàng năm trong 3 năm gần nhất - tăng trường doanh thu, lợi nhuận sau thuế, eps, trên mỗi cổ phiếu
                var N = string.Empty; //New products            - doanh nghiệp có ra mắt sản phẩm mới hay ko, cần đọc tài liệu (tài liệu gì, phần nào?)
                var S = string.Empty; //Share outstanding       - số lượng cổ phiếu trôi nổi trên thị trường nhiều hay ít (<35% thì ok), cung mạnh thì giá giảm, cầu mạnh thì giá tăng
                var L = string.Empty; //Leading industry        - dẫn đầu ngành hay ko, lọt top 10 ko?
                var I = string.Empty; //inutition               - bảo kê bởi những tay to - họ dựa vô ROA, ROE tốt để mua khi cp giá rẻ, trend line, tay to là doanh nghiệp lớn hoặc doanh nghiệp nước ngoài
                var M = string.Empty; //Marking direction       - xu thế thị trường,khi thị trường tăng -> 3/4 cp tăng, cái này phải đọc tài liệu và tìm hiểu thị trường

                var hasNewProduct = false;
                var cpTroiNoiByPercentage = 0;
                var isInTop10Industry = false;
                var hasBackup = false;
                var isGoodMarketingDirection = false;

                var yearAndQuarter = new Dictionary<int, int>();
                var timeline = new List<StockSymbolFinanceHistory>();

                #region "C" character
                foreach (var stock in orderedHistoryByStockCode)
                {
                    if (!timeline.Any(t => t.YearPeriod == stock.YearPeriod && t.Quarter == stock.Quarter))
                        timeline.Add(new StockSymbolFinanceHistory { Quarter = stock.Quarter, YearPeriod = stock.YearPeriod });
                }

                var currentItem = timeline.FirstOrDefault(t => t.YearPeriod == year && t.Quarter == quarter);
                var currentIndex = currentItem == null
                    ? 0
                    : timeline.IndexOf(currentItem);

                var expectedRange = timeline.Skip(currentIndex).Take(4).ToList();
                var last4Q = new List<StockSymbolFinanceHistory>();
                foreach (var item in expectedRange)
                {
                    last4Q.AddRange(orderedHistoryByStockCode.Where(h => h.Quarter == item.Quarter && h.YearPeriod == item.YearPeriod).ToList());
                }

                var EPSString = "Trailing EPS";
                decimal eps1stQValue = 0;
                decimal eps2ndQValue = 0;
                decimal eps3rdQValue = 0;
                decimal eps4thQValue = 0;
                for (int i = 0; i < timeline.Count(); i++)
                {
                    if (i == 0)
                        eps1stQValue = last4Q.Where(h => h.YearPeriod == timeline[i].YearPeriod && h.Quarter == timeline[i].Quarter && h.NameEn.Contains(EPSString)).Sum(h => h.Value ?? 0);
                    if (i == 1)
                        eps2ndQValue = last4Q.Where(h => h.YearPeriod == timeline[i].YearPeriod && h.Quarter == timeline[i].Quarter && h.NameEn.Contains(EPSString)).Sum(h => h.Value ?? 0);
                    if (i == 2)
                        eps3rdQValue = last4Q.Where(h => h.YearPeriod == timeline[i].YearPeriod && h.Quarter == timeline[i].Quarter && h.NameEn.Contains(EPSString)).Sum(h => h.Value ?? 0);
                    if (i == 3)
                        eps4thQValue = last4Q.Where(h => h.YearPeriod == timeline[i].YearPeriod && h.Quarter == timeline[i].Quarter && h.NameEn.Contains(EPSString)).Sum(h => h.Value ?? 0);

                    if (i > 3) break;
                }

                var hasC =
                       eps1stQValue >= eps2ndQValue
                    && eps2ndQValue >= eps3rdQValue
                    && eps3rdQValue >= eps4thQValue;
                #endregion

                var last3Y = new List<StockSymbolFinanceHistory>();
                currentItem = timeline.FirstOrDefault(t => t.YearPeriod == year && t.Quarter == 1);
                currentIndex = currentItem == null
                    ? 0
                    : timeline.IndexOf(currentItem);

                #region "A" character
                expectedRange = timeline.Skip(currentIndex).Take(4 * 3).ToList();
                foreach (var item in expectedRange)
                {
                    last3Y.AddRange(orderedHistoryByStockCode.Where(h => h.Quarter == item.Quarter && h.YearPeriod == item.YearPeriod).ToList());
                }

                var years = last3Y.Select(q => q.YearPeriod).OrderByDescending(q => q).Distinct().ToList();

                string doanhThuThuan = "3. Net revenue";
                decimal tangTruongDoanhThu1stY = 0;
                decimal tangTruongDoanhThu2ndY = 0;
                decimal tangTruongDoanhThu3rdY = 0;
                for (int i = 0; i < years.Count; i++)
                {
                    if (i == 0)
                        tangTruongDoanhThu1stY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(doanhThuThuan)).Sum(h => h.Value ?? 0);
                    if (i == 1)
                        tangTruongDoanhThu2ndY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(doanhThuThuan)).Sum(h => h.Value ?? 0);
                    if (i == 2)
                        tangTruongDoanhThu3rdY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(doanhThuThuan)).Sum(h => h.Value ?? 0);
                }

                string roe = "ROE"; //Tỷ suất lợi nhuận trên vốn chủ sở hữu bình quân (ROEA)
                decimal tangTruongLoiNhuan1stY = 0;
                decimal tangTruongLoiNhuan2ndY = 0;
                decimal tangTruongLoiNhuan3rdY = 0;
                for (int i = 0; i < years.Count; i++)
                {
                    if (i == 0)
                        tangTruongLoiNhuan1stY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(roe)).Sum(h => h.Value ?? 0);
                    if (i == 1)
                        tangTruongLoiNhuan2ndY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(roe)).Sum(h => h.Value ?? 0);
                    if (i == 2)
                        tangTruongLoiNhuan3rdY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(roe)).Sum(h => h.Value ?? 0);
                }

                string profit = "Profit after tax for shareholders of the parent company"; //Lợi nhuận sau thuế của cổ đông Công ty mẹ
                decimal tangTruongLoiNhuanSauThue1stY = 0;
                decimal tangTruongLoiNhuanSauThue2ndY = 0;
                decimal tangTruongLoiNhuanSauThue3rdY = 0;
                for (int i = 0; i < years.Count; i++)
                {
                    if (i == 0)
                        tangTruongLoiNhuanSauThue1stY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(profit)).Sum(h => h.Value ?? 0);
                    if (i == 1)
                        tangTruongLoiNhuanSauThue2ndY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(profit)).Sum(h => h.Value ?? 0);
                    if (i == 2)
                        tangTruongLoiNhuanSauThue3rdY = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(profit)).Sum(h => h.Value ?? 0);
                }
                #endregion

                #region good PEG
                var hasGoodPEG = false;
                string peString = "P/E";

                //TODO: PE lấy ngày đang xét
                var pe = last4Q.Where(h => h.YearPeriod == timeline[0].YearPeriod && h.NameEn.Contains(peString)).Sum(h => h.Value ?? 0) / (last4Q.Any() ? last4Q.Select(l => l.Quarter).Distinct().Count() : 1);

                var growth = (Math.Round((decimal)tangTruongLoiNhuanSauThue1stY / (decimal)tangTruongLoiNhuanSauThue2ndY, 2) - 1) * 100;
                hasGoodPEG = pe / growth > 0 && pe / growth < 1.35M;
                #endregion


                #region tang trưởng bền vững theo chỉ tiêu kế hoạch

                decimal growthByCTKHNam1 = 0;
                decimal growthByCTKHNam2 = 0;
                decimal growthByCTKHNam3 = 0;
                string totalRevenue = "Total revenue"; //
                for (int i = 0; i < years.Count; i++)
                {
                    if (i == 0)
                        growthByCTKHNam1 = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(totalRevenue)).Sum(h => h.Value ?? 0);
                    if (i == 1)
                        growthByCTKHNam2 = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(totalRevenue)).Sum(h => h.Value ?? 0);
                    if (i == 2)
                        growthByCTKHNam3 = last3Y.Where(h => h.YearPeriod == years[i] && h.NameEn.Contains(totalRevenue)).Sum(h => h.Value ?? 0);
                }

                decimal growthByPercentWithCTKH1 = growthByCTKHNam2 > 0
                    ? (Math.Round((decimal)growthByCTKHNam1 / (decimal)growthByCTKHNam2, 2) - 1) * 100
                    : 0;
                decimal growthByPercentWithCTKH2 = growthByCTKHNam3 > 0
                    ? (Math.Round((decimal)growthByCTKHNam2 / (decimal)growthByCTKHNam3, 2) - 1) * 100
                    : 0;


                #endregion

                var dk1 = hasC;
                var dk2 = tangTruongDoanhThu1stY >= tangTruongDoanhThu2ndY && tangTruongDoanhThu2ndY >= tangTruongDoanhThu3rdY;
                var dk3 = tangTruongLoiNhuan1stY >= tangTruongLoiNhuan2ndY && tangTruongLoiNhuan2ndY >= tangTruongLoiNhuan3rdY;
                var dk4 = tangTruongLoiNhuanSauThue1stY >= tangTruongLoiNhuanSauThue2ndY && tangTruongLoiNhuanSauThue2ndY >= tangTruongLoiNhuanSauThue3rdY;
                var dk5 = hasGoodPEG;
                var dk6 = growthByPercentWithCTKH1 >= growthByPercentWithCTKH2;

                if (dk1 && dk2 && dk3 && dk4 && dk5) //Start following
                {
                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;
                    patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                    {
                        MoreInformation = new
                        {
                            TangTruongEPSQuy = $"{eps1stQValue} > {eps2ndQValue} > {eps3rdQValue} > {eps4thQValue}",
                            TangTruongDoanhThuNam = $"{tangTruongDoanhThu1stY} > {tangTruongDoanhThu2ndY} > {tangTruongDoanhThu3rdY}",
                            TangTruongLoiNhuanNam = $"{tangTruongLoiNhuan1stY} > {tangTruongLoiNhuan2ndY} > {tangTruongLoiNhuan3rdY}",
                            TangTruongLoiNhuanSauThueNam = $"{tangTruongLoiNhuanSauThue1stY} > {tangTruongLoiNhuanSauThue2ndY} > {tangTruongLoiNhuanSauThue3rdY}",
                            TangTruongLoiNhuanBenVungTheoCTKH = $"Năm ngoái: {growthByPercentWithCTKH1} vs Năm trước nữa: {growthByPercentWithCTKH2}",
                            RealityExpectation = string.Empty,
                            ShouldBuy = true
                        }
                    });

                    result.Canslim.Items.Add(patternOnsymbol);
                }
            });

            result.Canslim.Items = result.Canslim.Items.OrderBy(s => s.StockCode).ToList();

            return result;
        }



        /// <summary>
        /// Condition
        /// - trong x nam
        ///     + Khong phải CP đang bị giao dịch 1 tuần/1 lần
        ///     + Trung bình giao dịch trong 30 phiên lần nhất > 100K
        ///     + ROIC > 10%
        ///     + ROE > 10% 
        ///     + EPS > 10%
        ///     + Thời gian trả nợ < 10 năm
        ///     + Giá hum nay < Ma 50% - 80 %
        /// </summary>
        /// <param name="code"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> PhanTichDoanhNghiepTheoNam(string code, int year)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var unexpectedBusiness = new List<string>() { "Tài chính và bảo hiểm" };
            //var t1 = await _context.StockSymbol.Where(s => !unexpectedBusiness.Contains(s._in_)).ToListAsync();

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => !unexpectedBusiness.Contains(s._in_)).ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_) && !unexpectedBusiness.Contains(s._in_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var histories = await _context.StockSymbolFinanceYearlyHistory
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod >= (year - 10))
                .ToListAsync();

            var today = await _context.History.OrderByDescending(d => d.Date).FirstAsync();

            var TodayData = await _context.History
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.Date == today.Date)
                .ToListAsync();

            var historiesInPeriodOfTimeByStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= today.Date.AddDays(-10))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            Parallel.ForEach(symbols, symbol =>
            {
                var historiesInPeriodOfTime = historiesInPeriodOfTimeByStockCode
                   .Where(ss => ss.StockSymbol == symbol._sc_)
                   .OrderBy(s => s.Date)
                   .ToList();

                var latestDate = historiesInPeriodOfTime.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(historiesInPeriodOfTime);

                if (biCanhCao) return;

                var avarageOfLastXXPhien = historiesInPeriodOfTime.Take(30).Sum(h => h.V) / 30;
                if (avarageOfLastXXPhien < 1000) return;



                var orderedHistoryByStockCode = histories
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.YearPeriod)
                    .ToList();
                decimal stockToday = TodayData.FirstOrDefault(t => t.StockSymbol == symbol._sc_)?.C ?? 0;
                if (!orderedHistoryByStockCode.Any() || stockToday <= 0) return;

                var groupedByYear = orderedHistoryByStockCode.GroupBy(g => g.YearPeriod).ToDictionary(g => g.Key, g => g.ToList());

                var lstData = new List<StockSymbolFinanceYearlyHistoryModel>();
                foreach (var item in groupedByYear)
                {
                    var previousYear = groupedByYear.ContainsKey(item.Key - 1) ? groupedByYear[item.Key - 1] : null;
                    if (previousYear == null) continue;

                    var noDaiHanYearly = item.Value.Where(y => y.NameEn == ConstantData.NameEn.tongTs).Sum(y => y.Value ?? 0)
                        - item.Value.Where(y => y.NameEn == ConstantData.NameEn.NoNganHan).Sum(y => y.Value ?? 0)
                        - item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0);

                    decimal roic = noDaiHanYearly + item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.lnstTuThuNhapDoanhNghiep).Sum(y => y.Value ?? 0)
                            / (noDaiHanYearly + item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0))
                            : 1;

                    decimal roe =
                            item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.lnthuanTuHdKinhDoanh).Sum(y => y.Value ?? 0)
                              / item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0)
                            : 0;

                    decimal salesGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.doanhThuThuan).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.doanhThuThuan).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.doanhThuThuan).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    decimal epsGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    decimal bvpsGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.BVPSCoBan).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.BVPSCoBan).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.BVPSCoBan).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    decimal cashGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.LuuChuyenTienThuanTuHDKD).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.LuuChuyenTienThuanTuHDKD).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.LuuChuyenTienThuanTuHDKD).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    lstData.Add(new StockSymbolFinanceYearlyHistoryModel
                    {
                        Year = item.Key,
                        bvpsGrowth = bvpsGrowth,
                        cashGrowth = cashGrowth,
                        epsGrowth = epsGrowth,
                        roe = roe,
                        roic = roic,
                        saleGrowth = salesGrowth,
                        PECoBan = item.Value.Where(y => y.NameEn == ConstantData.NameEn.PECoBan).Sum(y => y.Value ?? 0)
                    });
                }
                if (!lstData.Any()) return;
                decimal croic = lstData.Sum(d => d.roic) / lstData.Count();
                decimal croe = lstData.Sum(d => d.roe) / lstData.Count();
                decimal csalesGrowhrate = lstData.Sum(d => d.saleGrowth) / lstData.Count();
                decimal cepsGrowhrate = lstData.Sum(d => d.epsGrowth) / lstData.Count();
                decimal cbvpsGrowhrate = lstData.Sum(d => d.bvpsGrowth) / lstData.Count();
                decimal ccashGrowhrate = lstData.Sum(d => d.cashGrowth) / lstData.Count();
                var lastYear = groupedByYear.OrderByDescending(d => d.Key).First();
                var noDaiHan = lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.tongTs).Sum(y => y.Value ?? 0)
                        - lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.NoNganHan).Sum(y => y.Value ?? 0)
                        - lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0);
                decimal ctimeToPayTheDept = noDaiHan != 0
                        ? lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.lnstTuThuNhapDoanhNghiep).Sum(y => y.Value ?? 0) / noDaiHan
                        : 0;

                decimal chiSoTangTruong = croic;// (croe + croic + cepsGrowhrate) / 3;
                decimal PETrungBinhHangNam = lstData.Sum(d => d.PECoBan) / lstData.Count();
                decimal PETrungBinhMinMax = (lstData.Min(d => d.PECoBan) + lstData.Max(d => d.PECoBan)) / 2;
                decimal ChiSoTangTruongX2 = chiSoTangTruong * 2 * 100;
                decimal expectedPE = Math.Min(PETrungBinhMinMax, ChiSoTangTruongX2);
                decimal tileLoiNhuanToiThieu = 0.15M;


                decimal MOSPrice50 =
                    (lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0) * expectedPE
                    * (decimal)Math.Pow((double)(1M + chiSoTangTruong) / (double)(1M + tileLoiNhuanToiThieu), 10))
                    * 0.5M;

                decimal MOSPrice80 =
                    (lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0) * expectedPE
                    * (decimal)Math.Pow((double)(1M + chiSoTangTruong) / (double)(1M + tileLoiNhuanToiThieu), 10))
                    * 0.8M;


                var dk1 = croic > 0.1M;
                var dk2 = croe > 0.1M;
                var dk3 = csalesGrowhrate > 0.1M;
                var dk4 = cepsGrowhrate > 0.1M;
                var dk5 = cbvpsGrowhrate > 0.1M;
                var dk6 = ccashGrowhrate > 0.01M;
                var dk7 = ctimeToPayTheDept <= 10;
                var dk8 = stockToday < MOSPrice50
                    || stockToday > MOSPrice50 && stockToday <= MOSPrice50 * 1.2M;

                if (dk1
                    && dk2 && dk4 && dk7
                    && dk8) //Start following
                {
                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;
                    var text = (MOSPrice50 > stockToday)
                        ? $"MOSPrice50 > Giá hum nay {Math.Round(MOSPrice50 / stockToday, 2) - 1}%"
                        : $"MOSPrice50 < Giá hum nay {1 - Math.Round(MOSPrice50 / stockToday, 2)}%";
                    patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                    {
                        MoreInformation = new
                        {
                            Text = $"{text}",
                            ROIC = $"{croic}",
                            ROE = $"{croe}",
                            SALES = $"{csalesGrowhrate}",
                            EPS = $"{cepsGrowhrate}",
                            BVPS = $"{cbvpsGrowhrate}",
                            CASH = $"{ccashGrowhrate}",
                            DeptTime = $"{ctimeToPayTheDept}",
                            Price = $"{stockToday}",
                            MOSPrice50 = $"{MOSPrice50}",
                            RealityExpectation = string.Empty,
                            ShouldBuy = true
                        }
                    });

                    result.NhanDinhHDKD.Items.Add(patternOnsymbol);
                }
            });

            result.NhanDinhHDKD.Items = result.NhanDinhHDKD.Items.OrderBy(s => s.StockCode).ToList();

            return result;
        }

        /// <summary>
        /// Condition đơn giản
        /// - trong x nam
        ///     + Khong phải CP đang bị giao dịch 1 tuần/1 lần
        ///     + Trung bình giao dịch trong 30 phiên lần nhất > 100K
        ///     + Tăng trường lợi nhuận sau thuế trung bình trong x năm >= 15%
        ///     + P/E <= 15% trung bình trong x năm
        /// </summary>
        /// <param name="code"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> PhanTichDoanhNghiepTheoNamV1(string code, int year, int range, decimal tangtruong)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var unexpectedBusiness = new List<string>() { "Tài chính và bảo hiểm" };
            var today = await _context.History.OrderByDescending(d => d.Date).FirstAsync();


            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => !unexpectedBusiness.Contains(s._in_)).ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_) && !unexpectedBusiness.Contains(s._in_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var stockSymbolFinanceYearlyHistories = await _context.StockSymbolFinanceYearlyHistory
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod >= (year - range))
                .ToListAsync();

            var stockSymbolHistoryToday = await _context.History
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.Date == today.Date)
                .ToListAsync();

            var stockSymbolHistoryInRange = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= today.Date.AddDays(-10))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var quarterlyData = new List<string> { "P/E", "Profit after tax for shareholders of parent company" };
            var stockSymbolFinanceQuarterlyHistories = await _context.StockSymbolFinanceHistory
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod == (year - 1) && quarterlyData.Contains(f.NameEn))
                .ToListAsync();

            Parallel.ForEach(symbols, symbol =>
            {
                var stockSymbolHistoryInRangeBySockCode = stockSymbolHistoryInRange
                   .Where(ss => ss.StockSymbol == symbol._sc_)
                   .OrderBy(s => s.Date)
                   .ToList();

                var latestDate = stockSymbolHistoryInRangeBySockCode.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(stockSymbolHistoryInRangeBySockCode);
                if (biCanhCao) return;

                var avarageOfLastXXPhien = stockSymbolHistoryInRangeBySockCode.Take(30).Sum(h => h.V) / 30;
                if (avarageOfLastXXPhien < 1000) return;

                var stockSymbolFinanceYearlyHistoryByStockCode = stockSymbolFinanceYearlyHistories
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.YearPeriod)
                    .ToList();
                decimal stockToday = stockSymbolHistoryToday.FirstOrDefault(t => t.StockSymbol == symbol._sc_)?.C ?? 0;
                if (!stockSymbolFinanceYearlyHistoryByStockCode.Any() || stockToday <= 0) return;

                var groupedByYear = stockSymbolFinanceYearlyHistoryByStockCode.GroupBy(g => g.YearPeriod).ToDictionary(g => g.Key, g => g.ToList());

                var lstData = new List<StockSymbolFinanceYearlyHistoryModel>();

                var quarterly = stockSymbolFinanceQuarterlyHistories
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.Quarter)
                    .ToList();
                if (!quarterly.Any()) return;

                decimal pe4QuyTruoc = quarterly.Any(q => q.NameEn == quarterlyData[0])
                    ? quarterly.Where(q => q.NameEn == quarterlyData[0]).Sum(d => d.Value ?? 0) / quarterly.Count(q => q.NameEn == quarterlyData[0])
                    : 0;

                var t1 = quarterly.Where(q => q.NameEn == quarterlyData[1]).ToList();
                decimal lnst4QuyTruoc = t1.Sum(d => d.Value ?? 0);

                if (pe4QuyTruoc == 0 || lnst4QuyTruoc == 0) return; //cổ phiếu nhỏ lẻ

                foreach (var item in groupedByYear)
                {
                    var previousYear = groupedByYear.ContainsKey(item.Key - 1) ? groupedByYear[item.Key - 1] : null;
                    if (previousYear == null) continue;

                    var noDaiHanYearly = item.Value.Where(y => y.NameEn == ConstantData.NameEn.tongTs).Sum(y => y.Value ?? 0)
                        - item.Value.Where(y => y.NameEn == ConstantData.NameEn.NoNganHan).Sum(y => y.Value ?? 0)
                        - item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0);

                    decimal roic = noDaiHanYearly + item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.lnstTuThuNhapDoanhNghiep).Sum(y => y.Value ?? 0)
                            / (noDaiHanYearly + item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0))
                            : 1;

                    decimal roe =
                            item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.lnthuanTuHdKinhDoanh).Sum(y => y.Value ?? 0)
                              / item.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0)
                            : 0;

                    decimal salesGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.doanhThuThuan).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.doanhThuThuan).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.doanhThuThuan).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    decimal epsGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    decimal bvpsGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.BVPSCoBan).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.BVPSCoBan).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.BVPSCoBan).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    decimal cashGrowth =
                            previousYear.Where(y => y.NameEn == ConstantData.NameEn.LuuChuyenTienThuanTuHDKD).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => y.NameEn == ConstantData.NameEn.LuuChuyenTienThuanTuHDKD).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => y.NameEn == ConstantData.NameEn.LuuChuyenTienThuanTuHDKD).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    decimal lnstGrowth = previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    lstData.Add(new StockSymbolFinanceYearlyHistoryModel
                    {
                        Year = item.Key,
                        bvpsGrowth = bvpsGrowth,
                        cashGrowth = cashGrowth,
                        epsGrowth = epsGrowth,
                        roe = roe,
                        roic = roic,
                        saleGrowth = salesGrowth,
                        PECoBan = item.Value.Where(y => y.NameEn == ConstantData.NameEn.PECoBan).Sum(y => y.Value ?? 0),
                        LNSTGrowth = lnstGrowth
                    });
                }
                if (!lstData.Any()) return;
                //decimal croic = lstData.Sum(d => d.roic) / lstData.Count();
                //decimal croe = lstData.Sum(d => d.roe) / lstData.Count();
                //decimal csalesGrowhrate = lstData.Sum(d => d.saleGrowth) / lstData.Count();
                decimal cepsGrowhrate = lstData.Sum(d => d.epsGrowth) / lstData.Count();
                //decimal cbvpsGrowhrate = lstData.Sum(d => d.bvpsGrowth) / lstData.Count();
                //decimal ccashGrowhrate = lstData.Sum(d => d.cashGrowth) / lstData.Count();
                //var lastYear = groupedByYear.OrderByDescending(d => d.Key).First();
                //var noDaiHan = lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.tongTs).Sum(y => y.Value ?? 0)
                //        - lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.NoNganHan).Sum(y => y.Value ?? 0)
                //        - lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.VonChuSoHuu).Sum(y => y.Value ?? 0);
                //decimal ctimeToPayTheDept = noDaiHan != 0
                //        ? lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.lnstTuThuNhapDoanhNghiep).Sum(y => y.Value ?? 0) / noDaiHan
                //        : 0;

                //decimal chiSoTangTruong = croic;// (croe + croic + cepsGrowhrate) / 3;
                decimal PETrungBinhHangNam = lstData.Sum(d => d.PECoBan) / lstData.Count();
                //decimal PETrungBinhMinMax = (lstData.Min(d => d.PECoBan) + lstData.Max(d => d.PECoBan)) / 2;
                //decimal ChiSoTangTruongX2 = chiSoTangTruong * 2 * 100;
                //decimal expectedPE = Math.Min(PETrungBinhMinMax, ChiSoTangTruongX2);
                //decimal tileLoiNhuanToiThieu = 0.15M;

                //decimal MOSPrice50 =
                //    (lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0) * expectedPE
                //    * (decimal)Math.Pow((double)(1M + chiSoTangTruong) / (double)(1M + tileLoiNhuanToiThieu), 10))
                //    * 0.5M;

                //decimal MOSPrice80 =
                //    (lastYear.Value.Where(y => y.NameEn == ConstantData.NameEn.EPS4QuyGanNhat).Sum(y => y.Value ?? 0) * expectedPE
                //    * (decimal)Math.Pow((double)(1M + chiSoTangTruong) / (double)(1M + tileLoiNhuanToiThieu), 10))
                //    * 0.8M;

                decimal lnstGrowhrate = lstData.Sum(d => d.LNSTGrowth) / lstData.Count();
                decimal peToday = symbol._vhtt_ / lnst4QuyTruoc;
                //var peGrowthRateHienTaiSoVoiPeTrongThoiGianXet = (peToday / PETrungBinhHangNam) - 1;

                //var tiSuatTangTruongMongMuonSoVoiPE = peToday <= pe4QuyTruoc * (1 + lnstGrowhrate);

                var dk1 = lnstGrowhrate >= tangtruong;
                //var dk2 = lnst

                //var dk2 = pe4QuyTruoc <= peToday && peToday <= pe4QuyTruoc * (1 + tangtruong);
                //var dk3 = tiSuatTangTruongMongMuonSoVoiPE == true;

                if (dk1) //Start following
                {
                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;
                    //var text = $"{symbol._sc_} - LNST TB {range} năm: {Math.Round(lnstGrowhrate, 2)}, P/E hôm nay thấp hơn P/E 4 quý gần nhất * LNST là {Math.Round(peToday / pe4QuyTruoc * (1 + lnstGrowhrate), 2)}%)";
                    patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                    {
                        MoreInformation = new
                        {
                            LNST = lnstGrowhrate,
                            PE4QuyTruoc = pe4QuyTruoc,
                            PEHumNay = peToday,
                            RealityExpectation = string.Empty,
                            ShouldBuy = true
                        }
                    });

                    result.NhanDinhHDKD.Items.Add(patternOnsymbol);
                }
            });

            result.NhanDinhHDKD.Items = result.NhanDinhHDKD.Items.OrderBy(s => s.StockCode).ToList();

            return result;
        }

        /// <summary>
        /// Condition đơn giản
        /// - trong x nam
        ///     + Khong phải CP đang bị giao dịch 1 tuần/1 lần
        ///     + Trung bình giao dịch trong 30 phiên lần nhất > 100K
        ///     + Tăng trường lợi nhuận sau thuế trung bình trong x năm >= Y (số đầu vào - ví dụ 0.15 = 15%)
        ///         + Được tính bằng căn bậc N (số năm tính) của X (LNST năm cuối cùng / LNST năm đầu tiên)
        ///     + P/E <= 15% trung bình trong x năm
        /// </summary>
        /// <param name="code"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        //public async Task<PatternResponseModel> PhanTichDoanhNghiepTheoNamNoiDung(string code, int year, int range, double tangtruong,
        //    List<StockSymbol> symbols,
        //    List<StockSymbolFinanceYearlyHistory> stockSymbolFinanceYearlyHistories,
        //    List<History> stockSymbolHistoryToday,
        //    List<History> stockSymbolHistoryInRange,
        //    List<StockSymbolFinanceHistory> stockSymbolFinanceQuarterlyHistories)
        //{
        //    var result = new PatternResponseModel();

        //    //var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
        //    //var today = await _context.History.OrderByDescending(d => d.Date).FirstAsync();

        //    //var symbols = string.IsNullOrWhiteSpace(code)
        //    //    ? await _context.StockSymbol.Where(s => s._sc_.Length <= 3).ToListAsync()
        //    //    : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

        //    //var stockCodes = symbols.Select(s => s._sc_).ToList();

        //    //var stockSymbolFinanceYearlyHistories = await _context.StockSymbolFinanceYearlyHistory
        //    //    .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod >= (year - range - 1))
        //    //    .ToListAsync();

        //    //var stockSymbolHistoryToday = await _context.History
        //    //    .Where(f => stockCodes.Contains(f.StockSymbol) && f.Date == today.Date)
        //    //    .ToListAsync();

        //    //var stockSymbolHistoryInRange = await _context.History
        //    //    .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= today.Date.AddDays(-10))
        //    //    .OrderByDescending(ss => ss.Date)
        //    //    .ToListAsync();

        //    var quarterlyData = new List<string> { "P/E", "Profit after tax for shareholders of parent company" };
        //    //var stockSymbolFinanceQuarterlyHistories = await _context.StockSymbolFinanceHistory
        //    //    .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod == (year - 1) && quarterlyData.Contains(f.NameEn))
        //    //    .ToListAsync();

        //    Parallel.ForEach(symbols, symbol =>
        //    {
        //        if (symbol._vhtt_ <= 3000) return;

        //        var stockSymbolHistoryInRangeBySockCode = stockSymbolHistoryInRange
        //           .Where(ss => ss.StockSymbol == symbol._sc_)
        //           .OrderBy(s => s.Date)
        //           .ToList();

        //        var latestDate = stockSymbolHistoryInRangeBySockCode.OrderByDescending(h => h.Date).FirstOrDefault();
        //        var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(stockSymbolHistoryInRangeBySockCode);
        //        if (biCanhCao) return;

        //        var avarageOfLastXXPhien = stockSymbolHistoryInRangeBySockCode.Take(30).Sum(h => h.V) / 30;
        //        if (avarageOfLastXXPhien < 1000) return;

        //        var stockSymbolFinanceYearlyHistoryByStockCode = stockSymbolFinanceYearlyHistories
        //            .Where(ss => ss.StockSymbol == symbol._sc_)
        //            .OrderBy(s => s.YearPeriod)
        //            .ToList();
        //        decimal stockToday = stockSymbolHistoryToday.FirstOrDefault(t => t.StockSymbol == symbol._sc_)?.C ?? 0;
        //        if (!stockSymbolFinanceYearlyHistoryByStockCode.Any() || stockToday <= 0) return;


        //        var lnstNamDau = stockSymbolFinanceYearlyHistoryByStockCode
        //            .FirstOrDefault(d => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(d.NameEn) && d.Value.HasValue)
        //            ?.Value ?? 0;

        //        var lnstNamCuoi = stockSymbolFinanceYearlyHistoryByStockCode.OrderByDescending(d => d.YearPeriod)
        //            .FirstOrDefault(d => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(d.NameEn))
        //            ?.Value ?? 0;

        //        if (lnstNamDau == 0 || lnstNamCuoi == 0) return;

        //        var soLanTangLNST = (double)(lnstNamCuoi / lnstNamDau);
        //        var soNamKiemTra = stockSymbolFinanceYearlyHistoryByStockCode.Select(d => d.YearPeriod).Distinct().Count();

        //        double lnstTangTruongTrungBinhThucTe = soLanTangLNST.NthRoot(soNamKiemTra - 1) - 1;


        //        var groupedByYear = stockSymbolFinanceYearlyHistoryByStockCode.GroupBy(g => g.YearPeriod).ToDictionary(g => g.Key, g => g.ToList());

        //        var lstData = new List<StockSymbolFinanceYearlyHistoryModel>();

        //        var quarterly = stockSymbolFinanceQuarterlyHistories
        //            .Where(ss => ss.StockSymbol == symbol._sc_)
        //            .OrderBy(s => s.Quarter)
        //            .ToList();
        //        if (!quarterly.Any()) return;

        //        decimal pe4QuyTruoc = quarterly.Any(q => q.NameEn == quarterlyData[0])
        //            ? quarterly.Where(q => q.NameEn == quarterlyData[0]).Sum(d => d.Value ?? 0) / quarterly.Count(q => q.NameEn == quarterlyData[0])
        //            : 0;

        //        var t1 = quarterly.Where(q => q.NameEn == quarterlyData[1]).ToList();
        //        decimal lnst4QuyTruoc = t1.Sum(d => d.Value ?? 0);

        //        if (pe4QuyTruoc == 0 || lnst4QuyTruoc == 0) return; //cổ phiếu nhỏ lẻ

        //        foreach (var item in groupedByYear)
        //        {
        //            var previousYear = groupedByYear.ContainsKey(item.Key - 1) ? groupedByYear[item.Key - 1] : null;
        //            if (previousYear == null) continue;

        //            decimal lnstGrowth = previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0) != 0
        //                    ? item.Value.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
        //                      / previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
        //                      - 1
        //                    : 0;

        //            lstData.Add(new StockSymbolFinanceYearlyHistoryModel
        //            {
        //                Year = item.Key,
        //                PECoBan = item.Value.Where(y => y.NameEn == ConstantData.NameEn.PECoBan).Sum(y => y.Value ?? 0),
        //                LNSTGrowth = lnstGrowth
        //            });
        //        }
        //        if (!lstData.Any()) return;


        //        decimal lnstGrowhrate = lstData.Sum(d => d.LNSTGrowth) / lstData.Count();
        //        //decimal peToday = symbol._vhtt_ / lnst4QuyTruoc;
        //        var dk1 = lnstTangTruongTrungBinhThucTe >= tangtruong;

        //        if (dk1) //Start following
        //        {
        //            var patternOnsymbol = new PatternBySymbolResponseModel();
        //            patternOnsymbol.StockCode = symbol._sc_;
        //            //var text = $"{symbol._sc_} - LNST TB {range} năm: {Math.Round(lnstGrowhrate, 2)}, P/E hôm nay thấp hơn P/E 4 quý gần nhất * LNST là {Math.Round(peToday / pe4QuyTruoc * (1 + lnstGrowhrate), 2)}%)";
        //            patternOnsymbol.Details.Add(new PatternDetailsResponseModel
        //            {
        //                MoreInformation = new
        //                {
        //                    LNSTSoSach = lnstGrowhrate,
        //                    LNSTThucTe = lnstTangTruongTrungBinhThucTe,
        //                    RealityExpectation = string.Empty,
        //                    ShouldBuy = true
        //                }
        //            });

        //            result.NhanDinhHDKD.Items.Add(patternOnsymbol);
        //        }
        //    });

        //    result.NhanDinhHDKD.Items = result.NhanDinhHDKD.Items.OrderBy(s => s.StockCode).ToList();

        //    return result;
        //}

        /// <summary>
        /// Condition đơn giản
        /// - trong x nam
        ///     + Khong phải CP đang bị giao dịch 1 tuần/1 lần
        ///     + Trung bình giao dịch trong 30 phiên lần nhất > 100K
        ///     + Tăng trường lợi nhuận sau thuế trung bình trong x năm >= Y (số đầu vào - ví dụ 0.15 = 15%)
        ///         + Được tính bằng căn bậc N (số năm tính) của X (LNST năm cuối cùng / LNST năm đầu tiên)
        ///     + P/E <= 15% trung bình trong x năm
        /// </summary>
        /// <param name="code"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<PatternResponseModel> PhanTichDoanhNghiepTheoNamV2(string code, int year, int range, double tangtruong)
        {
            var result = new PatternResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var today = await _context.History.OrderByDescending(d => d.Date).FirstAsync();

            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length <= 3).ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();

            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var stockSymbolFinanceYearlyHistories = await _context.StockSymbolFinanceYearlyHistory
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod >= (year - range - 1))
                .ToListAsync();

            var stockSymbolHistoryToday = await _context.History
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.Date == today.Date)
                .ToListAsync();

            var stockSymbolHistoryInRange = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= today.Date.AddDays(-10))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            //var quarterlyData = new List<string> { "P/E", "Profit after tax for shareholders of parent company" };
            var stockSymbolFinanceQuarterlyLastYear = await _context.StockSymbolFinanceHistory
                .Where(f => stockCodes.Contains(f.StockSymbol) && f.YearPeriod == (year - 1) && ConstantData.NameEn.lnstTuTCDCtyMe.Contains(f.NameEn))
                .ToListAsync();

            Parallel.ForEach(symbols, async symbol =>
            {
                if (symbol._vhtt_ <= 3000) return;

                var stockSymbolHistoryInRangeBySockCode = stockSymbolHistoryInRange
                   .Where(ss => ss.StockSymbol == symbol._sc_)
                   .OrderBy(s => s.Date)
                   .ToList();

                var latestDate = stockSymbolHistoryInRangeBySockCode.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(stockSymbolHistoryInRangeBySockCode);
                if (biCanhCao) return;

                var avarageOfLastXXPhien = stockSymbolHistoryInRangeBySockCode.Take(30).Sum(h => h.V) / 30;
                if (avarageOfLastXXPhien < ConstantData.minMA20VolDaily) return;

                var stockSymbolFinanceYearlyHistoryByStockCode = stockSymbolFinanceYearlyHistories
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.YearPeriod)
                    .ToList();

                var stockToday = stockSymbolHistoryToday.FirstOrDefault(t => t.StockSymbol == symbol._sc_);
                decimal stockTodayPrice = stockToday?.C ?? 0;

                if (!stockSymbolFinanceYearlyHistoryByStockCode.Any() || stockTodayPrice <= 0) return;



                var lnstNamDau = stockSymbolFinanceYearlyHistoryByStockCode
                    .FirstOrDefault(d => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(d.NameEn) && d.Value.HasValue)
                    ?.Value ?? 0;

                var lnstNamCuoi = stockSymbolFinanceYearlyHistoryByStockCode.OrderByDescending(d => d.YearPeriod)
                    .FirstOrDefault(d => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(d.NameEn))
                    ?.Value ?? 0;

                if (lnstNamDau == 0 || lnstNamCuoi == 0) return;

                var soLanTangLNST = (double)(lnstNamCuoi / lnstNamDau);
                var soNamKiemTra = stockSymbolFinanceYearlyHistoryByStockCode.Select(d => d.YearPeriod).Distinct().Count();

                double lnstTangTruongTrungBinhThucTe = soLanTangLNST.NthRoot(soNamKiemTra - 1) - 1;


                var groupedByYear = stockSymbolFinanceYearlyHistoryByStockCode.GroupBy(g => g.YearPeriod).ToDictionary(g => g.Key, g => g.ToList());

                var lstData = new List<StockSymbolFinanceYearlyHistoryModel>();

                var quarterly = stockSymbolFinanceQuarterlyLastYear
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(s => s.Quarter)
                    .ToList();
                if (!quarterly.Any()) return;

                //decimal pe4QuyTruoc = quarterly.Any(q => q.NameEn == quarterlyData[0])
                //    ? quarterly.Where(q => q.NameEn == quarterlyData[0]).Sum(d => d.Value ?? 0) / quarterly.Count(q => q.NameEn == quarterlyData[0])
                //    : 0;

                //var t1 = quarterly.Where(q => q.NameEn == quarterlyData[1]).ToList();
                //decimal lnst4QuyTruoc = t1.Sum(d => d.Value ?? 0);

                //if (pe4QuyTruoc == 0 || lnst4QuyTruoc == 0) return; //cổ phiếu nhỏ lẻ

                foreach (var item in groupedByYear)
                {
                    var previousYear = groupedByYear.ContainsKey(item.Key - 1) ? groupedByYear[item.Key - 1] : null;
                    if (previousYear == null) continue;

                    decimal lnstGrowth = previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0) != 0
                            ? item.Value.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
                              / previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
                              - 1
                            : 0;

                    lstData.Add(new StockSymbolFinanceYearlyHistoryModel
                    {
                        Year = item.Key,
                        PECoBan = item.Value.Where(y => y.NameEn == ConstantData.NameEn.PECoBan).Sum(y => y.Value ?? 0),
                        LNSTGrowth = lnstGrowth
                    });
                }
                if (!lstData.Any()) return;


                decimal lnstGrowhrate = lstData.Sum(d => d.LNSTGrowth) / lstData.Count();
                //decimal peToday = symbol._vhtt_ / lnst4QuyTruoc;

                var dk1 = lnstTangTruongTrungBinhThucTe >= tangtruong;

                var dataOfDK2 = await PhanTichTungDoanhNghiepTheoNam(symbol, year, 4, tangtruong, stockToday, stockSymbolFinanceYearlyHistoryByStockCode, stockSymbolFinanceQuarterlyLastYear);
                var dk2 = dataOfDK2 != null && dataOfDK2.NhanDinhHDKD != null && dataOfDK2.NhanDinhHDKD.Items.Any();

                if (dk1 && dk2) //Start following
                {
                    var patternOnsymbol = new PatternBySymbolResponseModel();
                    patternOnsymbol.StockCode = symbol._sc_;
                    //var text = $"{symbol._sc_} - LNST TB {range} năm: {Math.Round(lnstGrowhrate, 2)}, P/E hôm nay thấp hơn P/E 4 quý gần nhất * LNST là {Math.Round(peToday / pe4QuyTruoc * (1 + lnstGrowhrate), 2)}%)";
                    patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                    {
                        MoreInformation = new
                        {
                            LNSTSoSach10Nam = lnstGrowhrate,
                            LNSTThucTe10Nam = lnstTangTruongTrungBinhThucTe,
                            LNSTThucTe4Nam = dataOfDK2.NhanDinhHDKD.Items.First().Details.First().MoreInformation.LNSTThucTe,
                            PEToday = dataOfDK2.NhanDinhHDKD.Items.First().Details.First().MoreInformation.PEToday,
                            PE4Nam = dataOfDK2.NhanDinhHDKD.Items.First().Details.First().MoreInformation.PE4Nam,
                            RealityExpectation = string.Empty,
                            ShouldBuy = true
                        }
                    });

                    result.NhanDinhHDKD.Items.Add(patternOnsymbol);
                }
            });

            result.NhanDinhHDKD.Items = result.NhanDinhHDKD.Items.OrderBy(s => s.StockCode).ToList();

            return result;
        }

        /// <summary>
        /// Condition đơn giản
        /// - trong x nam
        ///     + Khong phải CP đang bị giao dịch 1 tuần/1 lần
        ///     + Trung bình giao dịch trong 30 phiên lần nhất > 100K
        ///     + Tăng trường lợi nhuận sau thuế trung bình trong x năm >= Y (số đầu vào - ví dụ 0.15 = 15%)
        ///         + Được tính bằng căn bậc N (số năm tính) của X (LNST năm cuối cùng / LNST năm đầu tiên)
        ///     + P/E <= 15% trung bình trong x năm
        /// </summary>
        /// <param name="code"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private async Task<PatternResponseModel> PhanTichTungDoanhNghiepTheoNam(StockSymbol symbol, int year, int range, double tangtruong,
            History stockSymbolHistoryToday,
            List<StockSymbolFinanceYearlyHistory> stockSymbolFinanceYearlyHistoryByStockCodeAllYears,
            List<StockSymbolFinanceHistory> stockSymbolFinanceQuarterlyHistoriesByStockCodeAllYears)
        {
            var result = new PatternResponseModel();

            var stockSymbolFinanceYearlyHistoryByStockCode = stockSymbolFinanceYearlyHistoryByStockCodeAllYears.Where(y => y.YearPeriod >= (year - range - 1)).ToList();
            var stockSymbolFinanceQuarterlyHistories = stockSymbolFinanceQuarterlyHistoriesByStockCodeAllYears.Where(y => y.YearPeriod >= (year - range - 1)).ToList();

            if (stockSymbolFinanceYearlyHistoryByStockCode.Select(d => d.YearPeriod).Distinct().Count() < 1) return null;
            if (stockSymbolFinanceQuarterlyHistories.Select(d => d.YearPeriod).Distinct().Count() < 1) return null;

            var lnstNamDau = stockSymbolFinanceYearlyHistoryByStockCode
                .FirstOrDefault(d => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(d.NameEn) && d.Value.HasValue)
                ?.Value ?? 0;
            var lnstNamCuoi = stockSymbolFinanceYearlyHistoryByStockCode.OrderByDescending(d => d.YearPeriod)
                .FirstOrDefault(d => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(d.NameEn))
                ?.Value ?? 0;
            if (lnstNamDau == 0 || lnstNamCuoi == 0) return null;

            var soLanTangLNST = (double)(lnstNamCuoi / lnstNamDau);
            var soNamKiemTra = stockSymbolFinanceYearlyHistoryByStockCode.Select(d => d.YearPeriod).Distinct().Count();
            double lnstTangTruongTrungBinhThucTe = soLanTangLNST.NthRoot(soNamKiemTra - 1) - 1;

            var lstData = new List<StockSymbolFinanceYearlyHistoryModel>();
            var groupedByYear = stockSymbolFinanceYearlyHistoryByStockCode.GroupBy(g => g.YearPeriod).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var item in groupedByYear)
            {
                var previousYear = groupedByYear.ContainsKey(item.Key - 1) ? groupedByYear[item.Key - 1] : null;
                if (previousYear == null) continue;

                decimal lnstGrowth = previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0) != 0
                        ? item.Value.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
                          / previousYear.Where(y => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(y.NameEn)).Sum(y => y.Value ?? 0)
                          - 1
                        : 0;

                lstData.Add(new StockSymbolFinanceYearlyHistoryModel
                {
                    Year = item.Key,
                    PECoBan = item.Value.Where(y => y.NameEn == ConstantData.NameEn.PECoBan).Sum(y => y.Value ?? 0),
                    LNSTGrowth = lnstGrowth
                });
            }
            if (!lstData.Any()) return null;
            decimal peTrungBinhHangNam = lstData.Sum(d => d.PECoBan) / lstData.Count();

            var quarterly = stockSymbolFinanceQuarterlyHistories
                .Where(ss => ss.StockSymbol == symbol._sc_)
                .OrderBy(s => s.Quarter)
                .ToList();
            if (!quarterly.Any()) return null;

            var lnstTuTCDCtyMe = quarterly.Where(q => ConstantData.NameEn.lnstTuTCDCtyMe.Contains(q.NameEn)).ToList();
            decimal lnst4QuyTruoc = lnstTuTCDCtyMe.Sum(d => d.Value ?? 0);
            decimal peToday = symbol._vhtt_ / lnst4QuyTruoc;
            if (peTrungBinhHangNam == 0 || lnst4QuyTruoc == 0) return null; //cổ phiếu nhỏ lẻ

            var dk1 = lnstTangTruongTrungBinhThucTe >= tangtruong;
            var dk2 = peToday < peTrungBinhHangNam * 1.15M;
            if (dk1 && dk2)
            {
                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;
                //var text = $"{symbol._sc_} - LNST TB {range} năm: {Math.Round(lnstGrowhrate, 2)}, P/E hôm nay thấp hơn P/E 4 quý gần nhất * LNST là {Math.Round(peToday / pe4QuyTruoc * (1 + lnstGrowhrate), 2)}%)";
                patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                {
                    MoreInformation = new
                    {
                        //LNSTSoSach = lnstGrowhrate,
                        LNSTThucTe = lnstTangTruongTrungBinhThucTe,
                        PEToday = peToday,
                        PE4Nam = peTrungBinhHangNam,
                        RealityExpectation = string.Empty,
                        ShouldBuy = true
                    }
                });

                result.NhanDinhHDKD.Items.Add(patternOnsymbol);
            }

            result.NhanDinhHDKD.Items = result.NhanDinhHDKD.Items.OrderBy(s => s.StockCode).ToList();

            return result;
        }

        public async Task<PredictionResultModel> Prediction(string name)
        {
            var result = new PredictionResultModel();
            string path = @"C:\Projects\Test\Stock-app\Data\Json\Prediction\Prediction.json";
            var data = new List<PredictionModel>();
            using (StreamReader r = new StreamReader(path))
            {
                string json = await r.ReadToEndAsync();
                data = JsonConvert.DeserializeObject<List<PredictionModel>>(json);
            }

            //TODO: apply filter by name

            var minDate = data.SelectMany(d => d.Prediction).OrderBy(d => d.Ngay).First().Ngay;

            foreach (var username in data)
                foreach (var prediction in username.Prediction)
                {
                    foreach (var item in prediction.DuLieu)
                    {
                        var code = item.Split('-')[0];
                        var suggestedPrice = item.Split('-')[1];

                        prediction.DuLieuDuocPhanTich.Add(new PredictionDataModel { Code = code, SuggestedPrice = decimal.Parse(suggestedPrice) });
                    }
                }


            var selectedCodes = data.SelectMany(d => d.Prediction).SelectMany(d => d.DuLieuDuocPhanTich).Select(d => d.Code).Distinct().ToList();

            var codeHistories = await _context.History.Where(h => selectedCodes.Contains(h.StockSymbol) && h.Date >= minDate).ToListAsync();

            Parallel.ForEach(data, symbol =>
            {
                foreach (var predictionData in symbol.Prediction)
                {
                    foreach (var duLieu in predictionData.DuLieuDuocPhanTich)
                    {
                        var dataInRange = codeHistories.Where(h => h.StockSymbol == duLieu.Code && h.Date >= predictionData.Ngay)
                            .OrderBy(h => h.Date)
                            .Take(4)
                            .ToList();

                        if (!dataInRange.Any()) continue;

                        var isTheSuggestedPriceOk = dataInRange[0].L <= duLieu.SuggestedPrice * 1000;
                        if (isTheSuggestedPriceOk)
                        {
                            if (dataInRange.Last().C >= duLieu.SuggestedPrice * 1000 * 1.01M)
                                result.Details.Add(new PredictionResultDetailsModel { Code = duLieu.Code, Ngay = predictionData.Ngay, Result = true, Username = symbol.Name });
                            else
                                result.Details.Add(new PredictionResultDetailsModel { Code = duLieu.Code, Ngay = predictionData.Ngay, Username = symbol.Name });
                        }
                    }
                }
            });

            result.Rate = Math.Round((decimal)((decimal)result.Details.Count(d => d.Result) / (decimal)result.Details.Count()), 2);


            return result;
        }

        //public async Task<PatternDetailsResponseModel> RSI(string code,
        //    int maCanhBaoMua,
        //    int maCanhBaoBan,
        //    DateTime ngay)
        //{
        //    var result = new PatternDetailsResponseModel();
        //    int rsi = 14;
        //    var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
        //    var symbols = string.IsNullOrWhiteSpace(code)
        //        ? await _context.StockSymbol.ToListAsync()
        //        : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();
        //    var stockCodes = symbols.Select(s => s._sc_).ToList();

        //    var historiesInPeriodOfTimeByStockCode = await _context.History
        //        .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date <= ngay)
        //        .OrderByDescending(ss => ss.Date)
        //        .ToListAsync();

        //    var lstNhacMua = new List<string>();
        //    var lstNhacBan = new List<string>();

        //    Parallel.ForEach(symbols, symbol =>
        //    {
        //        var histories = historiesInPeriodOfTimeByStockCode
        //            .Where(ss => ss.StockSymbol == symbol._sc_)
        //            .ToList();

        //        var latestDate = histories.OrderByDescending(h => h.Date).FirstOrDefault();

        //        //var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(histories);
        //        //if (biCanhCao) return;

        //        //var avarageOfLastXXPhien = histories.Take(soPhienGd).Sum(h => h.V) / soPhienGd;
        //        //if (avarageOfLastXXPhien < trungbinhGd) return;


        //        var patternOnsymbol = new PatternBySymbolResponseModel();
        //        patternOnsymbol.StockCode = symbol._sc_;

        //        var history = histories.FirstOrDefault(h => h.Date == ngay);
        //        if (history == null) return;

        //        var yesterday = histories.FirstOrDefault(h => h.Date < ngay);
        //        if (yesterday == null) return;

        //        var rsiToday = history.RSI(histories, rsi);
        //        var rsiYesterday = history.RSI(histories, rsi);

        //        var diemNhacMua = history.MA(histories, maCanhBaoMua);
        //        var diemCanhBaoBan = history.MA(histories, maCanhBaoBan);

        //        var dk1 = rsiYesterday < diemNhacMua && rsiToday > diemNhacMua;

        //        var dk2 = rsiToday > diemCanhBaoBan || rsiToday < diemNhacMua;

        //        if (dk1) lstNhacMua.Add(code);
        //        if (dk2) lstNhacBan.Add(code);
        //    });

        //    if (lstNhacMua.Any() || lstNhacBan.Any())
        //        result.ConditionMatchAt = ngay;

        //    result.MoreInformation = new
        //    {
        //        NhacMua = string.Join(", ", lstNhacMua),
        //        NhacBan = string.Join(", ", lstNhacBan),
        //    };

        //    return result;
        //}

        public async Task<PatternDetailsResponseModel> MACD(string code,
            int maCanhBaoMua,
            int maCanhBaoBan,
            int rsi,
            DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternDetailsResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var maxRangeList = new List<int> { maCanhBaoBan, maCanhBaoMua, rsi, soPhienGd };
            var maxRange = maxRangeList.Max() + 5;//TODO: tại sao 5 ? vì mình cần lấy rsi + 1, cần lấy rsi ngày hum wa xem có phải điểm mua là ngày hum nay ko

            var historiesInPeriodOfTimeByStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date <= ngay)
                .OrderByDescending(ss => ss.Date)
                .Take(maxRange)
                .ToListAsync();

            //if (historiesInPeriodOfTimeByStockCode.FirstOrDefault() != null && historiesInPeriodOfTimeByStockCode.First().Date < ngay && ngay.Date == DateTime.Today.WithoutHours())
            //{
            //    var newPackages = new List<History>();
            //    var from = DateTime.Now.WithoutHours();
            //    var to = DateTime.Now.WithoutHours().AddDays(1);

            //    var service = new Service();
            //    await service.GetV(newPackages, symbols, from, to, from, 0);

            //    historiesInPeriodOfTimeByStockCode.AddRange(newPackages);
            //}

            var lstNhacMua = new List<string>();
            var lstNhacBan = new List<string>();

            Parallel.ForEach(symbols, symbol =>
            {
                var histories = historiesInPeriodOfTimeByStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .ToList();

                var latestDate = histories.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(histories);

                if (biCanhCao) return;

                var avarageOfLastXXPhien = histories.Take(soPhienGd).Sum(h => h.V) / soPhienGd;
                if (avarageOfLastXXPhien < trungbinhGd) return;


                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                var yesterday = histories.FirstOrDefault(h => h.Date < ngay);
                if (yesterday == null) return;

                var rsiToday = history.RSI(histories, rsi);
                var rsiYesterday = history.RSI(histories, rsi);

                var diemNhacMua = history.MA(histories, maCanhBaoMua);
                var diemCanhBaoBan = history.MA(histories, maCanhBaoBan);

                var dk1 = rsiYesterday < diemNhacMua && rsiToday > diemNhacMua;

                var dk2 = rsiToday > diemCanhBaoBan || rsiToday < diemNhacMua;

                if (dk1) lstNhacMua.Add(code);
                if (dk2) lstNhacBan.Add(code);
            });

            if (lstNhacMua.Any() || lstNhacBan.Any())
                result.ConditionMatchAt = ngay;

            result.MoreInformation = new
            {
                NhacMua = string.Join(", ", lstNhacMua),
                NhacBan = string.Join(", ", lstNhacBan),
            };

            return result;
        }


        public async Task<PatternDetailsResponseModel> Stoch(string code,
            int maCanhBaoMua,
            int maCanhBaoBan,
            int rsi,
            DateTime ngay, int soPhienGd, int trungbinhGd)
        {
            var result = new PatternDetailsResponseModel();

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.ToListAsync()
                : await _context.StockSymbol.Where(s => splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var maxRangeList = new List<int> { maCanhBaoBan, maCanhBaoMua, rsi, soPhienGd };
            var maxRange = maxRangeList.Max() + 5;//TODO: tại sao 5 ? vì mình cần lấy rsi + 1, cần lấy rsi ngày hum wa xem có phải điểm mua là ngày hum nay ko

            var historiesInPeriodOfTimeByStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date <= ngay)
                .OrderByDescending(ss => ss.Date)
                .Take(maxRange)
                .ToListAsync();

            //if (historiesInPeriodOfTimeByStockCode.FirstOrDefault() != null && historiesInPeriodOfTimeByStockCode.First().Date < ngay && ngay.Date == DateTime.Today.WithoutHours())
            //{
            //    var newPackages = new List<History>();
            //    var from = DateTime.Now.WithoutHours();
            //    var to = DateTime.Now.WithoutHours().AddDays(1);

            //    var service = new Service();
            //    await service.GetV(newPackages, symbols, from, to, from, 0);

            //    historiesInPeriodOfTimeByStockCode.AddRange(newPackages);
            //}

            var lstNhacMua = new List<string>();
            var lstNhacBan = new List<string>();

            Parallel.ForEach(symbols, symbol =>
            {
                var histories = historiesInPeriodOfTimeByStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .ToList();

                var latestDate = histories.OrderByDescending(h => h.Date).FirstOrDefault();
                var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(histories);

                if (biCanhCao) return;

                var avarageOfLastXXPhien = histories.Take(soPhienGd).Sum(h => h.V) / soPhienGd;
                if (avarageOfLastXXPhien < trungbinhGd) return;


                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                var yesterday = histories.FirstOrDefault(h => h.Date < ngay);
                if (yesterday == null) return;

                var rsiToday = history.RSI(histories, rsi);
                var rsiYesterday = history.RSI(histories, rsi);

                var diemNhacMua = history.MA(histories, maCanhBaoMua);
                var diemCanhBaoBan = history.MA(histories, maCanhBaoBan);

                var dk1 = rsiYesterday < diemNhacMua && rsiToday > diemNhacMua;

                var dk2 = rsiToday > diemCanhBaoBan || rsiToday < diemNhacMua;

                if (dk1) lstNhacMua.Add(code);
                if (dk2) lstNhacBan.Add(code);
            });

            if (lstNhacMua.Any() || lstNhacBan.Any())
                result.ConditionMatchAt = ngay;

            result.MoreInformation = new
            {
                NhacMua = string.Join(", ", lstNhacMua),
                NhacBan = string.Join(", ", lstNhacBan),
            };

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="ngay"></param>
        /// <param name="ngayCuoi"></param>
        /// <param name="KC2D"></param>
        /// <param name="SoPhienKT"></param>
        /// <param name="CL2D">Chênh lệch 2 đáy: Đáy RSI 1 * 0.98 > Đáy RSI 2 (đáy mới thấp hơn đáy cũ) - đáy 2 phải thấp hơn đáy 1 ít nhất 2%</param>
        /// <returns></returns>
        public async Task<List<string>> RSITestV1(string code, DateTime ngay, DateTime ngayCuoi, int KC2D, int SoPhienKT, decimal CL2D)
        {
            int rsi = 14;

            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily).ToListAsync()
                : await _context.StockSymbol.Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10)
                    && ss.Date >= ngayCuoi.AddDays(-(SoPhienKT * 3)))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var result1 = new List<string>();

            Parallel.ForEach(symbols, symbol =>
            {
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var histories = historiesStockCode
                                    .Where(ss => ss.StockSymbol == symbol._sc_)
                                    .OrderBy(h => h.Date)
                                    .ToList();
                var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngayCuoi);
                for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
                {
                    ngayBatDau = histories[i];
                    if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                    {
                        break;
                    }
                }

                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= ngay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);

                for (int i = startedI; i < stoppedI; i++)
                {
                    var buyingDate = histories[i];
                    //Giả định ngày trước đó là đáy
                    var dayGiaDinh = histories[i - 1];
                    //hum nay RSI tăng so với hum wa, thì hum wa mới là đáy, còn ko thì mai RSI vẫn có thể giảm tiếp, ko ai bik
                    if (dayGiaDinh.RSI == 0
                        || buyingDate.RSI <= dayGiaDinh.RSI
                        || buyingDate.NenBot < dayGiaDinh.NenBot)
                    {
                        continue;
                    }

                    //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                    var nhungNgaySoSanhVoiDayGiaDinh = histories.OrderByDescending(h => h.Date).Where(h => h.Date < dayGiaDinh.Date).Take(SoPhienKT).ToList();
                    if (nhungNgaySoSanhVoiDayGiaDinh.Count < SoPhienKT) continue;

                    var ngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh = nhungNgaySoSanhVoiDayGiaDinh.OrderBy(h => h.RSI).First();
                    var indexOfDayGiaDinh = histories.IndexOf(dayGiaDinh);
                    var indexOfngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh = histories.IndexOf(ngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh);
                    if (indexOfDayGiaDinh - indexOfngayCoRSIThapNhatDeSoSanhVoiDayGiaDinh <= KC2D) continue; //đáy RSI thấp nhất không dc nằm trong 3 ngày tính từ ngày của đáy giả định trở về trước

                    for (int j = KC2D; j < nhungNgaySoSanhVoiDayGiaDinh.Count - 1; j++)
                    {
                        var ngàyĐếmNgược = nhungNgaySoSanhVoiDayGiaDinh[j];

                        var hasPhanKyDuong = histories.HasPhanKyDuong(dayGiaDinh, ngàyĐếmNgược, "RSI", SoPhienKT, CL2D);
                        if (hasPhanKyDuong != null && hasPhanKyDuong.HasValue && hasPhanKyDuong.Value)
                        {
                            var tileChinhXac = 0;
                            var tPlus = histories.Where(h => h.Date >= buyingDate.Date)
                                .OrderBy(h => h.Date)
                                .Skip(3)
                                .Take(3)
                                .ToList();

                            if (tPlus.Any(t => t.C > buyingDate.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                            {
                                dung++;
                                result1.Add($"{symbol._sc_} - Đúng - T3-5 lời - Điểm xét: {buyingDate.Date.ToShortDateString()} - Đáy RSI: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm tín hiệu: {ngàyĐếmNgược.Date.ToShortDateString()} RSI {ngàyĐếmNgược.RSI.ToString("N2")} - Giá {ngàyĐếmNgược.NenBot.ToString("N2")}");
                            }
                            else
                            {
                                sai++;
                                result1.Add($"{symbol._sc_} - Sai  - T3-5 lỗ  - Điểm xét: {buyingDate.Date.ToShortDateString()} - Đáy RSI: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm tín hiệu: {ngàyĐếmNgược.Date.ToShortDateString()} RSI {ngàyĐếmNgược.RSI.ToString("N2")} - Giá {ngàyĐếmNgược.NenBot.ToString("N2")}");
                            }

                        }
                    }
                }
            });

            //tong = dung + sai;
            //var tile = Math.Round(dung / tong, 2);
            //result1.Add($"Tỉ lệ: {tile}");
            return result1;
        }



        public async Task<List<Tuple<string, decimal, List<string>>>> RSITestRSI(string code, DateTime ngay, DateTime ngayCuoi, int KC2D, int SoPhienKT, int ma20vol)
        {
            var result = new PatternDetailsResponseModel();
            int rsi = 14;
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.BiChanGiaoDich == false && s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s.BiChanGiaoDich == false && s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10) //calculate T
                    && ss.Date >= ngayCuoi.AddDays(-30)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var historiesInPeriodOfTime = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();

                var histories = historiesInPeriodOfTime
                    .Where(ss => ss.Date <= ngay && ss.Date >= ngayCuoi)
                    .OrderByDescending(h => h.Date)
                    .ToList();

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                var yesterday = histories.FirstOrDefault(h => h.Date < ngay);
                if (yesterday == null) return;

                for (int i = 0; i < histories.Count - 1; i++)
                {
                    var buyingDate = histories[i];

                    //Giả định ngày trước đó là đáy
                    var dayGiaDinh = histories[i + 1];

                    //trước đây đáy phải là 1 cây giảm:
                    if (i + 1 + 1 <= histories.Count - 1)
                    {
                        var cayTruocCayDayGiaDinh = histories[i + 1 + 1];
                        if (cayTruocCayDayGiaDinh.TangGia()) continue;
                    }

                    //Kiem tra đáy giả định: trong vòng 14 phiên trước không có cây nào trước đó thấp hơn nó
                    var rsi14Period = histories.Where(h => h.Date < dayGiaDinh.Date).Take(SoPhienKT).ToList();
                    if (rsi14Period.Count < SoPhienKT) continue;

                    var nhungNgaySoSanhVoiDayGiaDinh = rsi14Period.OrderByDescending(h => h.Date).Skip(KC2D).ToList();

                    var ngaySoSanhVoiDayGiaDinh = nhungNgaySoSanhVoiDayGiaDinh.Where(h => h.NenBot == nhungNgaySoSanhVoiDayGiaDinh.Min(f => f.NenBot)).OrderBy(h => h.RSI).First();

                    var isDayGiaDinhDung = ngaySoSanhVoiDayGiaDinh.NenBot > dayGiaDinh.NenBot;
                    if (isDayGiaDinhDung == false) continue;

                    var isRSIIncreasing = ngaySoSanhVoiDayGiaDinh.RSI < dayGiaDinh.RSI;

                    //TODO: giữa 2 điểm so sánh, ko có 1 điểm nào xen ngang vô cả
                    var middlePoints = histories.Where(h => h.Date > ngaySoSanhVoiDayGiaDinh.Date && h.Date < dayGiaDinh.Date).ToList();
                    if (middlePoints.Count < 2) continue; //ở giữa ít nhất 2 điểm

                    var trendLineRSI = new Line();
                    trendLineRSI.x1 = 0;  //x là trục tung - trục đối xứng
                    trendLineRSI.y1 = ngaySoSanhVoiDayGiaDinh.RSI;   //
                    trendLineRSI.x2 = (decimal)((dayGiaDinh.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    trendLineRSI.y2 = dayGiaDinh.RSI;
                    var crossLine = new Line();
                    crossLine.x1 = (decimal)((middlePoints.OrderByDescending(h => h.RSI).First().Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    crossLine.y1 = middlePoints.OrderByDescending(h => h.RSI).First().RSI;
                    crossLine.x2 = (decimal)((middlePoints.OrderByDescending(h => h.RSI).Last().Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    crossLine.y2 = middlePoints.OrderByDescending(h => h.RSI).Last().RSI;

                    //var trendLine = new Line();
                    //trendLine.x1 = 0;  //x là trục tung - trục đối xứng
                    //trendLine.y1 = ngaySoSanhVoiDayGiaDinh.NenBot;   //
                    //trendLine.x2 = (decimal)((dayGiaDinh.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    //trendLine.y2 = dayGiaDinh.NenBot;

                    //var crossLine = new Line();
                    //var point1 = middlePoints.OrderByDescending(h => h.NenBot).First();
                    //var point2 = middlePoints.OrderByDescending(h => h.NenBot).Last();
                    //crossLine.x1 = (decimal)((point1.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    //crossLine.y1 = point1.NenBot;
                    //crossLine.x2 = (decimal)((point2.Date - ngaySoSanhVoiDayGiaDinh.Date).TotalDays);
                    //crossLine.y2 = point2.NenBot;

                    var point = trendLineRSI.FindIntersection(crossLine);

                    if (isRSIIncreasing && point == null)
                    {
                        var tileChinhXac = 0;
                        var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= buyingDate.Date)
                            .OrderBy(h => h.Date)
                            .Skip(3)
                            .Take(3)
                            .ToList();

                        if (tPlus.Any(t => t.C > buyingDate.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                        {
                            dung++;
                            result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai mua: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm so sánh: {ngaySoSanhVoiDayGiaDinh.Date.ToShortDateString()} RSI {ngaySoSanhVoiDayGiaDinh.RSI.ToString("N2")} - Giá {ngaySoSanhVoiDayGiaDinh.NenBot.ToString("N2")}");
                        }
                        else
                        {
                            sai++;
                            result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai mua: {dayGiaDinh.Date.ToShortDateString()} RSI {dayGiaDinh.RSI.ToString("N2")} - Giá {dayGiaDinh.NenBot.ToString("N2")} - Điểm so sánh: {ngaySoSanhVoiDayGiaDinh.Date.ToShortDateString()} RSI {ngaySoSanhVoiDayGiaDinh.RSI.ToString("N2")} - Giá {ngaySoSanhVoiDayGiaDinh.NenBot.ToString("N2")}");
                        }
                    }
                }

                tong = dung + sai;
                var tile = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                //result1.Add($"Tỉ lệ: {tile}");
                tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, tile, result1));
            });

            return tup;
        }

        public async Task<List<Tuple<string, decimal, List<string>>>> RSITestMANhanhCatCham(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            var result = new PatternDetailsResponseModel();
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10) //calculate T
                    && ss.Date >= ngayCuoi.AddDays(-MACham * 2)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var NhậtKýMuaBán = new List<Tuple<string, DateTime, bool, decimal>>();

                var historiesInPeriodOfTime = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();
                if (historiesInPeriodOfTime.Count < 100) return;


                var ngayCuoiCuaMa = historiesInPeriodOfTime[0].Date.AddDays(30) > ngayCuoi
                    ? historiesInPeriodOfTime[0].Date.AddDays(30)
                    : ngayCuoi;

                var histories = historiesInPeriodOfTime
                    .Where(ss => ss.Date <= ngay && ss.Date >= ngayCuoiCuaMa)
                    .OrderBy(h => h.Date)
                    .ToList();

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                for (int i = 0; i < histories.Count; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = historiesInPeriodOfTime.Where(h => h.Date < phienHumNay.Date).OrderByDescending(h => h.Date).First();
                    var phienHumKia = historiesInPeriodOfTime.Where(h => h.Date < phienHumWa.Date).OrderByDescending(h => h.Date).First();

                    var phienHumNayMa20 = phienHumNay.MA(historiesInPeriodOfTime, -MACham);
                    var phienHumNayMa05 = phienHumNay.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa05 = phienHumWa.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa20 = phienHumWa.MA(historiesInPeriodOfTime, -MACham);


                    //tín hiệu mua
                    var Ma05DuoiMa20 = phienHumNayMa05 < phienHumNayMa20;
                    var MA05HuongLen = phienHumWaMa05 < phienHumNayMa05;
                    var nenTangGia = phienHumNay.TangGia();
                    var nenTangLenChamMa20 = phienHumNay.NenTop >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;             //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var râunếnTangLenChamMa20 = phienHumNay.H >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;               //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var duongMa05CatLenTrenMa20 = phienHumWaMa05 < phienHumNayMa20 && phienHumNayMa05 > phienHumNayMa20;     //MA 05 cắt lên trên MA 20
                    var nenNamDuoiMA20 = phienHumNay.NenBot < phienHumNayMa20;                                                            //Giá nằm dưới MA 20
                    var thânNếnKhôngVượtQuáBandTren = phienHumNay.NenTop < phienHumNay.BandsTop;


                    /*TODO: cảnh báo mua nếu giá mở cửa tăng chạm bands trên. Ví dụ 9/4/2021 KBC, 17/1/2022 VNM - tăng gần chạm bands mà ko thấy dấu hiệu mở bands rộng ra, mA 20 cũng ko hướng lên - sideway
                     * 10-2-22 VNM: bands ko mở rộng, bands tren hướng xuống, bands dưới đi ngang, các giá trước loanh quanh MA 20, ko có dấu hiệu phá bỏ sideway
                     * var khángCựĐỉnh = phienKiemTra.KhángCựĐỉnh(historiesInPeriodOfTime);
                     * var khángCựBands = phienKiemTra.KhángCựBands(historiesInPeriodOfTime);
                     * var khángCựFibonacci = phienKiemTra.KhángCựFibonacci(historiesInPeriodOfTime);
                     * var khángCựIchimoku   = phienKiemTra.KhángCựIchimoku(historiesInPeriodOfTime);
                     * 
                     * Ví dụ: 
                     *  KBC - 22/3/22 (MA + bands) -> nhưng có thể xét vì giá đã tăng gần tới viền mây dưới ichimoku rồi nên ko mua, hoặc giá từ MA 5 đi lên (hổ trợ lên kháng cự) ở MA 20, và bands bóp nên giá chỉ quay về MA20
                     *  KBC - 03/3/20 (MA + bands) -> MA, Bands đi ngang, có thể mua để lợi T+ => thất bại -> có thể xét tới MACD trong trường hợp này:
                     *                              + MACD dưới 0, đỏ cắt lên xanh
                     *                              + Đã mua ở ngày 26/2 rồi, MACD chưa vượt 0 lên dương mạnh thì cũng ko cần mua thêm
                     *                              + => bands hẹp, bands ko thay đổi, ma 20 k thay đổi, thân nến ở giữa bands => ko mua, vì rất dễ sảy T3
                     *                                      
                     *  KBC - 10/3/20 -> 17/3/20 - Nếu bất chấp nến ngoài bolinger bands dưới để mua, thì hãy cân nhắc KBC trong những ngày này -> nên kết hợp MACD (macd giảm, momentum âm mạnh) 
                     *                              + => CThuc A
                     *                                                     
                     *  KBC - 27/03/20 tới 31/03/2020 -> Nến rút chân xanh 3 cây liên tục, bands dưới vòng cung lên, band trên đi xuống => bands bóp => biên độ cực rộng => giá sẽ qua về MA 20
                     *                                + 3 cây nến xanh bám ở MA 5 liên tục, rút chân lên => mua vô ở cây sau được, giá mua vô từ trung bình râu nến dưới của 2 cây trước (do tín hiệu tăng) lên tới MA 5, ko cần mua đuổi
                     *                                + Theo dõi sau đó, vì nếu band tăng, MA 20 tăng, thì MA 20 sẽ là hỗ trợ cho nhịp hồi này, khi nào bán?                                          
                     *                                      + RSI rớt way về quanh 80 thì xả từ từ
                     *                                      + Giá dưới MA 5 2 phiên thì xả thêm 1 đoạn
                     *                                      + MA 5 cắt xuống MA 20 thì xả hết
                     *                              + => CThuc A
                     *                                  
                     *  KBC - 7/07/20 tới 22/07/2020 -> từ 26/6/20 tới 6/7/20 -> giao dịch quanh kháng cự là MA 5, và hổ trợ là bands dưới
                     *                                  + ngày 7/7/20 -> giá vượt kháng cự (MA 05), MACD xanh bẻ ngang lúc này kháng cự mới sẽ là MA 20, MA 5 sẽ là hỗ trợ
                     *                                  + có thể ra nhanh đoạn này khi T3 về (13/7/20) vì giá vượt kháng cự, nhưng lại lạ nến đỏ => ko qua dc, dễ dội về hỗ trợ => ko cần giữ lâu
                     *                              + => CThuc A
                     *  KBC - 10/8/20 (MA + bands) -> Nếu mua ngày 4/8/20 (trước đó 4 ngày) vì phân kì tăng RSI, thì mình có thể tránh trường hợp này
                     *                                + 31/7/20: nến doji tăng ngoài bands dưới
                     *                                + 03/8/20: nến tăng xác nhận doji trước là đáy -> cuối ngày ngồi coi - RSI tăng, Giá giảm -> tín hiệu đảo chiều -> nên mua vô
                     *                                + 04/8/20: mua vô ở giá đóng cửa của phiên trước (03/8/20)
                     *                                + Giá tăng liên tục trong những phiên sau, nhưng vol < ma 20, thân nến nhỏ => giao dịch ít, lưỡng lự, ko nên tham gia lâu dài trong tình huống này
                     *                                + Nếu lỡ nhịp mua ngày 04/8/20 rồi thì thôi
                     *                              + => CThuc A
                     *  KBC - 12/11/20 tới 17/11/2020 -> Nếu đã mua ngày 11/11/2020, thì nên theo dõi thêm MACD để tránh bán hàng mà lỡ nhịp tăng
                     *                                + MACD cắt ngày 11/11/20, tạo tín hiệu đảo chiều, kết hợp với những yếu tố đi kèm,
                     *                                + MACD tăng dần lên 0, momentum tăng dần theo, chờ vượt 0 là nổ
                     *                              + => CThuc B
                     *  KBC - 21/5/21 -> Lưu ý đặt sẵn giá mua ở giá sàn những ngày này nếu 2 nến trước đã tạo râu bám vô MA 05, nếu ko có râu bám vô MA 5 thì thôi
                     *                                + nếu có râu nên bám vô, thì đặt sẵn giá mua = giá từ giá thấp nhất của cây nến thứ 2 có râu
                     *                                  
                     *                                  
                     *  KBC - 12/7/21 - 26/7/21  -> giống đợt 31/7/20 tới 4/8/20
                     *                              + Ngày 12/7 1 nến con quay dài xuất hiện dưới bands bot => xác nhận đáy, cùng đáy với ngày 21/5/21 => hỗ trợ mạnh vùng thân nến đỏ trải xuống râu nến này, có thể vô tiền 1 ít tầm giá này
                     *                              + Sau đó giá bật lại MA 5
                     *                              + tới ngày 19/7/20 - 1 cây giảm mạnh, nhưng giá cũng chỉ loanh quanh vùng hỗ trợ này, vol trong nhịp này giảm => hết sức đạp, cũng chả ai muốn bán
                     *                              + RSI trong ngày 19/7, giá đóng cửa xuống giá thấp hơn ngày 12/7, nhưng RSI đang cao hơn => tín hiệu đảo chiều 
                     *                              + - nhưng cần theo dõi thêm 1 phiên, nếu phiên ngày mai xanh thì ok, xác nhận phân kỳ tăng => Có thể mua vô dc
                     *                              
                     *  Bands và giá rớt (A - Done)
                     *      + Nếu giá rớt liên tục giữa bands và MA 5, nếu xuất hiện 1 cây nến có thân rớt ra khỏi bands dưới, có râu trên dài > 2 lần thân nến, thì bắt đầu để ý vô lệnh mua
                     *      + (A1) Nếu nến rớt ngoài bands này là nến xanh => đặt mua ở giá quanh thân nến
                     *      + (A2) Nếu nến rớt ngoài bands này là nến đỏ   => tiếp tục chờ 1 cây nến sau cây đỏ này, nếu vẫn là nến đỏ thì bỏ, nếu là nến xanh thì đặt mua cây tiếp theo
                     *          + đặt mua ở giá trung bình giữa giá mở cửa của cây nến đỏ ngoài bands và giá MA 5 ngày hum nay
                     *          
                     *      + Ví dụ: KBC: 10/3/20 -> 17/3/20                                                    03/8/20                 3/11/20                                     12/7/21 - 26/7/21                   
                     *                  - RSI dương   (cây nến hiện tại hoặc 1 trong 3 cây trước là dc)         RSI dương               RSI dương                                   Cây nến 13/7/21 ko tăng
                     *                  - MACD momentum tăng->0                                                 Tăng                    Tăng                                        Tăng rất nhẹ (~2%)
                     *                  - MACD tăng                                                             Tăng                    Giảm nhẹ hơn trước (-5 -> -41 -> -50)       Giảm
                     *                  - nến tăng                                                              Tăng                    Tăng                                        Tăng
                     *                  - giá bật từ bands về MA 5                                              OK                      OK
                     *                  - 13 nến trước (100% bám bands dưới, thân nến dưới MA 5                 7 (100%) dưới MA 5      4/5 cây giảm (80%) ko chạm MA 5             4/6 nến giảm liên tục ko chạm MA 5
                     *                  - MA 5 bẻ ngang -> giảm nhẹ hơn 2 phiên trước:                          MA 5 tăng               14330 (-190) -> 14140 (-170)
                     *                      + T(-1) - T(0) < T(-3) - T(-2) && T(-2) - T(-1)                                             -> 13970 (-60) -> 13910
                     *                  - Khoảng cách từ MA 5 tới MA 20 >= 15%                                  > 15%                   4% (bỏ)                                     12%
                     *                      + vì giá sẽ về MA 20, nên canh tí còn ăn
                     *                      + mục tiêu là 10% trong những đợt hồi này, nên mua quanh +-3%       +-3%
                     *                      + Khoảng cách càng lớn thì đặt giá mua càng cao, tối đa 3%
                     *                      + Cân nhắc đặt ATO cho dễ tính => đặt giá C như bth
                     *      
                    */
                    var bandsTrenHumNay = phienHumNay.BandsTop;
                    var bandsDuoiHumNay = phienHumNay.BandsBot;
                    var bandsTrenHumWa = phienHumWa.BandsTop;
                    var bandsDuoiHumWa = phienHumWa.BandsBot;

                    var bắtĐầuMuaDoNếnTăngA1 = phienHumNay.NếnĐảoChiềuTăngMạnhA1();
                    var bắtĐầuMuaDoNếnTăngA2 = phienHumNay.NếnĐảoChiềuTăngMạnhA2(phienHumWa);

                    var bandsTrênĐangGiảm = bandsTrenHumNay < bandsTrenHumWa;
                    var bandsMởRộng = bandsTrenHumNay > bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var bandsĐangBópLại = bandsTrenHumNay < bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var ma20ĐangGiảm = phienHumNayMa20 < phienHumWaMa20;


                    var bandsKhôngĐổi = bandsTrenHumNay == bandsTrenHumWa && bandsDuoiHumNay == bandsDuoiHumWa;
                    var ma20KhôngĐổi = phienHumNayMa20 == phienHumWaMa20;
                    var giaOGiuaBands = phienHumNay.NenBot * 0.93M < phienHumNay.BandsBot && phienHumNay.NenTop * 1.07M > phienHumNay.BandsTop;

                    var muaTheoMA = thânNếnKhôngVượtQuáBandTren && nenTangGia && ((duongMa05CatLenTrenMa20 && nenNamDuoiMA20)
                                                    || (MA05HuongLen && (nenTangLenChamMa20 || râunếnTangLenChamMa20) && Ma05DuoiMa20));
                    var nếnTụtMạnhNgoàiBandDưới = phienHumNay.BandsBot > phienHumNay.NenBot + ((phienHumNay.NenTop - phienHumNay.NenBot) / 2);

                    var momenTumTốt = (phienHumKia.MACDMomentum.IsDifferenceInRank(phienHumWa.MACDMomentum, 0.01M) || phienHumWa.MACDMomentum > phienHumKia.MACDMomentum) && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;
                    var momenTumTăngTốt = phienHumWa.MACDMomentum > phienHumKia.MACDMomentum * 0.96M && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;

                    var nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands =
                        (phienHumWa.NenTop < phienHumWa.BandsBot
                            || (phienHumWa.NenBot.IsDifferenceInRank(phienHumWa.BandsBot, 0.02M) && phienHumWa.NenTop < phienHumWaMa05))
                        && (phienHumNay.NenTop >= phienHumNayMa05 || phienHumNay.NenBot >= phienHumNay.BandsBot);

                    var trongXuHướngGiảmMạnh = phienHumNay.TỉLệNếnCựcYếu(histories) >= 0.5M;
                    var trongXuHướngGiảm = phienHumNay.TỉLệNếnYếu(histories) >= 0.5M;

                    var ma05ĐangBẻNgang = phienHumNay.MAChuyểnDần(histories, false, -5, 3);
                    var khôngNênBánT3 = phienHumNay.MACDMomentum > phienHumWa.MACDMomentum && phienHumNay.MACD > phienHumWa.MACD && phienHumNay.MACDMomentum > -100;

                    var rsiDuong = phienHumNay.RSIDương(histories);
                    var tínHiệuMuaTrongSóngHồiMạnh =
                                                   momenTumTăngTốt
                                                && phienHumNay.TangGia()
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tínHiệuMuaTrongSóngHồiTrungBình =
                                                   momenTumTốt
                                                && (phienHumNay.TangGia() || phienHumNay.Doji())
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh || trongXuHướngGiảm)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tinhieuMuaManh = tínHiệuMuaTrongSóngHồiMạnh ? 10 : 0;
                    var tinhieuMuaTrungBinh = tínHiệuMuaTrongSóngHồiTrungBình ? 5 : 0;
                    var tinHieuMuaTrungBinh1 = muaTheoMA ? 5 : 0;
                    var tinHieuMuaTrungBinh2 = nếnTụtMạnhNgoàiBandDưới && ma05ĐangBẻNgang ? 5 : 0;
                    var tinHieuMuaYếu1 = bandsMởRộng ? 5 : 0;

                    var tinHieuGiảmMua1 = bandsTrênĐangGiảm && ma20ĐangGiảm ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    //var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi && giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua3 = giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua4 = !phienHumNay.SoSánhGiá(1) || !phienHumWa.SoSánhGiá(1) ? -5 : 0;

                    if (tinhieuMuaManh
                        + tinhieuMuaTrungBinh + tinHieuMuaTrungBinh1 + tinHieuMuaTrungBinh2
                        + tinHieuGiảmMua1 + tinHieuGiảmMua2 + tinHieuGiảmMua3 + tinHieuGiảmMua4 <= 0) continue;

                    var ngayMua = historiesInPeriodOfTime.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).FirstOrDefault();
                    if (ngayMua == null) ngayMua = new History() { Date = phienHumNay.Date.AddDays(1) };
                    var giáĐặtMua = nếnTụtMạnhNgoàiBandDưới
                        ? (phienHumNay.BandsBot + phienHumNay.NenBot) / 2
                        : phienHumNay.C;

                    //if (giáĐặtMua >= ngayMua.L && giáĐặtMua <= ngayMua.H)       //Giá hợp lệ
                    //{
                    var giữT = khôngNênBánT3 ? 6 : 3;
                    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayMua.Date)
                        .OrderBy(h => h.Date)
                        .Skip(3)
                        .Take(giữT)
                        .ToList();

                    if (tPlus.Count < 3) //hiện tại
                    {
                        result1.Add($"{symbol._sc_} - Hiện tại điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    }
                    else
                    {
                        if (tPlus.Any(t => t.C > ngayMua.O * (1M + percentProfit) || t.O > ngayMua.O * (1M + percentProfit)))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                        {
                            dung++;
                            result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                        else
                        {
                            sai++;
                            result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                    }

                    //if (bandsĐangGiảm && ma20ĐangGiảm && !nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}

                    //if (!bandsĐangGiảm || !ma20ĐangGiảm || nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}
                }
                //else
                //{
                //    result1.Add($"{symbol._sc_} - Không có giá {giáĐặtMua.ToString("N2")} ở ngày mai mua: {phienKiemTra.Date.ToShortDateString()}");
                //}


                ////tín hiệu bán
                //if ((phienTruocPhienKiemTraMa05 > phienKiemTraMa05              //MA 05 đang hướng lên
                //        && phienKiemTra.NenBot <= phienKiemTraMa20)             //Giá MA 05 chạm MA 20
                //    || (phienTruocPhienKiemTraMa05 >= phienKiemTraMa20 && phienKiemTraMa05 <= phienKiemTraMa20))  //MA 05 cắt xuống dưới MA 20
                //{
                //    var ngayBán = historiesInPeriodOfTime.Where(h => h.Date > phienKiemTra.Date).OrderBy(h => h.Date).First();
                //    var tileChinhXac = 0;
                //    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayBán.Date)
                //        .OrderBy(h => h.Date)
                //        .Skip(3)
                //        .Take(3)
                //        .ToList();

                //    if (tPlus.All(t => t.C > ngayBán.O || t.O > ngayBán.O))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                //    {
                //        dung++;
                //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()}");
                //    }
                //    else
                //    {
                //        sai++;
                //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()} - Bán: {ngayBán.Date.ToShortDateString()} giá {ngayBán.O}");
                //    }
                //}
                //}

                tong = dung + sai;
                var tile = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                //result1.Add($"Tỉ lệ: {tile}");
                tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, tile, result1));
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }


        public async Task<List<Tuple<string, decimal, List<string>>>> DoTimCongThuc(string code, DateTime ngay, DateTime ngayCuoi, int ma20vol, int MANhanh, int MACham, decimal percentProfit)
        {
            var result = new PatternDetailsResponseModel();
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol).ToListAsync()
                : await _context.StockSymbol.Where(s => s.MA20Vol > ma20vol && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= ngay.AddDays(10) //calculate T
                    && ss.Date >= ngayCuoi.AddDays(-MACham * 2)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();

            var tup = new List<Tuple<string, decimal, List<string>>>();

            Parallel.ForEach(symbols, symbol =>
            {
                var result1 = new List<string>();
                decimal tong = 0;
                decimal dung = 0;
                decimal sai = 0;

                var NhậtKýMuaBán = new List<Tuple<string, DateTime, bool, decimal>>();

                var historiesInPeriodOfTime = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbol._sc_)
                    .OrderBy(h => h.Date)
                    .ToList();
                if (historiesInPeriodOfTime.Count < 100) return;


                var ngayCuoiCuaMa = historiesInPeriodOfTime[0].Date.AddDays(30) > ngayCuoi
                    ? historiesInPeriodOfTime[0].Date.AddDays(30)
                    : ngayCuoi;

                var histories = historiesInPeriodOfTime
                    .Where(ss => ss.Date <= ngay && ss.Date >= ngayCuoiCuaMa)
                    .OrderBy(h => h.Date)
                    .ToList();

                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = symbol._sc_;

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return;

                for (int i = 0; i < histories.Count; i++)
                {
                    var phienHumNay = histories[i];
                    var phienHumWa = historiesInPeriodOfTime.Where(h => h.Date < phienHumNay.Date).OrderByDescending(h => h.Date).First();
                    var phienHumKia = historiesInPeriodOfTime.Where(h => h.Date < phienHumWa.Date).OrderByDescending(h => h.Date).First();

                    var phienHumNayMa20 = phienHumNay.MA(historiesInPeriodOfTime, -MACham);
                    var phienHumNayMa05 = phienHumNay.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa05 = phienHumWa.MA(historiesInPeriodOfTime, -MANhanh);
                    var phienHumWaMa20 = phienHumWa.MA(historiesInPeriodOfTime, -MACham);

                    /* 
                     * Xác định trend
                     *      + Tăng
                     *          + Kết thúc đà tăng
                     *      + Giảm
                     *          + Kết thúc đà giảm
                     *      + Sideway 
                     *          + Phân phối
                     *          + Tích lũy
                     *          
                     * bands dưới   giảm dần đều trong >= 1 phiên gần nhất >=1%                                     tín hiệu chuyển wa trend giảm nhẹ
                     *                                 >= 2 phiên gần nhất >=3%                                     tín hiệu chuyển wa trend giảm trung bình
                     *                                 >= 3 phiên gần nhất >=5%                                     tín hiệu chuyển wa trend giảm bền vững
                     *                                 >= 4 phiên gần nhất >=7%                                     tín hiệu chuyển wa trend giảm mạnh
                     *              có khoảng cách tới MA 20 tăng dần trong >= 1 phiên gần đây                      tín hiệu chuyển wa trend giảm nhẹ
                     *                                                      >= 2 phiên gần đây                      tín hiệu chuyển wa trend giảm trung bình
                     *                                                      >= 3 phiên gần đây                      tín hiệu chuyển wa trend giảm bền vững
                     *                                                      >= 4 phiên gần đây                      tín hiệu chuyển wa trend giảm mạnh
                     *              có khoảng cách tới MA 20 giảm dần trong >= 1 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ nhẹ
                     *                                                      >= 2 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ trung bình
                     *                                                      >= 3 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ bền vững
                     *                                                      >= 4 phiên gần đây                      tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ mạnh
                     *              tăng dần đều trong >= 1 phiên gần nhất >=1%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ nhẹ
                     *                                 >= 2 phiên gần nhất >=3%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ trung bình
                     *                                 >= 3 phiên gần nhất >=5%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ bền vững
                     *                                 >= 4 phiên gần nhất >=7%                                     tín hiệu kết thúc trend giảm tạo đáy way đầu, hoặc giá tiệm cận hỗ trợ mạnh
                     *              trong vòng >= 1 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway nhẹ
                     *              trong vòng >= 3 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway trung bình
                     *              trong vòng >= 5 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway mạnh
                     *              trong vòng >= 7 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway cực mạnh
                     *              
                     * bands trên   tăng dần đều trong >= 1 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng nhẹ
                     *                                 >= 2 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng trung bình
                     *                                 >= 3 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng bền vững
                     *                                 >= 4 phiên gần nhất                      + giá >= MA 20      tín hiệu chuyển wa trend tăng mạnh
                     *              có khoảng cách tới MA 20 tăng dần trong >= 1 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng nhẹ
                     *                                                      >= 2 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng trung bình
                     *                                                      >= 3 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng bền vững
                     *                                                      >= 4 phiên gần đây  + giá >= MA 20      tín hiệu chuyển wa trend tăng mạnh
                     *              có khoảng cách tới MA 20 giảm dần trong >= 1 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự nhẹ
                     *                                                      >= 2 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự trung bình
                     *                                                      >= 3 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự bền vững
                     *                                                      >= 4 phiên gần đây  + giá >= MA 20      tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự mạnh
                     *              tăng dần đều trong >= 1 phiên gần nhất >=1%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự nhẹ
                     *                                 >= 2 phiên gần nhất >=3%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự trung bình
                     *                                 >= 3 phiên gần nhất >=5%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự bền vững
                     *                                 >= 4 phiên gần nhất >=7%                                     tín hiệu kết thúc trend tăng tạo đỉnh way đầu, hoặc giá tiệm cận kháng cự mạnh
                     *              trong vòng >= 1 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway nhẹ
                     *              trong vòng >= 3 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway trung bình
                     *              trong vòng >= 5 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway mạnh
                     *              trong vòng >= 7 phiên, giá trị ko chênh lệch quá 2% so với ngày quá khứ         tín hiệu chuyển wa sideway cực mạnh
                     *
                     * 
                     * MA 5         cắt lên MA 20                                                                   tín hiệu chuyển wa trend tăng trung bình
                     *              cắt xuống MA 20                                                                 tín hiệu chuyển wa trend giảm trung bình
                     *              giảm dần đều trong >= 1 phiên gần nhất  >=1%                                    tín hiệu chuyển wa trend giảm nhẹ
                     *                                 >= 2 phiên gần nhất  >=3%                                    tín hiệu chuyển wa trend giảm trung bình
                     *                                 >= 3 phiên gần nhất  >=5%                                    tín hiệu chuyển wa trend giảm bền vững
                     *                                 >= 4 phiên gần nhất  >=7%                                    tín hiệu chuyển wa trend giảm mạnh
                     *              ở trên MA 20 và có khoảng cách với MA 20 tăng dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend tăng nhẹ
                     *                                                                       >= 2 phiên gần đây     tín hiệu chuyển wa trend tăng trung bình
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend tăng bền vững
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend tăng mạnh
                     *                                                                       
                     *                                                 MA 20 giảm dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend giảm nhẹ
                     *                                                                       >= 2 phiên gần đây     tín hiệu chuyển wa trend giảm trung bình
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend giảm bền vững
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend giảm mạnh
                     *                                                                       
                     *              ở dưới MA 20 và có khoảng cách với MA 20 giảm dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend tăng nhẹ
                     *                                                                       >= 2 phiên gần đây     tín hiệu chuyển wa trend tăng trung bình
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend tăng bền vững
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend tăng mạnh
                     *                                                                       
                     *                                                 MA 20 tăng dần trong  >= 1 phiên gần đây     tín hiệu chuyển wa trend giảm nhẹ               
                     *                                                                       >= 2 phiên gNhần đây     tín hiệu chuyển wa trend giảm trung bình                      
                     *                                                                       >= 3 phiên gần đây     tín hiệu chuyển wa trend giảm bền vững                    
                     *                                                                       >= 4 phiên gần đây     tín hiệu chuyển wa trend giảm mạnh                
                     *                                                                       
                     *              tăng dần đều trong >= 1 phiên gần nhất  >=1%                                    tín hiệu chuyển wa trend tăng nhẹ
                     *                                 >= 2 phiên gần nhất  >=3%                                    tín hiệu chuyển wa trend tăng trung bình
                     *                                 >= 3 phiên gần nhất  >=5%                                    tín hiệu chuyển wa trend tăng bền vững
                     *                                 >= 4 phiên gần nhất  >=7%                                    tín hiệu chuyển wa trend tăng mạnh
                     * 
                     * MACD         hướng lên                       trên 0                                          tín hiệu chuyển wa trend tăng trung bình
                     * MACD         hướng lên                       dưới 0                                          tín hiệu chuyển wa trend tăng nhẹ
                     * MACD         cắt lên signal                                                                  tín hiệu chuyển wa trend tăng trung bình
                     * MACD         hướng xuống                     dưới 0                                          tín hiệu chuyển wa trend giảm trung bình
                     * MACD         hướng xuống                     trên 0                                          tín hiệu chuyển wa trend giảm nhẹ
                     * MACD         cắt xuống signal                                                                tín hiệu chuyển wa trend giảm trung bình
                     * Momentum     giảm dần                        trên 0                                          tín hiệu chuyển wa trend giảm nhẹ
                     * Momentum     giảm dần                        dưới 0                                          tín hiệu chuyển wa trend giảm trung bình
                     * Momentum     tăng dần                        trên 0                                          tín hiệu chuyển wa trend tăng trung bình
                     * Momentum     tăng dần                        dưới 0                                          tín hiệu chuyển wa trend tăng nhẹ
                     * 
                     * 
                     * 
                     * Ichi         Tenkan cắt lên Kijun            trên mây,   chikou & Price trên mây             tín hiệu chuyển wa trend tăng mạnh
                     *                                              trong mây   chikou & Price trên mây             tín hiệu chuyển wa trend tăng trung bình
                     *                                              dưới mây                                        N/A
                     *              Tenkan xuống Kijun              trên mây                                        N/A
                     *                                              trong mây   chikou & Price trong mây            tín hiệu chuyển wa trend giảm trung bình
                     *                                              dưới mây    chikou & Price dưới mây             tín hiệu chuyển wa trend giảm mạnh
                     *              Span A cắt lên trên Span B                                                      tín hiệu chuyển wa trend tăng nhẹ
                     *              Span A cắt dưới Span B                                                          tín hiệu chuyển wa trend giảm nhẹ
                     *              
                     * Giá          nến top chạm bands top                                                          tín hiệu chuyển wa trend giảm nhẹ
                     *              nến bot chạm bands dưới                                                         tín hiệu chuyển wa trend tăng nhẹ
                     *              nến bot vượt ra khỏi bands top                                                  tín hiệu chuyển wa trend giảm trung bình
                     *              nến top vượt ra khỏi bands top                                                  tín hiệu chuyển wa trend giảm nhẹ
                     *              nến top vượt ra khỏi bands bot                                                  tín hiệu chuyển wa trend tăng trung bình
                     *              nến xanh                                                                        tín hiệu chuyển wa trend tăng nhẹ
                     *              nến đỏ                                                                          tín hiệu chuyển wa trend giảm nhẹ
                     *              thân nến xanh dài               từ dưới MA 20 vượt lên gần bands top            tín hiệu chuyển wa trend giảm trung binh
                     *              bật lên chạm MA 05                                                              tín hiệu chuyển wa trend tăng trung bình
                     *              tụt xuống chạm MA 20                                                            tín hiệu chuyển wa trend giảm trung bình
                     * 
                     * Vol          > MA 20                         giá tăng                                        tín hiệu chuyển wa trend tăng nhẹ
                     * Vol          > MA 20                         giá giảm                                        tín hiệu chuyển wa trend giảm nhẹ
                     * Vol          < MA 20                         giá tăng                                        tín hiệu chuyển wa trend tăng nhẹ
                     * Vol          < MA 20                         giá giảm                                        tín hiệu chuyển wa trend giảm nhẹ
                     *              
                     * 
                     * 
                     */

                    //tính hiệu quả

                    //tín hiệu mua
                    var Ma05DuoiMa20 = phienHumNayMa05 < phienHumNayMa20;
                    var MA05HuongLen = phienHumWaMa05 < phienHumNayMa05;
                    var nenTangGia = phienHumNay.TangGia();
                    var nenTangLenChamMa20 = phienHumNay.NenTop >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;             //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var râunếnTangLenChamMa20 = phienHumNay.H >= phienHumNayMa20 && phienHumNay.NenBot < phienHumNayMa20;               //Giá trong phiên MA 05 tăng lên chạm MA 20
                    var duongMa05CatLenTrenMa20 = phienHumWaMa05 < phienHumNayMa20 && phienHumNayMa05 > phienHumNayMa20;     //MA 05 cắt lên trên MA 20
                    var nenNamDuoiMA20 = phienHumNay.NenBot < phienHumNayMa20;                                                            //Giá nằm dưới MA 20
                    var thânNếnKhôngVượtQuáBandTren = phienHumNay.NenTop < phienHumNay.BandsTop;


                    /*TODO: cảnh báo mua nếu giá mở cửa tăng chạm bands trên. Ví dụ 9/4/2021 KBC, 17/1/2022 VNM - tăng gần chạm bands mà ko thấy dấu hiệu mở bands rộng ra, mA 20 cũng ko hướng lên - sideway
                     * 10-2-22 VNM: bands ko mở rộng, bands tren hướng xuống, bands dưới đi ngang, các giá trước loanh quanh MA 20, ko có dấu hiệu phá bỏ sideway
                     * var khángCựĐỉnh = phienKiemTra.KhángCựĐỉnh(historiesInPeriodOfTime);
                     * var khángCựBands = phienKiemTra.KhángCựBands(historiesInPeriodOfTime);
                     * var khángCựFibonacci = phienKiemTra.KhángCựFibonacci(historiesInPeriodOfTime);
                     * var khángCựIchimoku   = phienKiemTra.KhángCựIchimoku(historiesInPeriodOfTime);
                     * 
                     * Ví dụ: 
                     *  KBC - 22/3/22 (MA + bands) -> nhưng có thể xét vì giá đã tăng gần tới viền mây dưới ichimoku rồi nên ko mua, hoặc giá từ MA 5 đi lên (hổ trợ lên kháng cự) ở MA 20, và bands bóp nên giá chỉ quay về MA20
                     *  KBC - 03/3/20 (MA + bands) -> MA, Bands đi ngang, có thể mua để lợi T+ => thất bại -> có thể xét tới MACD trong trường hợp này:
                     *                              + MACD dưới 0, đỏ cắt lên xanh
                     *                              + Đã mua ở ngày 26/2 rồi, MACD chưa vượt 0 lên dương mạnh thì cũng ko cần mua thêm
                     *                              + => bands hẹp, bands ko thay đổi, ma 20 k thay đổi, thân nến ở giữa bands => ko mua, vì rất dễ sảy T3
                     *                                      
                     *  KBC - 10/3/20 -> 17/3/20 - Nếu bất chấp nến ngoài bolinger bands dưới để mua, thì hãy cân nhắc KBC trong những ngày này -> nên kết hợp MACD (macd giảm, momentum âm mạnh) 
                     *                              + => CThuc A
                     *                                                     
                     *  KBC - 27/03/20 tới 31/03/2020 -> Nến rút chân xanh 3 cây liên tục, bands dưới vòng cung lên, band trên đi xuống => bands bóp => biên độ cực rộng => giá sẽ qua về MA 20
                     *                                + 3 cây nến xanh bám ở MA 5 liên tục, rút chân lên => mua vô ở cây sau được, giá mua vô từ trung bình râu nến dưới của 2 cây trước (do tín hiệu tăng) lên tới MA 5, ko cần mua đuổi
                     *                                + Theo dõi sau đó, vì nếu band tăng, MA 20 tăng, thì MA 20 sẽ là hỗ trợ cho nhịp hồi này, khi nào bán?                                          
                     *                                      + RSI rớt way về quanh 80 thì xả từ từ
                     *                                      + Giá dưới MA 5 2 phiên thì xả thêm 1 đoạn
                     *                                      + MA 5 cắt xuống MA 20 thì xả hết
                     *                              + => CThuc A
                     *                                  
                     *  KBC - 7/07/20 tới 22/07/2020 -> từ 26/6/20 tới 6/7/20 -> giao dịch quanh kháng cự là MA 5, và hổ trợ là bands dưới
                     *                                  + ngày 7/7/20 -> giá vượt kháng cự (MA 05), MACD xanh bẻ ngang lúc này kháng cự mới sẽ là MA 20, MA 5 sẽ là hỗ trợ
                     *                                  + có thể ra nhanh đoạn này khi T3 về (13/7/20) vì giá vượt kháng cự, nhưng lại lạ nến đỏ => ko qua dc, dễ dội về hỗ trợ => ko cần giữ lâu
                     *                              + => CThuc A
                     *  KBC - 10/8/20 (MA + bands) -> Nếu mua ngày 4/8/20 (trước đó 4 ngày) vì phân kì tăng RSI, thì mình có thể tránh trường hợp này
                     *                                + 31/7/20: nến doji tăng ngoài bands dưới
                     *                                + 03/8/20: nến tăng xác nhận doji trước là đáy -> cuối ngày ngồi coi - RSI tăng, Giá giảm -> tín hiệu đảo chiều -> nên mua vô
                     *                                + 04/8/20: mua vô ở giá đóng cửa của phiên trước (03/8/20)
                     *                                + Giá tăng liên tục trong những phiên sau, nhưng vol < ma 20, thân nến nhỏ => giao dịch ít, lưỡng lự, ko nên tham gia lâu dài trong tình huống này
                     *                                + Nếu lỡ nhịp mua ngày 04/8/20 rồi thì thôi
                     *                              + => CThuc A
                     *  KBC - 12/11/20 tới 17/11/2020 -> Nếu đã mua ngày 11/11/2020, thì nên theo dõi thêm MACD để tránh bán hàng mà lỡ nhịp tăng
                     *                                + MACD cắt ngày 11/11/20, tạo tín hiệu đảo chiều, kết hợp với những yếu tố đi kèm,
                     *                                + MACD tăng dần lên 0, momentum tăng dần theo, chờ vượt 0 là nổ
                     *                              + => CThuc B
                     *  KBC - 21/5/21 -> Lưu ý đặt sẵn giá mua ở giá sàn những ngày này nếu 2 nến trước đã tạo râu bám vô MA 05, nếu ko có râu bám vô MA 5 thì thôi
                     *                                + nếu có râu nên bám vô, thì đặt sẵn giá mua = giá từ giá thấp nhất của cây nến thứ 2 có râu
                     *                                  
                     *                                  
                     *  KBC - 12/7/21 - 26/7/21  -> giống đợt 31/7/20 tới 4/8/20
                     *                              + Ngày 12/7 1 nến con quay dài xuất hiện dưới bands bot => xác nhận đáy, cùng đáy với ngày 21/5/21 => hỗ trợ mạnh vùng thân nến đỏ trải xuống râu nến này, có thể vô tiền 1 ít tầm giá này
                     *                              + Sau đó giá bật lại MA 5
                     *                              + tới ngày 19/7/20 - 1 cây giảm mạnh, nhưng giá cũng chỉ loanh quanh vùng hỗ trợ này, vol trong nhịp này giảm => hết sức đạp, cũng chả ai muốn bán
                     *                              + RSI trong ngày 19/7, giá đóng cửa xuống giá thấp hơn ngày 12/7, nhưng RSI đang cao hơn => tín hiệu đảo chiều 
                     *                              + - nhưng cần theo dõi thêm 1 phiên, nếu phiên ngày mai xanh thì ok, xác nhận phân kỳ tăng => Có thể mua vô dc
                     *                              
                     *  Bands và giá rớt (A - Done)
                     *      + Nếu giá rớt liên tục giữa bands và MA 5, nếu xuất hiện 1 cây nến có thân rớt ra khỏi bands dưới, có râu trên dài > 2 lần thân nến, thì bắt đầu để ý vô lệnh mua
                     *      + (A1) Nếu nến rớt ngoài bands này là nến xanh => đặt mua ở giá quanh thân nến
                     *      + (A2) Nếu nến rớt ngoài bands này là nến đỏ   => tiếp tục chờ 1 cây nến sau cây đỏ này, nếu vẫn là nến đỏ thì bỏ, nếu là nến xanh thì đặt mua cây tiếp theo
                     *          + đặt mua ở giá trung bình giữa giá mở cửa của cây nến đỏ ngoài bands và giá MA 5 ngày hum nay
                     *          
                     *      + Ví dụ: KBC: 10/3/20 -> 17/3/20                                                    03/8/20                 3/11/20                                     12/7/21 - 26/7/21                   
                     *                  - RSI dương   (cây nến hiện tại hoặc 1 trong 3 cây trước là dc)         RSI dương               RSI dương                                   Cây nến 13/7/21 ko tăng
                     *                  - MACD momentum tăng->0                                                 Tăng                    Tăng                                        Tăng rất nhẹ (~2%)
                     *                  - MACD tăng                                                             Tăng                    Giảm nhẹ hơn trước (-5 -> -41 -> -50)       Giảm
                     *                  - nến tăng                                                              Tăng                    Tăng                                        Tăng
                     *                  - giá bật từ bands về MA 5                                              OK                      OK
                     *                  - 13 nến trước (100% bám bands dưới, thân nến dưới MA 5                 7 (100%) dưới MA 5      4/5 cây giảm (80%) ko chạm MA 5             4/6 nến giảm liên tục ko chạm MA 5
                     *                  - MA 5 bẻ ngang -> giảm nhẹ hơn 2 phiên trước:                          MA 5 tăng               14330 (-190) -> 14140 (-170)
                     *                      + T(-1) - T(0) < T(-3) - T(-2) && T(-2) - T(-1)                                             -> 13970 (-60) -> 13910
                     *                  - Khoảng cách từ MA 5 tới MA 20 >= 15%                                  > 15%                   4% (bỏ)                                     12%
                     *                      + vì giá sẽ về MA 20, nên canh tí còn ăn
                     *                      + mục tiêu là 10% trong những đợt hồi này, nên mua quanh +-3%       +-3%
                     *                      + Khoảng cách càng lớn thì đặt giá mua càng cao, tối đa 3%
                     *                      + Cân nhắc đặt ATO cho dễ tính => đặt giá C như bth
                     *      
                    */
                    var bandsTrenHumNay = phienHumNay.BandsTop;
                    var bandsDuoiHumNay = phienHumNay.BandsBot;
                    var bandsTrenHumWa = phienHumWa.BandsTop;
                    var bandsDuoiHumWa = phienHumWa.BandsBot;

                    var bắtĐầuMuaDoNếnTăngA1 = phienHumNay.NếnĐảoChiềuTăngMạnhA1();
                    var bắtĐầuMuaDoNếnTăngA2 = phienHumNay.NếnĐảoChiềuTăngMạnhA2(phienHumWa);

                    var bandsTrênĐangGiảm = bandsTrenHumNay < bandsTrenHumWa;
                    var bandsMởRộng = bandsTrenHumNay > bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var bandsĐangBópLại = bandsTrenHumNay < bandsTrenHumWa && bandsDuoiHumNay > bandsDuoiHumWa;
                    var ma20ĐangGiảm = phienHumNayMa20 < phienHumWaMa20;


                    var bandsKhôngĐổi = bandsTrenHumNay == bandsTrenHumWa && bandsDuoiHumNay == bandsDuoiHumWa;
                    var ma20KhôngĐổi = phienHumNayMa20 == phienHumWaMa20;
                    var giaOGiuaBands = phienHumNay.NenBot * 0.93M < phienHumNay.BandsBot && phienHumNay.NenTop * 1.07M > phienHumNay.BandsTop;

                    var muaTheoMA = thânNếnKhôngVượtQuáBandTren && nenTangGia && ((duongMa05CatLenTrenMa20 && nenNamDuoiMA20)
                                                    || (MA05HuongLen && (nenTangLenChamMa20 || râunếnTangLenChamMa20) && Ma05DuoiMa20));
                    var nếnTụtMạnhNgoàiBandDưới = phienHumNay.BandsBot > phienHumNay.NenBot + ((phienHumNay.NenTop - phienHumNay.NenBot) / 2);

                    var momenTumTốt = (phienHumKia.MACDMomentum.IsDifferenceInRank(phienHumWa.MACDMomentum, 0.01M) || phienHumWa.MACDMomentum > phienHumKia.MACDMomentum) && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;
                    var momenTumTăngTốt = phienHumWa.MACDMomentum > phienHumKia.MACDMomentum * 0.96M && phienHumNay.MACDMomentum > phienHumWa.MACDMomentum * 0.96M;

                    var nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands =
                        (phienHumWa.NenTop < phienHumWa.BandsBot
                            || (phienHumWa.NenBot.IsDifferenceInRank(phienHumWa.BandsBot, 0.02M) && phienHumWa.NenTop < phienHumWaMa05))
                        && (phienHumNay.NenTop >= phienHumNayMa05 || phienHumNay.NenBot >= phienHumNay.BandsBot);

                    var trongXuHướngGiảmMạnh = phienHumNay.TỉLệNếnCựcYếu(histories) >= 0.5M;
                    var trongXuHướngGiảm = phienHumNay.TỉLệNếnYếu(histories) >= 0.5M;

                    var ma05ĐangBẻNgang = phienHumNay.MAChuyểnDần(histories, false, -5, 3);
                    var khôngNênBánT3 = phienHumNay.MACDMomentum > phienHumWa.MACDMomentum && phienHumNay.MACD > phienHumWa.MACD && phienHumNay.MACDMomentum > -100;

                    var rsiDuong = phienHumNay.RSIDương(histories);
                    var tínHiệuMuaTrongSóngHồiMạnh =
                                                   momenTumTăngTốt
                                                && phienHumNay.TangGia()
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tínHiệuMuaTrongSóngHồiTrungBình =
                                                   momenTumTốt
                                                && (phienHumNay.TangGia() || phienHumNay.Doji())
                                                && (nếnBậtMạnhLênTừBandsDướiVềMA05HoặcTrongBands || ma05ĐangBẻNgang)
                                                && (trongXuHướngGiảmMạnh || trongXuHướngGiảm)
                                                && phienHumNayMa20 / phienHumNayMa05 > 1.1M;

                    var tinhieuMuaManh = tínHiệuMuaTrongSóngHồiMạnh ? 10 : 0;
                    var tinhieuMuaTrungBinh = tínHiệuMuaTrongSóngHồiTrungBình ? 5 : 0;
                    var tinHieuMuaTrungBinh1 = muaTheoMA ? 5 : 0;
                    var tinHieuMuaTrungBinh2 = nếnTụtMạnhNgoàiBandDưới && ma05ĐangBẻNgang ? 5 : 0;
                    var tinHieuMuaYếu1 = bandsMởRộng ? 5 : 0;

                    var tinHieuGiảmMua1 = bandsTrênĐangGiảm && ma20ĐangGiảm ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    //var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi && giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua2 = bandsKhôngĐổi && ma20KhôngĐổi ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua3 = giaOGiuaBands ? -5 : 0;// && !nếnGiảmVượtMạnhNgoàiBandDưới ? -5 : 0;
                    var tinHieuGiảmMua4 = !phienHumNay.SoSánhGiá(1) || !phienHumWa.SoSánhGiá(1) ? -5 : 0;

                    if (tinhieuMuaManh
                        + tinhieuMuaTrungBinh + tinHieuMuaTrungBinh1 + tinHieuMuaTrungBinh2
                        + tinHieuGiảmMua1 + tinHieuGiảmMua2 + tinHieuGiảmMua3 + tinHieuGiảmMua4 <= 0) continue;

                    var ngayMua = historiesInPeriodOfTime.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).FirstOrDefault();
                    if (ngayMua == null) ngayMua = new History() { Date = phienHumNay.Date.AddDays(1) };
                    var giáĐặtMua = nếnTụtMạnhNgoàiBandDưới
                        ? (phienHumNay.BandsBot + phienHumNay.NenBot) / 2
                        : phienHumNay.C;

                    //if (giáĐặtMua >= ngayMua.L && giáĐặtMua <= ngayMua.H)       //Giá hợp lệ
                    //{
                    var giữT = khôngNênBánT3 ? 6 : 3;
                    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayMua.Date)
                        .OrderBy(h => h.Date)
                        .Skip(3)
                        .Take(giữT)
                        .ToList();

                    if (tPlus.Count < 3) //hiện tại
                    {
                        result1.Add($"{symbol._sc_} - Hiện tại điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    }
                    else
                    {
                        if (tPlus.Any(t => t.C > ngayMua.O * (1M + percentProfit) || t.O > ngayMua.O * (1M + percentProfit)))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                        {
                            dung++;
                            result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                        else
                        {
                            sai++;
                            result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc mua: {phienHumNay.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                        }
                    }

                    //if (bandsĐangGiảm && ma20ĐangGiảm && !nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng - Band xấu - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}

                    //if (!bandsĐangGiảm || !ma20ĐangGiảm || nếnGiảmVượtMạnhNgoàiBandDưới)
                    //{
                    //    if (tPlus.Any(t => t.C > ngayMua.O * 1.01M || t.O > ngayMua.O * 1.01M))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                    //    {
                    //        dung++;
                    //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //    else
                    //    {
                    //        sai++;
                    //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai mua: {phienKiemTra.Date.ToShortDateString()} ở giá {giáĐặtMua.ToString("N2")}");
                    //    }
                    //}
                }
                //else
                //{
                //    result1.Add($"{symbol._sc_} - Không có giá {giáĐặtMua.ToString("N2")} ở ngày mai mua: {phienKiemTra.Date.ToShortDateString()}");
                //}


                ////tín hiệu bán
                //if ((phienTruocPhienKiemTraMa05 > phienKiemTraMa05              //MA 05 đang hướng lên
                //        && phienKiemTra.NenBot <= phienKiemTraMa20)             //Giá MA 05 chạm MA 20
                //    || (phienTruocPhienKiemTraMa05 >= phienKiemTraMa20 && phienKiemTraMa05 <= phienKiemTraMa20))  //MA 05 cắt xuống dưới MA 20
                //{
                //    var ngayBán = historiesInPeriodOfTime.Where(h => h.Date > phienKiemTra.Date).OrderBy(h => h.Date).First();
                //    var tileChinhXac = 0;
                //    var tPlus = historiesInPeriodOfTime.Where(h => h.Date >= ngayBán.Date)
                //        .OrderBy(h => h.Date)
                //        .Skip(3)
                //        .Take(3)
                //        .ToList();

                //    if (tPlus.All(t => t.C > ngayBán.O || t.O > ngayBán.O))    //Mình đặt mua ở giá mở cửa ngày hum sau luôn
                //    {
                //        dung++;
                //        result1.Add($"{symbol._sc_} - Đúng T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()}");
                //    }
                //    else
                //    {
                //        sai++;
                //        result1.Add($"{symbol._sc_} - Sai  T3-5 - Điểm nhắc để ngày mai bán: {phienKiemTra.Date.ToShortDateString()} - Bán: {ngayBán.Date.ToShortDateString()} giá {ngayBán.O}");
                //    }
                //}
                //}

                tong = dung + sai;
                var tile = tong == 0 ? 0 : Math.Round(dung / tong, 2);
                //result1.Add($"Tỉ lệ: {tile}");
                tup.Add(new Tuple<string, decimal, List<string>>(symbol._sc_, tile, result1));
            });

            tup = tup.OrderByDescending(t => t.Item2).ToList();

            return tup;
        }

        public async Task<List<string>> ToiUuLoiNhuan(string code, DateTime toiNgay, DateTime tuNgay)
        {
            var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");
            var symbols = string.IsNullOrWhiteSpace(code)
                ? await _context.StockSymbol.Where(s => s.MA20Vol > ConstantData.minMA20VolDaily).ToListAsync()
                : await _context.StockSymbol.Where(s => s.MA20Vol > ConstantData.minMA20VolDaily && splitStringCode.Contains(s._sc_)).ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol)
                    && ss.Date <= toiNgay.AddDays(10) //calculate T
                    && ss.Date >= tuNgay.AddDays(-150)) //caculate SRI
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();


            //var histories = await _context.History
            //    .Where(ss => ss.StockSymbol == code
            //        && ss.Date <= ngay.AddDays(10) //calculate T
            //        && ss.Date >= ngayCuoi.AddDays(-50)) //caculate SRI
            //    .OrderBy(ss => ss.Date)
            //    .ToListAsync();

            var result1 = new List<string>();
            var NhậtKýMuaBán = new List<LearningRealDataModel>();

            for (int s = 0; s < symbols.Count; s++)
            {
                var byCodeList = historiesStockCode.Where(ss => ss.StockSymbol == symbols[s]._sc_).ToList();
                var pastList = byCodeList.Where(ss => ss.Date <= tuNgay.AddDays(-100)).OrderByDescending(h => h.Date).ToList();
                var firstDate = pastList.FirstOrDefault();
                var ngayBatDauCuaCoPhieu = firstDate != null ? tuNgay : historiesStockCode.Where(ss => ss.StockSymbol == symbols[s]._sc_).OrderBy(h => h.Date).Skip(100).FirstOrDefault()?.Date;
                if (!ngayBatDauCuaCoPhieu.HasValue) continue;
                var histories = historiesStockCode
                    .Where(ss => ss.StockSymbol == symbols[s]._sc_ && ss.Date >= ngayBatDauCuaCoPhieu.Value.AddDays(-50))
                    .OrderBy(ss => ss.Date)
                    .ToList();

                decimal root = 1M;
                var hasMoney = true;
                var ngayMuaToiUu = new History();
                var ngayMuaT3 = new History();

                var ngayBatDau = histories.First(h => h.Date >= ngayBatDauCuaCoPhieu);
                var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= toiNgay);
                if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
                {
                    ngayDungLai = histories.OrderByDescending(h => h.Date).First();
                }
                var startedI = histories.IndexOf(ngayBatDau);
                var stoppedI = histories.IndexOf(ngayDungLai);
                for (int i = startedI; i <= stoppedI; i++)
                {
                    try
                    {
                        var phienHumKia = histories[i - 2];
                        var phienHumWa = histories[i - 1];
                        var phienHumNay = histories[i];
                        if (phienHumNay.Date < ngayMuaT3.Date) continue;
                        if (hasMoney && phienHumNay.VOL(histories, -20) < ConstantData.minMA20VolDaily) continue;

                        var phienT1 = histories[i + 1];
                        var phienT2 = histories[i + 2];
                        var phienT3 = histories[i + 3];
                        var phienT4 = histories[i + 4];

                        if (hasMoney)
                        {
                            if (phienT3.C <= phienHumNay.C || (phienT1.C < phienHumNay.C && phienT1.C <= phienT4.C))
                            {
                                continue;
                            }
                            else
                            {
                                ngayMuaToiUu = phienHumNay;
                                ngayMuaT3 = phienT3;
                                hasMoney = false;
                                result1.Add($"{phienHumNay.StockSymbol}-{phienHumNay.Date.ToShortDateString()} - MUA - {phienHumNay.C} - Vốn {root}");
                                NhậtKýMuaBán.Add(new LearningRealDataModel(histories, phienHumNay, phienHumWa, phienHumKia, true, 0, root));
                            }
                        }
                        else
                        {
                            var nextPhiens = new List<History>() { phienT1, phienT2, phienT3, phienT4 };
                            if (nextPhiens.All(p => p.C <= phienHumNay.C))
                            {
                                hasMoney = true;
                                var lời = ((Math.Round((decimal)phienHumNay.C / (decimal)(ngayMuaToiUu.C), 2)) - 1) * 100;
                                root = root + (Math.Round((decimal)root * (decimal)(lời) / 100, 2));
                                result1.Add($"{phienHumNay.StockSymbol}-{phienHumNay.Date.ToShortDateString()} - BÁN - {phienHumNay.C} Lời {lời}% - Vốn {root}");

                                //update lời cho lần trước
                                NhậtKýMuaBán.Last().Loi = lời;
                                NhậtKýMuaBán.Add(new LearningRealDataModel(histories, phienHumNay, phienHumWa, phienHumKia, false, lời, root));
                                ngayMuaToiUu = new History();
                                ngayMuaT3 = new History();
                                continue;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        result1.Add($"{symbols[s]._sc_}-{histories[i].Date.ToShortDateString()} - Lỗi {ex.ToString()}");
                        continue;
                    }
                }
            }

            /*
             * Tìm kịch bản cho những ngày trước khi mua/bán    (Done)
             * Tìm kịch bản cho những ngày mua/bán              (thêm cột cho những ngày mua/bán)
             * 
             * [Vòng lặp]
             *  -   duyệt lại kịch bản, chọn thông số phù hợp
             *  -   chạy lại kịch bản dựa trên thông số phù hợp với điểm mua và bán
             *  
             * Chạy cho tất cả các mã
             * [Vòng lặp]
             *  -   duyệt lại kịch bản, chọn thông số phù hợp
             *  -   chạy lại kịch bản dựa trên thông số phù hợp với điểm mua và bán
             */

            var folder = ConstantPath.Path;
            var g = Guid.NewGuid();
            var name = $@"{folder}{g}.xlsx";
            NhậtKýMuaBán.ToDataTable().WriteToExcel(name);

            return result1;
        }

        public async Task<List<string>> KiemTraTileDungSaiTheoPattern(string code, DateTime tuNgay, DateTime toiNgay, LocCoPhieuFilterRequest filter)
        {
            var histories = await _context.History
                .Where(ss => ss.StockSymbol == code
                    && ss.Date <= toiNgay.AddDays(10)
                    && ss.Date >= tuNgay.AddDays(-30))
                .OrderBy(ss => ss.Date)
                .ToListAsync();

            var result1 = new List<string>();
            var ngayBatDau = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= tuNgay);
            for (int i = histories.IndexOf(ngayBatDau); i < histories.Count; i++)
            {
                ngayBatDau = histories[i];
                if (ngayBatDau != null && ngayBatDau.HadAllIndicators())
                {
                    break;
                }
            }

            var ngayMuaToiUu = new History();
            var ngayMuaT3 = new History();
            var tup = new List<Tuple<string, decimal, List<string>>>();
            decimal dung = 0;
            decimal sai = 0;
            var ngayDungLai = histories.OrderBy(h => h.Date).FirstOrDefault(h => h.Date >= toiNgay);
            if (ngayDungLai == null) //đang nhập ngày trong tương lai -> chuyển về hiện tại
            {
                ngayDungLai = histories.OrderByDescending(h => h.Date).First();
            }
            var startedI = histories.IndexOf(ngayBatDau);
            var stoppedI = histories.IndexOf(ngayDungLai);
            for (int i = startedI; i <= stoppedI; i++)
            {
                var phienHumNay = histories[i];
                var phienHumWa = histories[i - 1];
                var lstBan = new List<Tuple<string, bool>>();

                var thoaDK = ThỏaĐiềuKiệnLọc(filter, histories, phienHumNay);
                if (!thoaDK) continue;

                lstBan.Add(new Tuple<string, bool>(filter.Name, thoaDK));

                var giaMua = TimGiaMuaMongMuon(histories, phienHumNay, lstBan);

                if (!giaMua.Item3.HasValue)
                {
                    var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                    result1.Add($"{code} - {stringCTMua} - Mua phiên sau ngày {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Chưa đủ dữ liệu T3 để tính toán giá bán.");
                    continue;
                }

                if (giaMua.Item3.HasValue && giaMua.Item3.Value < 0)
                {
                    var ngayMua = histories.Where(h => h.Date > phienHumNay.Date).OrderBy(h => h.Date).First();
                    var stringCTMua = string.Join(",", lstBan.Where(ct => ct.Item2).Select(ct => ct.Item1).ToList());
                    result1.Add($"{code} - {stringCTMua} - Mua phiên sau ngày {phienHumNay.Date.ToShortDateString()} tại giá {giaMua.Item1} tới giá {giaMua.Item2} - Nhưng thực tế giá thấp nhất ở {ngayMua.L}");
                    continue;
                }

                TimThoiGianBan(code, result1, ref dung, ref sai, histories, phienHumNay, giaMua.Item3.Value, lstBan);
            }

            if (result1.Any())
            {
                //tong = dung + sai;
                dung = dung - sai;
                var winRate = dung;// tong == 0 ? 0 : Math.Round(dung / tong, 2);
                tup.Add(new Tuple<string, decimal, List<string>>(code, winRate, result1));
            }


            //var tile = dung + sai > 0 ? Math.Round(dung / (dung + sai), 2) : 0;
            //result1.Insert(0, $"{code} - Tỉ lệ: {tile}");

            return result1;
        }



        public async Task<List<string>> CongThucLungTung1()
        {
            var symbols = await _context.StockSymbol
                .Where(s => s._sc_.Length == 3 && s.BiChanGiaoDich == false && s.MA20Vol > ConstantData.minMA20VolDaily)
                //.Where(h => h._sc_ == "VKC")
                .ToListAsync();
            var stockCodes = symbols.Select(s => s._sc_).ToList();

            var historiesStockCode = await _context.History
                .Where(ss => stockCodes.Contains(ss.StockSymbol) && ss.Date >= new DateTime(2022, 1, 1))
                .OrderByDescending(ss => ss.Date)
                .ToListAsync();
            /* 1 CP 
             * 2 Nhom Nganh 
             * 3 Gia
             * 4 So sánh với Mây
             * 5 So sánh với MA 20
             * 6 So sánh với đáy T6
             * 7 manh/yeu
             */

            var tup = new List<Tuple<string, string, decimal, decimal>>();
            var result1 = new List<string>();
            Parallel.ForEach(symbols, (Action<StockSymbol>)(symbol =>
            {
                var histories = historiesStockCode
                                    .Where(ss => ss.StockSymbol == symbol._sc_)
                                    .OrderByDescending(h => h.Date)
                                    .ToList();


                var humnay = histories[0];
                var humqua = histories[1];

                if (humnay.C < humnay.BandsMid && humnay.C > humqua.C && humnay.RSI > humqua.RSI)
                {
                    tup.Add(new Tuple<string, string, decimal, decimal>(symbol._in_, humnay.StockSymbol, humnay.V, humnay.C));
                }

                //if (humnay.RSI > humqua.RSI && (humnay.C < humqua.C || humnay.NenBot < humqua.NenBot))
                //{
                //    tup.Add(new Tuple<string, string, decimal, decimal>(symbol._in_, humnay.StockSymbol, humnay.V, humnay.C));
                //}
            }));

            tup = tup
                .OrderBy(h => h.Item1)
                .ThenByDescending(h => h.Item3)
                .ThenBy(h => h.Item4)
                .ThenBy(h => h.Item2).ToList();

            foreach (var item in tup)
            {
                result1.Add($"{item.Item1} - {item.Item3.ToString("N2")} - {item.Item4.ToString("N2")} - {item.Item2}");
            }

            return result1;
        }

        /*
         * Trend giảm
         *              MACD 
         *                              - Tìm 3 đỉnh giảm dần, nối lại tạo trend line giảm
         *                              - Giữa 3 đỉnh sẽ có 2 đáy
         *                              - ở 2 đáy xét động lượng của MACD -> nếu động lượng cao dần thì đó là tín hiệu đảo chiều
         *                              - Giờ chỉ còn chờ đỉnh 4 - sẽ là điểm breakout
         *                              - Kết hợp thêm kháng cự và hỗ trợ 
         *                              
         *                              Có thể kết hợp thêm
         *                               - MACD đường xanh cắt lên đỏ
         *                               - Bolinger bands
         *                               - Ichimoku
         *                               - RSI
         *                               
         *                              Example:
         *                                  Good:
         *                                      AAA: 04/04/2022 - 19/05/2022
         *                              
         *                              Hướng dẫn
         *                                                          https://www.youtube.com/watch?v=fmtBWx9eMHc
         *                                                          confirm pyll back/trendline  (MUST)
         *                                                          confirm macf histogram       (MUST)
         *                                                          confirm support/resistance   (NO)
         *                                                          confirm ending volume        (NO)
         *                                                          confirm entry breakout point (MUST)
         *              
         *              RSI Trendline
         *                              - Tìm đáy trong quá khứ, nối lại tạo trend line A
         *                              - Ở 2 đáy, nối 2 điểm RSI tạo thành trendline B
         *                              - Nếu A đi xuống hoặc ngang, và B đi lên, đây là tín hiệu đảo chiều, cbi tăng
         *                              - Kết hợp thêm kháng cự và hỗ trợ 
         *
         *                              Có thể kết hợp thêm
         *                               - MACD đường xanh cắt lên đỏ
         *                               - Bolinger bands
         *                               - Ichimoku
         *
         *                              BAD examples:
         *                                  - AAA: 20/8/2019 -> 02/10/2019
         *                                      - Sửa: Nếu kết hợp Breakout thì ăn (Breakout: 23/10/2019, vậy là mua ở cây 24/10/2019 - điểm nối từ 14/08/2019, cắt qua 16/9/2019, nó sẽ kéo tới 22/10/2019, sau cây này là cây breakout)
         *                                              - Sau cây breakout, giá chạm MA 20 là bật lên, MA 20 trở thành hỗ trợ - chờ ra hàng
         *                                              - Kết hợp bolinger bands để tìm điểm bán phù hợp trong sideway (top của bolinger bands)
         *                                              - Ngày 14/11/2019 - MA 20 bị phá vỡ, MACD xanh cắt xuống MACD đỏ, momentum MACD âm, RSI dốc xướng dưới 40 => tín hiệu bán mạnh
         *                                         
         *                                         
         * 
         
         */

        /*
         * Nhìn vô TA
         *  - Xu hướng (mục đích làm gì?)
         *      + Xu hướng tăng: MACD 26 từ âm vượt wa dương
         *      + Xu hướng giảm: MACD 26 từ âm vượt wa dương
         *      + Xu hướng sideway:
         *          
         *      + Chart Tuần: 
         *          + 
         *      + Chart Ngày:
         *  - Vẽ trend line:
         *      + 
         *  - RSI
         *  - MACD
         *  - Bolinger Bands
         *      + upway  : bands trên và dưới ko chênh lệnh quá so với trung bình của bands trên/dưới tính từ 3 phiên trước > 3%
         *      + sideway: bands trên và dưới ko chênh lệnh quá so với trung bình của bands trên/dưới tính từ 3 phiên trước > -3% < 3%
         *      + down   : bands trên và dưới ko chênh lệnh quá so với trung bình của bands trên/dưới tính từ 3 phiên trước < 3%
         *  - Ichimoku
         *  - MA 20
         *  - MA 50
         *  - MA 200
         *  
         *  - Mẫu sideway
         *      + Bán đỉnh bolinger bands
         *      + Mua đáy bolinger bands
         *      + Nếu sideway trên MA 20 - thì MA 20 sẽ là hỗ trợ nhẹ, bot bands là hỗ trợ mạnh, top bands là kháng cự -> mua ở hỗ trợ nhẹ, bán ở gần kháng cự * 0.8
         *      + Nếu sideway dưới hoặc ngang MA 20 - không làm gì cả
         *              + Định nghĩa sideway 
         *                  + trên  MA 20: > 80% các nến trong lần kiểm tra đều có giá dưới cùng của nến (O OR C) cao  hơn MA 20 3-5%
         *                  + ngang MA 20: > 80% các nến trong lần kiểm tra đều có thân nến đề lên MA 20
         *                  + dưới  MA 20: > 80% các nến trong lần kiểm tra đều có giá trên cùng của nến (O OR C) thấp hơn MA 20 3-5% 
         
         */
        /*
         * Goi ý mua / bán
         *  - Mã
         *  - Tỉ lệ
         *  - Chu kỳ (từ ngày nào tới ngày nào để tính tỉ lệ)
         *  - Ngày
         *  - Gợi ý (MUA/BÁN)
         *      + MUA: Tổng points > 70
         *      + Bán: Tổng points < -70
         *  - GIÁ
         *  - T0
         *  - T1
         *  - T2
         *  - T3 (NGÀY BÁN) - dừng theo dõi T4, T5 nếu T3 lãi > 10%
         *  - T4 (NGÀY BÁN)
         *  - T5 (NGÀY BÁN)
         *  - RSI
         *  - ICHIMOKU
         *  - MACD
         *  - MA20
         *  - NẾN
         *  - BANDS
         *  - GIÁ
         *  - VOL
         *  - KHÁNG CỰ
         *  - HỖ TRỢ
         * 
         * 
         * RSI: 10 points
         *      - Tính từ hiện tại về tìm 2 điểm chạm nhau
         *          - Đỏ cắt lên xanh
         *              + Giá bắt đầu tăng ngắn hạn => có thể tạo xu hướng tăng ngắn hạn
         *          - Đỏ cắt xuống xanh
         *              + Giá bắt đầu giảm ngắn hạn => có thể tạo xu hướng giảm ngắn hạn
         *      - Phân kì âm/dương
         *          - Tìm từ giao điểm hiện tại (P1) quay ngược lại giao điểm trước đó (P2)
         *              + P1 > P2 => RSI chu kì tăng
         *              + P1 < P2 => RSI chu kì giảm
         *              + Else: RSI chưa xuất hiện chu kỳ
         *          - Từ P1 và P2 của RSI, tìm lên giá đóng của 2 ngày P1 và P2, gọi là C1 và C2
         *              + P1 > P2 
         *                  + C1 > C2 => RSI phân kỳ dương => tiếp tục xu thế
         *                  + C1 < C2 => RSI phân kỳ âm => báo tín hiệu đảo chiều xu thế hiện tại
         *                  + Else: RSI chưa xuất hiện chu kỳ
         *              + Ngược lại y chang
         *      => Báo tín hiệu mua/bán
         *      
         *  MACD
         *      - Tính từ hiện tại về tìm 2 điểm chạm nhau
         *          - Đỏ cắt lên xanh
         *              + Giá bắt đầu tăng ngắn hạn => có thể tạo xu hướng tăng ngắn hạn
         *          - Đỏ cắt xuống xanh
         *              + Giá bắt đầu giảm ngắn hạn => có thể tạo xu hướng giảm ngắn hạn
         *      - Phân kì âm/dương
         *          - Tìm từ giao điểm hiện tại (P1) quay ngược lại giao điểm trước đó (P2)
         *              + P1 > P2 => MACD chu kì tăng
         *              + P1 < P2 => MACD chu kì giảm
         *              + Else: MACD chưa xuất hiện chu kỳ
         *          - Từ P1 và P2 của MACD, tìm lên giá đóng của 2 ngày P1 và P2, gọi là C1 và C2
         *              + P1 > P2 
         *                  + C1 > C2 => MACD phân kỳ dương => tiếp tục xu thế
         *                  + C1 < C2 => MACD phân kỳ âm => báo tín hiệu đảo chiều xu thế hiện tại
         *                  + Else: MACD chưa xuất hiện chu kỳ
         *              + Ngược lại y chang
         *      => Báo tín hiệu mua/bán
         *      
         *  Stoch
         *      - Tính từ hiện tại về tìm 2 điểm chạm nhau
         *          - Đỏ cắt lên xanh
         *              + Giá bắt đầu tăng ngắn hạn => có thể tạo xu hướng tăng ngắn hạn
         *          - Đỏ cắt xuống xanh
         *              + Giá bắt đầu giảm ngắn hạn => có thể tạo xu hướng giảm ngắn hạn
         *      - Phân kì âm/dương
         *          - Tìm từ giao điểm hiện tại (P1) quay ngược lại giao điểm trước đó (P2)
         *              + P1 > P2 => Stoch chu kì tăng
         *              + P1 < P2 => Stoch chu kì giảm
         *              + Else: Stoch chưa xuất hiện chu kỳ
         *          - Từ P1 và P2 của Stoch, tìm lên giá đóng của 2 ngày P1 và P2, gọi là C1 và C2
         *              + P1 > P2 
         *                  + C1 > C2 => Stoch phân kỳ dương => tiếp tục xu thế
         *                  + C1 < C2 => Stoch phân kỳ âm => báo tín hiệu đảo chiều xu thế hiện tại
         *                  + Else: Stoch chưa xuất hiện chu kỳ
         *              + Ngược lại y chang
         *      => Báo tín hiệu mua/bán
         *      
         *  MA20
         *      - Giá cắt xuống dưới MA20: bán
         *      - Giá cắt lên MA 20: mua
         *      
         *  Bollinger bands
         *      - Giá chạm bands trên: tín hiệu đảo chiều
         *      - Giá chạm bands dưới: tín hiệu đảo chiều
         *      
         *  Ichimoku
         *      - Giá Trong mây
         *          - Tenkan cắt lên Kijun : tín hiệu mua trung bình
         *          - Tenkan cắt xuống Kijun: tín hiệu bán trung bình
         *      - Giá Trên mây
         *          - Tenkan cắt lên Kijun : tín hiệu mua mạnh
         *          - Tenkan cắt xuống Kijun: tín hiệu bán nhẹ
         *      - Giá dưới mây: N/A
         *          - Stoch từ 80 trở xuống 
         *      - 
         *      
         *  Giá
         *      - Giá tăng: mua
         *      - Giá giảm: bán
         *      
         *  Vol
         *      - >= MA 20: vol to
         *      - <= MA 20: vol bé
         *      
         *  Giá vs Vol
         *      - Giá tăng - vol tăng: mua => tiep tuc xu hướng mua
         *      - Giá giảm - vol tăng: bán => tiep tuc xu hướng bán
         *      - Giá tăng - vol giảm: giữ hàng => xu hướng đang yếu đi và cbi đảo chiều
         *      - Giá giảm - vol giảm: giữ hàng => xu hướng đang yếu đi và cbi đảo chiều
         *      - Giá tăng đột biến - vol giữ nguyên: bẫy, trừ khi cp đã tăng giảm hết biên độ, hết hàng và trần/sàn
         *      - Giá giữ nguyên - vol đột biến: báo hiệu xu hướng có thể đảo chiều tức xu hướng tăng có thể đảo chiều sang giảm - xu hướng giảm có thể đảo chiều sang tăng.
         *      - giá giữ nguyên, vol giữ nguyên: Thường xuất hiện trong chu kỳ điều chỉnh của cả xu hướng tăng và xu hướng giảm. Sau đó cổ phiếu cần dành phần lớn thời gian để đi ngang với khối lượng nhỏ trước khi tăng tốc mạnh. Thời gian tích lũy càng lâu thì rủi ro mua càng thấp và đà tăng giá sẽ càng mạnh.
         *      
         *  Kháng cự: bán
         *      
         */
    }
}


/*
 * CEO:
 *  + 20/1/22:  vol tăng lớn >= vol của 3 cây giảm liên tiếp trước đó * 0.95
 *              + giá tăng, thân nến xanh dày
 *              + giá bot ~ giá sàn * 0,2%
 *              + RSI hướng lên
 *          ==> Mua vô, trong vòng t5 bán nếu lời >= 15%, hoặc lỗ >= 5%
 *  
 *  1 - rsi phân kì âm ?
 *  2 - MACD hướng lên trong N phiên
 *  3 - MACD hướng xuống trong N phiên
 */