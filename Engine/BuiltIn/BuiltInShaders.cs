using SharpDX;

namespace Engine.BuiltIn
{
    using Engine.BuiltInEffects;
    using Engine.Common;

    /// <summary>
    /// Built-in shaders resource helper
    /// </summary>
    public static class BuiltInShaders
    {
        /// <summary>
        /// Vertex shader global resources
        /// </summary>
        private static ResourcesVSGlobal vsGlobalResources;
        /// <summary>
        /// Vertex shader per-frame resources
        /// </summary>
        private static ResourcesVSPerFrame vsPerFrameResources;
        /// <summary>
        /// Lights pixel shader per-frame resources
        /// </summary>
        private static ResourcesPSPerFrameLit psPerFrameLitResources;
        /// <summary>
        /// No lights pixel shader per-frame resources
        /// </summary>
        private static ResourcesPSPerFrameNoLit psPerFrameNoLitResources;

        /// <summary>
        /// Position color pixel shader
        /// </summary>
        public static PositionColorPs PositionColorPs { get; private set; }
        /// <summary>
        /// Position color vertex shader
        /// </summary>
        public static PositionColorVs PositionColorVs { get; private set; }
        /// <summary>
        /// Position color vertex shader instanced
        /// </summary>
        public static PositionColorVsI PositionColorVsI { get; private set; }
        /// <summary>
        /// Position color skinned vertex shader
        /// </summary>
        public static SkinnedPositionColorVs PositionColorVsSkinned { get; private set; }
        /// <summary>
        /// Position color skinned vertex shader instanced
        /// </summary>
        public static SkinnedPositionColorVsI PositionColorVsSkinnedI { get; private set; }

        /// <summary>
        /// Basic position color drawer
        /// </summary>
        public static BasicPositionColor BasicPositionColor { get; private set; }

        /// <summary>
        /// Position normal color pixel shader
        /// </summary>
        public static PositionNormalColorPs PositionNormalColorPs { get; private set; }
        /// <summary>
        /// Position normal color vertex shader
        /// </summary>
        public static PositionNormalColorVs PositionNormalColorVs { get; private set; }
        /// <summary>
        /// Position normal color vertex shader instanced
        /// </summary>
        public static PositionNormalColorVsI PositionNormalColorVsI { get; private set; }
        /// <summary>
        /// Position normal color skinned vertex shader
        /// </summary>
        public static SkinnedPositionNormalColorVs PositionNormalColorVsSkinned { get; private set; }
        /// <summary>
        /// Position normal color skinned vertex shader instanced
        /// </summary>
        public static SkinnedPositionNormalColorVsI PositionNormalColorVsSkinnedI { get; private set; }

        /// <summary>
        /// Basic position normal color drawer
        /// </summary>
        public static BasicPositionNormalColor BasicPositionNormalColor { get; private set; }

        /// <summary>
        /// Position texture pixel shader
        /// </summary>
        public static PositionTexturePs PositionTexturePs { get; private set; }
        /// <summary>
        /// Position texture vertex shader
        /// </summary>
        public static PositionTextureVs PositionTextureVs { get; private set; }
        /// <summary>
        /// Position texture vertex shader instanced
        /// </summary>
        public static PositionTextureVsI PositionTextureVsI { get; private set; }
        /// <summary>
        /// Position texture skinned vertex shader
        /// </summary>
        public static SkinnedPositionTextureVs PositionTextureVsSkinned { get; private set; }
        /// <summary>
        /// Position texture skinned vertex shader instanced
        /// </summary>
        public static SkinnedPositionTextureVsI PositionTextureVsSkinnedI { get; private set; }

        /// <summary>
        /// Basic position texture drawer
        /// </summary>
        public static BasicPositionTexture BasicPositionTexture { get; private set; }

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="graphics">Device</param>
        public static void Initialize(Graphics graphics)
        {
            vsGlobalResources = new ResourcesVSGlobal(graphics);
            vsPerFrameResources = new ResourcesVSPerFrame(graphics);
            psPerFrameLitResources = new ResourcesPSPerFrameLit(graphics);
            psPerFrameNoLitResources = new ResourcesPSPerFrameNoLit(graphics);

            PositionColorPs = new PositionColorPs(graphics);
            PositionColorVs = new PositionColorVs(graphics);
            PositionColorVsI = new PositionColorVsI(graphics);
            PositionColorVsSkinned = new SkinnedPositionColorVs(graphics);
            PositionColorVsSkinnedI = new SkinnedPositionColorVsI(graphics);

            BasicPositionColor = new BasicPositionColor(graphics, PositionColorVs, PositionColorPs);

            PositionNormalColorPs = new PositionNormalColorPs(graphics);
            PositionNormalColorVs = new PositionNormalColorVs(graphics);
            PositionNormalColorVsI = new PositionNormalColorVsI(graphics);
            PositionNormalColorVsSkinned = new SkinnedPositionNormalColorVs(graphics);
            PositionNormalColorVsSkinnedI = new SkinnedPositionNormalColorVsI(graphics);

            BasicPositionNormalColor = new BasicPositionNormalColor(graphics, PositionNormalColorVs, PositionNormalColorPs);

            PositionTexturePs = new PositionTexturePs(graphics);
            PositionTextureVs = new PositionTextureVs(graphics);
            PositionTextureVsI = new PositionTextureVsI(graphics);
            PositionTextureVsSkinned = new SkinnedPositionTextureVs(graphics);
            PositionTextureVsSkinnedI = new SkinnedPositionTextureVsI(graphics);

            BasicPositionTexture = new BasicPositionTexture(graphics, PositionTextureVs, PositionTexturePs);
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void DisposeResources()
        {
            vsGlobalResources?.Dispose();
            vsGlobalResources = null;
            vsPerFrameResources?.Dispose();
            vsPerFrameResources = null;
            psPerFrameLitResources?.Dispose();
            psPerFrameLitResources = null;
            psPerFrameNoLitResources?.Dispose();
            psPerFrameNoLitResources = null;

            PositionColorPs?.Dispose();
            PositionColorPs = null;
            PositionColorVs?.Dispose();
            PositionColorVs = null;
            PositionColorVsI?.Dispose();
            PositionColorVsI = null;
            PositionColorVsSkinned?.Dispose();
            PositionColorVsSkinned = null;
            PositionColorVsSkinnedI?.Dispose();
            PositionColorVsSkinnedI = null;

            PositionNormalColorPs?.Dispose();
            PositionNormalColorPs = null;
            PositionNormalColorVs?.Dispose();
            PositionNormalColorVs = null;
            PositionNormalColorVsI?.Dispose();
            PositionNormalColorVsI = null;
            PositionNormalColorVsSkinned?.Dispose();
            PositionNormalColorVsSkinned = null;
            PositionNormalColorVsSkinnedI?.Dispose();
            PositionNormalColorVsSkinnedI = null;

            PositionTexturePs?.Dispose();
            PositionTexturePs = null;
            PositionTextureVs?.Dispose();
            PositionTextureVs = null;
            PositionTextureVsI?.Dispose();
            PositionTextureVsI = null;
            PositionTextureVsSkinned?.Dispose();
            PositionTextureVsSkinned = null;
            PositionTextureVsSkinnedI?.Dispose();
            PositionTextureVsSkinnedI = null;
        }

        /// <summary>
        /// Updates global data
        /// </summary>
        /// <param name="materialPalette">Material palette resource view</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        /// <param name="animationPalette">Animation palette resource view</param>
        /// <param name="animationPaletteWidth">Animation palette width</param>
        public static void UpdateGlobals(EngineShaderResourceView materialPalette, uint materialPaletteWidth, EngineShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            vsGlobalResources?.SetCBGlobals(
                materialPalette, materialPaletteWidth,
                animationPalette, animationPaletteWidth);
        }
        /// <summary>
        /// Updates per-frame data
        /// </summary>
        /// <param name="localTransform">Local transform</param>
        /// <param name="context">Draw context</param>
        public static void UpdatePerFrame(Matrix localTransform, DrawContext context)
        {
            vsPerFrameResources?.SetCBPerFrame(localTransform, localTransform * context.ViewProjection);
            psPerFrameLitResources?.SetCBPerFrame(context.EyePosition, context.Lights, context.LevelOfDetail);
            psPerFrameNoLitResources?.SetCBPerFrame(context.EyePosition, context.Lights);
        }

        public static IEngineConstantBuffer GetVSGlobal()
        {
            return vsGlobalResources?.Globals;
        }
        public static EngineShaderResourceView GetMaterialPalette()
        {
            return vsGlobalResources?.MaterialPalette;
        }
        public static EngineShaderResourceView GetAnimationPalette()
        {
            return vsGlobalResources?.AnimationPalette;
        }
        public static IEngineConstantBuffer GetVSPerFrame()
        {
            return vsPerFrameResources?.PerFrame;
        }
        public static IEngineConstantBuffer GetPSPerFrameLit()
        {
            return psPerFrameLitResources?.PerFrame;
        }
        public static IEngineConstantBuffer GetPSPerFrameNoLit()
        {
            return psPerFrameNoLitResources?.PerFrame;
        }
    }
}
