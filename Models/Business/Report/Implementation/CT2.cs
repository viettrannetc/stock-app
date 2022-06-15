using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// Đáy 2 
    /// </summary>
    public class ReportFormularCT2 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
            if (lowest == null) return null;

            var secondLowest = lowest.LookingForSecondLowest(histories, history);
            if (secondLowest == null) return null;

            var previousDaysForHigestFromLowest = histories.Where(h => h.Date < lowest.Date).OrderByDescending(h => h.Date).Take(30).ToList();
            var highest = previousDaysForHigestFromLowest.OrderByDescending(h => h.C).FirstOrDefault();
            if (highest == null) return null;

            var dk1 = highest.C * 0.85M >= lowest.C;
            //var dk2 = history.C >= secondLowest.C * 1.02M;
            var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID;
            var dk4 = lowest.C * 1.15M >= secondLowest.C;

            if (dk1 /*&& dk2*/ && dk3 && dk4) //basically we should start buying
            {
                result.Name = ConstantData.CT02;
                result.Price = history.C;
            }
                        
            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
