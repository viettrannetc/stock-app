using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormularCT4 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime checkingDate, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            //var detailedModel = new PatternResponseModel();

            var history = histories.FirstOrDefault(h => h.Date == checkingDate);
            if (history == null) return null;

            //var patternOnsymbol = new PatternBySymbolResponseModel();
            //patternOnsymbol.StockCode = code;

            //int sidewayRange = 3;


            var todayVol = history.C;
            var yesterdayVol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < checkingDate);
            if (yesterdayVol == null) return null;
            var yesterdayV1Vol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterdayVol.Date);
            if (yesterdayV1Vol == null) return null;
            var yesterdayV2Vol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterdayV1Vol.Date);
            if (yesterdayV2Vol == null) return null;

            var dk1 = yesterdayVol.C.IsDifferenceInRank(yesterdayV1Vol.C, 0.03M);
            var dk2 = yesterdayV1Vol.C.IsDifferenceInRank(yesterdayV2Vol.C, 0.03M);
            var dk3 = yesterdayVol.C.IsDifferenceInRank(yesterdayV2Vol.C, 0.03M);

            if (dk1 && dk2 && dk3)
            {
                //var sidewayValue = history.MASideway(histories, sidewayRange);
                //var multipleIncrement = sidewayValue > 0
                //    ? Math.Round(history.V / sidewayValue, 2)
                //    : history.V;

                //patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                //{
                //    ConditionMatchAt = checkingDate,
                //    MoreInformation = new
                //    {
                //        Text = @$"Sideway {sidewayRange} ngày",
                //        TodayOpening = history.O,
                //        TodayClosing = history.C,
                //        TodayLowest = history.L,
                //        TodayTrading = history.V,
                //        RealityExpectation = string.Empty,
                //        ShouldBuy = true
                //    }
                //});

                result.Name = ConstantData.CT4;
                result.Price = history.C;
            }

            //if (patternOnsymbol.Details.Any())
            //{
            //    detailedModel.TimDay2.Items.Add(patternOnsymbol);
            //}

            //detailedModel.TimDay2.Items = detailedModel.TimDay2.Items.OrderBy(s => s.StockCode).ToList();
            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
