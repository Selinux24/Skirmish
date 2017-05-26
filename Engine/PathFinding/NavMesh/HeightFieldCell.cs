using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A cell is a column of voxels represented in <see cref="HeightFieldSpan"/>s.
    /// </summary>
    class HeightFieldCell
    {
        private List<HeightFieldSpan> spans = new List<HeightFieldSpan>();

        /// <summary>
        /// Gets the number of spans in the cell.
        /// </summary>
        public int SpanCount
        {
            get
            {
                return this.spans.Count;
            }
        }
        /// <summary>
        /// Gets the number of spans that are in walkable <see cref="Area"/>s.
        /// </summary>
        public int WalkableSpanCount
        {
            get
            {
                return this.spans.Count(s => s.Area.IsWalkable);
            }
        }
        /// <summary>
        /// Gets a readonly list of all the <see cref="HeightFieldSpan"/>s contained in the cell.
        /// </summary>
        /// <value>A readonly list of spans.</value>
        public ReadOnlyCollection<HeightFieldSpan> Spans
        {
            get
            {
                return this.spans.AsReadOnly();
            }
        }
        /// <summary>
        /// Gets a modifiable list of all the <see cref="HeightFieldSpan"/>s contained in the cell.
        /// Should only be used for filtering in <see cref="HeightField"/>.
        /// </summary>
        /// <value>A list of spans for modification.</value>
        internal List<HeightFieldSpan> MutableSpans
        {
            get
            {
                return this.spans;
            }
        }
        /// <summary>
        /// Gets the <see cref="HeightFieldSpan"/> that contains the specified voxel.
        /// </summary>
        /// <param name="location">The voxel to search for.</param>
        /// <returns>The span containing the voxel. Null if the voxel is empty.</returns>
        public HeightFieldSpan? this[int location]
        {
            get
            {
                //Iterate the list of spans
                foreach (HeightFieldSpan s in this.spans)
                {
                    if (s.Minimum > location)
                    {
                        break;
                    }
                    else if (s.Maximum >= location)
                    {
                        return s;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightFieldCell"/> class.
        /// </summary>
        public HeightFieldCell()
        {
            
        }

        /// <summary>
        /// Adds a <see cref="HeightFieldSpan"/> to the cell.
        /// </summary>
        /// <param name="span">A span.</param>
        /// <param name="height">The number of voxels in the column.</param>
        /// <exception cref="ArgumentException">Thrown if an invalid span is provided.</exception>
        public void AddSpan(HeightFieldSpan span, int height)
        {
            if (span.Minimum > span.Maximum)
            {
                int tmp = span.Minimum;
                span.Minimum = span.Maximum;
                span.Maximum = tmp;
            }

            //Clamp the span to the cell's range of [0, maxHeight]
            span.Minimum = MathUtil.Clamp(span.Minimum, 0, height);
            span.Maximum = MathUtil.Clamp(span.Maximum, 0, height);

            lock (this.spans)
            {
                for (int i = 0; i < this.spans.Count; i++)
                {
                    //Check whether the current span is below, or overlapping existing spans.
                    //If the span is completely above the current span the loop will continue.
                    HeightFieldSpan cur = this.spans[i];
                    if (cur.Minimum > span.Maximum)
                    {
                        //The new span is below the current one and is not intersecting.
                        this.spans.Insert(i, span);
                        return;
                    }
                    else if (cur.Maximum >= span.Minimum)
                    {
                        //The new span is colliding with the current one, merge them together.
                        if (cur.Minimum < span.Minimum)
                        {
                            span.Minimum = cur.Minimum;
                        }

                        if (cur.Maximum == span.Maximum)
                        {
                            //In the case that both spans end at the same voxel, the area gets merged. The new span's area
                            //has priority if both spans are walkable, so the only case where the area gets set is when
                            //the new area isn't walkable and the old one is.
                            if (!span.Area.IsWalkable && cur.Area.IsWalkable)
                            {
                                span.Area = cur.Area;
                            }
                        }
                        else if (cur.Maximum > span.Maximum)
                        {
                            span.Maximum = cur.Maximum;
                            span.Area = cur.Area;
                        }

                        //Remove the current span and adjust i.
                        //We do this to avoid duplicating the current span.
                        this.spans.RemoveAt(i);
                        i--;
                    }
                }

                //If the span is not inserted, it is the highest span and will be added to the end.
                this.spans.Add(span);
            }
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns a string represening the instance</returns>
        public override string ToString()
        {
            return string.Format("Spans: {0}; Walkables: {1}", this.SpanCount, this.WalkableSpanCount);
        }
    }
}
