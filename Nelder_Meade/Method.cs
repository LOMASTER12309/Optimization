using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using MyMath;

namespace Nelder_Meade
{
    //Метод Нелдера-Мида 
    public interface NMmethod
    {
        public double Alpha { get; set; }
        public double Beta { get; set; }
        public double Gamma { get; set; }
        public NMmethod Clone();
        public Point Center(Point[] points, IFunction function);
        public Point Reflection(Point[] simplex, Point center, IFunction function);
        public Point Stretching(Point center, Point refPoint, IFunction function);
        public Point Compression(Point[] simplex, Point center, Point refPoint, IFunction function);

    }
    public class OrigNMmethod: NMmethod
    {
        protected double alpha = 1;
        protected double betta = 2;
        protected double gamma = 0.5;
        public double Alpha //коэффициент отражения
        {
            get => alpha;
            set => alpha = value;
        }
        public double Beta  //коэффициент растяжения
        {
            get => betta;
            set => betta = value;
        }
        public double Gamma //коэффициент сжатия
        {
            get => gamma;
            set => gamma = value;
        }
        public OrigNMmethod(double alpha = 1, double betta = 2, double gamma = 0.5)
        {
            this.alpha = alpha;
            this.betta = betta;
            this.gamma = gamma;
        }
        public virtual NMmethod Clone()
        {
            return new OrigNMmethod(Alpha, Beta, Gamma);
        }
        public virtual Point Center(Point[] points, IFunction function)
        {
            Point result = new Point(points[0].Dimension);
            for (int i = 0; i < points.Length - 1; i++)
                result += points[i];
            result /= (points.Length - 1);
            result.CalculateValue(function);
            return result;
        }
        public Point Reflection(Point[] simplex, Point center, IFunction function)
        {
            int n = simplex.Length-1;
            Point result = new Point(simplex[0].Dimension);
            result = center + alpha * (center - simplex[n]);
            result.CalculateValue(function);
            return result;
        }
        public Point Stretching(Point center, Point refPoint, IFunction function)
        {
            int n = center.Dimension;
            Point result = new Point(n);
            result = center + betta * (refPoint - center);
            result.CalculateValue(function);
            return result;
        }
        public Point Compression(Point[] simplex, Point center, Point refPoint, IFunction function)
        {
            int n = simplex[0].Dimension;
            Point result = new Point(n);
            if (simplex[n].Value <= refPoint.Value)
                result = center + gamma * (simplex[n] - center);
            else
                result = center + gamma * (refPoint - center);
            result.CalculateValue(function);
            return result;
        }
        public Point Compression1(Point Xn, Point center, Point refPoint, IFunction function)
        {
            int n = Xn.Dimension;
            Point result = new Point(n);
            if (Xn.Value <= refPoint.Value)
                result = center + gamma * (Xn - center);
            else
                result = center + gamma * (refPoint - center);
            result.CalculateValue(function);
            return result;
        }
    }
    public class ModNMmethod: OrigNMmethod, NMmethod
    {
        double[] h = null;
        double sumH = 0;
        bool initialised = false;
        public ModNMmethod(double alpha = 1, double betta = 2, double gamma = 0.5) : base(alpha, betta, gamma) { }
        public override Point Center(Point[] points, IFunction function)
        {
            int n = points.Length;
            if (h == null) 
                h= new double[points.Length-1];
            sumH = 0;
            for (int i = 0;  i < n-1; i++)
            {
                h[i] = (points[n - 1].Value - points[i].Value) / (Point.Distance(points[n - 1], points[i]));
                sumH += h[i];
            }
            for (int i = 0; i < n - 1; i++)
                h[i] /= sumH;
            Point result = new Point(points[0].Dimension);
            for (int i = 0; i < n - 1; i++)
                for (int j = 0; j < points[0].Dimension; j++)
                    result[j] += h[i] * points[i][j];
            return result;
        }
        public override NMmethod Clone()
        {
            return new ModNMmethod(Alpha, Beta, Gamma);
        }
    }
}
