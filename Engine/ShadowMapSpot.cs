using SharpDX;

namespace Engine
{
    using Engine.Effects;

    /// <summary>
    /// Spot shadow map
    /// </summary>
    public class ShadowMapSpot : ShadowMap
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="width">With</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        public ShadowMapSpot(Scene scene, string name, int width, int height, int arraySize) : base(scene, name, width, height, arraySize)
        {
            var (DepthStencils, ShaderResource) = scene.Game.Graphics.CreateShadowMapTextureArrays(name, width, height, 1, arraySize);

            DepthMap = DepthStencils;
            Texture = ShaderResource;
        }

        /// <inheritdoc/>
        public override void UpdateFromLightViewProjection(Camera camera, ISceneLight light)
        {
            if (light is ISceneLightSpot lightSpot)
            {
                var near = 1f;
                var projection = Matrix.PerspectiveFovLH(lightSpot.AngleRadians * 2f, 1f, near, lightSpot.Radius);

                var pos = lightSpot.Position;
                var look = lightSpot.Position + (lightSpot.Direction * lightSpot.Radius);
                var view = Matrix.LookAtLH(pos, look, Vector3.Up);

                var vp = view * projection;

                ToShadowMatrix = vp;
                LightPosition = lightSpot.Position;
                FromLightViewProjectionArray = new[] { vp };
            }
        }
        /// <inheritdoc/>
        public override IShadowMapDrawer GetEffect()
        {
            return DrawerPool.EffectShadowBasic;
        }

        /// <inheritdoc/>
        public override void UpdateGlobals()
        {

        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapSpot)} - LightPosition: {LightPosition} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
