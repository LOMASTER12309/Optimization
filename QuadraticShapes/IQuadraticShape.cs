using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MyMath;

namespace QuadraticShapes
{
    //Квадратичная форма
    public class QuadraticShape : IFunction
    {
        protected ISquareMatrix matrix;
        protected Point vector;
        protected double freeTerm;
        protected Point extremum;
        protected double minValue;
        protected int n;
        public int Dimension { get => n; }
        public ISquareMatrix GetMatrix() { return matrix; }
        public Point GetVector() { return vector; }
        public double GetFreeTerm() { return freeTerm; }
        //public Point Extremum { get { return (Point)extremum.Clone(); } }
        public Point Extremum => extremum;
        public double MinValue { 
            get { return minValue; }
            set 
            { 
                var delta = value - minValue;
                minValue = value;
                freeTerm += delta;
            }
        }
        public QuadraticShape() { }
        public QuadraticShape(ISquareMatrix matrix)
        {
            n = matrix.Dimension;
            this.matrix = matrix;
            vector = new Point(n);
            freeTerm = 0;
            extremum = new Point(n);
            minValue = 0;
        }
        public QuadraticShape(ISquareMatrix matrix, Point vector, double freeTerm, bool CalcExtremum = false)
        {
            if (matrix.Dimension != vector.Dimension)
                throw new Exception("Несовместимые размерности матрицы и вектора");
            this.matrix = matrix;
            this.vector = vector;
            this.freeTerm = freeTerm;
            n = matrix.Dimension;
            if (CalcExtremum) CalculateExtremum();
        }
        public static QuadraticShape RandomPosDefQuadraticShape(int n, double DensityOfZeros = 0, int ModulusRestriction = 9, int ModulusRestrictionForVector = 9, int ModResForFreeTerm = 9)
        {
            Random rnd = Random.Shared;
            Point randomVector = new Point(n);
            for (int i = 0; i < n; i++)
                randomVector[i] = rnd.Next((int)-ModulusRestrictionForVector, (int)ModulusRestrictionForVector + 1);
            int randomFreeTerm = rnd.Next((int)-ModResForFreeTerm, (int)ModResForFreeTerm + 1);
            return new QuadraticShape(SquareIntMatrix.RandomPosDefMatrix(n, ModulusRestriction, DensityOfZeros), randomVector, randomFreeTerm, true);
        }
        public static QuadraticShape RandomQSbyExtremum(int n, Point extremum, double RestrictionLyambda = 9, int minValue = 0, double matrixMultiplier = 1)
        {
            Random rnd = Random.Shared;
            //ISquareMatrix matrix = SquareDoubleMatrix.RandomPosDefMatrixU(n);
            ISquareMatrix matrix = SquareDoubleMatrix.RandomNormalMatrix(n);
            var tr_matrix = matrix.Transpose();

            double[] lyamda = new double[n];
            for (int i = 0; i < n; i++)
            {
                lyamda[i] = 0.01 + RestrictionLyambda * (1 - rnd.NextDouble());
                //Console.Write($"{lyamda[i]} ");//консоль
                matrix.ColumnMult(i, lyamda[i]);
            }
            //Console.WriteLine();//консоль
            matrix = matrix.Multiply(tr_matrix);
            matrix.Multiply(matrixMultiplier);
            int randomFreeTerm = minValue;
            Point vectorB = -2 * (matrix.Multiply(extremum));
            var qs = new QuadraticShape(matrix, vectorB, 0, true);
            qs.MinValue = randomFreeTerm;
            return qs;
        }

        public virtual string toString(int accuracy = 0)
        {
            string toString = "";
            for (int i = 0; i < n; i++)
            {
                //double t = matrix[i, i];
                double t = Math.Round(matrix[i, i], accuracy);
                if (Math.Abs(t) != 1)
                {
                    if (t > 0)
                        toString += $"+ {t.ToString($"F{accuracy}")}*x{i + 1}^2 ";
                    if (t < 0)
                        toString += $"- {Math.Abs(t).ToString($"F{accuracy}")}*x{i + 1}^2 ";
                }
                else
                {
                    if (t > 0)
                        toString += $"+ x{i + 1}^2 ";
                    if (t < 0)
                        toString += $"- x{i + 1}^2 ";
                }
            }
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                {
                    double t = 2 * Math.Round(matrix[i, j], accuracy);
                    if (Math.Abs(t) != 1)
                    {
                        if (t > 0)
                            toString += $"+ {t.ToString($"F{accuracy}")}*x{i + 1}*x{j + 1} ";
                        if (t < 0)
                            toString += $"- {Math.Abs(t).ToString($"F{accuracy}")}*x{i + 1}*x{j + 1} ";
                    }
                    else
                    {
                        if (matrix[i, j] > 0)
                            toString += $"+ x{i + 1}*x{j + 1} ";
                        if (matrix[i, j] < 0)
                            toString += $"- x{i + 1}*x{j + 1} ";
                    }
                }
            for (int i = 0; i < n; i++)
            {
                if (Math.Abs(vector[i]) != 1)
                {
                    if (vector[i] > 0)
                        toString += $"+ {Math.Round(vector[i], accuracy).ToString($"F{accuracy}")}*x{i + 1} ";
                    if (vector[i] < 0)
                        toString += $"- {Math.Abs(Math.Round(vector[i], accuracy)).ToString($"F{accuracy}")}*x{i + 1} ";
                }
                else
                {
                    if (vector[i] > 0)
                        toString += $"+ x{i + 1} ";
                    if (vector[i] < 0)
                        toString += $"- x{i + 1} ";
                }
            }
            if (freeTerm > 0)
                toString += $"+ {freeTerm} ";
            if (freeTerm < 0)
                toString += $"- {Math.Abs(freeTerm)}";
            if (toString[0] == '+') toString = toString.Remove(0, 2);
            return toString;
        }

        public void PrintMatrix()
        {
            Console.WriteLine(matrix.toString());
        }
        public void PrintVector()
        {
            Console.Write("(");
            for (int i = 0; i < n - 1; i++)
                Console.Write($"{vector[i]}, ");
            Console.Write($"{vector[n - 1]})'");
        }
        public double CalculateValue(Point vector)
        {
            if (vector.Dimension != n)
                throw new Exception("Некорректный размер вектора");
            double value = 0;
            for (int i = 0; i < n; i++)
                value += matrix[i, i] * vector[i] * vector[i];
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    value += 2 * matrix[i, j] * vector[i] * vector[j];
            for (int i = 0; i < n; i++)
                value += this.vector[i] * vector[i];
            value += freeTerm;
            return value;
        }
        public virtual Point CalculateExtremum()
        {
            //X* = 0.5*A^(-1)*(-B)

            var inverse = this.matrix.Inverse();
            Point X = this.matrix.Inverse().Multiply(vector.Negation());
            for (int i = 0; i < X.Dimension; i++) X[i] /= 2;
            extremum = X;
            minValue = extremum.CalculateValue(this);
            return Extremum;
        }
        public void PrintExtremum(int precision = 3, string name = "", bool withValue = true)
        {
            if (name != "")
            {
                Console.Write(name + " = ");
                extremum.PrintPoint(precision);
                if (withValue) Console.WriteLine($"\nf({name}) = {MinValue}");
            }
            else
            {
                Console.Write("X* = ");
                extremum.PrintPoint(precision);
                if (withValue) Console.WriteLine($"\n f(X*) = {MinValue}");
            }
            if (withValue)
                extremum.PrintPointWithValue(name, precision);
            else extremum.PrintPoint(precision, name);
        }
        public QuadraticShape Clone()
        {
            return new QuadraticShape((ISquareMatrix)matrix.Clone(), (Point)vector.Clone(), freeTerm);
        }

        public virtual double Calculate(Point point)
        {
            return CalculateValue(point);
        }
    }
    //Составная квадратичная форма Q(X)=max(Q1(X), Q2(X), ...)
    public class СompositeQuadraticShape : IFunction
    {
        List<QuadraticShape> QuadraticShapes;
        List<QuadraticShape> QSwithMinimum;
        int n;
        int count;
        Point minimum;
        double minValue;

        public int Dimension { get { return n; } }
        public int Count { get { return count; } }
        public Point GlobalMinimum => minimum;
        public double MinValue { get { return minValue; } }

        public СompositeQuadraticShape(List<QuadraticShape> QuadraticShapes)
        {
            this.QuadraticShapes = new List<QuadraticShape>(QuadraticShapes);
            n = QuadraticShapes[0].Dimension;
            count = QuadraticShapes.Count;
            this.QSwithMinimum = new List<QuadraticShape>(count);
        }

        public void CalculateExtremes()
        {
            foreach (var qs in this.QuadraticShapes)
            {
                Point point = qs.CalculateExtremum();
                if (this.CalculateValue(point) == qs.MinValue)
                    QSwithMinimum.Add(qs);
            }
        }
        public Point CalculateGlobalExtremum()
        {
            if (Count == 0) throw new Exception("Нет функций");
            CalculateExtremes();
            double minValue = QuadraticShapes[0].MinValue;
            int index = 0;
            for (int i = 1; i < count; i++)
            {
                if (QuadraticShapes[i].MinValue < minValue)
                {
                    minValue = QuadraticShapes[i].MinValue;
                    index = i;
                }
            }
            this.minValue = minValue;
            this.minimum = QuadraticShapes[index].Extremum;
            return GlobalMinimum;
        }
        public List<QuadraticShape> GetQuadraticShapes()
        {
            List<QuadraticShape> quadraticShapes = new List<QuadraticShape>(count);
            foreach (var quadraticShape in this.QuadraticShapes)
                quadraticShapes.Add(quadraticShape);
            return quadraticShapes;
        }
        public double CalculateValue(Point point)
        {
            double result = QuadraticShapes[0].CalculateValue(point);
            double curResult = 0;
            foreach (var quadraticShape in QuadraticShapes)
            {
                curResult = quadraticShape.CalculateValue(point);
                if (curResult < result)
                    result = curResult;
            }
            return result;
        }
        public double Calculate(Point point)
        {
            return CalculateValue(point);
        }
        public void PrintLocalMinima(int precision = 3, bool withValue = true)
        {
            int i = 1;
            foreach (var qs in QSwithMinimum)
            {
                qs.PrintExtremum(precision, $"X{i}", withValue);
                i++;
            }
        }
        public void PrintGlobalExtremum(int precision = 3, bool withValue = true)
        {
            if (withValue)
                minimum.PrintPointWithValue("X*", precision);
            else
                minimum.PrintPoint(precision, "X*");
            Console.WriteLine();
        }
        public string toString(int accuracy = 0)
        {
            string toString = "";
            int count = 1;
            foreach (var quadraticShape in QuadraticShapes)
            {
                toString += $"{count}) " + quadraticShape.toString(accuracy) + "\n";
                count++;
            }
            return toString;
        }
    }
}