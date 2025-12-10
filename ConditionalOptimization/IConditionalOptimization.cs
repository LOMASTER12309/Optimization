using MyMath;

namespace ConditionalOptimization
{
    //Задача условной оптимизации
    public interface IConditionalOptimization
    {
        public int Dimension { get; }
        public IFunction Function { get; }
        public List<IRestriction> Restrictions { get; }
        public int countRestrictions { get; }
        public bool IsAcceptableSolution(Point point);
        public static bool IsCorrectRests(IFunction function, List<IRestriction> rests)
        {
            foreach (IRestriction restriction in rests)
                if (restriction.Dimension != function.Dimension)
                    return false;
            return true;
        }
    }
    // Реализация
    public class FunctionWithRestrictions : IConditionalOptimization
    {
        IFunction function;
        List<IRestriction> restrictions;
        public IFunction Function => function;
        public List<IRestriction> Restrictions => restrictions;
        public int countRestrictions => restrictions.Count;
        public int Dimension => function.Dimension;
        public FunctionWithRestrictions(IFunction function, List<IRestriction> rests)
        {
            if (!IConditionalOptimization.IsCorrectRests(function, rests))
                throw new ArgumentException("Несопадение размерностей");
            this.function = function;
            this.restrictions = rests;
        }
        public bool IsAcceptableSolution(Point point)
        {
            foreach (var rest in restrictions)
                if (!rest.IsFulfilled(point)) return false;
            return true;
        }
    }
}
