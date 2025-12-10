using System.Reflection.Metadata;
using System.Collections.Generic;
using MyMath;
using System.Collections;
using System.Reflection;

namespace Optimization
{
    //Метод кусочно-линейной аппроксимации
    //На отрезке в n-мерном пространстве
    public class PiecewiseLinearApprox
    {
        LinkedList<(Point point, bool suspected)> points;
        Point leftBorder;
        Point rightBorder;
        int m;
        int iteration;
        int countSuspected;
        int countMatches;
        double eps;
        List<Point> localMinima;
        public PiecewiseLinearApprox(Point leftBorder, Point rightBorder, int m, double eps)
        {
            this.m = m;
            this.leftBorder = leftBorder;
            this.rightBorder = rightBorder;
            this.eps = eps;
        }
        Point Middle(Point point1, Point point2, IFunction func)
        {
            Point middle = (point1 + point2)/2;
            middle.CalculateValue(func);
            return middle;
        }
        public void Start(IFunction function)
        {
            Initialize(function);
            PrintPoints();
            int curCountSuspected = 0;
            while (countMatches < m)
            {
                iteration++;
                Console.WriteLine($"Iteration {iteration}");
                curCountSuspected = 0;
                var leftPoint = points.First;
                var rightPoint = leftPoint.Next;
                points.AddAfter(leftPoint, (Middle(leftPoint.Value.point, rightPoint.Value.point, function), false));
                var curPoint = leftPoint.Next;

                //leftPoint.Value = (leftPoint.Value.point, (leftPoint.Value.point.Value < curPoint.Value.point.Value));
                leftPoint.ValueRef.suspected = (leftPoint.Value.point.Value < curPoint.Value.point.Value);
                curPoint.ValueRef.suspected = curPoint.Value.point.Value < Math.Min(leftPoint.Value.point.Value, rightPoint.Value.point.Value);

                curCountSuspected += leftPoint.Value.suspected ? 1 : 0;
                curCountSuspected += curPoint.Value.suspected ? 1 : 0;
                int j = 1;
                while (rightPoint.Next != null)
                {
                    leftPoint = rightPoint;
                    rightPoint = leftPoint.Next;
                    points.AddAfter(leftPoint, (Middle(leftPoint.Value.point, rightPoint.Value.point, function), false));
                    curPoint = leftPoint.Next;

                    leftPoint.ValueRef.suspected = (leftPoint.Value.point.Value < Math.Min(leftPoint.Previous.Value.point.Value, curPoint.Value.point.Value));
                    curPoint.ValueRef.suspected = (curPoint.Value.point.Value < Math.Min(leftPoint.Value.point.Value, rightPoint.Value.point.Value));

                    curCountSuspected += leftPoint.Value.suspected ? 1 : 0;
                    curCountSuspected += curPoint.Value.suspected ? 1 : 0;
                    j += 2;
                }

                rightPoint.ValueRef.suspected = (rightPoint.Value.point.Value < curPoint.Value.point.Value);
                curCountSuspected += rightPoint.Value.suspected ? 1 : 0;

                if (curCountSuspected == countSuspected) countMatches++;
                else countMatches = 0;
                countSuspected = curCountSuspected;

                PrintPoints();
            }
            Console.WriteLine("\nРезультат");
            var sections = SearchSection();
            Console.WriteLine($"Количество подозрительных отрезков = {countSuspected}");
            Console.WriteLine("Подозрительные отрезки:");
            localMinima = new List<Point>(countSuspected);
            foreach (var section in sections)
            {
                int accuracy = -(int)(Math.Min(0, Math.Ceiling(Math.Log10(Point.Distance(section.point1, section.point2)))) - 5);
                Console.Write($"[{section.point1.toString(accuracy)}; {section.point2.toString(accuracy)}];    ");
                double eps = Math.Min(Math.Pow(0.1, accuracy), this.eps);
                var method = new GoldenSectionMethod(section.point1, section.point2, eps);
                Point extremum = method.FindExtremum(function);
                localMinima.Add(extremum);
                extremum.PrintPointWithValue("X*", accuracy+2);
            }
            Point result = localMinima.MinBy(p => p.Value);
            Console.WriteLine("\nНаилуший результат: ");
            result.PrintPointWithValue("X*", 10);

        }
        public Point FindExtremum(IFunction function)
        {
            Initialize(function);
            int curCountSuspected = 0;
            while (countMatches < m)
            {
                iteration++;

                curCountSuspected = 0;
                var leftPoint = points.First;
                var rightPoint = leftPoint.Next;
                points.AddAfter(leftPoint, (Middle(leftPoint.Value.point, rightPoint.Value.point, function), false));
                var curPoint = leftPoint.Next;

                //leftPoint.Value = (leftPoint.Value.point, (leftPoint.Value.point.Value < curPoint.Value.point.Value));
                leftPoint.ValueRef.suspected = (leftPoint.Value.point.Value < curPoint.Value.point.Value);
                curPoint.ValueRef.suspected = curPoint.Value.point.Value < Math.Min(leftPoint.Value.point.Value, rightPoint.Value.point.Value);

                curCountSuspected += leftPoint.Value.suspected ? 1 : 0;
                curCountSuspected += curPoint.Value.suspected ? 1 : 0;
                int j = 1;
                while (rightPoint.Next != null)
                {
                    leftPoint = rightPoint;
                    rightPoint = leftPoint.Next;
                    points.AddAfter(leftPoint, (Middle(leftPoint.Value.point, rightPoint.Value.point, function), false));
                    curPoint = leftPoint.Next;

                    leftPoint.ValueRef.suspected = (leftPoint.Value.point.Value < Math.Min(leftPoint.Previous.Value.point.Value, curPoint.Value.point.Value));
                    curPoint.ValueRef.suspected = (curPoint.Value.point.Value < Math.Min(leftPoint.Value.point.Value, rightPoint.Value.point.Value));

                    curCountSuspected += leftPoint.Value.suspected ? 1 : 0;
                    curCountSuspected += curPoint.Value.suspected ? 1 : 0;
                    j += 2;
                }

                rightPoint.ValueRef.suspected = (rightPoint.Value.point.Value < curPoint.Value.point.Value);
                curCountSuspected += rightPoint.Value.suspected ? 1 : 0;

                if (curCountSuspected == countSuspected) countMatches++;
                else countMatches = 0;
                countSuspected = curCountSuspected;
            }
            var sections = SearchSection();
            localMinima = new List<Point>(countSuspected);
            foreach (var section in sections)
            {
                int accuracy = -(int)(Math.Min(0, Math.Ceiling(Math.Log10(Point.Distance(section.point1, section.point2)))) - 5);
                double eps = Math.Min(Math.Pow(0.1, accuracy), this.eps);
                var method = new GoldenSectionMethod(section.point1, section.point2, eps);
                Point extremum = method.FindExtremum(function);
                localMinima.Add(extremum);
            }
            return localMinima.MinBy(p => p.Value);
        }
        public void Initialize(IFunction function)
        {
            points = new LinkedList<(Point point, bool suspected)>();
            countSuspected = 0;
            iteration = 0;
            countMatches = 0;
            Point middle = Middle(leftBorder, rightBorder, function);
            Point leftMiddle = Middle(middle, leftBorder, function);
            Point rightMiddle = Middle(middle, rightBorder, function);

            leftBorder.CalculateValue(function);
            rightBorder.CalculateValue(function);
            /*middle.CalculateValue(function);
            leftMiddle.CalculateValue(function);
            rightMiddle.CalculateValue(function);*/

            points.AddLast((leftBorder, leftBorder.Value < leftMiddle.Value));
            points.AddLast((rightBorder, rightBorder.Value < rightMiddle.Value));

            var curPoint = points.First;
            points.AddAfter(curPoint, (middle, middle.Value < Math.Min(leftMiddle.Value, rightMiddle.Value)));
            curPoint = curPoint.Next;
            points.AddBefore(curPoint, (leftMiddle, leftMiddle.Value < Math.Min(leftBorder.Value, middle.Value)));
            points.AddAfter(curPoint, (rightMiddle, rightMiddle.Value < Math.Min(middle.Value, rightBorder.Value)));

            countSuspected = points.Count(p => p.suspected);
        }
        public List<(Point point1, Point point2)> SearchSection()
        {
            var list = new List<(Point point1, Point point2)>();
            var leftPoint = points.First;
            if (leftPoint.Value.suspected)
                list.Add((leftPoint.Value.point, leftPoint.Next.Value.point));
            var curPoint = leftPoint.Next;
            while (curPoint.Next != null)
            {
                if (curPoint.Value.suspected)
                    list.Add((curPoint.Previous.Value.point, curPoint.Next.Value.point));
                curPoint = curPoint.Next;
            }
            if (curPoint.Value.suspected)
                list.Add((curPoint.Previous.Value.point, curPoint.Value.point));
            return list;
        }
        public void PrintPoints()
        {
            int index = 0;
            foreach (var point in points)
            {
                index++;
                point.point.PrintPointWithValue($"X[{index}]", 10);
                Console.WriteLine($"X[{index}] - {((point.suspected) ? ("Подозрительная") : ("Обычная"))}");
            }
        }
    }
}
