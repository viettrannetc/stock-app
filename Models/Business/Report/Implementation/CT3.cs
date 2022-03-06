using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{

    public class ReportFormularCT3 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            int expectedLowerPercentage = 25;
            var result = new ReportFormularModel();

            //var detailedModel = new PatternResponseModel();
            //var splitStringCode = string.IsNullOrWhiteSpace(code) ? new string[0] : code.Split(",");

            //try
            //{
            //    var patternOnsymbol = new PatternBySymbolResponseModel();
            //    patternOnsymbol.StockCode = code;
                histories = histories.OrderBy(s => s.Date).ToList();

                var history = histories.FirstOrDefault(h => h.Date == ngay);
                if (history == null) return null;

                var currentDateToCheck = history.Date;
                var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(5).ToList();

                var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
                if (lowest == null) return null;

                var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(20).ToList();
                var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
                if (highest == null) return null;

                var dk1 = highest.C * (100 - expectedLowerPercentage) / 100 >= lowest.C;

                if (dk1) //Start following
                {
                    //patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                    //{
                    //    ConditionMatchAt = currentDateToCheck,
                    //    MoreInformation = new
                    //    {
                    //        Text = @$"Giá giảm hơn {expectedLowerPercentage} từ đáy gần nhất ({lowest.Date}) ở giá {lowest.C} so với đỉnh {highest.C} ngày {highest.Date}",
                    //        TodayOpening = history.O,
                    //        TodayClosing = history.C,
                    //        TodayLowest = history.L,
                    //        TodayTrading = history.V,
                    //        Previous1stLowest = lowest.C,
                    //        Previous1stLowestDate = lowest.Date,
                    //        RealityExpectation = string.Empty,
                    //        ShouldBuy = true
                    //    }
                    //});

                    result.Name = ConstantData.CT3;
                    result.Price = history.C;
                }

            //    if (patternOnsymbol.Details.Any())
            //    {
            //        detailedModel.TimDay2.Items.Add(patternOnsymbol);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}

            //detailedModel.TimDay2.Items = detailedModel.TimDay2.Items.OrderBy(s => s.StockCode).ToList();
            return string.IsNullOrEmpty(result.Name)
                            ? null
                            : result;
        }
    }
}
