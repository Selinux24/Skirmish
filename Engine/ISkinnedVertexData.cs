
namespace Engine
{
    /// <summary>
    /// Skinned vertex data interface
    /// </summary>
    public interface ISkinnedVertexData : IVertexData
    {
        /// <summary>
        /// Gets the bone weight by index
        /// </summary>
        /// <param name="index">Index</param>
        float GetBoneWeight(int index);
        /// <summary>
        /// Gets the bone weight by index
        /// </summary>
        /// <param name="index">Index</param>
        byte GetBoneIndex(int index);
    }
}
