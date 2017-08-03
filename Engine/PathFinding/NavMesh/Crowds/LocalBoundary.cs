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

        private Vector3 center;
        private Segment[] segs;
        private int segCount;

        private PolyId[] polys;
        private int numPolys;

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
        public Segment[] Segs
        {
            get
            {
                return segs;
            }
        }
        /// <summary>
        /// Gets the number of segments
        /// </summary>
        public int SegCount
        {
            get
            {
                return segCount;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalBoundary" /> class.
        /// </summary>
        public LocalBoundary()
        {
            Reset();
            segs = new Segment[MaxLocalSegs];
            polys = new PolyId[MaxLocalPolys];
        }

        /// <summary>
        /// Reset all the internal data
        /// </summary>
        public void Reset()
        {
            center = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            segCount = 0;
            numPolys = 0;
        }
        /// <summary>
        /// Add a line segment
        /// </summary>
        /// <param name="dist">The distance</param>
        /// <param name="s">The line segment</param>
        public void AddSegment(float dist, Segment s)
        {
            //insert neighbor based on distance
            int segPos = 0;
            if (segCount == 0)
            {
                segPos = 0;
            }
            else if (dist >= segs[segCount - 1].Dist)
            {
                //further than the last segment, skip
                if (segCount >= MaxLocalSegs)
                    return;

                //last, trivial accept
                segPos = segCount;
            }
            else
            {
                //insert inbetween
                int i;
                for (i = 0; i < segCount; i++)
                    if (dist <= segs[i].Dist)
                        break;
                int tgt = i + 1;
                int n = Math.Min(segCount - i, MaxLocalSegs - tgt);
                if (n > 0)
                {
                    for (int j = 0; j < n; j++)
                        segs[tgt + j] = segs[i + j];
                }

                segPos = i;
            }

            segs[segPos].Dist = dist;
            segs[segPos].Start = s.Start;
            segs[segPos].End = s.End;

            if (segCount < MaxLocalSegs)
                segCount++;
        }
        /// <summary>
        /// Examine polygons in the NavMeshQuery and add polygon edges
        /// </summary>
        /// <param name="reference">The starting polygon reference</param>
        /// <param name="pos">Current position</param>
        /// <param name="collisionQueryRange">Range to query</param>
        /// <param name="navquery">The NavMeshQuery</param>
        public void Update(PolyId reference, Vector3 pos, float collisionQueryRange, NavigationMeshQuery navquery)
        {
            const int MAX_SEGS_PER_POLY = NavigationMeshQuery.VertsPerPolygon;

            if (reference == PolyId.Null)
            {
                this.center = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                this.segCount = 0;
                this.numPolys = 0;
                return;
            }

            this.center = pos;

            //first query non-overlapping polygons
            PolyId[] tempArray = new PolyId[polys.Length];
            PathPoint centerPoint = new PathPoint(reference, pos);
            navquery.FindLocalNeighborhood(ref centerPoint, collisionQueryRange, polys, tempArray, ref numPolys, MaxLocalPolys);

            //secondly, store all polygon edges
            this.segCount = 0;
            Segment[] segs = new Segment[MAX_SEGS_PER_POLY];
            int numSegs = 0;
            for (int j = 0; j < numPolys; j++)
            {
                tempArray = new PolyId[segs.Length];
                navquery.GetPolyWallSegments(polys[j], segs, tempArray, ref numSegs, MAX_SEGS_PER_POLY);
                for (int k = 0; k < numSegs; k++)
                {
                    //skip too distant segments
                    float tseg;
                    float distSqr = Intersection.PointToSegment2DSquared(ref pos, ref segs[k].Start, ref segs[k].End, out tseg);
                    if (distSqr > collisionQueryRange * collisionQueryRange)
                        continue;
                    AddSegment(distSqr, segs[k]);
                }
            }
        }
        /// <summary>
        /// Determines whether the polygon reference is a part of the NavMeshQuery.
        /// </summary>
        /// <param name="navquery">The NavMeshQuery</param>
        /// <returns>True if valid, false if not</returns>
        public bool IsValid(NavigationMeshQuery navquery)
        {
            if (numPolys == 0)
                return false;

            for (int i = 0; i < numPolys; i++)
            {
                if (!navquery.IsValidPolyRef(polys[i]))
                    return false;
            }

            return true;
        }
    }
}
