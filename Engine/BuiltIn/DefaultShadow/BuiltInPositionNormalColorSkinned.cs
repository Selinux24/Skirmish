
namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-color drawer
    /// </summary>
    public class BuiltInPositionNormalColorSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkinnedPositionNormalColorVs>();
        }

        /// <inheritdoc/>
        public void Update(AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<SkinnedPositionNormalColorVs>();
            vertexShader?.WriteCBPerInstance(animation);
        }
    }
}
