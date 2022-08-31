
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow Skinned position-normal-texture-tangent instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangentSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureTangentSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkinnedPositionNormalTextureTangentVsI>();
        }
    }
}
