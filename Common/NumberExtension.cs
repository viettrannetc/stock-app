using System;

namespace DotNetCoreSqlDb.Common
{
    public static class NumberExtension
    {
        public static bool IsDifferenceInRank(this decimal firstNumber, decimal secondNumber, decimal percentage)
        {
            var isDifferenceInRank = (firstNumber - firstNumber * percentage) <= secondNumber && secondNumber <= (firstNumber + (firstNumber * percentage));
            
            return isDifferenceInRank;
        }
    }
}
