using MyMath;
using Nelder_Meade;

namespace ConditionalOptimization
{
    //Метод штрафов (локальный решатель - метод Нелдера-Мида)
    public class PenaltyMethod
    {
        IConditionalOptimization problem;
        FunctionWithFine function;
        FineFunction fineFunction;
        ANelderMeade method;
        NMmethod nMmethod;
        double eps;
        double u0;
        double betta;
        Point result;
        int mod;
        double l;
        int iterationsNM = 0;
        int iterations = 0;
        public int Iterations => iterations;
        public int IterationsNM => iterationsNM;
        public double L => l;
        public Point Result => result;
        public PenaltyMethod(IConditionalOptimization problem, FineFunction fineFunction, ANelderMeade method, double eps, double u0, double betta, int mod = 0)
        {
            this.problem = problem;
            this.fineFunction = fineFunction;
            this.method = method;
            this.l = method.getOffset();
            this.nMmethod = method.GetMethod();
            this.eps = eps;
            this.u0 = u0;
            this.betta = betta;
            this.function = new FunctionWithFine(problem.Function, fineFunction, u0);
            this.mod = mod;
        }
        public void SetMethod(ANelderMeade nm)
        {
            this.method = nm;
            this.l = method.getOffset();
            this.nMmethod = method.GetMethod();
        }
        public void Start()
        {
            double u = u0;
            Point result = null;
            iterations = 1;
            function.SetBetta(u0);
            iterationsNM = 0;
            double fine = 0;
            do
            {
                Console.WriteLine($"Iteration {iterations}:");
                //находим промежуточное решение
                result = method.FindExtremum(function);
                iterationsNM += method.Iteration;
                result.CalculateValue(problem.Function);
                result.PrintPointWithValue("X*", 10);
                Console.WriteLine($"u(i) = {u}");
                //проверка штрафа
                fine = fineFunction.Calculate(result);
                Console.WriteLine($"a(X*) = {fine}");
                Console.WriteLine($"u(i)a(X*) = {fine*u}");
                Console.WriteLine($"iterationsNM = {method.Iteration}");
                if (u*fine < eps)
                    break;
                else
                {
                    u = betta * u;
                    function.SetBetta(u);
                    iterations++;
                }
                
                Console.WriteLine();
                //method.RestartSimplex(l);
                if (mod > 0)
                    method.RestartSimplex(mod);
                method.SetMethod(nMmethod);
            }
            while(true);
            result.CalculateValue(problem.Function);
            Console.WriteLine();
            Console.WriteLine("Итоговое решение: ");
            result.PrintPointWithValue("X**", 10);
            Console.WriteLine($"Кол-во NM = {iterationsNM}");
            this.result = result;
        }
        public Point FindExtremum()
        {
            double u = u0;
            Point result = null;
            iterations = 1;
            iterationsNM = 0; 
            function.SetBetta(u0);
            double fine = 0;
            do
            {
                //находим промежуточное решение
                result = method.FindExtremum(function);
                iterationsNM += method.Iteration;
                result.CalculateValue(problem.Function);
                //проверка штрафа
                fine = fineFunction.Calculate(result);
                if (u * fine < eps)
                    break;
                else
                {
                    u = betta * u;
                    function.SetBetta(u);
                    iterations++;
                }
                //method.RestartSimplex(l);
                if (mod > 0)
                    method.RestartSimplex(mod);
            }
            while (true);
            result.CalculateValue(problem.Function);
            this.result = result;
            return result;
        }
    }
}
