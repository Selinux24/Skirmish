using System;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Span
    /// </summary>
    class Span
    {
        /// <summary>
        /// The lower limit of the span
        /// </summary>
        public int Min { get; private set; }
        /// <summary>
        /// The upper limit of the span
        /// </summary>
        public int Max { get; private set; }
        /// <summary>
        /// The area id assigned to the span.
        /// </summary>
        public AreaTypes Area { get; set; }
        /// <summary>
        /// The next span higher up in column.
        /// </summary>
        public Span Next { get; set; }

        /// <summary>
        /// Initializes the span data
        /// </summary>
        /// <param name="min">Lower limit</param>
        /// <param name="max">Upper limit</param>
        /// <param name="area">Area</param>
        /// <param name="next">Next span</param>
        public void Initialize(int min, int max, AreaTypes area, Span next = null)
        {
            Min = min;
            Max = max;
            Area = area;
            Next = next;
        }
        /// <summary>
        /// Merges the specified span with current instance
        /// </summary>
        /// <param name="s">Span to merge</param>
        /// <param name="flagMergeThr">Flag merge threshold</param>
        public void Merge(Span s, int flagMergeThr)
        {
            if (s == null)
            {
                return;
            }

            Min = Math.Min(Min, s.Min);
            Max = Math.Max(Max, s.Max);

            if (Area == s.Area)
            {
                return;
            }

            // Merge flags.
            if (Math.Abs(Max - s.Max) <= flagMergeThr)
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
            int amin = minimumAccessibleNeigbor;
            int amax = maximumAccessibleNeigbor;

            int bot = Max;
            int top = Next?.Min ?? int.MaxValue;

            // From minus infinity to the first span.
            int nbot = -walkableClimb;
            int ntop = span?.Min ?? int.MaxValue;

            // Skip neightbour if the gap between the spans is too small.
            if (Math.Min(top, ntop) - Math.Max(bot, nbot) > walkableHeight)
            {
                minh = Math.Min(minh, nbot - bot);
            }

            var ns = span;
            while (ns != null)
            {
                nbot = ns.Max;
                ntop = ns.Next?.Min ?? int.MaxValue;

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

                amin = Math.Min(nbot, amin);
                amax = Math.Max(nbot, amax);
            }

            return (minh, amin, amax);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Min {Min} Max {Max} Area: {Area}; {(Next != null ? $"Next => {Next};" : "")}";
        }
    }
}
