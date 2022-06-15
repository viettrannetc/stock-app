using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormularCT6 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime checkingDate, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            //var detailedModel = new PatternResponseModel();

            var history = histories.FirstOrDefault(h => h.Date == checkingDate);
            if (history == null) return null;

            //try
            //{
            //    var patternOnsymbol = new PatternBySymbolResponseModel();
            //    patternOnsymbol.StockCode = code;
            int sidewayRange = 1;
            var dk1 = history.IsVolDown(histories, sidewayRange);

            if (dk1)
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

                result.Name = ConstantData.CT06;
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
