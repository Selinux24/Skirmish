using Engine.Common;
using Engine.Mathematics;

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
        public Matrix4x4 BindShapeMatrix { get; set; }
        /// <summary>
        /// Weight list
        /// </summary>
        public Weight[] Weights { get; set; }
        /// <summary>
        /// Bone names
        /// </summary>
        public string[] BoneNames { get; set; }
    }
}
