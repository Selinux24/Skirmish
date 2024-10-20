﻿using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Ray-pickable object
    /// </summary>
    public interface IRayPickable<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        bool PickNearest(PickingRay ray, out PickingResult<T> result);
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        /// <remarks>Based on geometry, not distance. For distance tests use PickNearest instead.</remarks>
        bool PickFirst(PickingRay ray, out PickingResult<T> result);
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        bool PickAll(PickingRay ray, out IEnumerable<PickingResult<T>> results);

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        BoundingSphere GetBoundingSphere(bool refresh = false);
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        BoundingBox GetBoundingBox(bool refresh = false);
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns oriented bounding box. Empty if the vertex type hasn't position channel</returns>
        OrientedBoundingBox GetOrientedBoundingBox(bool refresh = false);

        /// <summary>
        /// Picking hull
        /// </summary>
        PickingHullTypes PickingHull { get; set; }
        /// <summary>
        /// Path finding hull
        /// </summary>
        PickingHullTypes PathFindingHull { get; set; }
        /// <summary>
        /// Gets the picking hull of the instance
        /// </summary>
        /// <param name="geometryType">Geometry type</param>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns>Returns the geometry of the instance</returns>
        IEnumerable<T> GetGeometry(GeometryTypes geometryType, bool refresh = false);
        /// <summary>
        /// Gets primitive list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns></returns>
        IEnumerable<T> GetGeometry(bool refresh = false);
        /// <summary>
        /// Gets point list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refreshed or not</param>
        /// <returns></returns>
        IEnumerable<Vector3> GetPoints(bool refresh = false);
    }

    /// <summary>
    /// Geometry types
    /// </summary>
    public enum GeometryTypes
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Picking
        /// </summary>
        Picking = 1,
        /// <summary>
        /// Path finding
        /// </summary>
        PathFinding = 2,
    }
}
