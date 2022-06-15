using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// CT23 = "Có đáy 1"
    /// </summary>
    public class ReportFormularCT23 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);
            if (history == null) return null;
            var lowest = history.LookingForLowestWithout2Percent(histories);
            if (lowest == null) return null;

            result.Name = ConstantData.CT23;
            result.Price = history.C;

            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
