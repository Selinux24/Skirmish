
namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow skinned position-color drawer
    /// </summary>
    public class BuiltInPositionColorSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="positionColorVsSkinned">Skinned position color vertex shader</param>
        /// <param name="positionColorPs">Position color pixel shader</param>
        public BuiltInPositionColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkinnedPositionColorVs>();
        }

        /// <inheritdoc/>
        public void Update(AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<SkinnedPositionColorVs>();
            vertexShader?.WriteCBPerInstance(animation);
        }
    }
}
