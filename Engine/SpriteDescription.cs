using Engine.UI;
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Sprite description
    /// </summary>
    public class SpriteDescription : UIControlDescription
    {
        /// <summary>
        /// Sprite textures
        /// </summary>
        public string[] Textures { get; set; }
        /// <summary>
        /// Initial texture index
        /// </summary>
        public int TextureIndex { get; set; } = 0;
        /// <summary>
        /// UV map
        /// </summary>
        public Vector4? UVMap { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteDescription()
            : base()
        {

        }
    }
}
