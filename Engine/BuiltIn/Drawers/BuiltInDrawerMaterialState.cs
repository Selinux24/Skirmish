using SharpDX;

namespace Engine.BuiltIn.Drawers
{
    /// <summary>
    /// Drawer material state
    /// </summary>
    public struct BuiltInDrawerMaterialState : IDrawerMaterialState
    {
        /// <summary>
        /// Default state
        /// </summary>
        public static BuiltInDrawerMaterialState Default()
        {
            return new BuiltInDrawerMaterialState
            {
                Material = null,
                UseAnisotropic = false,
                TextureIndex = 0,
                TintColor = Color4.White,
            };
        }

        /// <inheritdoc/>
        public IMeshMaterial Material { get; set; }
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; }
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; }
    }
}
