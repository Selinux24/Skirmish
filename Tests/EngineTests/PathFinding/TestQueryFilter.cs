using Engine.PathFinding;

namespace EngineTests.PathFinding
{
    class TestQueryFilter : GraphQueryFilter
    {
        public TestQueryFilter() : base(1)
        {

        }

        public override int EvaluateArea(int area)
        {
            if (area != 0)
            {
                return 1;
            }

            return 0;
        }

        public override TAction EvaluateArea<TArea, TAction>(TArea area)
        {
            return (TAction)(object)0;
        }
    }
}
