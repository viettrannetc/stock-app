using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;
//using DotNetCoreSqlDb.Common;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// CT17 = "Nến xác nhận đáy có HP less than CP * 1.01";
    /// HP: Highest price
    /// CP: closed price
    /// </summary>
    public class ReportFormularCT17 : IReportFormular
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

            var dk1 = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).First().ID == secondLowest.ID; //nến xác nhận đáy là ngày ngay sau đáy 2
            var dk2 = history.H > history.C * 1.01M;

            if (dk2 && dk2)
            {
                result.Name = ConstantData.CT17;
                result.Price = history.C;
            }

            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
