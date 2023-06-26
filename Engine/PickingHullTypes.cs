using System;

namespace Engine
{
    /// <summary>
    /// Ray-picking flags
    /// </summary>
    [Flags]
    public enum PickingHullTypes
    {
        /// <summary>
        /// No params
        /// </summary>
        None = 0,
        /// <summary>
        /// Default flags - Hull meshes when available & facing-only triangles
        /// </summary>
        Default = Coarse | Hull | Geometry | FacingOnly,
        /// <summary>
        /// Fast picking - Mesh bounding volumes & facing-only triangles
        /// </summary>
        Fast = Coarse | FacingOnly,
        /// <summary>
        /// Perfect picking - Mesh geometry & facing-only triangles
        /// </summary>
        Perfect = Geometry | FacingOnly,
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
        Geometry = 4,
        /// <summary>
        /// Select only facing triangles
        /// </summary>
        /// <remarks>By default, ray picking test uses all triangles</remarks>
        FacingOnly = 8,
    }
}
