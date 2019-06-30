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
        Coarse = 0x01,
        /// <summary>
        /// Geometry picking. Use mesh geometry
        /// </summary>
        Geometry = 0x02,
        /// <summary>
        /// Select only facing triangles
        /// </summary>
        /// <remarks>By default, ray picking test uses all triangles</remarks>
        FacingOnly = 0x04,
        /// <summary>
        /// Select all triangles
        /// </summary>
        AllTriangles = 0x08,
    }
}
