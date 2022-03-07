using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormularCT2 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            //var detailedAnalysis = new PatternResponseModel();
            //try
            //{
            //    var patternOnsymbol = new PatternBySymbolResponseModel();
            //    patternOnsymbol.StockCode = code;

            //var historiesInPeriodOfTime = historiesInPeriodOfTimeNonDB
            //    .Where(ss => ss.StockSymbol == code)
            //    .ToList();

            //var histories = historiesInPeriodOfTime
            //    .OrderBy(s => s.Date)
            //    .ToList();
            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            //TODO: lowest & 2nd lowest can be reused to improve performance
            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
            if (lowest == null) return null;

            var secondLowest = LookingForSecondLowest(histories, lowest, history);
            if (secondLowest == null) return null;

            var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(30).ToList();
            var highest = previousDaysFromCurrentDay.OrderByDescending(h => h.C).FirstOrDefault();
            if (highest == null) return null;

            var dk1 = highest.C * 0.85M >= lowest.C;
            var dk2 = history.C >= secondLowest.C * 1.02M;
            var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
            var dk4 = lowest.C * 1.15M >= secondLowest.C;

            if (dk1 && dk2 && dk3 && dk4) //basically we should start buying
            {
                //patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                //{
                //    ConditionMatchAt = currentDateToCheck,
                //    MoreInformation = new
                //    {
                //        Text = @$"{history.StockSymbol}: Đáy 1 {lowest.Date.ToShortDateString()}: {lowest.C},
                //                    Đáy 2 {secondLowest.Date.ToShortDateString()}: {secondLowest.C},
                //                    Giá đóng cửa hum nay ({history.C}) cao hơn giá đóng của đáy 2 {secondLowest.C},
                //                    Đỉnh trong vòng 30 ngày ({highest.C}) giảm 15% ({highest.C * 0.85M}) vẫn cao hơn giá đóng cửa của đáy 1 {lowest.C},
                //                    Giữa đáy 1 và đáy 2, có giá trị cao hơn đáy 2 và giá đóng cửa ngày hum nay ít nhất 2%",
                //        TodayOpening = history.O,
                //        TodayClosing = history.C,
                //        TodayLowest = history.L,
                //        TodayTrading = history.V,
                //        Previous1stLowest = lowest.C,
                //        Previous1stLowestDate = lowest.Date,
                //        Previous2ndLowest = secondLowest.C,
                //        Previous2ndLowestDate = secondLowest.Date,
                //        AverageNumberOfTradingInPreviousTimes = 0,
                //        RealityExpectation = string.Empty,
                //        ShouldBuy = true
                //    }
                //});

                result.Name = ConstantData.CT2;
                result.Price = history.C;
            }

            //if (patternOnsymbol.Details.Any())
            //{
            //    detailedAnalysis.TimDay2.Items.Add(patternOnsymbol);
            //}
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}

            //detailedAnalysis.TimDay2.Items = detailedAnalysis.TimDay2.Items.OrderBy(s => s.StockCode).ToList();
            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }

        private StockSymbolHistory LookingForSecondLowest(List<StockSymbolHistory> histories, StockSymbolHistory lowest, StockSymbolHistory currentDateHistory)
        {
            var theDaysAfterLowest = histories.Where(h => h.Date > lowest.Date && h.Date <= currentDateHistory.Date)
                .OrderBy(h => h.Date)
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
    }
}
