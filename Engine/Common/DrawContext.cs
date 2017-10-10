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
        /// Game time
        /// </summary>
        public GameTime GameTime { get; set; }
        /// <summary>
        /// Drawer mode
        /// </summary>
        public DrawerModesEnum DrawerMode { get; set; }
        /// <summary>
        /// World matrix
        /// </summary>
        public Matrix World { get; set; }
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
        public int ShadowMaps { get; set; }
        /// <summary>
        /// View * projection from light matrix for static shadows
        /// </summary>
        public Matrix FromLightViewProjectionLow { get; set; }
        /// <summary>
        /// View * projection from light matrix for dynamic shadows
        /// </summary>
        public Matrix FromLightViewProjectionHigh { get; set; }
        /// <summary>
        /// Low definition shadow map
        /// </summary>
        public EngineTexture ShadowMapLow { get; set; }
        /// <summary>
        /// High definition shadow map
        /// </summary>
        public EngineTexture ShadowMapHigh { get; set; }

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
