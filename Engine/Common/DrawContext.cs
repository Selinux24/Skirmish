using SharpDX;
using SharpDX.Direct3D11;

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
        public string Name = "";
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime;
        /// <summary>
        /// Drawer mode
        /// </summary>
        public DrawerModesEnum DrawerMode = DrawerModesEnum.Forward;
        /// <summary>
        /// World matrix
        /// </summary>
        public Matrix World;
        /// <summary>
        /// View matrix
        /// </summary>
        public Matrix View;
        /// <summary>
        /// Projection matrix
        /// </summary>
        public Matrix Projection;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        public Matrix ViewProjection;
        /// <summary>
        /// Bounding frustum
        /// </summary>
        public BoundingFrustum Frustum;
        /// <summary>
        /// Eye position
        /// </summary>
        public Vector3 EyePosition;
        /// <summary>
        /// Eye target
        /// </summary>
        public Vector3 EyeTarget;
        /// <summary>
        /// Lights
        /// </summary>
        public SceneLights Lights;
        /// <summary>
        /// View * projection from light matrix
        /// </summary>
        public Matrix FromLightViewProjection;
        /// <summary>
        /// Shadow maps
        /// </summary>
        public int ShadowMaps;
        /// <summary>
        /// Static shadow map
        /// </summary>
        public ShaderResourceView ShadowMapStatic;
        /// <summary>
        /// Dynamic shadow map
        /// </summary>
        public ShaderResourceView ShadowMapDynamic;
    }
}
