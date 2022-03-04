using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    public class ReportFormular3SideWayThenTangDotBien : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime checkingDate, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();
            
            var detailedModel = new PatternResponseModel();

            var history = histories.FirstOrDefault(h => h.Date == checkingDate);
            if (history == null) return null;

            try
            {
                var patternOnsymbol = new PatternBySymbolResponseModel();
                patternOnsymbol.StockCode = code;
                int sidewayRange = 3;
                var dk1 = history.IsUpFromSideway(histories, sidewayRange);

                if (dk1) //basically we should start buying
                {
                    var sidewayValue = history.MASideway(histories, sidewayRange);
                    var multipleIncrement = sidewayValue > 0
                        ? Math.Round(history.V / sidewayValue, 2)
                        : history.V;
                    patternOnsymbol.Details.Add(new PatternDetailsResponseModel
                    {
                        ConditionMatchAt = checkingDate,
                        MoreInformation = new
                        {
                            Text = @$"Sideway {sidewayRange} ngày rùi lượng mua tăng ({multipleIncrement}) lần",
                            TodayOpening = history.O,
                            TodayClosing = history.C,
                            TodayLowest = history.L,
                            TodayTrading = history.V,
                            AverageSideway = sidewayValue,
                            RealityExpectation = string.Empty,
                            ShouldBuy = true
                        }
                    });

                    //res.Date = history.Date; //TODO: find the next transaction date -> will be used to calculate T+3
                    //res.IsActive = true;
                    result.Name = "Tăng mạnh sau sideway";
                    result.Price = history.C;
                }

                if (patternOnsymbol.Details.Any())
                {
                    detailedModel.TimDay2.Items.Add(patternOnsymbol);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            detailedModel.TimDay2.Items = detailedModel.TimDay2.Items.OrderBy(s => s.StockCode).ToList();
            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
