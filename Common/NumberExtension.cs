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

        // method returns Nth power of A
        public static double NthRoot(this double A, int N)
        {
            return Math.Pow(A, (double)(1.0M / N));
            //Random rand = new Random();
            //// initially guessing a random number between
            //// 0 and 9
            //double xPre = rand.Next(10);

            //// smaller eps, denotes more accuracy
            //double eps = 0.001;

            //// initializing difference between two
            //// roots by INT_MAX
            //double delX = 2147483647;

            //// xK denotes current value of x
            //double xK = 0.0;

            //// loop until we reach desired accuracy
            //while (delX > eps)
            //{
            //    // calculating current value from previous
            //    // value by newton's method
            //    xK = ((N - 1.0) * xPre +
            //    (double)A / Math.Pow(xPre, N - 1)) / (double)N;
            //    delX = Math.Abs(xK - xPre);
            //    xPre = xK;
            //}

            //return xK;
        }
    }
}
