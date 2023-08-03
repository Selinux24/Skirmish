
namespace Engine.Common
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.PostProcess;

    /// <summary>
    /// Post-processing drawer interface
    /// </summary>
    public interface IPostProcessingDrawer
    {
        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        IBuiltInDrawer UpdateEffectCombine(IEngineDeviceContext dc, EngineShaderResourceView texture1, EngineShaderResourceView texture2);
        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">State</param>
        IBuiltInDrawer UpdateEffectParameters(IEngineDeviceContext dc, BuiltInPostProcessState state);
        /// <summary>
        /// Updates the effect parameters
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="sourceTexture">Source texture</param>
        /// <param name="effect">Effect</param>
        IBuiltInDrawer UpdateEffect(IEngineDeviceContext dc, EngineShaderResourceView sourceTexture, BuiltInPostProcessEffects effect);
        /// <summary>
        /// Draws the resulting light composition
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="drawer">Drawer</param>
        void Draw(IEngineDeviceContext dc, IBuiltInDrawer drawer);
        /// <summary>
        /// Updates the internal buffers according to the new render dimension
        /// </summary>
        void Resize();
    }
}