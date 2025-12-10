using MyMath;

using Nelder_Meade;

using QuadraticShapes;
using ConditionalOptimization;
namespace Test_QuadraticShapes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            QuadraticShape qs = new QuadraticShape(new SquareIntMatrix(new int[,] { { 1, 0 }, { 0, 1 } }), new Point(2), 0);
            LinearRestriction rest = new LinearRestriction(new Point(new double[] { 1, 1 }), -1);
            QSWithLinearRestriction task = new QSWithLinearRestriction(qs, rest);
            Point result = task.CalculateExtremum();
            result.CalculateValue(qs);
            result.PrintPointWithValue(null, 10);

            NelderMeade nm = new NelderMeade(new NMmethod(1, 2, 0.5), new Simplex(new Point(new double[] { 50, 20 }), 1), 0.0000001);
            MaxFine fineFunction = new MaxFine(new List<IRestriction> { rest }, 2);
            //FunctionWithFine fwf = new FunctionWithFine(qs, fineFunction, 1);

            Console.WriteLine("----------------------------------------------------------");

            /*Point result2 = nm.FindExtremum(fwf);
            result2.CalculateValue(qs);
            result2.PrintPointWithValue(null, 10);

            fwf = new FunctionWithFine(qs, fineFunction, 200);
            result2 = nm.FindExtremum(fwf);
            result2.CalculateValue(qs);
            result2.PrintPointWithValue(null, 10);*/

            PenaltyMethod method = new PenaltyMethod(task, fineFunction, nm, 0.000001, 1, 100);
            method.Start();
            Console.WriteLine($"Ошибка: {Point.Distance(result, method.Result)}");

            /*IFunction function = new Function((Point p) => Math.Pow(1 - p[0], 2) + 100 * Math.Pow(p[1] - Math.Pow(p[0], 2), 2), 2);
            NonStrictRestriction rest = new NonStrictRestriction(new Function((Point p) => 2, 2));*/
        }
    }
}
