
namespace Engine.BuiltIn.Components.ShadowMapping
{
    /// <summary>
    /// Cascaded shadow map
    /// </summary>
    public class ShadowMapCascade : ShadowMap
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="size">Map size</param>
        /// <param name="mapCount">Map count</param>
        /// <param name="cascades">Cascade far clip distances</param>
        public ShadowMapCascade(Scene scene, string name, uint size, uint mapCount, uint arraySize, float[] cascades) : base(scene, name, size, size, (uint)cascades.Length)
        {
            var (DepthStencils, ShaderResource) = scene.Game.Graphics.CreateShadowMapTextureArrays(name, size, size, mapCount, arraySize);

            DepthMap = DepthStencils;
            DepthMapTexture = ShaderResource;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(ShadowMapCascade)} - Light: {LightSource} HighResolutionMap: {HighResolutionMap}";
        }
    }
}
