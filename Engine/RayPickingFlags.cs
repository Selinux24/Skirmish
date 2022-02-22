using System;

namespace Engine
{
    /// <summary>
    /// Ray-picking flags
    /// </summary>
    [Flags]
    public enum RayPickingParams
    {
        /// <summary>
        /// Default flags - Mesh volumes, facing-only triangles
        /// </summary>
        Default = Coarse | FacingOnly,
        /// <summary>
        /// Perfect picking - Mesh geometry, all triangles
        /// </summary>
        Perfect = Geometry | AllTriangles,
        /// <summary>
        /// Coarse picking. Use mesh volumes instead of geometry
        /// </summary>
        /// <remarks>If the model has no mesh volumes, the ray pincking test uses the model's geometry</remarks>
        Coarse = 1,
        /// <summary>
        /// Geometry picking. Use mesh geometry
        /// </summary>
        Geometry = 2,
        /// <summary>
        /// Select only facing triangles
        /// </summary>
        /// <remarks>By default, ray picking test uses all triangles</remarks>
        FacingOnly = 4,
        /// <summary>
        /// Select all triangles
        /// </summary>
        AllTriangles = 8,
        /// <summary>
        /// Test volumes only
        /// </summary>
        Volumes = 16,
    }
}
