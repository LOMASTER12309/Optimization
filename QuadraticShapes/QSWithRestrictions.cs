using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

using MyMath;
using ConditionalOptimization;
using Point = MyMath.Point;

namespace QuadraticShapes
{
    //Задача минимизации квадратичной формы при линейных ограничениях
    public class QSWithLinearRestriction : IConditionalOptimization
    {
        LinearRestriction rest;
        QuadraticShape qs;
        List<IRestriction> restrictions;
        // QSWithLinearRestriction() { }
        public LinearRestriction? Restriction => rest;
        public IFunction Function => qs;
        public List<IRestriction> Restrictions => restrictions;
        public int countRestrictions => 1;
        public int Dimension => qs.Dimension;
        public Point extremum;
        public Point Extremum => extremum;
        public QSWithLinearRestriction(QuadraticShape quadraticShape, LinearRestriction rest)
        {
            if (quadraticShape.Dimension != rest.Dimension)
                throw new ArgumentException("Несопадение размерностей");
            this.qs = quadraticShape;
            this.rest = rest;
            restrictions = new List<IRestriction>() { rest };
        }
        public Point CalculateExtremum()
        {
            ISquareMatrix A = qs.GetMatrix();
            Point B = qs.GetVector();
            double betta = rest.Betta;
            Point C = rest.VectorC;

            ISquareMatrix adjA = A.AdjMatrix();
            double detA = A.Determinant;
            Point ac = adjA.Multiply(C);
            double s = Point.ScalarProduct(ac, C);
            double Y = - (2*betta*detA + Point.ScalarProduct(C, adjA.Multiply(B)))/Point.ScalarProduct(C, adjA.Multiply(C));

            Point X = -0.5 * A.Inverse().Multiply(B + (Y * C));
            extremum = X;
            return X;
        }
        public bool IsAcceptableSolution(Point point)
        {
            return rest.IsFulfilled(point);
        }
    }
}
