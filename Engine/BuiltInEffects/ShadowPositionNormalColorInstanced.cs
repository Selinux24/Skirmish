
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow position-normal-color instanced drawer
    /// </summary>
    public class ShadowPositionNormalColorInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionNormalColorVsI>();
        }
    }
}
