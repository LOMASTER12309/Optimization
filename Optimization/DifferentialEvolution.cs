using System.Reflection.Metadata;
using System.Collections.Generic;
using MyMath;
using System.Collections;
using System.Diagnostics;
using System.Xml.Linq;
using Nelder_Meade;
using Microsoft.VisualBasic;

namespace Optimization
{
    //Метод дифференциальной эволюции (без локального оптимизатора)
    public class DifferentialEvolution
    {
        //Границы
        protected double[] leftBorders;
        protected double[] rightBorders;
        //Базовые параметры метода
        protected int n;              //Размерность
        protected int nP;             //Размер популяции
        protected List<Point> points; //Текущие особи
        protected double f;           //Весовой коэффициент
        protected double cr;          //Вероятность кроссовера
        protected int m;              //Число смен поколений
        //Метрики
        protected int iteration = 0;
        protected Point trueExtremum; //Целевой экстремум
        protected Point bestPoint;    //Точка лучшая по значению
        protected Point nearestPoint; //Точка лучшая по приближению
        protected double AVGbest;     //Среднее лучшее значение
        protected int requiredNumOfPopulation = 0;
        protected bool accuracyAchieved = false;
        protected double requiredAccuracy = 0;
        protected double currentBestAccuracy = 0;
        public int Dimension => n;
        public double F => f;
        public int NP => nP;
        public int M => m;
        public double CR => cr;
        public List<Point> Points => points;
        public double AVGBestValue => AVGbest;
        public int RequiredNumOfPopulation => requiredNumOfPopulation;
        public bool AcuuracyAchieved => accuracyAchieved;
        public double BestAccuracy => currentBestAccuracy;
        public DifferentialEvolution(int n, double[] leftBorders, double[] rightBorders, int nP, double f, double cR, int m, Point trueExtremum, double requiredAccuracy)
        {
            this.n = n;
            this.leftBorders = leftBorders;
            this.rightBorders = rightBorders;
            this.nP = nP;
            points = new List<Point>(nP);
            this.f = f;
            this.cr = cR;
            this.m = m;
            this.trueExtremum = trueExtremum;
            this.requiredAccuracy = requiredAccuracy;
        }
        public void SetOptimum(Point trueExtremum)
        {
            this.trueExtremum = trueExtremum;
        }
        public void GenerateTheInitialPopulation()
        {
            points.Clear();
            requiredNumOfPopulation = 0;
            accuracyAchieved = false;
            currentBestAccuracy = 0;
            AVGbest = 0;
            for (int i = 0; i < NP; i++)
                points.Add(GenerateIndivid());
        }
        public Point GenerateIndivid()
        {
            Point result = new Point(new double[n]);
            for (int i = 0; i < n; i++)
                result[i] = leftBorders[i] + Random.Shared.NextDouble()*(rightBorders[i]-leftBorders[i]);
            return result;
        }
        public Point PointAVG()
        {
            var result = new Point(new double[n]);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < NP; j++)
                    result[i] += points[j][i];
                result[i] /= NP;
            }
            return result;
        }
        public double StandartDeviation()
        {
            double result = 0;
            Point pointAVG = PointAVG();
            for (int i = 0; i < NP; i++)
                result += Math.Pow(Point.Distance(points[i], pointAVG), 2);
            result /= NP;
            return Math.Sqrt(result);
        }
        public virtual void Start(IFunction function)
        {
            GenerateTheInitialPopulation();
            foreach (Point p in points)
                p.CalculateValue(function);
            Console.WriteLine("Начальная популяция:");
            PrintPopulation(); Console.WriteLine();
            Console.WriteLine("Лучшая особь:");

            bestPoint = points.MinBy(p => p.Value);
            nearestPoint = points.MinBy(p => Point.Distance(p, trueExtremum));

            currentBestAccuracy = Point.Distance(trueExtremum, nearestPoint);
            bestPoint.PrintPointWithValue("X*", 10); Console.WriteLine();

            iteration = 0;
            List<int> indexes;
            List<Point> childs = new List<Point>(NP);
            int a;
            int b;
            int c;
            Point xc1;
            Point xs;
            double curCR;
            double curAccuracy = 0;
            while (iteration < m)
            {
                iteration++;
                Console.WriteLine($"Итерация {iteration}");
                childs.Clear();

                //Формирование потомков
                for (int i = 0; i < NP; i++)
                {
                    //выбор трёх случайных различных точек
                    indexes = CreateListIndexes(NP, i);
                    a = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(a);
                    b = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(b);
                    c = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(c);

                    //Мутация
                    xc1 = points[c] + F * (points[a] - points[b]);

                    //Скрещивание
                    xs = new Point(new double[n]);
                    for (int w = 0; w < n; w++)
                    {
                        curCR = Random.Shared.NextDouble();
                        if (curCR < CR) 
                            xs[w] = xc1[w];
                        else
                            xs[w] = points[i][w];
                    }
                    xs.CalculateValue(function);
                    childs.Add(xs);
                }
                //Отбор для следующей популяции
                for (int i = 0; i < NP; i++)
                    if (childs[i].Value < points[i].Value)
                    {
                        points[i] = childs[i];
                        if (points[i].Value < bestPoint.Value)
                            bestPoint = points[i];
                        curAccuracy = Point.Distance(points[i], trueExtremum);
                        if (curAccuracy < currentBestAccuracy)
                        {
                            nearestPoint = points[i];
                            currentBestAccuracy = curAccuracy;
                            if ((!accuracyAchieved) && (currentBestAccuracy < requiredAccuracy))
                            {
                                accuracyAchieved = true;
                                requiredNumOfPopulation = iteration;
                            }
                        }
                    }
                Console.WriteLine("Новая популяция:"); 
                PrintPopulation(); 
                Console.WriteLine("Лучшая особь:");
                AVGbest += bestPoint.Value - trueExtremum.Value;
                //lastBest = curBest;
                bestPoint.PrintPointWithValue("X*", 10); Console.WriteLine();
                Console.WriteLine("Ближайшая точка:");
            }
            AVGbest /= m;
            if (accuracyAchieved)
                Console.WriteLine($"Требуема точность достигнута на итерации {requiredNumOfPopulation}");
            else
                Console.WriteLine("Требуема точность НЕ достигнута");
            Console.WriteLine($"Среднее лучшее значение = {AVGbest}");
        }
        public virtual Point FindExtremum(IFunction function)
        {
            GenerateTheInitialPopulation();
            foreach (Point p in points)
                p.CalculateValue(function);

            bestPoint = points.MinBy(p => p.Value);
            nearestPoint = points.MinBy(p => Point.Distance(p, trueExtremum));

            currentBestAccuracy = Point.Distance(trueExtremum, nearestPoint);

            iteration = 0;
            List<int> indexes;
            List<Point> childs = new List<Point>(NP);
            int a;
            int b;
            int c;
            Point xc1;
            Point xs;
            double curCR;
            double curAccuracy = 0;
            while (iteration < m)
            {
                iteration++;
                childs.Clear();

                //Формирование потомков
                for (int i = 0; i < NP; i++)
                {
                    //выбор трёх случайных различных точек
                    indexes = CreateListIndexes(NP, i);
                    a = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(a);
                    b = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(b);
                    c = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(c);

                    //Мутация
                    xc1 = points[c] + F * (points[a] - points[b]);

                    //Скрещивание
                    xs = new Point(new double[n]);
                    for (int w = 0; w < n; w++)
                    {
                        curCR = Random.Shared.NextDouble();
                        if (curCR < CR)
                            xs[w] = xc1[w];
                        else
                            xs[w] = points[i][w];
                    }
                    xs.CalculateValue(function);
                    childs.Add(xs);
                }
                //Отбор для следующей популяции
                for (int i = 0; i < NP; i++)
                    if (childs[i].Value < points[i].Value)
                    {
                        points[i] = childs[i];
                        if (points[i].Value < bestPoint.Value)
                            bestPoint = points[i];
                        curAccuracy = Point.Distance(points[i], trueExtremum);
                        if (curAccuracy < currentBestAccuracy)
                        {
                            nearestPoint = points[i];
                            currentBestAccuracy = curAccuracy;
                            if ((!accuracyAchieved) && (currentBestAccuracy < requiredAccuracy))
                            {
                                accuracyAchieved = true;
                                requiredNumOfPopulation = iteration;
                            }
                        }
                    }
                AVGbest += bestPoint.Value - trueExtremum.Value;
            }
            AVGbest /= m; ;
            return points.MinBy(p => p.Value);
        }
        public List<int> CreateListIndexes(int n, int index)
        {
            List<int> indexes = new List<int>(n);
            for (int i = 0; i < Math.Min(index, n); i++)
                indexes.Add(i);
            for (int i = Math.Max(index+1, 0); i < n; i++)
                indexes.Add(i);
            return indexes;
        }
        public void PrintPopulation()
        {
            for(int i = 0; i < NP; i++)
                points[i].PrintPointWithValue($"X[{i+1}]", 10);
        }
    }
    //Метод дифференциальной эволюции (локальный оптимизатор - метод Нелдера-Мида)
    public class ModDifferentialEvolution: DifferentialEvolution
    {
        //Индексы локально оптимальных точек
        List<bool> methodIsApplied;
        int countMethodIsApplied = 0;
        NMmethod nmParam = new OrigNMmethod(1, 2, 0.5);
        protected double eps;
        public double Epsilon => eps;
        public ModDifferentialEvolution(int n, double[] leftBorders, double[] rightBorders, int nP, double f, double cR, int m, double epsilon, Point trueExtremum, double requiredAccuracy) : base(n, leftBorders, rightBorders, nP, f, cR, m, trueExtremum, requiredAccuracy)
        {
            this.eps = epsilon;
            this.methodIsApplied = new List<bool>(new bool[NP]);
        }
        public override void Start(IFunction function)
        {
            GenerateTheInitialPopulation();
            foreach (Point p in points)
                p.CalculateValue(function);
            Console.WriteLine("Начальная популяция:");
            PrintPopulation(); Console.WriteLine();
            Console.WriteLine("Лучшая особь:");

            bestPoint = points.MinBy(p => p.Value);
            nearestPoint = points.MinBy(p => Point.Distance(p, trueExtremum));

            currentBestAccuracy = Point.Distance(trueExtremum, nearestPoint);
            bestPoint.PrintPointWithValue("X*", 10); Console.WriteLine();

            iteration = 0;
            List<int> indexes;
            List<Point> childs = new List<Point>(NP);
            int a;
            int b;
            int c;
            Point xc1;
            Point xs;
            double curCR;
            double curAccuracy = 0;
            while (iteration < m)
            {
                iteration++;
                Console.WriteLine($"Итерация {iteration}");
                childs.Clear();

                //Формирование потомков
                for (int i = 0; i < NP; i++)
                {
                    //выбор трёх случайных различных точек
                    indexes = CreateListIndexes(NP, i);
                    a = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(a);
                    b = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(b);
                    c = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(c);

                    //Мутация
                    xc1 = points[c] + F * (points[a] - points[b]);

                    //Скрещивание
                    xs = new Point(new double[n]);
                    for (int w = 0; w < n; w++)
                    {
                        curCR = Random.Shared.NextDouble();
                        if (curCR < CR)
                            xs[w] = xc1[w];
                        else
                            xs[w] = points[i][w];
                    }
                    xs.CalculateValue(function);
                    childs.Add(xs);
                }
                //Отбор для следующей популяции
                for (int i = 0; i < NP; i++)
                    if (childs[i].Value < points[i].Value)
                    {
                        points[i] = childs[i];
                        if (points[i].Value < bestPoint.Value)
                            bestPoint = points[i];
                        curAccuracy = Point.Distance(points[i], trueExtremum);
                        if (curAccuracy < currentBestAccuracy)
                        {
                            nearestPoint = points[i];
                            currentBestAccuracy = curAccuracy;
                            if ((!accuracyAchieved) && (currentBestAccuracy < requiredAccuracy))
                            {
                                accuracyAchieved = true;
                                requiredNumOfPopulation = iteration;
                            }
                        }
                        if (methodIsApplied[i])
                        {
                            countMethodIsApplied--;
                            methodIsApplied[i] = false;
                        }
                    }

                LocalOptimization(function);
                AVGbest += bestPoint.Value - trueExtremum.Value;
                Console.WriteLine("Новая популяция:");
                PrintPopulation();
                Console.WriteLine("Лучшая особь:");
                bestPoint.PrintPointWithValue("X*", 10); Console.WriteLine();
                Console.WriteLine($"Отклонение = {StandartDeviation()}"); Console.WriteLine("\n");
            }
            if (accuracyAchieved)
                Console.WriteLine($"Требуема точность достигнута на итерации {requiredNumOfPopulation}");
            else
                Console.WriteLine("Требуема точность НЕ достигнута");
            AVGbest /= m;
            Console.WriteLine($"Среднее лучшее значение = {AVGbest}");
        }

        int RandomIsNotApplied()
        {
            int chosenIndex = -1;
            int count = 0;
            var rnd = Random.Shared;
            for (int i = 0; i < NP; i++)
            {
                if (!methodIsApplied[i])
                {
                    count++;
                    if (rnd.Next(count) == 0)
                        chosenIndex = i;
                }
            }
            return chosenIndex;
        }
        
        public void LocalOptimization(IFunction function)
        {
            int index = 0;
            if (countMethodIsApplied < NP)
            {
                index = RandomIsNotApplied();
                methodIsApplied[index] = true;
                countMethodIsApplied++;
            }
            else
            {
                return;
                //index = Random.Shared.Next(NP);
            }
            /*int index = Random.Shared.Next(NP);
            if (!methodIsApplied[index])
            {
                countMethodIsApplied++;
                methodIsApplied[index] = true;
            }*/


            NelderMeade nm1 = new NelderMeade(nmParam, new Simplex(points[index], StandartDeviation()), eps);
            points[index] = nm1.FindExtremum(function);
            if (points[index].Value < bestPoint.Value)
                bestPoint = points[index];
            if (!accuracyAchieved)
            {
                var curAccuracy = Point.Distance(points[index], trueExtremum);
                if (curAccuracy < currentBestAccuracy)
                {
                    nearestPoint = points[index];
                    currentBestAccuracy = curAccuracy;
                    if ((!accuracyAchieved) && (currentBestAccuracy < requiredAccuracy))
                    {
                        accuracyAchieved = true;
                        requiredNumOfPopulation = iteration;
                    }
                }
            }
        }
        public override Point FindExtremum(IFunction function)
        {
            GenerateTheInitialPopulation();
            foreach (Point p in points)
                p.CalculateValue(function); 
            bestPoint = points.MinBy(p => p.Value);

            nearestPoint = points.MinBy(p => Point.Distance(p, trueExtremum));
            currentBestAccuracy = Point.Distance(trueExtremum, nearestPoint);

            iteration = 0;
            List<int> indexes;
            List<Point> childs = new List<Point>(NP);
            int a;
            int b;
            int c;
            Point xc1;
            Point xs;
            double curCR;
            double curAccuracy = 0;
            while (iteration < m)
            {
                iteration++;
                childs.Clear();

                //Формирование потомков
                for (int i = 0; i < NP; i++)
                {
                    //выбор трёх случайных различных точек
                    indexes = CreateListIndexes(NP, i);
                    a = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(a);
                    b = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(b);
                    c = indexes[Random.Shared.Next(indexes.Count)];
                    indexes.Remove(c);

                    //Мутация
                    xc1 = points[c] + F * (points[a] - points[b]);

                    //Скрещивание
                    xs = new Point(new double[n]);
                    for (int w = 0; w < n; w++)
                    {
                        curCR = Random.Shared.NextDouble();
                        if (curCR < CR)
                            xs[w] = xc1[w];
                        else
                            xs[w] = points[i][w];
                    }
                    xs.CalculateValue(function);
                    childs.Add(xs);
                }
                //Отбор для следующей популяции
                for (int i = 0; i < NP; i++)
                    if (childs[i].Value < points[i].Value)
                    {
                        points[i] = childs[i];
                        if (points[i].Value < bestPoint.Value)
                            bestPoint = points[i];
                        curAccuracy = Point.Distance(points[i], trueExtremum);
                        if ((!accuracyAchieved) && (currentBestAccuracy < requiredAccuracy))
                        {
                            nearestPoint = points[i];
                            currentBestAccuracy = curAccuracy;
                            if (currentBestAccuracy < requiredAccuracy)
                            {
                                accuracyAchieved = true;
                                requiredNumOfPopulation = iteration;
                            }
                        }
                        if (methodIsApplied[i])
                        {
                            countMethodIsApplied--;
                            methodIsApplied[i] = false;
                        }
                    }
                LocalOptimization(function);
                AVGbest += bestPoint.Value - trueExtremum.Value;
            }
            AVGbest /= m;
            return bestPoint;
        }
        public void PrintIndivid(int index, int precision)
        {
            precision = Math.Abs(precision);
            Console.Write("(");
            for (int i = 0; i < n - 1; i++)
            {
                Console.Write($"{Math.Round(points[index][i], precision)}, ");
            }
            Console.Write($"{Math.Round(points[index][n - 1], precision)})");
        }
    }
}