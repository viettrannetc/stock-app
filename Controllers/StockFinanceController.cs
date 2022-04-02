using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Common;
using Flurl.Http;

namespace DotNetCoreSqlDb.Controllers
{
    public class StockFinanceController : Controller
    {
        private readonly MyDatabaseContext _context;

        public StockFinanceController(MyDatabaseContext context)
        {
            _context = context;
        }

        // GET: Finance
        public async Task<List<string>> Index()
        {
            return await _context.StockSymbol.Select(s => s._sc_).ToListAsync();
        }


        public async Task<bool> Get(string code)
        {
            //CDKT
            //CSTC
            //CTKH
            //KQKD
            //LCTT

            return false;
        }


        public async Task<string> Pull(string code)
        {
            var restService = new RestServiceHelper();

            return await restService.HexecuteVietStock();
            //var symbols = await _context.StockSymbol.ToListAsync();

            //Parallel.ForEach(symbols, symbol =>
            //{
            //    var orderedHistoryByStockCode = historiesInPeriodOfTimeByStockCode
            //        .Where(ss => ss.StockSymbol == symbol._sc_)
            //        .OrderBy(s => s.Date)
            //        .ToList();

            //    var latestDate = orderedHistoryByStockCode.OrderByDescending(h => h.Date).FirstOrDefault();
            //    var biCanhCao = latestDate.DangBiCanhCaoGD1Tuan(orderedHistoryByStockCode);

            //    if (biCanhCao) return;

            //    var patternOnsymbol = new PatternBySymbolResponseModel();
            //    patternOnsymbol.StockCode = symbol._sc_;

            //    var historiesInPeriodOfTime = historiesInPeriodOfTimeByStockCode
            //        .Where(ss => ss.StockSymbol == symbol._sc_)
            //        .ToList();

            //    var histories = historiesInPeriodOfTime
            //        .OrderBy(s => s.Date)
            //        .ToList();

            //    var avarageOfLastXXPhien = histories.Take(soPhienGd).Sum(h => h.V) / soPhienGd;
            //    if (avarageOfLastXXPhien < trungbinhGd) return;

            //    var history = histories.FirstOrDefault(h => h.Date == ngay);
            //    if (history == null) return;

            //    var currentDateToCheck = history.Date;
            //    var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();

            //    var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
            //    if (lowest == null) return;

            //    var secondLowest = lowest.LookingForSecondLowest(histories, history);
            //    if (secondLowest == null) return;

            //    var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(soPhienGd).ToList();
            //    var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
            //    if (highest == null) return;

            //    var dk1 = highest.C * 0.85M >= lowest.C;
            //    //var dk2 = history.C >= secondLowest.C * 1.02M;//default value when we have 2nd lowest
            //    var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
            //    var dk4 = lowest.C * 1.15M >= secondLowest.C;

            //    if (dk1 /*&& dk2*/ && dk3 && dk4) //basically we should start buying
            //    {
            //        patternOnsymbol.Details.Add(new PatternDetailsResponseModel
            //        {
            //            ConditionMatchAt = currentDateToCheck,
            //            MoreInformation = new
            //            {
            //                Text = @$"{history.StockSymbol}: Đáy 1 {lowest.Date.ToShortDateString()}: {lowest.C},
            //                            Đáy 2 {secondLowest.Date.ToShortDateString()}: {secondLowest.C},
            //                            Giá đóng cửa hum nay ({history.C}) cao hơn giá đóng của đáy 2 {secondLowest.C},
            //                            Đỉnh trong vòng 30 ngày ({highest.C}) giảm 15% ({highest.C * 0.85M}) vẫn cao hơn giá đóng cửa của đáy 1 {lowest.C},
            //                            Giữa đáy 1 và đáy 2, có giá trị cao hơn đáy 2 và giá đóng cửa ngày hum nay ít nhất 2%",
            //                TodayOpening = history.O,
            //                TodayClosing = history.C,
            //                TodayLowest = history.L,
            //                TodayTrading = history.V,
            //                Previous1stLowest = lowest.C,
            //                Previous1stLowestDate = lowest.Date,
            //                Previous2ndLowest = secondLowest.C,
            //                Previous2ndLowestDate = secondLowest.Date,
            //                AverageNumberOfTradingInPreviousTimes = avarageOfLastXXPhien,
            //                RealityExpectation = string.Empty,
            //                ShouldBuy = true
            //            }
            //        });
            //    }

            //    if (patternOnsymbol.Details.Any())
            //    {
            //        result.TimDay2.Items.Add(patternOnsymbol);
            //    }
            //});

            //result.TimDay2.Items = result.TimDay2.Items.OrderBy(s => s.StockCode).ToList();

            //return result;
        }

    }
}
