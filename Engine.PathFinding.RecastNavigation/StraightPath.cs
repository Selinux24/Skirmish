﻿using SharpDX;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Straight path
    /// </summary>
    public class StraightPath
    {
        /// <summary>
        /// Maximum nodes
        /// </summary>
        public const int MaxStraightPath = 256;

        /// <summary>
        /// Maximum path elements
        /// </summary>
        private readonly int maxSize;
        /// <summary>
        /// Path position list
        /// </summary>
        private Vector3[] pathPositions;
        /// <summary>
        /// Path flags
        /// </summary>
        private StraightPathFlagTypes[] pathFlags;
        /// <summary>
        /// Path references
        /// </summary>
        private int[] pathRefs;

        /// <summary>
        /// Start position value
        /// </summary>
        public Vector3 StartPath
        {
            get
            {
                return pathPositions[0];
            }
        }
        /// <summary>
        /// End position value
        /// </summary>
        public Vector3 EndPath
        {
            get
            {
                return pathPositions[Count - 1];
            }
        }
        /// <summary>
        /// Start flags value
        /// </summary>
        public StraightPathFlagTypes StartFlags
        {
            get
            {
                return pathFlags[0];
            }
        }
        /// <summary>
        /// End flags value
        /// </summary>
        public StraightPathFlagTypes EndFlags
        {
            get
            {
                return pathFlags[Count - 1];
            }
        }
        /// <summary>
        /// Start reference value
        /// </summary>
        public int StartRef
        {
            get
            {
                return pathRefs[0];
            }
        }
        /// <summary>
        /// End reference value
        /// </summary>
        public int EndRef
        {
            get
            {
                return pathRefs[Count - 1];
            }
        }
        /// <summary>
        /// Position count
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StraightPath() : this(MaxStraightPath)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxStraightPath">Maximum path count</param>
        public StraightPath(int maxStraightPath)
        {
            this.maxSize = maxStraightPath;
            this.pathPositions = new Vector3[maxStraightPath];
            this.pathFlags = new StraightPathFlagTypes[maxStraightPath];
            this.pathRefs = new int[maxStraightPath];
            this.Count = 0;
        }

        /// <summary>
        /// Gets the current path
        /// </summary>
        /// <returns>Returns the reference list array</returns>
        public IEnumerable<Vector3> GetPaths()
        {
            return pathPositions.Take(Count).ToArray();
        }
        /// <summary>
        /// Gets the current flags
        /// </summary>
        /// <returns>Returns the reference list array</returns>
        public IEnumerable<StraightPathFlagTypes> GetFlags()
        {
            return pathFlags.Take(Count).ToArray();
        }
        /// <summary>
        /// Gets the current reference list
        /// </summary>
        /// <returns>Returns the reference list array</returns>
        public IEnumerable<int> GetRefs()
        {
            return pathRefs.Take(Count).ToArray();
        }

        /// <summary>
        /// Append a new vertex to the path
        /// </summary>
        /// <param name="pos">Position</param>
        /// <param name="flags">Flags</param>
        /// <param name="r">Poly reference</param>
        public bool Append(Vector3 pos, StraightPathFlagTypes flags, int r)
        {
            if (Count >= maxSize)
            {
                return false;
            }

            pathPositions[Count] = pos;
            if (pathFlags != null)
            {
                pathFlags[Count] = flags;
            }
            if (pathRefs != null)
            {
                pathRefs[Count] = r;
            }
            Count++;

            return true;
        }

        /// <summary>
        /// Gets the path position at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the position at index</returns>
        public Vector3 GetPath(int index)
        {
            return pathPositions[index];
        }
        /// <summary>
        /// Gets the path flags at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the flags at index</returns>
        public StraightPathFlagTypes GetFlag(int index)
        {
            return pathFlags[index];
        }
        /// <summary>
        /// Gets the path polygon reference at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Returns the polygon reference at index</returns>
        public int GetRef(int index)
        {
            return pathRefs[index];
        }

        /// <summary>
        /// Sets the flags value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="flags">Flag value</param>
        public void SetFlags(int index, StraightPathFlagTypes flags)
        {
            pathFlags[index] = flags;
        }
        /// <summary>
        /// Sets the reference value at index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="r">Reference value</param>
        public void SetRef(int index, int r)
        {
            pathRefs[index] = r;
        }

        /// <summary>
        /// Copies the instance
        /// </summary>
        /// <returns>Returns a new instance</returns>
        public StraightPath Copy()
        {
            return new StraightPath
            {
                pathPositions = pathPositions.ToArray(),
                pathFlags = pathFlags.ToArray(),
                pathRefs = pathRefs.ToArray(),
                Count = Count,
            };
        }
        /// <summary>
        /// Removes the first path item
        /// </summary>
        public void RemoveFirst()
        {
            Count--;
            if (Count > 0)
            {
                Array.ConstrainedCopy(pathFlags, 1, pathFlags, 0, Count);
                Array.ConstrainedCopy(pathRefs, 1, pathRefs, 0, Count);
                Array.ConstrainedCopy(pathPositions, 1, pathPositions, 0, Count);
            }

            pathFlags[Count] = 0;
            pathRefs[Count] = 0;
            pathPositions[Count] = Vector3.Zero;
        }
        /// <summary>
        /// Prune path at position
        /// </summary>
        /// <param name="npos">Position index</param>
        public void Prune(int npos)
        {
            Count = npos;
        }
        /// <summary>
        /// Clears the path
        /// </summary>
        public void Clear()
        {
            Count = 0;
        }
    }
}
