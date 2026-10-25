using System.Numerics;
using Vortice.Mathematics;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// Minimap description
    /// </summary>
    public class UIMinimapDescription : SceneObjectDescription
    {
        /// <summary>
        /// Top position
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Left position
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public uint Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public uint Height { get; set; }
        /// <summary>
        /// Terrain to draw
        /// </summary>
        public IDrawable[] Drawables { get; set; }
        /// <summary>
        /// Back color
        /// </summary>
        public Vector4 BackColor { get; set; } = Vector4.Zero;
        /// <summary>
        /// Minimap render area
        /// </summary>
        public BoundingBox MinimapArea { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UIMinimapDescription()
            : base()
        {
            DeferredEnabled = false;
            DepthEnabled = false;
            BlendMode = BlendModes.Alpha;
        }
    }
}
