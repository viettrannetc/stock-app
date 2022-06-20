﻿using System;

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
            //decimal xPre = rand.Next(10);

            //// smaller eps, denotes more accuracy
            //decimal eps = 0.001;

            //// initializing difference between two
            //// roots by INT_MAX
            //decimal delX = 2147483647;

            //// xK denotes current value of x
            //decimal xK = 0.0;

            //// loop until we reach desired accuracy
            //while (delX > eps)
            //{
            //    // calculating current value from previous
            //    // value by newton's method
            //    xK = ((N - 1.0) * xPre +
            //    (decimal)A / Math.Pow(xPre, N - 1)) / (decimal)N;
            //    delX = Math.Abs(xK - xPre);
            //    xPre = xK;
            //}

            //return xK;
        }
    }


    public class Line
    {
        public decimal x1 { get; set; }
        public decimal y1 { get; set; }

        public decimal x2 { get; set; }
        public decimal y2 { get; set; }
    }

    public class Point
    {
        public decimal x { get; set; }
        public decimal y { get; set; }
    }

    public static class LineIntersection
    {
        //  Returns Point of intersection if do intersect otherwise default Point (null)
        public static Point FindIntersection(this Line lineA, Line lineB, decimal tolerance = 0.001m)
        {
            decimal x1 = lineA.x1, y1 = lineA.y1;
            decimal x2 = lineA.x2, y2 = lineA.y2;

            decimal x3 = lineB.x1, y3 = lineB.y1;
            decimal x4 = lineB.x2, y4 = lineB.y2;

            // equations of the form x = c (two vertical lines)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance && Math.Abs(x1 - x3) < tolerance)
            {
                throw new Exception("Both lines overlap vertically, ambiguous intersection points.");
            }

            //equations of the form y=c (two horizontal lines)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
            {
                return default(Point);
                throw new Exception("Both lines overlap horizontally, ambiguous intersection points.");
            }

            //equations of the form x=c (two vertical parallel lines)
            if (Math.Abs(x1 - x2) < tolerance && Math.Abs(x3 - x4) < tolerance)
            {
                //return default (no intersection)
                return default(Point);
            }

            //equations of the form y=c (two horizontal parallel lines)
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
            {
                //return default (no intersection)
                return default(Point);
            }

            //general equation of line is y = mx + c where m is the slope
            //assume equation of line 1 as y1 = m1x1 + c1 
            //=> -m1x1 + y1 = c1 ----(1)
            //assume equation of line 2 as y2 = m2x2 + c2
            //=> -m2x2 + y2 = c2 -----(2)
            //if line 1 and 2 intersect then x1=x2=x & y1=y2=y where (x,y) is the intersection point
            //so we will get below two equations 
            //-m1x + y = c1 --------(3)
            //-m2x + y = c2 --------(4)

            decimal x, y;

            //lineA is vertical x1 = x2
            //slope will be infinity
            //so lets derive another solution
            if (Math.Abs(x1 - x2) < tolerance)
            {
                //compute slope of line 2 (m2) and c2
                decimal m2 = (y4 - y3) / (x4 - x3);
                decimal c2 = -m2 * x3 + y3;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x1=c1=x
                //subsitute x=x1 in (4) => -m2x1 + y = c2
                // => y = c2 + m2x1 
                x = x1;
                y = c2 + m2 * x1;
            }
            //lineB is vertical x3 = x4
            //slope will be infinity
            //so lets derive another solution
            else if (Math.Abs(x3 - x4) < tolerance)
            {
                //compute slope of line 1 (m1) and c2
                decimal m1 = (y2 - y1) / (x2 - x1);
                decimal c1 = -m1 * x1 + y1;

                //equation of vertical line is x = c
                //if line 1 and 2 intersect then x3=c3=x
                //subsitute x=x3 in (3) => -m1x3 + y = c1
                // => y = c1 + m1x3 
                x = x3;
                y = c1 + m1 * x3;
            }
            //lineA & lineB are not vertical 
            //(could be horizontal we can handle it with slope = 0)
            else
            {
                //compute slope of line 1 (m1) and c2
                decimal m1 = (y2 - y1) / (x2 - x1);
                decimal c1 = -m1 * x1 + y1;

                //compute slope of line 2 (m2) and c2
                decimal m2 = (y4 - y3) / (x4 - x3);
                decimal c2 = -m2 * x3 + y3;

                //solving equations (3) & (4) => x = (c1-c2)/(m2-m1)
                //plugging x value in equation (4) => y = c2 + m2 * x
                if ((m2 - m1) == 0) return default(Point);

                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                //verify by plugging intersection point (x, y)
                //in orginal equations (1) & (2) to see if they intersect
                //otherwise x,y values will not be finite and will fail this check
                if (!(Math.Abs(-m1 * x + y - c1) < tolerance
                    && Math.Abs(-m2 * x + y - c2) < tolerance))
                {
                    //return default (no intersection)
                    return default(Point);
                }
            }

            //x,y can intersect outside the line segment since line is infinitely long
            //so finally check if x, y is within both the line segments
            if (IsInsideLine(lineA, x, y) &&
                IsInsideLine(lineB, x, y))
            {
                return new Point { x = x, y = y };
            }

            //return default (no intersection)
            return default(Point);

        }

        // Returns true if given point(x,y) is inside the given line segment
        private static bool IsInsideLine(Line line, decimal x, decimal y)
        {
            return (x >= line.x1 && x <= line.x2
                        || x >= line.x2 && x <= line.x1)
                   && (y >= line.y1 && y <= line.y2
                        || y >= line.y2 && y <= line.y1);
        }
    }
}
