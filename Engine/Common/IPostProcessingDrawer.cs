
namespace Engine.Common
{
    /// <summary>
    /// Post-processing drawer interface
    /// </summary>
    public interface IPostProcessingDrawer<in T> where T : IPostProcessState
    {
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="sourceTexture">Source texture</param>
        /// <param name="effect">Effect</param>
        /// <param name="state">State</param>
        void Draw(IEngineDeviceContext dc, EngineShaderResourceView sourceTexture, int effect, T state);
        /// <summary>
        /// Combines the effect with the target source
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        void Combine(IEngineDeviceContext dc, EngineShaderResourceView texture1, EngineShaderResourceView texture2);
        /// <summary>
        /// Updates the internal buffers according to the new render dimension
        /// </summary>
        void Resize();
    }
}