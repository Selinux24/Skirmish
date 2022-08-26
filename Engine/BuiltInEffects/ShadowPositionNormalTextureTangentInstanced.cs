
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-normal-texture-tangent instanced drawer
    /// </summary>
    public class ShadowPositionNormalTextureTangentInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureTangentInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionNormalTextureTangentVsI>();
        }
    }
}
