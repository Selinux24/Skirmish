using Engine.Common;
using System.Collections.Generic;
using System.Numerics;

namespace Engine.Content
{
    /// <summary>
    /// Skinning information
    /// </summary>
    public struct SkinningInfo
    {
        /// <summary>
        /// Bind shape matrix
        /// </summary>
        public System.Numerics.Matrix4x4 BindShapeMatrix { get; set; }
        /// <summary>
        /// Weight list
        /// </summary>
        public IEnumerable<Weight> Weights { get; set; }
        /// <summary>
        /// Bone names
        /// </summary>
        public IEnumerable<string> BoneNames { get; set; }
    }
}
