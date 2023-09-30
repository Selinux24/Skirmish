﻿using SharpDX;
using System.Collections.Generic;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph node
    /// </summary>
    public interface IGraphNode : IValueWithCost
    {
        /// <summary>
        /// Center position
        /// </summary>
        Vector3 Center { get; }

        /// <summary>
        /// Gets whether this node contains specified point
        /// </summary>
        /// <param name="point">Point to test</param>
        /// <returns>Returns whether this node contains specified point</returns>
        bool Contains(Vector3 point);
        /// <summary>
        /// Gets the point list of this node perimeter
        /// </summary>
        /// <returns>Returns the point list of this node perimeter</returns>
        IEnumerable<Vector3> GetPoints();
    }
}
