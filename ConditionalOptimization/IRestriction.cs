using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyMath;

namespace ConditionalOptimization
{
    //Тип ограничения (равенство, неравенство)
    public enum TypeRestriction
    {
        Equality,
        Inequality
    }
    //Интерфейс ограничения как функции g(X)
    public interface IRestriction : IFunction
    {
        public TypeRestriction TypeRestriction { get; }
        public bool IsFulfilled(Point point);
    }
    //Ограничение как функция g(X) <= 0
    public class NonStrictRestriction : IRestriction
    {
        IFunction function;
        TypeRestriction typeRestriction;
        public TypeRestriction TypeRestriction => typeRestriction;
        public NonStrictRestriction(TypeRestriction tr, IFunction function)
        {
            this.function = function;
            this.typeRestriction = tr;
        }
        public int Dimension => function.Dimension;

        public double Calculate(Point point)
        {
            return function.Calculate(point);
        }

        public bool IsFulfilled(Point point)
        {
            if (function.Calculate(point) <= 0) return true;
            return false;
        }
    }
    //Линейное ограничение g(X) = <C,X> - betta <= 0
    public class LinearRestriction : IRestriction
    {
        TypeRestriction typeRestriction;
        public TypeRestriction TypeRestriction => typeRestriction;
        int dimension;
        Point vectorC;
        double betta;
        public Point VectorC => vectorC;
        public double Betta => betta;
        public int Dimension => dimension;
        public LinearRestriction(int dimension)
        {
            this.vectorC = new Point(dimension);
            this.betta = 0;
            this.dimension = dimension;
        }
        public LinearRestriction(TypeRestriction tr, Point vectorC, double betta = 0)
        {
            this.typeRestriction = tr;
            this.vectorC = vectorC;
            this.betta = betta;
            this.dimension = vectorC.Dimension;
        }
        public bool IsFulfilled(Point point)
        {
            if (dimension != point.Dimension)
                throw new ArgumentException("Несовпадение размерностей");
            if (Point.ScalarProduct(point, vectorC) - betta <= 0)
                return true;
            return false;
        }
        public void SetVector(Point vectorC)
        {
            if (dimension != vectorC.Dimension)
                throw new ArgumentException("Несовпадение размерностей");
            this.vectorC = vectorC;
        }
        public void SetBetta(double betta) => this.betta = betta;

        public double Calculate(Point point)
        {
            return Point.ScalarProduct(point, vectorC) - betta;
        }
    }

    //Сферическое ограничение g(X) = ||X-C||^2 - R^2 <= 0
    public class CircleRestriction : IRestriction
    {
        TypeRestriction typeRestriction;
        public TypeRestriction TypeRestriction => typeRestriction;
        int dimension;
        Point center;
        double radius2;
        public Point Center => center;
        public double Radius => radius2;
        public int Dimension => dimension;
        public CircleRestriction(TypeRestriction typeRestriction, int dimension)
        {
            this.typeRestriction = typeRestriction;
            this.center = new Point(dimension);
            this.radius2 = 1;
            this.dimension = dimension;
        }
        public CircleRestriction(TypeRestriction typeRestriction, Point center, double radius2 = 0)
        {
            this.typeRestriction = typeRestriction;
            this.center = center;
            this.radius2= radius2;
            this.dimension = center.Dimension;
        }
        public bool IsFulfilled(Point point)
        {
            if (dimension != point.Dimension)
                throw new ArgumentException("Несовпадение размерностей");
            if (this.Calculate(point) <= 0)
                return true;
            return false;
        }
        public void SetCenter(Point center)
        {
            if (dimension != center.Dimension)
                throw new ArgumentException("Несовпадение размерностей");
            this.center = center;
        }
        public void SetRadius(double radius2)
        {
            if (radius2 <= 0) throw new ArgumentException("Нельзя отрицательный радиус!");
            this.radius2 = radius2;
        }

        public double Calculate(Point point)
        {
            double res = 0;
            for (int i = 0; i < dimension; i++)
                res += Math.Pow(center[i] -  point[i], 2);
            return res - radius2;
        }
    }
}
