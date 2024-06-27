using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Basic query filter
    /// </summary>
    [Serializable]
    public class BasicQueryFilter : GraphQueryFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BasicQueryFilter() : base(1)
        {

        }

        /// <inheritdoc/>
        public override int EvaluateArea(int area)
        {
            if (area != 0)
            {
                return 1;
            }

            return 0;
        }
        /// <inheritdoc/>
        public override TAction EvaluateArea<TArea, TAction>(TArea area)
        {
            return (TAction)(object)0;
        }
    }
}
