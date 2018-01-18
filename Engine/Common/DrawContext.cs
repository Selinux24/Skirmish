using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Drawing context
    /// </summary>
    public class DrawContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Drawer mode
        /// </summary>
        public DrawerModesEnum DrawerMode { get; set; }

        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime { get; set; }
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection { get; set; }
        /// <summary>
        /// Bounding frustum
        /// </summary>
        public BoundingFrustum Frustum { get; set; }
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition { get; set; }
        /// <summary>
        /// Eye target
        /// </summary>
        public Vector3 EyeTarget { get; set; }
        /// <summary>
        /// Lights
        /// </summary>
        public SceneLights Lights { get; set; }

        /// <summary>
        /// Shadow maps
        /// </summary>
        public ShadowMapFlags ShadowMaps { get; set; }
        /// <summary>
        /// Low definition shadow map
        /// </summary>
        public IShadowMap ShadowMapLow { get; set; }
        /// <summary>
        /// High definition shadow map
        /// </summary>
        public IShadowMap ShadowMapHigh { get; set; }
        /// <summary>
        /// Cube shadow map
        /// </summary>
        public IShadowMap[] ShadowMapCube { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DrawContext()
        {
            this.Name = string.Empty;
            this.DrawerMode = DrawerModesEnum.Forward;
        }
    }
}
