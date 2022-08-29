
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow skinned position-normal-color instanced drawer
    /// </summary>
    public class ShadowPositionNormalColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionNormalColorVsI>();
        }
    }
}
