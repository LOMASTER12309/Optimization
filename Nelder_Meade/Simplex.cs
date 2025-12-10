using MyMath;

namespace Nelder_Meade
{
    public enum Action
    {
        Reflection,        //отражение
        Stretching,        //растяжение
        Compression,       //сжатие
        GlobalCompression, //глобальное сжатие
        Restart            //перезапуск
    }
    //Симплекс 
    public abstract class ASimplex
    {
        protected Point[] points;
        protected int dimension;
        protected int count;
        public abstract double Offset { get; }
        public Point[] Points => points;
        public int Count => count;
        public abstract void UpdateSimplex(Action action, Point newPoint, Point c, double multiplier = 1);
        public abstract void GenerateStartSimplex();
        public abstract ASimplex Clone();
        public abstract void RestartSimplex(Point startPoint, double l);
        public abstract void RestartSimplex(Point startPoint, int mod = 1);
        //public abstract ASimplex Clone();
        public void PrintSimplex(int precision, bool withValues = false, int precForValues = 2)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Console.Write($"X{i + 1} = {points[i].toString(precision)}");
                if (withValues) Console.Write($", F(X{i + 1}) = {Math.Round(points[i].Value,precForValues)}");
                Console.WriteLine();
            }
        }
        public void Sort(IComparer<Point> comp)
        {
            Array.Sort( points, comp );
        }
        public void CalculateValues(IFunction function)
        {
            foreach (Point point in points)
                point.CalculateValue(function);
        }
        public abstract bool StopCondition(double eps);
        public abstract string SimplexInfo(int precision);
    }
    //Симплекс - сфера
    public class Hypersphere : ASimplex
    {
        double radius;
        Point center;
        Point[] defaultHypersphere; //гиперсфера радиуса 1 в центре координат
        Action ?lastAction = null;
        public Hypersphere(double radius, Point center)
        {
            this.radius = radius;
            this.center = center;
            this.dimension = center.Dimension;
            this.count = (int)Math.Pow(3, dimension) - 1;
            CreateDefaultHypersphere();
            GenerateStartSimplex();
        }
        public Point Center => center;
        public override ASimplex Clone()
        {
            var result = new Hypersphere(radius, (Point)center.Clone());
            result.GenerateStartSimplex();
            return result;
        }
        public double Radius => radius;

        public override double Offset => radius;

        public override void UpdateSimplex(Action action, Point newPoint, Point c, double multiplier = 1)
        {
            Console.WriteLine();
            switch (action)
            {
                case Action.Reflection:
                    //GenerateSimplex((newPoint+c)/2, Point.Distance(c, newPoint)/2);
                    //GenerateSimplex(c, Point.Distance(c, newPoint));
                    GenerateSimplex(newPoint, radius*multiplier);
                    newPoint.PrintPointWithValue("refPoint");
                    break;
                case Action.Stretching:
                    GenerateSimplex(newPoint, this.radius*multiplier); //увеличить радиус
                    newPoint.PrintPointWithValue("stretchPoint");
                    break;
                case Action.Compression:
                    //GenerateSimplex((newPoint + c) / 2, Point.Distance(c, newPoint)/2);
                    //GenerateSimplex(newPoint, Point.Distance(c, newPoint));
                    GenerateSimplex(newPoint, this.radius*multiplier);
                    newPoint.PrintPointWithValue("compressPoint");
                    break;
                case Action.GlobalCompression:
                    GenerateSimplex(center, this.radius / 2);
                    break;
            }
            lastAction = action;
        }
        public override void GenerateStartSimplex()
        {
            points = new Point[count];
            for (int i = 0; i < count; i++)
                points[i] = center + radius * defaultHypersphere[i];
        }
        public void GenerateSimplex(Point center, double radius)
        {
            this.center = center;
            this.radius = radius;
            GenerateStartSimplex();
        }
        void CreateDefaultHypersphere()
        {
            List<Point> points = new List<Point>((int)Math.Pow(3, dimension) - 1);
            List<int> currentCombination = new List<int>(dimension);
            for (int i = 1; i <= dimension; i++)
            {
                currentCombination.Clear();
                GenerateCombinations(dimension, i, 0, currentCombination, points);
            }
            defaultHypersphere = points.ToArray();
        }
        void GenerateCombinations(int size, int k, int startIndex, List<int> currentCombination, List<Point> points)
        {
            if (currentCombination.Count == k)
            {
                List<int> minusCombinations = new List<int>(k);
                for (int j = 0; j <= k; j++)
                {
                    minusCombinations.Clear();
                    GenerateMinus(k, j, 0, minusCombinations, currentCombination, points, size);
                }
                return;
            }
            for (int i = startIndex; i < size; i++)
            {
                currentCombination.Add(i);
                GenerateCombinations(size, k, i + 1, currentCombination, points);
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }
        void GenerateMinus(int size, int k, int startIndex, List<int> minusCombinations, List<int> currentCombination, List<Point> points, int dimension)
        {
            if (minusCombinations.Count == k)
            {
                Point newPoint = new Point(dimension);
                for (int i = 0; i < size; i++)
                    newPoint[currentCombination[i]] = (double)Math.Sqrt(size) / (double)size;
                for (int i = 0; i < minusCombinations.Count; i++)
                    newPoint[currentCombination[minusCombinations[i]]] *= -1;
                points.Add(newPoint);
                return;
            }
            for (int i = startIndex; i < size; i++)
            {
                minusCombinations.Add(i);
                GenerateMinus(size, k, i + 1, minusCombinations, currentCombination, points, dimension);
                minusCombinations.RemoveAt(minusCombinations.Count - 1);
            }
        }
        public string CurrentInformation(int precision)
        {
            string result = "";
            if (precision > 0)
            {
                result += $"R = {Math.Round(radius, precision)}\n";
                result += $"O = {center.toString(precision)}\n";
                result += $"F(O) = {center.Value}\n";
            }
            else
            {
                result += $"R = {radius}\n";
                result += $"O = {center.toString(10)}\n";
                result += $"F(O) = {center.Value}\n";
            }
            return result;
        }
        /*public override ASimplex Clone()
        {
            return new Hypersphere(Point.Clone(points), (Point)center.Clone(), radius);
        }*/
        public override string SimplexInfo(int precision = -1)
        {
            string result = CurrentInformation(precision);
            result += $"Последняя операция: ";
            switch (lastAction)
            {
                case Action.Reflection:
                    result += "Отражение";
                    break;
                case Action.Stretching:
                    result += "Растяжение";
                    break;
                case Action.Compression:
                    result += "Сжатие";
                    break;
                case Action.GlobalCompression:
                    result += "Глобальное сжатие";
                    break;
                case null:
                    result += "Отсутствует";
                    break;
            }
            result += "\n";
            return result;
        }

        public override bool StopCondition(double eps)
        {
            if (radius < eps) return true;
            return false;
        }

        public override void RestartSimplex(Point startPoint, double l)
        {
            throw new NotImplementedException();
        }

        public override void RestartSimplex(Point startPoint, int mod = 1)
        {
            throw new NotImplementedException();
        }
    }
    //Обычный симплекс
    public class Simplex : ASimplex
    {
        Point startPoint;
        double offset;
        Action? lastAction = null;

        public override double Offset => offset;

        public Simplex(Point startPoint, double offset)
        {
            this.startPoint = startPoint;
            this.offset = offset;

            this.dimension = startPoint.Dimension;
            this.count = dimension+1;
            
            GenerateStartSimplex();
        }
        public Simplex(List<Point> points, double offset)
        {
            this.dimension = points.Count-1;
            this.count = dimension + 1;
            this.points = points.ToArray();
            this.offset = offset;
        }
        public override void GenerateStartSimplex()
        {
            points = new Point[count];
            points[0] = (Point)startPoint.Clone();
            for (int i = 1; i < count; i++) 
            {
                points[i] = (Point)startPoint.Clone();
                points[i][i - 1] += offset;
            }
        }
        public override ASimplex Clone()
        {
            return new Simplex(Point.Clone(points).ToList<Point>(), Offset);
        }
        public override string SimplexInfo(int precision = -1)
        {
            string result = $"Последняя операция: ";
            switch (lastAction)
            {
                case Action.Reflection:
                    result += "Отражение";
                    break;
                case Action.Stretching:
                    result += "Растяжение";
                    break;
                case Action.Compression:
                    result += "Сжатие";
                    break;
                case Action.GlobalCompression:
                    result += "Глобальное сжатие";
                    break;
                case Action.Restart:
                    result += "Перезапуск";
                    break;
                case null:
                    result += "Отсутствует";
                    break;
            }
            result += "\n";
            return result;
        }

        public override bool StopCondition(double eps)
        {
            double sum = 0;
            for (int i = 1; i < count; i++)
                sum += Math.Pow(points[i].Value - points[0].Value, 2);
            sum = Math.Sqrt(sum / dimension);
            //Console.WriteLine(sum);
            if (sum < eps)
                return true;
            return false;
        }
        public override void UpdateSimplex(Action action, Point newPoint, Point c, double multiplier = 1)
        {
            switch (action)
            {
                case Action.Reflection:
                    points[count - 1] = newPoint;
                    break;
                case Action.Stretching:
                    points[count - 1] = newPoint;
                    break;
                case Action.Compression:
                    points[count - 1] = newPoint;
                    break;
                case Action.GlobalCompression:
                    for (int i = 1; i < count; i++)
                        points[i] = points[i] + (points[0] - points[i])/2;
                    break;
                case Action.Restart:
                    RestartSimplex(newPoint, multiplier);
                    break;
            }
            lastAction = action;
        }

        public override void RestartSimplex(Point startPoint, double l)
        {
            points[0] = startPoint;
            for (int i = 1; i < count; i++)
            {
                points[i] = (Point)startPoint.Clone();
                points[i][i - 1] += l;
            }
        }

        public override void RestartSimplex(Point startPoint, int mod = 1)
        {
            double maxLength = 0;
            double curLength = 0;
            if (mod == 1)
            {
                for (int i = 0; i < count; i++)
                    for (int j = i + 1; j < count; j++)
                    {
                        curLength = Point.Distance(points[i], points[j]);
                        if (curLength > maxLength) maxLength = curLength;
                    }
                RestartSimplex(startPoint,  2* maxLength);
            }
            else if (mod == 2)
            {
                for (int i = 1; i < count; i++)
                    for (int j = 0; j < dimension; j++)
                        points[i][j] += (points[i][j] - points[0][j]);
            }
        }
    }
}
