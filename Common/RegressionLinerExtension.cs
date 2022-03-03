using DotNetCoreSqlDb.Common.ArrayExtensions;
using DotNetCoreSqlDb.Models;
using DotNetCoreSqlDb.Models.Business;
//using Extreme.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetCoreSqlDb.Common
{
    public static class RegressionLinerExtension
    {
        /// <summary>
        /// x la hoanh
        /// y la tung
        /// </summary>
        /// <param name="transactions"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static List<StockSymbolTradingHistory> TimDiemTangGiaDotBien(this List<StockSymbolTradingHistory> transactions,
            StockSymbolTradingHistory transaction)
        {
            //var prevousTransactions = transactions.Where(t => t.Date < transaction.Date && !t.IsTangDotBien).ToList();

            //double[] xData = prevousTransactions.Select(t => double.Parse(t.Date.ConvertToPhpInt().ToString())).ToArray();
            //double[] yData = prevousTransactions.Select(t => double.Parse(t.MatchQtty.ToString())).ToArray();

            ////double[] yData1 = { 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140 };
            ////double[] xData1 = { 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70 };

            //// Next, we create the regression model. We can pass the data arrays directly.
            //SimpleRegressionModel model1 = new SimpleRegressionModel(yData, xData);
            //model1.NoIntercept = true;
            //model1.Fit();

            ////https://www.extremeoptimization.com/QuickStart/CSharp/SimpleRegression.aspx
            ////model1.AnovaTable.

            var currentIndex = transactions.IndexOf(transaction);
            if (currentIndex >= 1)
            {
                var previousObjectByIndex = transactions[currentIndex - 1];
                if (transaction.MatchQtty > previousObjectByIndex.MatchQtty * 50)
                    transaction.IsTangDotBien = true;
            }

            return transactions;
        }
    }

}
