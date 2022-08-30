
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow position-color drawer
    /// </summary>
    public class ShadowPositionColor : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionColorVs>();
        }
    }
}
