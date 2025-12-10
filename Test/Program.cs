using System.Collections;

using Optimization;
using MyMath;
using System.Net.Http.Headers;
using System.Collections.Generic;
using QuadraticShapes;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FillNames();
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            int dim = 1;
            IFunction function1 = new Function((Point p) => Math.Pow(Math.Sin(3 * Math.PI * p[0]), 2) + Math.Pow(p[0] - 1, 2) * (1 + Math.Pow(Math.Sin(3 * Math.PI * p[1]), 2)) + Math.Pow(p[1] - 1, 2) * (1 + Math.Pow(Math.Sin(2 * Math.PI * p[1]), 2)), 2); //ф-я Леви
            IFunction function2 = new Function((Point p) => Math.Pow(1 - p[0], 2) + 100 * Math.Pow(p[1] - Math.Pow(p[0], 2), 2), 2); //банан
            IFunction function3 = new Function((Point p) => 20 + Math.Pow(p[0], 2) + Math.Pow(p[1], 2) - 10 * (Math.Cos(2 * Math.PI * p[0]) + Math.Cos(2 * Math.PI * p[1])), 2); //ф-я Растригина 
            IFunction function4 = new Function((Point p) =>
            {
                double A = 10;
                int n = dim;
                double result = A * n;
                for (int i = 0; i < n; i++)
                    result += Math.Pow(p[i], 2) - A * Math.Cos(2 * Math.PI * p[i]);
                return result;
            }, dim); //ф-я Растригина многомерная
            IFunction function5 = new Function((Point p) => Math.Pow(Math.Sin(3 * Math.PI * p[0]), 2) + Math.Pow(p[0] - 1, 2) * (1 + Math.Pow(Math.Sin(3 * Math.PI * p[1]), 2)) + Math.Pow(p[1] - 1, 2) * (1 + Math.Pow(Math.Sin(2 * Math.PI * p[1]), 2)), 2); //ф-я Леви
            IFunction function6 = new Function((Point p) => 0.5 + (Math.Pow(Math.Sin(Math.Pow(p[0], 2) - Math.Pow(p[1], 2)), 2) - 0.5) /(Math.Pow(1 + 0.001 * (Math.Pow(p[0], 2) + Math.Pow(p[1], 2)), 2)), 2); //ф-я Шаффера
            IFunction function7 = new Function((Point p) => 2 * Math.Pow(p[0], 2) - 1.05 * Math.Pow(p[0], 4) + (Math.Pow(p[0], 6)/6) + p[0] * p[1] + Math.Pow(p[1], 2), 2);

            int N = 30;
            List<QuadraticShape> shapes = new List<QuadraticShape>();
            double[] leftBorders = new double[dim]; for (int i = 0; i < dim; i++) leftBorders[i] = -1000;
            double[] rigthBorders = new double[dim]; for (int i = 0; i < dim; i++) rigthBorders[i] = 1000;
            int mod = 500;
            for (int i = 0; i < N; i++)
                shapes.Add(QuadraticShape.RandomQSbyExtremum(dim, RandomPointInt(dim, leftBorders, rigthBorders), 1, -mod + 2 * Random.Shared.Next(mod), 1));
            shapes.Add(QuadraticShape.RandomQSbyExtremum(dim, RandomPointInt(dim, leftBorders, rigthBorders), 1, -500, 0.05));
            foreach (var shape in shapes)
            {
                //Console.Write($"{j}) ");
                Console.Write(ChangeNames(shape.toString(10))); Console.Write(", ");
                shape.CalculateExtremum();
                //shape.PrintExtremum();
            }
            foreach (var shape in shapes)
                shape.PrintExtremum(10, "X*", true);
            Console.WriteLine();
            СompositeQuadraticShape qs = new СompositeQuadraticShape(shapes);
            var opt = qs.CalculateGlobalExtremum();
            qs.PrintLocalMinima();
            Console.WriteLine("Глобальный экстремум:");
            opt.PrintPointWithValue("X*", 10);
            Console.WriteLine();
            //Point opt = new Point(dim); opt.CalculateValue(function1); //opt.PrintPointWithValue("X*", 10);
            /*Point opt = new Point(new double[] {1,1}); opt.CalculateValue(function1);


            int N = 25;
            List<QuadraticShape> shapes = new List<QuadraticShape>();
            double[] leftBorders = new double[dim]; for (int i = 0; i < dim; i++) leftBorders[i] = -1000;
            double[] rigthBorders = new double[dim]; for (int i = 0; i < dim; i++) rigthBorders[i] = 1000;
            int mod = 400;
            var method1 = new DifferentialEvolution(dim, leftBorders, rigthBorders, 50, 0.8, 0.8, 700, opt, 0.00001);
            var method2 = new ModDifferentialEvolution(dim, leftBorders, rigthBorders, 50, 0.8, 0.8, 700, 0.00001, opt, 0.00001);

            int n1 = 10; int n2 = 10; int n = n1 * n2;
            int sum1 = 0; double sumDist1 = 0; double sumValue1 = 0; int accuracyAchieved1 = 0;
            int sum2 = 0; double sumDist2 = 0; double sumValue2 = 0; int accuracyAchieved2 = 0;
            for (int j = 0; j < n1; j++)
            {
                var qs = GenerateFunction(dim, leftBorders, rigthBorders, N, mod);
                var optimum = qs.GlobalMinimum;
                method1.SetOptimum(optimum);
                method2.SetOptimum(optimum);
                for (int i = 0; i < n2; i++)
                {
                    var res1 = method1.FindExtremum(qs);
                    var res2 = method2.FindExtremum(qs);

                    accuracyAchieved1 += (method1.AcuuracyAchieved ? 1 : 0);
                    accuracyAchieved2 += (method2.AcuuracyAchieved ? 1 : 0);

                    sum1 += method1.RequiredNumOfPopulation * (method1.AcuuracyAchieved ? 1 : 0);
                    sum2 += method2.RequiredNumOfPopulation * (method2.AcuuracyAchieved ? 1 : 0);

                    sumDist1 += Point.Distance(res1, optimum);
                    sumDist2 += Point.Distance(res2, optimum);

                    sumValue1 += method1.AVGBestValue;
                    sumValue2 += method2.AVGBestValue;
                }
            }
            Console.WriteLine($"v1 = {sumValue1 / n}");
            Console.WriteLine($"v2 = {sumValue2 / n}");
            Console.WriteLine();
            Console.WriteLine($"a1 = {Math.Round((((double)accuracyAchieved1) / n) * 100, 2)}%");
            Console.WriteLine($"a1 = {Math.Round((((double)accuracyAchieved2) / n) * 100, 2)}%");
            Console.WriteLine();
            Console.WriteLine($"p1 = {((double)sum1) / (double)accuracyAchieved1}");
            Console.WriteLine($"p2 = {((double)sum2) / accuracyAchieved2}");
            Console.WriteLine();
            Console.WriteLine($"r1 = {sumDist1 / n}");
            Console.WriteLine($"r2 = {sumDist2 / n}");*/

            /*for (int i = 0; i < 10; i++)
                Console.WriteLine(Math.Abs(BoxMuller.Generate(1, 0.5).Item1));*/
        }

        public static СompositeQuadraticShape GenerateFunction(int dim, double[] leftBorders, double[] rigthBorders, int N, int mod)
        {
            List<QuadraticShape> shapes = new List<QuadraticShape>();
            for (int i = 0; i < N; i++)
                shapes.Add(QuadraticShape.RandomQSbyExtremum(dim, RandomPointInt(dim, leftBorders, rigthBorders), 0.1, -mod + 2 * Random.Shared.Next(mod), 0.01));
            shapes.Add(QuadraticShape.RandomQSbyExtremum(dim, RandomPointInt(dim, leftBorders, rigthBorders), 0.1, -1-mod, 0.01));
            СompositeQuadraticShape qs = new СompositeQuadraticShape(shapes);
            qs.CalculateGlobalExtremum();
            return qs;
        }
        static Point RandomPointInt(int n, double[] leftBorders, double[] rigthBorders)
        {
            Point result = new Point(n);
            for (int i = 0; i < n; i++)
                result[i] = (int)leftBorders[i] + Random.Shared.Next((int)(rigthBorders[i] - leftBorders[i]));
            return result;
        }

        static Dictionary<string, string> names = new Dictionary<string, string>();
        static string pattern = "";
        static void FillNames()
        {
            string[] newNames = { "x", "y", "z", "w" };

            // Строим словарь замен: x1 → x, x2 → y, ...
            for (int i = 0; i < newNames.Length; i++)
            {
                names[$"x{i + 1}"] = newNames[i];
            }
            // Строим паттерн для Regex: x1|x2|x3|x4
            pattern = string.Join("|", names.Keys);
        }
        static string ChangeNames(string formula)
        {
            string result = Regex.Replace(formula, pattern, match =>
            {
                return names[match.Value];
            });
            return result;
        }
    }
}
