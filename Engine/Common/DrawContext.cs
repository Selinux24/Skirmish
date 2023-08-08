using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Drawing context
    /// </summary>
    public struct DrawContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Drawer mode
        /// </summary>
        public DrawerModes DrawerMode { get; set; }
        /// <summary>
        /// Engine form
        /// </summary>
        public IEngineForm Form { get; set; }
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime { get; set; }
        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera { get; set; }
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
        /// Pass context
        /// </summary>
        public PassContext PassContext { get; set; }
        /// <summary>
        /// Device context
        /// </summary>
        public readonly IEngineDeviceContext DeviceContext { get => PassContext.DeviceContext; }

        /// <summary>
        /// Validates the drawing stage
        /// </summary>
        /// <param name="blendMode">Blend mode</param>
        /// <returns>Returns true if the specified blend mode is valid for the current drawing stage</returns>
        public readonly bool ValidateDraw(BlendModes blendMode)
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
        public readonly bool ValidateDraw(BlendModes blendMode, bool transparent)
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

        /// <summary>
        /// Clones the actual draw context
        /// </summary>
        /// <param name="name">New name</param>
        /// <param name="drawerMode">New drawer mode</param>
        public DrawContext Clone(string name, DrawerModes drawerMode)
        {
            return new DrawContext
            {
                Name = name,
                DrawerMode = drawerMode,
                Form = Form,
                GameTime = GameTime,
                Camera = Camera,
                Lights = Lights,
                LevelOfDetail = LevelOfDetail,
                ShadowMapDirectional = ShadowMapDirectional,
                ShadowMapPoint = ShadowMapPoint,
                ShadowMapSpot = ShadowMapSpot,
                PassContext = PassContext,
            };
        }
    }
}
