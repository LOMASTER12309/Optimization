using static System.Runtime.InteropServices.JavaScript.JSType;
using MyMath;
using Nelder_Meade;
using Function = MyMath.Function;
using ScottPlot;
using System;
using ScottPlot.Colormaps;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using ScottPlot.Interactivity.UserActionResponses;
namespace Test_Nelder_Meade
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            
            int dim = 2;
            IFunction function1 = new Function((Point p) => 
            {
                int n = dim;
                double result = 0;
                for (int i = 0; i < n; i++)
                    //result += Math.Pow(p[i], 2);
                result += Math.Pow(i + 1, 2) * Math.Pow(p[i], 2);
                return result;
            }, dim);
            IFunction function2 = new Function((Point p) => Math.Pow(Math.Sin(3 * Math.PI * p[0]), 2) + Math.Pow(p[0]-1, 2)*(1+ Math.Pow(Math.Sin(3 * Math.PI * p[1]), 2)) + Math.Pow(p[1] - 1, 2) * (1+Math.Pow(Math.Sin(2 * Math.PI * p[1]), 2)), 2); //ф-я Леви
            IFunction function3 = new Function((Point p) => Math.Pow(1 - p[0], 2) + 100 * Math.Pow(p[1] - Math.Pow(p[0], 2), 2), 2); //банан
            IFunction function4 = new Function((Point p) =>
            {
                double A = 10;
                int n = dim;
                double result = A*n;
                for (int i = 0; i < n; i++)
                    result += Math.Pow(p[i], 2) - A * Math.Cos(2 * Math.PI * p[i]);
                return result;
            }, dim); //ф-я Растригина многомерная
            IFunction function5 = new Function((Point p) =>
            {
                return Math.Pow(1.5 - p[0] + p[0] * p[1], 2) + Math.Pow(2.25 - p[0] + p[0] * Math.Pow(p[1], 2), 2) + Math.Pow(2.625 - p[0] + p[0] * Math.Pow(p[1], 3), 2);
            }, dim); //ф-я Била
            IFunction function6 = new Function((Point p) =>
            {
                int n = dim;
                double result = 0;
                for (int i = 0; i < n; i++)
                    result += Math.Pow(p[i], 2);
                return 1 - Math.Exp(-0.05*result);
            }, dim); //ф-я экспоненциального затухания

            IFunction function7 = new Function((Point p) =>
            {
                int n = dim;
                double result = 0;
                for (int i = 0; i < n; i++)
                    result += Math.Pow(p[i], 2);
                return Math.Log(1 + result);
            }, dim); //ф-я логарифмическая

            IFunction function8 = new Function((Point p) =>
            {
                int n = dim;
                double result = 0;
                for (int i = 0; i < n; i++)
                {
                    result += Math.Pow(i+1,2)*Math.Abs(p[i]);
                    for (int j = i + 1; j < n; j++)
                        result += Math.Sqrt(Math.Abs(p[i] * p[j]));
                }
                return Math.Log(1 + result);
            }, dim); //ф-я модуль


            var method1 = new OrigNMmethod(1, 2, 0.5);
            Simplex s = new Simplex(new Point(new double[] {13590,-10303}), 100.12345);
            NelderMeade nm1;// = new NelderMeade(method1, s, 0.00001); nm1.Start(function8);
            Mod1NelderMeade nm2;// = new Mod1NelderMeade(method1, s, (0.7, 2), (2, 3), (0.3, 0.7), 0.00001); nm2.Start(function8);
            Mod1NelderMeade nm3;// = new NelderMeade(method1, s, (0.7, 2), (2, 3), (0.3, 0.7), 0.00001);
            Mod1NelderMeade nm4;// = new NelderMeade(method1, s, (0.7, 2), (2, 3), (0.3, 0.7), 0.00001);

            Stopwatch stopwatch1 = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            Stopwatch stopwatch3 = new Stopwatch();
            Stopwatch stopwatch4 = new Stopwatch();

            int sum1 = 0; double sumTime1 = 0; double sumDist1 = 0;
            int sum2 = 0; double sumTime2 = 0; double sumDist2 = 0;
            int sum3 = 0; double sumTime3 = 0; double sumDist3 = 0;
            int sum4 = 0; double sumTime4 = 0; double sumDist4 = 0;

            double[] leftBorders = new double[dim]; for (int i = 0; i < dim; i++) leftBorders[i] = -50;
            double[] rigthBorders = new double[dim]; for (int i = 0; i < dim; i++) rigthBorders[i] = 50;

            /*double[] leftBorders = new double[] { 0, -2 };
            double[] rigthBorders = new double[] { 4, 1 };*/

            //Point opt = new Point(new double[] { 1, 1 });
            Point opt = new Point(dim);
            int n = 100;
            double l = 5;
            for (int i = 0; i < n; i++)
            {
                s = new Simplex(RandomPoint(dim, leftBorders, rigthBorders), -l + 2 * l * (1 - Random.Shared.NextDouble()));
                nm1 = new NelderMeade(method1, s, 0.00001);
                nm2 = new Mod1NelderMeade(method1, s, (0.5, 2.5), (1.5, 4), (0.2, 0.8), 0.00001);
                nm3 = new Mod1NelderMeade(method1, s, (0.8, 1.8), (2, 3), (0.4, 0.6), 0.00001);
                nm4 = new Mod1NelderMeade(method1, s, (0.9, 1.2), (1.9, 2.2), (0.4, 0.6), 0.00001);

                stopwatch1 = new Stopwatch();
                stopwatch1.Start();
                var res1 = nm1.FindExtremum(function7);
                stopwatch1.Stop();
                sumTime1 += stopwatch1.Elapsed.TotalNanoseconds;

                stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                var res2 = nm2.FindExtremum(function7);
                stopwatch2.Stop();
                sumTime2 += stopwatch2.Elapsed.TotalNanoseconds;

                stopwatch3 = new Stopwatch();
                stopwatch3.Start();
                var res3 = nm3.FindExtremum(function7);
                stopwatch3.Stop();
                sumTime3 += stopwatch3.Elapsed.TotalNanoseconds;

                stopwatch4 = new Stopwatch();
                stopwatch4.Start();
                var res4 = nm4.FindExtremum(function7);
                stopwatch4.Stop();
                sumTime4 += stopwatch4.Elapsed.TotalNanoseconds;

                sum1 += nm1.Iteration;
                sum2 += nm2.Iteration;
                sum3 += nm3.Iteration;
                sum4 += nm4.Iteration;

                sumDist1 += Point.Distance(opt, res1);
                sumDist2 += Point.Distance(opt, res2);
                sumDist3 += Point.Distance(opt, res3);
                sumDist4 += Point.Distance(opt, res4);
            }

            Console.WriteLine($"k1 = {((double)sum1) / n}");
            Console.WriteLine($"k2 = {((double)sum2) / n}");
            Console.WriteLine($"k3 = {((double)sum3) / n}");
            Console.WriteLine($"k4 = {((double)sum4) / n}");
            Console.WriteLine();
            Console.WriteLine($"t1 = {sumTime1 / n}");
            Console.WriteLine($"t2 = {sumTime2 / n}");
            Console.WriteLine($"t3 = {sumTime3 / n}");
            Console.WriteLine($"t4 = {sumTime4 / n}");
            Console.WriteLine();
            Console.WriteLine($"d1 = {Math.Round(sumDist1 / n, 10)}");
            Console.WriteLine($"d2 = {Math.Round(sumDist2 / n, 10)}");
            Console.WriteLine($"d3 = {Math.Round(sumDist3 / n, 10)}");
            Console.WriteLine($"d4 = {Math.Round(sumDist4 / n, 10)}");
        }
        static Point RandomPoint(int n, double[] leftBorders, double[] rigthBorders)
        {
            Point result = new Point(n);
            for (int i = 0;i < n; i++)
                result[i] = leftBorders[i] + Random.Shared.NextDouble()*(rigthBorders[i] - leftBorders[i]);
            return result;
        }
    }
}
