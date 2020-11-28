using System;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Properties;

    /// <summary>
    /// Pool of drawers
    /// </summary>
    static class DrawerPool
    {
        /// <summary>
        /// Null effect
        /// </summary>
        public static EffectNull EffectNull { get; private set; }

        /// <summary>
        /// Sprite effect
        /// </summary>
        public static EffectDefaultSprite EffectDefaultSprite { get; private set; }
        /// <summary>
        /// Font drawing effect
        /// </summary>
        public static EffectDefaultFont EffectDefaultFont { get; private set; }
        /// <summary>
        /// Cube map effect
        /// </summary>
        public static EffectDefaultCubemap EffectDefaultCubemap { get; private set; }
        /// <summary>
        /// Billboards effect
        /// </summary>
        public static EffectDefaultBillboard EffectDefaultBillboard { get; private set; }
        /// <summary>
        /// Foliage effect
        /// </summary>
        public static EffectDefaultFoliage EffectDefaultFoliage { get; private set; }
        /// <summary>
        /// Clouds effect
        /// </summary>
        public static EffectDefaultClouds EffectDefaultClouds { get; private set; }
        /// <summary>
        /// Basic effect
        /// </summary>
        public static EffectDefaultBasic EffectDefaultBasic { get; private set; }
        /// <summary>
        /// Terrain drawing effect
        /// </summary>
        public static EffectDefaultTerrain EffectDefaultTerrain { get; private set; }
        /// <summary>
        /// Sky scattering effect
        /// </summary>
        public static EffectDefaultSkyScattering EffectDefaultSkyScattering { get; private set; }
        /// <summary>
        /// CPU Particles drawing effect
        /// </summary>
        public static EffectDefaultCpuParticles EffectDefaultCPUParticles { get; private set; }
        /// <summary>
        /// GPU Particles drawing effect
        /// </summary>
        public static EffectDefaultGpuParticles EffectDefaultGPUParticles { get; private set; }
        /// <summary>
        /// Water drawing effect
        /// </summary>
        public static EffectDefaultWater EffectDefaultWater { get; private set; }

        /// <summary>
        /// Deferred lightning effect
        /// </summary>
        public static EffectDeferredComposer EffectDeferredComposer { get; private set; }
        /// <summary>
        /// Geometry Buffer effect
        /// </summary>
        public static EffectDeferredBasic EffectDeferredBasic { get; private set; }
        /// <summary>
        /// Terrain drawing effect
        /// </summary>
        public static EffectDeferredTerrain EffectDeferredTerrain { get; private set; }

        /// <summary>
        /// Billboard shadows effect
        /// </summary>
        public static EffectShadowBillboard EffectShadowBillboard { get; private set; }
        /// <summary>
        /// Foliage shadows effect
        /// </summary>
        public static EffectShadowFoliage EffectShadowFoliage { get; private set; }
        /// <summary>
        /// Shadows effect
        /// </summary>
        public static EffectShadowBasic EffectShadowBasic { get; private set; }
        /// <summary>
        /// Terrain drawing effect
        /// </summary>
        public static EffectShadowTerrain EffectShadowTerrain { get; private set; }
        /// <summary>
        /// Point shadows effect
        /// </summary>
        public static EffectShadowPoint EffectShadowPoint { get; private set; }
        /// <summary>
        /// Cascade shadows effect
        /// </summary>
        public static EffectShadowCascade EffectShadowCascade { get; private set; }

        /// <summary>
        /// Blur effect
        /// </summary>
        public static EffectPostBlur EffectPostBlur { get; private set; }

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="graphics">Device</param>
        public static void Initialize(Graphics graphics)
        {
            EffectNull = CreateEffect<EffectNull>(graphics, Resources.ShaderNullFxo, Resources.ShaderNullFx);

            EffectDefaultSprite = CreateEffect<EffectDefaultSprite>(graphics, Resources.ShaderDefaultSpriteFxo, Resources.ShaderDefaultSpriteFxo);
            EffectDefaultFont = CreateEffect<EffectDefaultFont>(graphics, Resources.ShaderDefaultFontFxo, Resources.ShaderDefaultFontFxo);
            EffectDefaultCubemap = CreateEffect<EffectDefaultCubemap>(graphics, Resources.ShaderDefaultCubemapFxo, Resources.ShaderDefaultCubemapFx);
            EffectDefaultBillboard = CreateEffect<EffectDefaultBillboard>(graphics, Resources.ShaderDefaultBillboardFxo, Resources.ShaderDefaultBillboardFx);
            EffectDefaultFoliage = CreateEffect<EffectDefaultFoliage>(graphics, Resources.ShaderDefaultFoliageFxo, Resources.ShaderDefaultFoliageFx);
            EffectDefaultClouds = CreateEffect<EffectDefaultClouds>(graphics, Resources.ShaderDefaultCloudsFxo, Resources.ShaderDefaultCloudsFx);
            EffectDefaultBasic = CreateEffect<EffectDefaultBasic>(graphics, Resources.ShaderDefaultBasicFxo, Resources.ShaderDefaultBasicFx);
            EffectDefaultTerrain = CreateEffect<EffectDefaultTerrain>(graphics, Resources.ShaderDefaultTerrainFxo, Resources.ShaderDefaultTerrainFx);
            EffectDefaultSkyScattering = CreateEffect<EffectDefaultSkyScattering>(graphics, Resources.ShaderDefaultSkyScatteringFxo, Resources.ShaderDefaultSkyScatteringFx);
            EffectDefaultCPUParticles = CreateEffect<EffectDefaultCpuParticles>(graphics, Resources.ShaderDefaultCPUParticlesFxo, Resources.ShaderDefaultCPUParticlesFx);
            EffectDefaultGPUParticles = CreateEffect<EffectDefaultGpuParticles>(graphics, Resources.ShaderDefaultGPUParticlesFxo, Resources.ShaderDefaultGPUParticlesFx);
            EffectDefaultWater = CreateEffect<EffectDefaultWater>(graphics, Resources.ShaderDefaultWaterFxo, Resources.ShaderDefaultWaterFx);

            EffectDeferredComposer = CreateEffect<EffectDeferredComposer>(graphics, Resources.ShaderDeferredComposerFxo, Resources.ShaderDeferredComposerFx);
            EffectDeferredBasic = CreateEffect<EffectDeferredBasic>(graphics, Resources.ShaderDeferredBasicFxo, Resources.ShaderDeferredBasicFxo);
            EffectDeferredTerrain = CreateEffect<EffectDeferredTerrain>(graphics, Resources.ShaderDeferredTerrainFxo, Resources.ShaderDeferredTerrainFx);

            EffectShadowBillboard = CreateEffect<EffectShadowBillboard>(graphics, Resources.ShaderShadowBillboardFxo, Resources.ShaderShadowBillboardFx);
            EffectShadowFoliage = CreateEffect<EffectShadowFoliage>(graphics, Resources.ShaderShadowFoliageFxo, Resources.ShaderShadowFoliageFx);
            EffectShadowBasic = CreateEffect<EffectShadowBasic>(graphics, Resources.ShaderShadowBasicFxo, Resources.ShaderShadowBasicFx);
            EffectShadowTerrain = CreateEffect<EffectShadowTerrain>(graphics, Resources.ShaderShadowTerrainFxo, Resources.ShaderShadowTerrainFx);
            EffectShadowPoint = CreateEffect<EffectShadowPoint>(graphics, Resources.ShaderShadowPointFxo, Resources.ShaderShadowPointFx);
            EffectShadowCascade = CreateEffect<EffectShadowCascade>(graphics, Resources.ShaderShadowCascadeFxo, Resources.ShaderShadowCascadeFx);

            EffectPostBlur = CreateEffect<EffectPostBlur>(graphics, Resources.ShaderPostBlurFxo, Resources.ShaderPostBlurFx);
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void DisposeResources()
        {
            EffectNull?.Dispose();
            EffectNull = null;

            EffectDefaultSprite?.Dispose();
            EffectDefaultSprite = null;
            EffectDefaultFont?.Dispose();
            EffectDefaultFont = null;
            EffectDefaultCubemap?.Dispose();
            EffectDefaultCubemap = null;
            EffectDefaultBillboard?.Dispose();
            EffectDefaultBillboard = null;
            EffectDefaultFoliage?.Dispose();
            EffectDefaultFoliage = null;
            EffectDefaultClouds?.Dispose();
            EffectDefaultClouds = null;
            EffectDefaultBasic?.Dispose();
            EffectDefaultBasic = null;
            EffectDefaultTerrain?.Dispose();
            EffectDefaultTerrain = null;
            EffectDefaultSkyScattering?.Dispose();
            EffectDefaultSkyScattering = null;
            EffectDefaultCPUParticles?.Dispose();
            EffectDefaultCPUParticles = null;
            EffectDefaultGPUParticles?.Dispose();
            EffectDefaultGPUParticles = null;
            EffectDefaultWater?.Dispose();
            EffectDefaultWater = null;

            EffectDeferredComposer?.Dispose();
            EffectDeferredComposer = null;
            EffectDeferredBasic?.Dispose();
            EffectDeferredBasic = null;
            EffectDeferredTerrain?.Dispose();
            EffectDeferredTerrain = null;

            EffectShadowBillboard?.Dispose();
            EffectShadowBillboard = null;
            EffectShadowFoliage?.Dispose();
            EffectShadowFoliage = null;
            EffectShadowBasic?.Dispose();
            EffectShadowBasic = null;
            EffectShadowTerrain?.Dispose();
            EffectShadowTerrain = null;
            EffectShadowPoint?.Dispose();
            EffectShadowPoint = null;
            EffectShadowCascade?.Dispose();
            EffectShadowCascade = null;

            EffectPostBlur?.Dispose();
            EffectPostBlur = null;
        }

        /// <summary>
        /// Creates a new effect from resources
        /// </summary>
        /// <typeparam name="T">Effect type</typeparam>
        /// <param name="graphics">Graphics device</param>
        /// <param name="resFxo">Compiled resource</param>
        /// <param name="resFx">Source code resource</param>
        /// <returns>Returns the new generated effect instance</returns>
        private static T CreateEffect<T>(Graphics graphics, byte[] resFxo, byte[] resFx) where T : Drawer
        {
            var res = resFxo ?? resFx;

            var effect = (T)Activator.CreateInstance(typeof(T), graphics, res, false);

            effect.Optimize();

            return effect;
        }

        /// <summary>
        /// Update scene globals
        /// </summary>
        /// <param name="environment">Game environment</param>
        /// <param name="materialPalette">Material palette</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        public static void UpdateSceneGlobals(
            GameEnvironment environment,
            EngineShaderResourceView materialPalette, uint materialPaletteWidth,
            EngineShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            EffectDefaultBillboard.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                environment.LODDistanceHigh, environment.LODDistanceMedium, environment.LODDistanceLow);

            EffectDefaultFoliage.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                environment.LODDistanceHigh, environment.LODDistanceMedium, environment.LODDistanceLow);

            EffectDefaultBasic.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                animationPalette, animationPaletteWidth,
                environment.LODDistanceHigh, environment.LODDistanceMedium, environment.LODDistanceLow);
            EffectDefaultTerrain.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                environment.LODDistanceHigh, environment.LODDistanceMedium, environment.LODDistanceLow);

            EffectDeferredBasic.UpdateGlobals(animationPalette, animationPaletteWidth);
            EffectDeferredComposer.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                environment.LODDistanceHigh, environment.LODDistanceMedium, environment.LODDistanceLow);

            EffectShadowCascade.UpdateGlobals(animationPalette, animationPaletteWidth);

            EffectShadowBasic.UpdateGlobals(animationPalette, animationPaletteWidth);

            EffectShadowPoint.UpdateGlobals(animationPalette, animationPaletteWidth);
        }
    }
}
