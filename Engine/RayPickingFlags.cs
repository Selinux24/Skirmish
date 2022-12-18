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
        /// No params
        /// </summary>
        None = 0,
        /// <summary>
        /// Default flags - Hull meshes when available & facing-only triangles
        /// </summary>
        Default = Hull | FacingOnly,
        /// <summary>
        /// Fast picking - Mesh bounding volumes & facing-only triangles
        /// </summary>
        Fast = Coarse | FacingOnly,
        /// <summary>
        /// Perfect picking - Mesh geometry & facing-only triangles
        /// </summary>
        Perfect = Objects | FacingOnly,
        /// <summary>
        /// Use bounding volumes instead of geometry
        /// </summary>
        Coarse = 1,
        /// <summary>
        /// Use hull geometry if available
        /// </summary>
        Hull = 2,
        /// <summary>
        /// Use mesh geometry
        /// </summary>
        Objects = 4,
        /// <summary>
        /// Select only facing triangles
        /// </summary>
        /// <remarks>By default, ray picking test uses all triangles</remarks>
        FacingOnly = 8,
    }
}
