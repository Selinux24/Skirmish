using SharpDX;
using System;

namespace Engine.PathFinding.NavMesh.Crowds
{
    using Engine.Common;

    /// <summary>
	/// The LocalBoundary class stores segments and polygon indices for temporary use.
	/// </summary>
	class LocalBoundary
    {
        private const int MaxLocalSegs = 8;
        private const int MaxLocalPolys = 16;

        private Vector3 center = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        private Segment[] segments = new Segment[MaxLocalSegs];
        private int segmentCount = 0;
        private PolyId[] polygons = new PolyId[MaxLocalPolys];
        private int polygonCount = 0;

        /// <summary>
        /// Gets the center
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return center;
            }
        }
        /// <summary>
        /// Gets the segments
        /// </summary>
        public Segment[] Segments
        {
            get
            {
                return segments;
            }
        }
        /// <summary>
        /// Gets the number of segments
        /// </summary>
        public int SegmentCount
        {
            get
            {
                return segmentCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalBoundary" /> class.
        /// </summary>
        public LocalBoundary()
        {

        }

        /// <summary>
        /// Reset all the internal data
        /// </summary>
        public void Reset()
        {
            center = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            segmentCount = 0;
            polygonCount = 0;
        }
        /// <summary>
        /// Add a line segment
        /// </summary>
        /// <param name="distance">The distance</param>
        /// <param name="segment">The line segment</param>
        public void AddSegment(float distance, Segment segment)
        {
            //insert neighbor based on distance
            int segmentPosition = 0;
            if (this.segmentCount == 0)
            {
                segmentPosition = 0;
            }
            else if (distance >= this.segments[this.segmentCount - 1].Dist)
            {
                //further than the last segment, skip
                if (this.segmentCount >= MaxLocalSegs)
                {
                    return;
                }

                //last, trivial accept
                segmentPosition = this.segmentCount;
            }
            else
            {
                //insert inbetween
                int i;
                for (i = 0; i < this.segmentCount; i++)
                {
                    if (distance <= this.segments[i].Dist)
                    {
                        break;
                    }
                }

                int tgt = i + 1;
                int n = Math.Min(this.segmentCount - i, MaxLocalSegs - tgt);
                if (n > 0)
                {
                    for (int j = 0; j < n; j++)
                    {
                        this.segments[tgt + j] = this.segments[i + j];
                    }
                }

                segmentPosition = i;
            }

            this.segments[segmentPosition].Dist = distance;
            this.segments[segmentPosition].Start = segment.Start;
            this.segments[segmentPosition].End = segment.End;

            if (this.segmentCount < MaxLocalSegs)
            {
                this.segmentCount++;
            }
        }
        /// <summary>
        /// Examine polygons in the NavMeshQuery and add polygon edges
        /// </summary>
        /// <param name="reference">The starting polygon reference</param>
        /// <param name="position">Current position</param>
        /// <param name="collisionQueryRange">Range to query</param>
        /// <param name="navQuery">The NavMeshQuery</param>
        public void Update(PolyId reference, Vector3 position, float collisionQueryRange, NavigationMeshQuery navQuery)
        {
            if (reference == PolyId.Null)
            {
                this.center = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                this.segmentCount = 0;
                this.polygonCount = 0;
            }
            else
            {
                this.center = position;

                //first query non-overlapping polygons
                PathPoint centerPoint = new PathPoint(reference, position);
                PolyId[] polys;
                PolyId[] parentPolys;
                int polyCount;
                if (navQuery.FindLocalNeighborhood(centerPoint, collisionQueryRange, MaxLocalPolys, out polys, out parentPolys, out polyCount))
                {
                    this.polygons = polys;
                    this.polygonCount = polyCount;
                    this.segmentCount = 0;

                    //secondly, store all polygon edges
                    float collisionQueryRangeSqr = collisionQueryRange * collisionQueryRange;
                    for (int i = 0; i < polygonCount; i++)
                    {
                        Segment[] segs;
                        PolyId[] tempArray;
                        int numSegs;
                        if (navQuery.GetPolyWallSegments(
                            polygons[i],
                            NavigationMeshBuilder.VerticesPerPolygon,
                            out segs, out tempArray, out numSegs))
                        {
                            for (int j = 0; j < numSegs; j++)
                            {
                                var segment = segs[j];

                                //skip too distant segments
                                float tseg;
                                float distSqr = Intersection.PointToSegment2DSquared(position, segment.Start, segment.End, out tseg);
                                if (distSqr <= collisionQueryRangeSqr)
                                {
                                    this.AddSegment(distSqr, segment);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Determines whether the polygon reference is a part of the NavMeshQuery.
        /// </summary>
        /// <param name="navQuery">The NavMeshQuery</param>
        /// <returns>True if valid, false if not</returns>
        public bool IsValid(NavigationMeshQuery navQuery)
        {
            if (polygonCount == 0)
            {
                return false;
            }

            for (int i = 0; i < polygonCount; i++)
            {
                if (!navQuery.IsValidPolyRef(polygons[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
