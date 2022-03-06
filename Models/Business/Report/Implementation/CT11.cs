using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormularCT11 : IReportFormular
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

            var today = history;
            var yesterday = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < checkingDate);
            var yesterdayV1 = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterday.Date);
            var yesterdayV2 = histories.OrderByDescending(h => h.Date).FirstOrDefault(h => h.Date < yesterdayV1.Date);

            var dk1 = today.C > yesterday.C * 1.1M;
            var dk2 = yesterday.C > yesterdayV1.C * 1.1M;
            var dk3 = yesterdayV1.C > yesterdayV2.C * 1.1M;


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
