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
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Drawer mode
        /// </summary>
        public DrawerModes DrawerMode { get; set; } = DrawerModes.Forward;

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
        public IntersectionVolumeFrustum CameraVolume { get; set; }
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
        /// Level of detail
        /// </summary>
        public Vector3 LevelOfDetail { get; set; }

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
        /// Validates the drawing stage
        /// </summary>
        /// <param name="blendMode">Blend mode</param>
        /// <returns>Returns true if the specified blend mode is valid for the current drawing stage</returns>
        public bool ValidateDraw(BlendModes blendMode)
        {
            if (DrawerMode.HasFlag(DrawerModes.OpaqueOnly))
            {
                return blendMode.HasFlag(BlendModes.Opaque);
            }

            if (DrawerMode.HasFlag(DrawerModes.TransparentOnly))
            {
                return blendMode.HasFlag(BlendModes.Alpha) || blendMode.HasFlag(BlendModes.Transparent);
            }

            return false;
        }
        /// <summary>
        /// Validates the drawing stage
        /// </summary>
        /// <param name="blendMode">Blend mode</param>
        /// <param name="transparent">The component to draw is has transparency</param>
        /// <returns>Returns true if the specified blend mode is valid for the current drawing stage</returns>
        public bool ValidateDraw(BlendModes blendMode, bool transparent)
        {
            if (DrawerMode.HasFlag(DrawerModes.OpaqueOnly) && !transparent)
            {
                return blendMode.HasFlag(BlendModes.Opaque);
            }

            if (DrawerMode.HasFlag(DrawerModes.TransparentOnly) && transparent)
            {
                return blendMode.HasFlag(BlendModes.Alpha) || blendMode.HasFlag(BlendModes.Transparent);
            }

            return false;
        }
    }
}
