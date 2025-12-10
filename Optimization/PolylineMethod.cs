using System.Reflection.Metadata;
using System.Collections.Generic;
using MyMath;
using System.Collections;
namespace Optimization
{
    //Метод ломанных
    //На отрезке в n-мерном пространстве
    public class PolylineMethod
    {
        int dimension;
        double l;
        Point leftBorder;
        Point rightBorder;
        Point vector;
        double dist;
        List<(Point, double prX, double prY)> points;
        List<(double prX, double prY, bool isActual)> candidates;
        List<Point> startPoints;
        Point bestPoint;
        (double prX, double prY, bool isActual) bestCandidate;
        int indexBestCandidate;
        double eps;
        int iteration = 0;
        public int Dimension => dimension;
        public int Iterations => iteration;
        public double L
        {
            get { return l; }
            set { l = value; }
        }
        public PolylineMethod(int dimension, Point leftBorder, Point rightBorder, double LipschitzConstant, double eps)
        {
            this.dimension = dimension;
            this.leftBorder = leftBorder;
            this.rightBorder = rightBorder;
            this.vector = rightBorder - leftBorder;
            this.eps = eps;
            dist = Point.Distance(leftBorder, rightBorder);
            l = LipschitzConstant;
            this.points = new List<(Point, double prX, double prY)>();
            this.startPoints = null;
        }
        public PolylineMethod(int dimension, Point leftBorder, Point rightBorder, double LipschitzConstant, double eps, List<Point> startPoints)
        {
            this.dimension = dimension;
            this.leftBorder = leftBorder;
            this.rightBorder = rightBorder;
            this.vector = rightBorder - leftBorder;
            this.eps = eps;
            dist = Point.Distance(leftBorder, rightBorder);
            l = LipschitzConstant;
            this.points = new List<(Point, double prX, double prY)>();
            this.startPoints = startPoints;
        }
        void MakeListPoint(IFunction func)
        {
            leftBorder.CalculateValue(func);
            rightBorder.CalculateValue(func);
            this.points.Add((leftBorder, 0, leftBorder.Value));
            bestPoint = leftBorder;
            if (startPoints != null)
                foreach (Point point in startPoints)
                {
                    double Ypr = point.CalculateValue(func);
                    if (point.Value < bestPoint.Value) 
                        bestPoint = point;
                    this.points.Add((point, Point.Distance(leftBorder, point), Ypr));
                }
            this.points.Add((rightBorder, dist, rightBorder.Value));
            if (rightBorder.Value < bestPoint.Value)
                bestPoint = rightBorder;
        }
        public Point FindExtremum(IFunction func)
        {
            MakeListPoint(func);

            FillCandidates(func);

            iteration = 0;
            while (bestPoint.Value - bestCandidate.prY >= eps)
            {
                iteration++;
                
                Point newPoint = CalcTruePoint(bestCandidate.prX);
                newPoint.CalculateValue(func);

                var curPoint = (newPoint, bestCandidate.prX, newPoint.Value);
                points.Insert(indexBestCandidate + 1, curPoint);
                if (newPoint.Value < bestPoint.Value)
                    bestPoint = newPoint;

                var leftPoint = points[indexBestCandidate];
                var rigthPoint = points[indexBestCandidate + 2];
                var newCandidate1 = CalcCandidate(leftPoint, curPoint);
                var newCandidate2 = CalcCandidate(curPoint, rigthPoint);

                candidates.RemoveAt(indexBestCandidate);
                candidates.InsertRange(indexBestCandidate, new[] { newCandidate1, newCandidate2 });
                bestCandidate = candidates.MinBy(p => p.prY);
                indexBestCandidate = candidates.IndexOf(bestCandidate);
            }
            return bestPoint;
        }
        public void Start(IFunction func)
        {
            MakeListPoint(func);

            FillCandidates(func);
            int i = 0;
            foreach (var cand in candidates)
            {
                i++;
                Point trueCand = CalcTruePoint(cand.prX);
                Console.WriteLine($"Cand{i}: prX = {cand.prX}; prY = {cand.prY}");
                Console.Write($"TrueCand{1}: ");
                Console.WriteLine($"isActual = {cand.isActual}");
                trueCand.PrintPoint(10);
                Console.WriteLine();
            }
            Console.WriteLine($"bestCandidate = ({CalcTruePoint(bestCandidate.prX)[0]}, {bestCandidate.prY})");

            iteration = 0;
            while (bestPoint.Value - bestCandidate.prY >= eps)
            {
                iteration++;
                Console.WriteLine($"Условие останова = {bestPoint.Value - bestCandidate.prY}");
                Console.WriteLine($" Iteration {iteration}:");
                Point newPoint = CalcTruePoint(bestCandidate.prX);
                newPoint.CalculateValue(func);

                newPoint.PrintPointWithValue("newPoint", 10);

                var curPoint = (newPoint, bestCandidate.prX, newPoint.Value);
                points.Insert(indexBestCandidate + 1, curPoint);
                if (newPoint.Value < bestPoint.Value)
                    bestPoint = newPoint;

                var leftPoint = points[indexBestCandidate];
                var rigthPoint = points[indexBestCandidate + 2];
                var newCandidate1 = CalcCandidate(leftPoint, curPoint);
                var newCandidate2 = CalcCandidate(curPoint, rigthPoint);

                candidates.RemoveAt(indexBestCandidate);
                candidates.InsertRange(indexBestCandidate, new[] { newCandidate1, newCandidate2 });
                bestCandidate = candidates.MinBy(p => p.prY);
                indexBestCandidate = candidates.IndexOf(bestCandidate);

                Console.WriteLine("Список кандидатов: ");
                int j = 1;
                foreach (var candidate in candidates)
                {
                    Console.WriteLine($"cand[{j}] = ({CalcTruePoint(candidate.prX)[0]}, {candidate.prY})");
                    j++;
                }
                Console.WriteLine($"bestCandidate = ({CalcTruePoint(bestCandidate.prX)[0]}, {bestCandidate.prY})");
                Console.WriteLine($"h = {bestPoint.Value}");
                Console.WriteLine();
            }

            Console.WriteLine("Result:");
            bestPoint.PrintPointWithValue("X*", 10);
        }
        void FillCandidates(IFunction func)
        {
            bestCandidate = (0, double.PositiveInfinity, true);
            candidates = new List<(double Xpr, double Ypr, bool isActual)>();
            IEnumerator pointEnumerator = points.GetEnumerator();
            pointEnumerator.MoveNext();
            (Point, double Xpr, double Ypr) firstPoint = ((Point, double Xpr, double Ypr))pointEnumerator.Current;
            (Point, double Xpr, double Ypr) secondPoint;


            int index = 0;
            while (pointEnumerator.MoveNext())
            {
                secondPoint = ((Point, double Xpr, double Ypr))pointEnumerator.Current;
                var candidate = CalcCandidate(firstPoint, secondPoint);
                if (candidate.isActual)
                    if (candidate.Ypr < bestCandidate.prY)
                    {
                        bestCandidate = candidate;
                        indexBestCandidate = index;
                    }
                candidates.Add(candidate);
                firstPoint = secondPoint;
                index++;
            }
        }
        public (double Xpr, double Ypr, bool isActual) CalcCandidate((Point, double Xpr, double Ypr) point1, (Point, double Xpr, double Ypr) point2)
        {
            double projectionXCandidate = 0.5 * (point1.Xpr + point2.Xpr + (point1.Ypr - point2.Ypr) / L);
            double projectionYCandidate = point1.Ypr + L * (point1.Xpr - projectionXCandidate);  //важно
            bool isActual = (projectionYCandidate < bestPoint.Value);
            return (projectionXCandidate, projectionYCandidate, isActual);
        }
        public Point CalcTruePoint(double proj)
        {
            return leftBorder + (proj / dist) * vector;
        }
    }
}
