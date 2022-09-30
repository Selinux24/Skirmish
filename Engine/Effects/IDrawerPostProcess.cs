
namespace Engine.Effects
{
    using Engine.BuiltIn;
    using Engine.Common;
    using Engine.PostProcessing;

    /// <summary>
    /// Post-processing drawer interface
    /// </summary>
    public interface IDrawerPostProcess
    {
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        IBuiltInDrawer UpdatePerFrameCombine(EngineShaderResourceView texture1, EngineShaderResourceView texture2);
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="texture">Texture</param>
        IBuiltInDrawer UpdatePerFrame(EngineShaderResourceView texture);
        /// <summary>
        /// Update effect parameters
        /// </summary>
        /// <typeparam name="T">Type of parameter</typeparam>
        /// <param name="parameters">Parameters</param>
        void UpdatePerEffect<T>(T parameters) where T : IDrawerPostProcessParams;
    }
}
