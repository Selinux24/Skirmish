using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Span
    /// </summary>
    class Span
    {
        /// <summary>
        /// Span height bits
        /// </summary>
        const int SpanHeightBits = 13;
        /// <summary>
        /// Defines the maximum value for smin and smax.
        /// </summary>
        public const int SpanMaxHeight = (1 << SpanHeightBits) - 1;

        /// <summary>
        /// The lower limit of the span
        /// </summary>
        public int SMin;
        /// <summary>
        /// The upper limit of the span
        /// </summary>
        public int SMax;
        /// <summary>
        /// The area id assigned to the span.
        /// </summary>
        public AreaTypes Area;
        /// <summary>
        /// The next span higher up in column.
        /// </summary>
        public Span Next;

        /// <summary>
        /// Merges the specified span with current instance
        /// </summary>
        /// <param name="s">Span to merge</param>
        /// <param name="flagMergeThr">Flag merge threshold</param>
        public void MergeSpans(Span s, int flagMergeThr)
        {
            if (s == null) return;

            SMin = Math.Min(SMin, s.SMin);
            SMax = Math.Max(SMax, s.SMax);

            // Merge flags.
            if (Math.Abs(SMax - s.SMax) <= flagMergeThr)
            {
                Area = (AreaTypes)Math.Max((int)Area, (int)s.Area);
            }
        }
        /// <summary>
        /// Finds the minimum height between the specified spans
        /// </summary>
        /// <param name="span">Span</param>
        /// <param name="walkableHeight">Walkable height</param>
        /// <param name="walkableClimb">Walkable climb</param>
        /// <param name="minHeight">Minimum height to start the search</param>
        /// <param name="minimumAccessibleNeigbor">Minimum accessible neighbour to start the search</param>
        /// <param name="maximumAccessibleNeigbor">Maximum accessible neighbour to start the search</param>
        /// <returns>Returns the found minimum height, minimum and maximum accesible neighbours</returns>
        public (int MinHeight, int MinNei, int MaxNei) FindMinimumHeight(Span span, int walkableHeight, int walkableClimb, int minHeight, int minimumAccessibleNeigbor, int maximumAccessibleNeigbor)
        {
            int minh = minHeight;
            int asmin = minimumAccessibleNeigbor;
            int asmax = maximumAccessibleNeigbor;

            int bot = SMax;
            int top = Next?.SMin ?? int.MaxValue;

            // From minus infinity to the first span.
            int nbot = -walkableClimb;
            int ntop = span?.SMin ?? int.MaxValue;

            // Skip neightbour if the gap between the spans is too small.
            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
            {
                minh = Math.Min(minh, nbot - bot);
            }

            var ns = span;
            while (ns != null)
            {
                nbot = ns.SMax;
                ntop = ns.Next?.SMin ?? int.MaxValue;

                ns = ns.Next;

                // Skip neightbour if the gap between the spans is too small.
                if (Math.Min(top, ntop) - Math.Max(bot, nbot) <= walkableHeight)
                {
                    continue;
                }

                minh = Math.Min(minh, nbot - bot);

                // Find min/max accessible neighbour height. 
                if (Math.Abs(nbot - bot) > walkableClimb)
                {
                    continue;
                }

                asmin = Math.Min(nbot, asmin);
                asmax = Math.Max(nbot, asmax);
            }

            return (minh, asmin, asmax);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Min {SMin} Max {SMax} Area: {Area}; {(Next != null ? "Next =>" : "")};";
        }
    }
}
