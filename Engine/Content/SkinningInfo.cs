using Engine.Common;
using SharpDX;
using System.Collections.Generic;

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
        public Matrix BindShapeMatrix { get; set; }
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
