using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyMath;

namespace ConditionalOptimization
{
    //Интерфейс функции штрафа
    public interface FineFunction : IFunction
    {
        public List<IRestriction> Rests { get; }
        public static bool IsCorrectRests(List<IRestriction> rests)
        {
            int dimension = rests[0].Dimension;
            foreach (IRestriction restriction in rests)
                if (restriction.Dimension != dimension)
                    return false;
            return true;
        }
    }

    //Спецальный функции штрафа 
    public class MaxFine : FineFunction
    {
        double P = 1;
        double D = 1;
        int dimension;
        List<IRestriction> rests;
        public int Dimension => dimension;
        public List<IRestriction> Rests => rests;
        public MaxFine(List<IRestriction> rests, double P = 1, double D = 1)
        {
            if (!FineFunction.IsCorrectRests(rests))
                throw new ArgumentException("Несовпадение размерностей");
            this.dimension = rests[0].Dimension;
            this.rests = rests;
            this.P = P;
            this.D = D;
        }
        public void SetP(double P)
        {
            if (P <= 0) throw new ArgumentException("Неположительные значения недопустимы");
            this.P = P;
        }
        public void SetD(double D)
        {
            if (D <= 0) throw new ArgumentException("Неположительные значения недопустимы");
            this.D = D;
        }
        public double Calculate(Point point)
        {
            double result = 0;
            foreach (IRestriction restriction in rests)
                if (restriction.TypeRestriction == TypeRestriction.Inequality)
                    result += Math.Pow(Math.Max(0, restriction.Calculate(point)), P);
                else 
                    result += Math.Pow(Math.Abs(restriction.Calculate(point)), D);
            return result;
        }
    }

    //Функция с штрафом
    public class FunctionWithFine : IFunction
    {
        FineFunction fineFunction;
        IFunction function;
        double betta;
        public FineFunction FineFunction { get; set; }
        public IFunction Function { get; set; }
        public double Betta => betta;

        public int Dimension => function.Dimension;

        public FunctionWithFine(IFunction function, FineFunction fineFunction, double betta = 1)
        {
            if (function.Dimension != fineFunction.Dimension)
                throw new ArgumentException("Несовпадение размерностей");
            this.function = function;
            this.fineFunction = fineFunction;
            this.betta = betta;
        }
        public void SetBetta(double betta)
        {
            if (betta <= 0) throw new ArgumentException("Параметр может принимать только пеоложительные значния");
            this.betta = betta;
        }
        public double Calculate(Point point)
        {
            return function.Calculate(point) + fineFunction.Calculate(point) * betta;
        }
    }
}
