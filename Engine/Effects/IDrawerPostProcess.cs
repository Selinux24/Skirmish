using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.PostProcessing;

    /// <summary>
    /// Post-processing drawer interface
    /// </summary>
    public interface IDrawerPostProcess
    {
        /// <summary>
        /// Gets the specified effect technique
        /// </summary>
        /// <param name="effect">Effect enum</param>
        EngineEffectTechnique GetTechnique(PostProcessingEffects effect);

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="texture">Texture</param>
        void UpdatePerFrameEmpty(Matrix viewProjection, EngineShaderResourceView texture);
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        void UpdatePerFrameCombine(Matrix viewProjection, EngineShaderResourceView texture1, EngineShaderResourceView texture2);
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="viewportSize">Viewport size</param>
        /// <param name="time">Time</param>
        /// <param name="texture">Texture</param>
        void UpdatePerFrame(Matrix viewProjection, Vector2 viewportSize, float time, EngineShaderResourceView texture);
        /// <summary>
        /// Update effect parameters
        /// </summary>
        /// <typeparam name="T">Type of parameter</typeparam>
        /// <param name="parameters">Parameters</param>
        void UpdatePerEffect<T>(T parameters) where T : IDrawerPostProcessParams;
    }
}
