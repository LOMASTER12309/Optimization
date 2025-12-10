using System.IO;
using System.Reflection;

using MyMath;

using Optimization;
namespace Nelder_Meade
{
    //Разичные оптимизации метода Нелдера-Мида
    public abstract class ANelderMeade
    {
        protected NMmethod method;
        protected ASimplex simplex;
        protected IFunction function;
        protected double eps = 0;
        protected List<(string, Point[])> WorkReport = new List<(string, Point[])>();
        protected Point result = null;
        protected int iteration;
        public int Iteration => iteration;
        public ANelderMeade(NMmethod method, ASimplex simplex, double eps = 0.01)
        {
            this.method = method.Clone();
            this.simplex = simplex.Clone();
            this.eps = eps;
        }
        public void SetMethod(NMmethod method)
        {
            this.method = method.Clone();
        }
        public NMmethod GetMethod()
        {
            return method.Clone();
        }
        public void RestartSimplex(double l)
        {
            simplex.RestartSimplex(simplex.Points[0], l);
        }
        public void RestartSimplex(int mod = 1)
        {
            simplex.RestartSimplex(simplex.Points[0], mod);
        }
        public double getOffset()
        {
            return simplex.Offset;
        }
        public double Epsilon => eps;
        public abstract void Start(IFunction function);
        public abstract Point FindExtremum(IFunction function);
        public Point Result => result;
    }
    public class NelderMeadeForHypersphere: ANelderMeade
    {
        public NelderMeadeForHypersphere(NMmethod method, Hypersphere simplex, double eps = 0.01) : base(method, simplex, eps) { }

        public override Point FindExtremum(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            while (!simplex.StopCondition(eps))
            {
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
                iteration++;
                //Point center = method.Center(simplex.Points);
                Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);
                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
            }
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            return simplex.Points[0];
        }

        public override void Start(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            while (!simplex.StopCondition(eps))
            {
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
                iteration++;
                Console.WriteLine($"\nIteration {iteration}");
                Console.WriteLine(simplex.SimplexInfo(10));
                simplex.PrintSimplex(10, true, 10);

                //Point center = method.Center(simplex.Points);
                Point center = (Point)((Hypersphere)simplex).Center.Clone();

                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);

                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);

                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);

                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);  //отражение
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                        simplex.UpdateSimplex(Action.Stretching, stretch, center); //растяжение
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center); //отражение
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center); //сжатие
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
            }
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            Console.WriteLine("Итоговый симплекс");
            Console.WriteLine(simplex.SimplexInfo(10));
            simplex.PrintSimplex(10, true, 10);
            result = simplex.Points[0];
        }
    }
    public class NelderMeade : ANelderMeade
    {
        public NelderMeade(NMmethod method, Simplex simplex, double eps = 0.01) : base(method, simplex, eps) { }

        public override Point FindExtremum(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);
                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            return simplex.Points[0];
        }

        public override void Start(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                Console.WriteLine($"\nIteration {iteration}");
                Console.WriteLine(simplex.SimplexInfo(10));
                simplex.PrintSimplex(10, true, 10);
                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function);            reflect.CalculateValue(function);
                reflect.PrintPointWithValue("reflectPoint", 10);
                Point stretch = method.Stretching(center, reflect, function);                   stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);

                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                    {
                        /*Point rc = reflect - center;
                        Point stretch1 = stretch + rc;
                        stretch1.CalculateValue(function);
                        while (stretch1.Value < stretch.Value)
                        {
                            stretch = stretch1;
                            stretch1 += rc;
                            stretch1.CalculateValue(function);
                        }*/
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    }
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            Console.WriteLine("Итоговый симплекс");
            Console.WriteLine(simplex.SimplexInfo(10));
            simplex.PrintSimplex(10, true, 10);
            result = simplex.Points[0];
        }
    }
    public class Mod1NelderMeade : ANelderMeade
    {
        (double left, double rigrt) alphaBorder;
        (double left, double rigrt) bettaBorder;
        (double left, double rigrt) gammaBorder;
        double bp = 1.3;
        double bm = 0.7;
        public Mod1NelderMeade(NMmethod method, Simplex simplex, (double, double) alphaBorder, (double, double) bettaBorder, (double, double) gammaBorder, double eps = 0.01) : base(method, simplex, eps)
        {
            this.alphaBorder = alphaBorder;
            this.bettaBorder = bettaBorder;
            this.gammaBorder = gammaBorder;
        }

        public override Point FindExtremum(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;

                Point center = method.Center(simplex.Points, function);
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);

                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                    method.Gamma = Math.Min(gammaBorder.rigrt, method.Gamma * bp);
                    method.Alpha = Math.Min(alphaBorder.rigrt, method.Alpha * bp);
                    //method.Beta = Math.Max(2, method.Beta * bp);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                    {

                        Point rc = reflect - center;
                        Point stretch1 = stretch + rc;
                        stretch1.CalculateValue(function);
                        while (stretch1.Value < stretch.Value)
                        {
                            //j++;
                            stretch = stretch1;
                            stretch1.Add(rc);
                            stretch1.CalculateValue(function);
                        }
                        method.Beta = Math.Min(bettaBorder.rigrt, method.Beta * bp);
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    }
                    else
                    {
                        simplex.UpdateSimplex(Action.Reflection, reflect, center);
                        method.Beta = Math.Max(bettaBorder.left, method.Beta * bm);
                    }
                    method.Alpha = Math.Min(alphaBorder.rigrt, method.Alpha * bp);
                    method.Gamma = Math.Min(gammaBorder.rigrt, method.Gamma * bp);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                    method.Alpha = Math.Max(alphaBorder.left, method.Alpha * bm);
                    method.Gamma = Math.Max(gammaBorder.left, method.Gamma * bm);
                    method.Beta = Math.Max(bettaBorder.left, method.Beta * bm);
                }
                else
                {
                    simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                    method.Alpha = Math.Max(alphaBorder.left, method.Alpha * bm);
                    method.Gamma = Math.Max(gammaBorder.left, method.Gamma * bm);
                    method.Beta = Math.Max(bettaBorder.left, method.Beta * bm);
                }
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            return simplex.Points[0];
        }

        public override void Start(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                Console.WriteLine($"\nIteration {iteration}");
                Console.WriteLine(simplex.SimplexInfo(10));
                simplex.PrintSimplex(10, true, 10);

                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                reflect.PrintPointWithValue("reflectPoint", 10);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);

                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                    method.Gamma = Math.Min(gammaBorder.rigrt, method.Gamma* 1.1);
                    method.Alpha = Math.Min(alphaBorder.rigrt, method.Alpha * 1.1);
                    //method.Beta = Math.Max(2, method.Beta * 0.9);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                    {
                        Point rc = reflect - center;
                        Point stretch1 = stretch + rc;
                        stretch1.CalculateValue(function);
                        while (stretch1.Value < stretch.Value)
                        {
                            stretch = stretch1;
                            stretch1 += rc;
                            stretch1.CalculateValue(function);
                        }
                        method.Beta = Math.Min(bettaBorder.rigrt, method.Beta *1.1);
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    }
                    else
                    {
                        simplex.UpdateSimplex(Action.Reflection, reflect, center);
                        method.Beta = Math.Max(bettaBorder.left, method.Beta * 0.9);
                    }
                    method.Alpha = Math.Min(alphaBorder.rigrt, method.Alpha * 1.1);
                    method.Gamma = Math.Min(gammaBorder.rigrt, method.Gamma * 1.1);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                    method.Alpha = Math.Max(alphaBorder.left, method.Alpha * 0.9);
                    method.Gamma = Math.Max(gammaBorder.left, method.Gamma * 0.9);
                    method.Beta = Math.Max(bettaBorder.left, method.Beta * 0.9);
                }
                else
                {
                    simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                    method.Alpha = Math.Max(alphaBorder.left, method.Alpha * 0.9);
                    method.Gamma = Math.Max(gammaBorder.left, method.Gamma * 0.9);
                    method.Beta = Math.Max(bettaBorder.left, method.Beta * 0.9);
                }
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            Console.WriteLine("Итоговый симплекс");
            Console.WriteLine(simplex.SimplexInfo(10));
            simplex.PrintSimplex(10, true, 10);
            result = simplex.Points[0];
            Console.WriteLine("\nРезультат: ");
            result.PrintPointWithValue("X*", 10);
        }
    }
    public class Mod2NelderMeade : ANelderMeade
    {
        (double left, double rigrt) alphaBorder;
        (double left, double rigrt) bettaBorder;
        (double left, double rigrt) gammaBorder;
        public Mod2NelderMeade(NMmethod method, Simplex simplex, (double, double) alphaBorder, (double, double) bettaBorder, (double, double) gammaBorder, double eps = 0.01) : base(method, simplex, eps) 
        { 
            this.alphaBorder = alphaBorder;
            this.bettaBorder = bettaBorder;
            this.gammaBorder = gammaBorder;
        }

        public override Point FindExtremum(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                method.Alpha = alphaBorder.left + Random.Shared.NextDouble() * (alphaBorder.rigrt - alphaBorder.left);
                method.Beta = bettaBorder.left + Random.Shared.NextDouble() * (bettaBorder.rigrt - bettaBorder.left);
                method.Gamma = gammaBorder.left + Random.Shared.NextDouble() * (gammaBorder.rigrt - gammaBorder.left);

                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);
                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            return simplex.Points[0];
        }

        public override void Start(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                method.Alpha = alphaBorder.left + Random.Shared.NextDouble() * (alphaBorder.rigrt - alphaBorder.left);
                method.Beta = bettaBorder.left + Random.Shared.NextDouble() * (bettaBorder.rigrt - bettaBorder.left);
                method.Gamma = gammaBorder.left + Random.Shared.NextDouble() * (gammaBorder.rigrt - gammaBorder.left);
                Console.WriteLine($"\nIteration {iteration}");
                Console.WriteLine(simplex.SimplexInfo(10));
                simplex.PrintSimplex(10, true, 10);
                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                reflect.PrintPointWithValue("reflectPoint", 10);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);

                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                    {
                        Point rc = reflect - center;
                        Point stretch1 = stretch + rc;
                        stretch1.CalculateValue(function);
                        while (stretch1.Value < stretch.Value)
                        {
                            stretch = stretch1;
                            stretch1 += rc;
                            stretch1.CalculateValue(function);
                        }
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    }
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            Console.WriteLine("Итоговый симплекс");
            Console.WriteLine(simplex.SimplexInfo(10));
            simplex.PrintSimplex(10, true, 10);
            result = simplex.Points[0];
        }
    }
    public class Mod3NelderMeade : ANelderMeade //Адаптация на основе улучшения функции
    {
        (double left, double rigrt) alphaBorder;
        (double left, double rigrt) bettaBorder;
        (double left, double rigrt) gammaBorder;
        public Mod3NelderMeade(NMmethod method, Simplex simplex, (double, double) alphaBorder, (double, double) bettaBorder, (double, double) gammaBorder, double eps = 0.01) : base(method, simplex, eps)
        {
            this.alphaBorder = alphaBorder;
            this.bettaBorder = bettaBorder;
            this.gammaBorder = gammaBorder;
        }

        public override Point FindExtremum(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            Point bestPoint = simplex.Points[0];
            Point curBestPoint;
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);
                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                    {
                        /*Point rc = reflect - center;
                        Point stretch1 = stretch + rc;
                        stretch1.CalculateValue(function);
                        while (stretch1.Value < stretch.Value)
                        {
                            stretch = stretch1;
                            stretch1 += rc;
                            stretch1.CalculateValue(function);
                        }*/
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    }
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
                if (bestPoint.Value - simplex.Points[0].Value > 0)
                {
                    method.Alpha = Math.Min(alphaBorder.rigrt, method.Alpha * 1.3);
                    method.Beta = Math.Min(bettaBorder.rigrt, method.Beta * 1.3);
                    method.Gamma = Math.Min(gammaBorder.rigrt, method.Gamma * 1.3);
                }
                else
                {
                    method.Alpha = Math.Max(alphaBorder.left, method.Alpha * 0.7);
                    method.Beta = Math.Max(bettaBorder.left, method.Beta * 0.7);
                    method.Gamma = Math.Max(gammaBorder.left, method.Gamma * 0.7);
                }
                bestPoint = simplex.Points[0];
            }
            return simplex.Points[0];
        }

        public override void Start(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            Point bestPoint = simplex.Points[0];
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                Console.WriteLine($"\nIteration {iteration}");
                Console.WriteLine(simplex.SimplexInfo(10));
                simplex.PrintSimplex(10, true, 10);
                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                reflect.PrintPointWithValue("reflectPoint", 10);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);

                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                    {
                        /*Point rc = reflect - center;
                        Point stretch1 = stretch + rc;
                        stretch1.CalculateValue(function);
                        while (stretch1.Value < stretch.Value)
                        {
                            stretch = stretch1;
                            stretch1 += rc;
                            stretch1.CalculateValue(function);
                        }*/
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    }
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
                if (bestPoint.Value - simplex.Points[0].Value > eps)
                {
                    method.Alpha = Math.Min(alphaBorder.rigrt, method.Alpha * 1.1);
                    method.Beta = Math.Min(bettaBorder.rigrt, method.Beta * 1.1);
                    method.Gamma = Math.Min(gammaBorder.rigrt, method.Gamma * 1.1);
                }
                else
                {
                    method.Alpha = Math.Max(alphaBorder.left, method.Alpha * 0.9);
                    method.Beta = Math.Max(bettaBorder.left, method.Beta * 0.9);
                    method.Gamma = Math.Max(gammaBorder.left, method.Gamma * 0.9);
                }
            }
            Console.WriteLine("Итоговый симплекс");
            Console.WriteLine(simplex.SimplexInfo(10));
            simplex.PrintSimplex(10, true, 10);
            result = simplex.Points[0];
        }
    }
    public class Mod4NelderMeade : ANelderMeade
    {
        public Mod4NelderMeade(NMmethod method, Simplex simplex, double eps = 0.01) : base(method, simplex, eps) { }

        public override Point FindExtremum(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                Point center = method.Center(simplex.Points, function);

                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);
                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            return simplex.Points[0];
        }

        public override void Start(IFunction function)
        {
            int n = simplex.Count;
            var comparer = new ValueIncreasingComparer();
            iteration = 0;
            //WorkReport.Add((simplex.SimplexInfo(), Point.Clone(simplex.Points)));
            simplex.CalculateValues(function);
            simplex.Sort(comparer);
            while (!simplex.StopCondition(eps))
            {
                iteration++;
                Console.WriteLine($"\nIteration {iteration}");
                Console.WriteLine(simplex.SimplexInfo(10));
                simplex.PrintSimplex(10, true, 10);
                Point center = method.Center(simplex.Points, function);
                //Point center = (Point)((Hypersphere)simplex).Center.Clone();
                Point reflect = method.Reflection(simplex.Points, center, function); reflect.CalculateValue(function);
                reflect.PrintPointWithValue("reflectPoint", 10);
                Point stretch = method.Stretching(center, reflect, function); stretch.CalculateValue(function);
                Point compress = method.Compression(simplex.Points, center, reflect, function); compress.CalculateValue(function);

                if ((simplex.Points[0].Value <= reflect.Value) && (reflect.Value <= simplex.Points[n - 2].Value))
                {
                    simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (simplex.Points[0].Value > reflect.Value)
                {
                    if (stretch.Value < reflect.Value)
                    {
                        Point rc = reflect - center;
                        Point stretch1 = stretch + rc;
                        stretch1.CalculateValue(function);
                        while (stretch1.Value < stretch.Value)
                        {
                            stretch = stretch1;
                            stretch1 += rc;
                            stretch1.CalculateValue(function);
                        }
                        simplex.UpdateSimplex(Action.Stretching, stretch, center);
                    }
                    else simplex.UpdateSimplex(Action.Reflection, reflect, center);
                }
                else if (compress.Value < Math.Min(simplex.Points[n - 1].Value, reflect.Value))
                {
                    simplex.UpdateSimplex(Action.Compression, compress, center);
                }
                else simplex.UpdateSimplex(Action.GlobalCompression, center, center);
                simplex.CalculateValues(function);
                simplex.Sort(comparer);
            }
            Console.WriteLine("Итоговый симплекс");
            Console.WriteLine(simplex.SimplexInfo(10));
            simplex.PrintSimplex(10, true, 10);
            result = simplex.Points[0];
        }
    }
}
