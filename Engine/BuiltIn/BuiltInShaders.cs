using SharpDX;

namespace Engine.BuiltIn
{
    using Engine.BuiltInEffects;
    using Engine.Common;

    /// <summary>
    /// Built-in shaders resource helper
    /// </summary>
    internal static partial class BuiltInShaders
    {
        /// <summary>
        /// Vertex shader global constant buffer
        /// </summary>
        private static EngineConstantBuffer<VSGlobal> vsGlobalConstantBuffer;
        /// <summary>
        /// Material palette resource view
        /// </summary>
        private static EngineShaderResourceView vsGlobalMaterialPalette;
        /// <summary>
        /// Animation palette resource view
        /// </summary>
        private static EngineShaderResourceView vsGlobalAnimationPalette;

        /// <summary>
        /// Vertex shader per-frame constant buffer
        /// </summary>
        private static EngineConstantBuffer<VSPerFrame> vsPerFrameConstantBuffer;

        /// <summary>
        /// No lights pixel shader per-frame constant buffer
        /// </summary>
        private static EngineConstantBuffer<PSPerFrameNoLit> psPerFrameNoLitConstantBuffer;

        /// <summary>
        /// Lights pixel shader per-frame constant buffer
        /// </summary>
        private static EngineConstantBuffer<PSPerFrameLit> psPerFrameLitConstantBuffer;
        /// <summary>
        /// Directional shadow map resource view
        /// </summary>
        private static EngineShaderResourceView psPerFrameLitShadowMapDir;
        /// <summary>
        /// Spot shadow map resource view
        /// </summary>
        private static EngineShaderResourceView psPerFrameLitShadowMapSpot;
        /// <summary>
        /// Point shadow map resource view
        /// </summary>
        private static EngineShaderResourceView psPerFrameLitShadowMapPoint;

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
        /// Skinned position color drawer
        /// </summary>
        public static SkinnedPositionColor SkinnedPositionColor { get; private set; }

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
        /// Skinned position normal color drawer
        /// </summary>
        public static SkinnedPositionNormalColor SkinnedPositionNormalColor { get; private set; }

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
        /// Skinned position texture drawer
        /// </summary>
        public static SkinnedPositionTexture SkinnedPositionTexture { get; private set; }

        /// <summary>
        /// Position normal texture pixel shader
        /// </summary>
        public static PositionNormalTexturePs PositionNormalTexturePs { get; private set; }
        /// <summary>
        /// Position normal texture vertex shader
        /// </summary>
        public static PositionNormalTextureVs PositionNormalTextureVs { get; private set; }
        /// <summary>
        /// Position normal texture vertex shader instanced
        /// </summary>
        public static PositionNormalTextureVsI PositionNormalTextureVsI { get; private set; }
        /// <summary>
        /// Position normal texture skinned vertex shader
        /// </summary>
        public static SkinnedPositionNormalTextureVs PositionNormalTextureVsSkinned { get; private set; }
        /// <summary>
        /// Position normal texture skinned vertex shader instanced
        /// </summary>
        public static SkinnedPositionNormalTextureVsI PositionNormalTextureVsSkinnedI { get; private set; }

        /// <summary>
        /// Basic position normal texture drawer
        /// </summary>
        public static BasicPositionNormalTexture BasicPositionNormalTexture { get; private set; }
        /// <summary>
        /// Skinned position normal texture drawer
        /// </summary>
        public static SkinnedPositionNormalTexture SkinnedPositionNormalTexture { get; private set; }

        /// <summary>
        /// Position normal texture tangent pixel shader
        /// </summary>
        public static PositionNormalTextureTangentPs PositionNormalTextureTangentPs { get; private set; }
        /// <summary>
        /// Position normal texture tangent vertex shader
        /// </summary>
        public static PositionNormalTextureTangentVs PositionNormalTextureTangentVs { get; private set; }
        /// <summary>
        /// Position normal texture tangent vertex shader instanced
        /// </summary>
        public static PositionNormalTextureTangentVsI PositionNormalTextureTangentVsI { get; private set; }
        /// <summary>
        /// Position normal texture tangent skinned vertex shader
        /// </summary>
        public static SkinnedPositionNormalTextureTangentVs PositionNormalTextureTangentVsSkinned { get; private set; }
        /// <summary>
        /// Position normal texture tangent skinned vertex shader instanced
        /// </summary>
        public static SkinnedPositionNormalTextureTangentVsI PositionNormalTextureTangentVsSkinnedI { get; private set; }

        /// <summary>
        /// Basic position normal texture tangent drawer
        /// </summary>
        public static BasicPositionNormalTextureTangent BasicPositionNormalTextureTangent { get; private set; }
        /// <summary>
        /// Skinned position normal texture tangent drawer
        /// </summary>
        public static SkinnedPositionNormalTextureTangent SkinnedPositionNormalTextureTangent { get; private set; }

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="graphics">Device</param>
        public static void Initialize(Graphics graphics)
        {
            vsGlobalConstantBuffer = new EngineConstantBuffer<VSGlobal>(graphics, nameof(BuiltInShaders) + "." + nameof(VSGlobal));
            vsPerFrameConstantBuffer = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(BuiltInShaders) + "." + nameof(VSPerFrame));
            psPerFrameLitConstantBuffer = new EngineConstantBuffer<PSPerFrameLit>(graphics, nameof(BuiltInShaders) + "." + nameof(PSPerFrameLit));
            psPerFrameNoLitConstantBuffer = new EngineConstantBuffer<PSPerFrameNoLit>(graphics, nameof(BuiltInShaders) + "." + nameof(PSPerFrameNoLit));

            PositionColorPs = new PositionColorPs(graphics);
            PositionColorVs = new PositionColorVs(graphics);
            PositionColorVsI = new PositionColorVsI(graphics);
            PositionColorVsSkinned = new SkinnedPositionColorVs(graphics);
            PositionColorVsSkinnedI = new SkinnedPositionColorVsI(graphics);

            BasicPositionColor = new BasicPositionColor(graphics, PositionColorVs, PositionColorPs);
            SkinnedPositionColor = new SkinnedPositionColor(graphics, PositionColorVsSkinned, PositionColorPs);

            PositionNormalColorPs = new PositionNormalColorPs(graphics);
            PositionNormalColorVs = new PositionNormalColorVs(graphics);
            PositionNormalColorVsI = new PositionNormalColorVsI(graphics);
            PositionNormalColorVsSkinned = new SkinnedPositionNormalColorVs(graphics);
            PositionNormalColorVsSkinnedI = new SkinnedPositionNormalColorVsI(graphics);

            BasicPositionNormalColor = new BasicPositionNormalColor(graphics, PositionNormalColorVs, PositionNormalColorPs);
            SkinnedPositionNormalColor = new SkinnedPositionNormalColor(graphics, PositionNormalColorVsSkinned, PositionNormalColorPs);

            PositionTexturePs = new PositionTexturePs(graphics);
            PositionTextureVs = new PositionTextureVs(graphics);
            PositionTextureVsI = new PositionTextureVsI(graphics);
            PositionTextureVsSkinned = new SkinnedPositionTextureVs(graphics);
            PositionTextureVsSkinnedI = new SkinnedPositionTextureVsI(graphics);

            BasicPositionTexture = new BasicPositionTexture(graphics, PositionTextureVs, PositionTexturePs);
            SkinnedPositionTexture = new SkinnedPositionTexture(graphics, PositionTextureVsSkinned, PositionTexturePs);

            PositionNormalTexturePs = new PositionNormalTexturePs(graphics);
            PositionNormalTextureVs = new PositionNormalTextureVs(graphics);
            PositionNormalTextureVsI = new PositionNormalTextureVsI(graphics);
            PositionNormalTextureVsSkinned = new SkinnedPositionNormalTextureVs(graphics);
            PositionNormalTextureVsSkinnedI = new SkinnedPositionNormalTextureVsI(graphics);

            BasicPositionNormalTexture = new BasicPositionNormalTexture(graphics, PositionNormalTextureVs, PositionNormalTexturePs);
            SkinnedPositionNormalTexture = new SkinnedPositionNormalTexture(graphics, PositionNormalTextureVsSkinned, PositionNormalTexturePs);

            PositionNormalTextureTangentPs = new PositionNormalTextureTangentPs(graphics);
            PositionNormalTextureTangentVs = new PositionNormalTextureTangentVs(graphics);
            PositionNormalTextureTangentVsI = new PositionNormalTextureTangentVsI(graphics);
            PositionNormalTextureTangentVsSkinned = new SkinnedPositionNormalTextureTangentVs(graphics);
            PositionNormalTextureTangentVsSkinnedI = new SkinnedPositionNormalTextureTangentVsI(graphics);

            BasicPositionNormalTextureTangent = new BasicPositionNormalTextureTangent(graphics, PositionNormalTextureTangentVs, PositionNormalTextureTangentPs);
            SkinnedPositionNormalTextureTangent = new SkinnedPositionNormalTextureTangent(graphics, PositionNormalTextureTangentVsSkinned, PositionNormalTextureTangentPs);
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void DisposeResources()
        {
            vsGlobalConstantBuffer?.Dispose();
            vsGlobalConstantBuffer = null;
            vsPerFrameConstantBuffer?.Dispose();
            vsPerFrameConstantBuffer = null;
            psPerFrameLitConstantBuffer?.Dispose();
            psPerFrameLitConstantBuffer = null;
            psPerFrameNoLitConstantBuffer?.Dispose();
            psPerFrameNoLitConstantBuffer = null;

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

            PositionNormalTexturePs?.Dispose();
            PositionNormalTexturePs = null;
            PositionNormalTextureVs?.Dispose();
            PositionNormalTextureVs = null;
            PositionNormalTextureVsI?.Dispose();
            PositionNormalTextureVsI = null;
            PositionNormalTextureVsSkinned?.Dispose();
            PositionNormalTextureVsSkinned = null;
            PositionNormalTextureVsSkinnedI?.Dispose();
            PositionNormalTextureVsSkinnedI = null;

            PositionNormalTextureTangentPs?.Dispose();
            PositionNormalTextureTangentPs = null;
            PositionNormalTextureTangentVs?.Dispose();
            PositionNormalTextureTangentVs = null;
            PositionNormalTextureTangentVsI?.Dispose();
            PositionNormalTextureTangentVsI = null;
            PositionNormalTextureTangentVsSkinned?.Dispose();
            PositionNormalTextureTangentVsSkinned = null;
            PositionNormalTextureTangentVsSkinnedI?.Dispose();
            PositionNormalTextureTangentVsSkinnedI = null;
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
            vsGlobalMaterialPalette = materialPalette;
            vsGlobalAnimationPalette = animationPalette;

            vsGlobalConstantBuffer?.WriteData(VSGlobal.Build(materialPaletteWidth, animationPaletteWidth));
        }
        /// <summary>
        /// Updates per-frame data
        /// </summary>
        /// <param name="localTransform">Local transform</param>
        /// <param name="context">Draw context</param>
        public static void UpdatePerFrame(Matrix localTransform, DrawContextShadows context)
        {
            if (context == null)
            {
                return;
            }

            vsPerFrameConstantBuffer?.WriteData(VSPerFrame.Build(localTransform, context));
        }
        /// <summary>
        /// Updates per-frame data
        /// </summary>
        /// <param name="localTransform">Local transform</param>
        /// <param name="context">Draw context</param>
        public static void UpdatePerFrame(Matrix localTransform, DrawContext context)
        {
            if (context == null)
            {
                return;
            }

            vsPerFrameConstantBuffer?.WriteData(VSPerFrame.Build(localTransform, context));

            psPerFrameNoLitConstantBuffer?.WriteData(PSPerFrameNoLit.Build(context));

            psPerFrameLitConstantBuffer?.WriteData(PSPerFrameLit.Build(context));
            psPerFrameLitShadowMapDir = context.ShadowMapDirectional?.Texture;
            psPerFrameLitShadowMapSpot = context.ShadowMapSpot?.Texture;
            psPerFrameLitShadowMapPoint = context.ShadowMapPoint?.Texture;
        }

        public static IEngineConstantBuffer GetVSGlobal()
        {
            return vsGlobalConstantBuffer;
        }
        public static EngineShaderResourceView GetMaterialPalette()
        {
            return vsGlobalMaterialPalette;
        }
        public static EngineShaderResourceView GetAnimationPalette()
        {
            return vsGlobalAnimationPalette;
        }
        public static IEngineConstantBuffer GetVSPerFrame()
        {
            return vsPerFrameConstantBuffer;
        }
        public static IEngineConstantBuffer GetPSPerFrameNoLit()
        {
            return psPerFrameNoLitConstantBuffer;
        }
        public static IEngineConstantBuffer GetPSPerFrameLit()
        {
            return psPerFrameLitConstantBuffer;
        }
        public static EngineShaderResourceView GetPSPerFrameLitShadowMapDir()
        {
            return psPerFrameLitShadowMapDir;
        }
        public static EngineShaderResourceView GetPSPerFrameLitShadowMapSpot()
        {
            return psPerFrameLitShadowMapSpot;
        }
        public static EngineShaderResourceView GetPSPerFrameLitShadowMapPoint()
        {
            return psPerFrameLitShadowMapPoint;
        }
    }
}
