using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormularCT9 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime checkingDate, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();
            var history = histories.FirstOrDefault(h => h.Date == checkingDate);
            if (history == null) return null;

            //var detailedModel = new PatternResponseModel();
            //try
            //{
            //    var patternOnsymbol = new PatternBySymbolResponseModel();
            //    patternOnsymbol.StockCode = code;

            var todayVol = history.V;
            var yesterdayVol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < checkingDate);
            if (yesterdayVol == null) return null;
            var yesterdayV1Vol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterdayVol.Date);
            if (yesterdayV1Vol == null) return null;
            var yesterdayV2Vol = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterdayV1Vol.Date);
            if (yesterdayV2Vol == null) return null;

            //var dk1 = todayVol < yesterdayVol.V;
            var dk1 = yesterdayVol.V < yesterdayV1Vol.V;
            var dk2 = yesterdayV1Vol.V < yesterdayV2Vol.V;


            if (dk1 && dk2)
            {
                //patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                //{
                //    ConditionMatchAt = checkingDate,
                //    MoreInformation = new
                //    {
                //        Text = @$"Lượng giao dịch giảm",
                //        TodayOpening = history.O,
                //        TodayClosing = history.C,
                //        TodayLowest = history.L,
                //        TodayTrading = history.V,
                //        RealityExpectation = string.Empty,
                //        ShouldBuy = true
                //    }
                //});

                result.Name = ConstantData.CT9;
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
