using System.Reflection.Metadata;
using System.Collections.Generic;
using MyMath;
using System.Collections;

namespace Optimization
{
    //Метод золотого сечения
    //На отрезке в n-мерном пространстве
    public class GoldenSectionMethod
    {
        Point leftBorder;
        Point rightBorder;
        double eps;
        static double fi = (Math.Sqrt(5) - 1) / 2;
        static double fi1 = 1 - fi;
        public GoldenSectionMethod(Point leftBorder, Point rigthBorder, double eps)
        {
            this.leftBorder = leftBorder;
            this.rightBorder = rigthBorder;
            this.eps = eps;
        }
        public Point FindExtremum(IFunction function)
        {
            Point a = leftBorder;
            Point b = rightBorder;
            Point x1;
            Point ba;
            Point x2;
            int i = 0;
            while (Point.Distance(a,b) >= eps)
            {
                i++;
                ba = b - a;
                x1 = a + fi1 * ba;
                x2 = a + fi * ba;
                x1.CalculateValue(function);
                x2.CalculateValue(function);
                if (x1.Value > x2.Value)
                    a = x1;
                else b = x2;
            }
            //Console.WriteLine(i);
            Point result = (a + b) / 2;
            result.CalculateValue(function);
            return result;
        }
    }
}