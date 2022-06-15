using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormularCT7 : IReportFormular
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
            int sidewayRange = 1;
            var dk1 = history.IsPriceUp(histories, sidewayRange);

            if (dk1)
            {
                var sidewayValue = history.MAPrice(histories, sidewayRange);
                var multipleIncrement = sidewayValue > 0
                    ? Math.Round(history.C / sidewayValue, 2)
                    : history.C;
                //patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                //{
                //    ConditionMatchAt = checkingDate,
                //    MoreInformation = new
                //    {
                //        Text = @$"Lượng mua tăng ({multipleIncrement}) lần",
                //        TodayOpening = history.O,
                //        TodayClosing = history.C,
                //        TodayLowest = history.L,
                //        TodayTrading = history.V,
                //        AverageSideway = sidewayValue,
                //        RealityExpectation = string.Empty,
                //        ShouldBuy = true
                //    }
                //});

                result.Name = ConstantData.CT07;
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

            //            detailedModel.TimDay2.Items = detailedModel.TimDay2.Items.OrderBy(s => s.StockCode).ToList();
            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
