using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormularCT11 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime checkingDate, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();
            var history = histories.FirstOrDefault(h => h.Date == checkingDate);
            if (history == null) return null;

            //var detailedModel = new PatternResponseModel();
            //try
            //{
            //    var patternOnsymbol = new PatternBySymbolResponseModel();
            //    patternOnsymbol.StockCode = code;

            var today = history;
            var yesterdayVol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < checkingDate);
            if (yesterdayVol == null) return null;
            var yesterdayV1Vol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterdayVol.Date);
            if (yesterdayV1Vol == null) return null;
            var yesterdayV2Vol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterdayV1Vol.Date);
            if (yesterdayV2Vol == null) return null;

            var dk1 = today.C > yesterdayVol.C * 1.1M;
            var dk2 = yesterdayVol.C > yesterdayV1Vol.C * 1.1M;
            var dk3 = yesterdayV1Vol.C > yesterdayV2Vol.C * 1.1M;


            if (dk1 & dk2 && dk3)
            {
                //patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                //{
                //    ConditionMatchAt = checkingDate,
                //    MoreInformation = new
                //    {
                //        Text = @$"Tăng trần liên tục",
                //        TodayOpening = history.O,
                //        TodayClosing = history.C,
                //        TodayLowest = history.L,
                //        TodayTrading = history.V,
                //        RealityExpectation = string.Empty,
                //        ShouldBuy = true
                //    }
                //});

                result.Name = ConstantData.CT11;
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
