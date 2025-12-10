using static System.Runtime.InteropServices.JavaScript.JSType;
using MyMath;
using Nelder_Meade;
using Function = MyMath.Function;
using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using ConditionalOptimization;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
namespace Test_Nelder_Meade
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            int dim = 3;
            var eq = TypeRestriction.Equality;
            var ineq = TypeRestriction.Inequality;
            IFunction f1 = new Function((Point p) => Math.Pow(p[0], 2) + Math.Pow(p[1], 2) + Math.Pow(p[2], 2), 3);

            IFunction f3 = new Function((Point p) => Math.Abs(p[0] - 1) + Math.Abs(p[1] - 2), 2);

            IFunction f2 = new Function((Point p) => Math.Pow(p[0] - 4, 2) + Math.Pow(p[1] + 2, 2) - 2, 2);
            IFunction f21 = new Function((Point p) => p[0] + Math.Pow(p[1], 2)-4, 2);
            IFunction f22 = new Function((Point p) => -p[0] - p[1]+1, 2);
            IFunction f23 = new Function((Point p) => -p[0], 2);
            IFunction f24 = new Function((Point p) => -p[1], 2);
            IRestriction r21 = new NonStrictRestriction(ineq, f21);
            IRestriction r22 = new NonStrictRestriction(ineq, f22);
            IRestriction r23 = new NonStrictRestriction(ineq, f23);
            IRestriction r24 = new NonStrictRestriction(ineq, f24);


            IRestriction r1 = new CircleRestriction(ineq, new Point(new double[] { 0, 0 }), 1);
            List<IRestriction> lr1 = new List<IRestriction> { r1 };
            IConditionalOptimization pr1 = new FunctionWithRestrictions(f3, lr1);
            FineFunction ff1 = new MaxFine(lr1, 2, 2);

            List<IRestriction> lr2 = new List<IRestriction> { r21, r22, r23, r24 };
            IConditionalOptimization pr2 = new FunctionWithRestrictions(f2, lr2);
            FineFunction ff2 = new MaxFine(lr2, 2, 2);

            IFunction f31 = new Function((Point p) => p[0] + p[1] + p[2] - 2, 3);
            IFunction f32 = new Function((Point p) => 2*p[0] - p[1] + p[2] - 1, 3);
            IRestriction r31 = new NonStrictRestriction(eq, f31);
            IRestriction r32 = new NonStrictRestriction(ineq, f32);
            List<IRestriction> lr3 = new List<IRestriction> { r31, r32 };
            IConditionalOptimization pr3 = new FunctionWithRestrictions(f1, lr3);
            FineFunction ff3 = new MaxFine(lr3, 2, 1);

            var method1 = new OrigNMmethod(1, 2, 0.5);
            Simplex s = new Simplex(new Point(new double[] {-10, 10, -10}), 1);
            NelderMeade nm = new NelderMeade(method1, s, 0.000001);
            Mod1NelderMeade nm1 = new Mod1NelderMeade(method1, s, (1, 2), (2, 3), (0.4, 0.6), 0.000001);
            Mod3NelderMeade nm3 = new Mod3NelderMeade(method1, s, (1, 2), (2, 3), (0.4, 0.6), 0.000001);

            double u0 = 0.1;
            double betta = 2;
            int mod = 1;

            PenaltyMethod p1 = new PenaltyMethod(pr3, ff3, nm, 0.000001, u0, betta, mod);
            PenaltyMethod p2 = new PenaltyMethod(pr3, ff3, nm1, 0.000001, u0, betta, mod);
            PenaltyMethod p3 = new PenaltyMethod(pr3, ff3, nm3, 0.000001, u0, betta, mod);

            double[] leftBorders = new double[dim]; for (int i = 0; i < dim; i++) leftBorders[i] = -10;
            double[] rigthBorders = new double[dim]; for (int i = 0; i < dim; i++) rigthBorders[i] = 10;

            Point opt1 = new Point(new double[] {Math.Sqrt(0.5), Math.Sqrt(0.5)});
            Point opt2 = new Point(new double[] { 4, 0});
            Point opt3 = new Point(new double[] { 8d/14, 11d/14, 9d/14 }); //opt3.PrintPoint(10);
            int n = 100; double l = 1;

            int sum1 = 0; int sumNM1 = 0; double sumDist1 = 0;
            int sum2 = 0; int sumNM2 = 0; double sumDist2 = 0;
            int sum3 = 0; int sumNM3 = 0; double sumDist3 = 0;

            for (int i = 0; i < n; i++)
            {
                s = new Simplex(RandomPoint(dim, leftBorders, rigthBorders), -l + 2 * l * (1 - Random.Shared.NextDouble()));
                nm = new NelderMeade(method1, s, 0.000001);
                nm1 = new Mod1NelderMeade(method1, s, (1, 2), (2, 3), (0.4, 0.6), 0.000001);
                nm3 = new Mod3NelderMeade(method1, s, (1, 2), (2, 3), (0.4, 0.6), 0.000001);

                p1.SetMethod(nm);
                p2.SetMethod(nm1);
                p3.SetMethod(nm3);

                var res1 = p1.FindExtremum();
                var res2 = p2.FindExtremum();
                var res3 = p3.FindExtremum();

                sum1 += p1.Iterations;
                sum2 += p2.Iterations;
                sum3 += p3.Iterations;

                sumNM1 += p1.IterationsNM;
                sumNM2 += p2.IterationsNM;
                sumNM3 += p3.IterationsNM;

                sumDist1 += Point.Distance(opt3, res1);
                sumDist2 += Point.Distance(opt3, res2);
                sumDist3 += Point.Distance(opt3, res3);
            }

            Console.WriteLine($"k1 = {((double)sum1) / n}");
            Console.WriteLine($"k2 = {((double)sum2) / n}");
            Console.WriteLine($"k3 = {((double)sum3) / n}");
            Console.WriteLine();
            Console.WriteLine($"i1 = {((double)sumNM1) / n}");
            Console.WriteLine($"i2 = {((double)sumNM2) / n}");
            Console.WriteLine($"i3 = {((double)sumNM3) / n}");
            Console.WriteLine();
            Console.WriteLine($"d1 = {Math.Round(sumDist1 / n, 10)}");
            Console.WriteLine($"d2 = {Math.Round(sumDist2 / n, 10)}");
            Console.WriteLine($"d3 = {Math.Round(sumDist3 / n, 10)}");
            //Console.WriteLine(r2.Calculate(new Point(new double[] { 0, 0 })));
        }
        static Point RandomPoint(int n, double[] leftBorders, double[] rigthBorders)
        {
            Point result = new Point(n);
            for (int i = 0; i < n; i++)
                result[i] = leftBorders[i] + Random.Shared.NextDouble() * (rigthBorders[i] - leftBorders[i]);
            return result;
        }
    }

}