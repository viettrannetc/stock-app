﻿using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// CT19 = "Vol nến đáy 2 less than tb 20 phiên";
    /// Nến xác nhận đáy: ngày sau đáy 2 
    /// Nến đáy 2: đáy 2 
    /// </summary>
    public class ReportFormularCT19 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime ngay, List<StockSymbolHistory> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            histories = histories.OrderBy(s => s.Date).ToList();

            var history = histories.FirstOrDefault(h => h.Date == ngay);
            if (history == null) return null;

            var currentDateToCheck = history.Date;
            var previousDaysFromCurrentDay = histories.Where(h => h.Date < currentDateToCheck).OrderByDescending(h => h.Date).Take(30).ToList();

            var lowest = previousDaysFromCurrentDay.OrderBy(h => h.C).FirstOrDefault();
            if (lowest == null) return null;

            var secondLowest = lowest.LookingForSecondLowestWithout2Percent(histories, history);
            if (secondLowest == null) return null;

            var vol20Phien = history.VOL(histories, -20);

            var dk1 = secondLowest.V < vol20Phien;
            if (dk1)
            {
                result.Name = ConstantData.CT19;
                result.Price = history.C;
            }

            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }

    }
}
