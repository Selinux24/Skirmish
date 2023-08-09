
namespace Engine
{
    /// <summary>
    /// Cubic shadow map
    /// </summary>
    public class ShadowMapPoint : ShadowMap
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="width">With</param>
        /// <param name="height">Height</param>
        /// <param name="arraySize">Array size</param>
        public ShadowMapPoint(Scene scene, string name, int width, int height, int arraySize) : base(scene, name, width, height, 6)
        {
            var (DepthStencils, ShaderResource) = scene.Game.Graphics.CreateCubicShadowMapTextureArrays(name, width, height, arraySize);

            DepthMap = DepthStencils;
            Texture = ShaderResource;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapPoint)} - Light: {Light} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
