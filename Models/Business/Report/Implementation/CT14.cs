using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// "Nến xác nhận đáy có CP less than OP less than CP * 1.02";
    /// </summary>
    public class ReportFormularCT14 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);        //nến xác nhận đáy
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
            if (lowest == null) return null;

            var secondLowest = lowest.LookingForSecondLowestWithout2Percent(histories, history);
            if (secondLowest == null) return null;
            var vol20Phien = history.VOL(histories, -20);

            //var dk1 = history.C.IsDifferenceInRank(history.O, 0.02M);
            var dk3 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID; //nến xác nhận đáy là ngày ngay sau đáy 2

            var dk1 = history.C < history.O;
            var dk2 = history.O < history.C * 1.02M;

            if (dk1 && dk2 && dk3)
            {
                result.Name = ConstantData.CT14;
                result.Price = history.C;
            }

            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }

    }
}
