
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow position-color instanced drawer
    /// </summary>
    public class ShadowPositionColorInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionColorInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionColorVsI>();
        }
    }
}
