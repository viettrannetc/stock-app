using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// CT24 = "Có đáy 2";
    /// </summary>
    public class ReportFormularCT24 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);
            if (history == null) return null;

            var lowest = history.LookingForLowestWithout2Percent(histories);
            if (lowest == null) return null;

            var secondLowest = lowest.LookingForSecondLowestWithout2Percent(histories, history);
            if (secondLowest == null) return null;



            var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(30).ToList();
            var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
            if (highest == null) return null;

            var dk1 = highest.C * 0.85M >= lowest.C;
            //var dk2 = history.C >= secondLowest.C * 1.02M;
            var dk3 = histories.Where(h => h.Date < history.Date).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
            var dk4 = lowest.C * 1.15M >= secondLowest.C;

            if (dk1 && dk3 && dk4)
            {
                result.Name = ConstantData.CT24;
                result.Price = history.C;
            }

            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
