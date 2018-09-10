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
        /// Camera culling volume
        /// </summary>
        public CullingVolumeCamera CameraVolume { get; set; }
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
        /// Directional shadow map
        /// </summary>
        public IShadowMap ShadowMapDirectional { get; set; }
        /// <summary>
        /// Point light shadow map
        /// </summary>
        public IShadowMap ShadowMapPoint { get; set; }
        /// <summary>
        /// Spot light shadow map
        /// </summary>
        public IShadowMap ShadowMapSpot { get; set; }

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
