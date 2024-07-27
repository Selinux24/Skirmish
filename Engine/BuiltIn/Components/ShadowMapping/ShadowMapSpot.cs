
namespace Engine.BuiltIn.Components.ShadowMapping
{
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
            DepthMapTexture = ShaderResource;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapSpot)} - Light: {LightSource} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
