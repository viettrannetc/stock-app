using DotNetCoreSqlDb.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetCoreSqlDb.Models.Business.Report.Implementation
{
    /// <summary>
    /// CT20 = "Price T4 -> T10 > CP X 1.1";
    /// </summary>
    public class ReportFormularCT22 : IReportFormular
    {
        public ReportFormularModel Calculation(string code, DateTime checkingDate, List<History> histories, List<StockSymbolTradingHistory> tradingHistories)
        {
            var result = new ReportFormularModel();

            var history = histories.FirstOrDefault(h => h.Date == checkingDate);
            if (history == null) return null;

            var today = history;
            var higestPrice = history.LayGiaCaoNhatCuaCacPhienSau(histories, 4, 10);

            var dk1 = today.C * 1.1M < higestPrice;

            if (dk1)
            {
                result.Name = ConstantData.CT22;
                result.Price = history.C;
            }

            return string.IsNullOrEmpty(result.Name)
                ? null
                : result;
        }
    }
}
