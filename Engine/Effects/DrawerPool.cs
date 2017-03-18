using SharpDX.Direct3D11;
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
        public static EffectDefaultCPUParticles EffectDefaultCPUParticles { get; private set; }
        /// <summary>
        /// GPU Particles drawing effect
        /// </summary>
        public static EffectDefaultGPUParticles EffectDefaultGPUParticles { get; private set; }

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
        /// Billboards effect
        /// </summary>
        public static EffectShadowBillboard EffectShadowBillboard { get; private set; }
        /// <summary>
        /// Shadows effect
        /// </summary>
        public static EffectShadowBasic EffectShadowBasic { get; private set; }
        /// <summary>
        /// Terrain drawing effect
        /// </summary>
        public static EffectShadowTerrain EffectShadowTerrain { get; private set; }

        /// <summary>
        /// Blur effect
        /// </summary>
        public static EffectPostBlur EffectPostBlur { get; private set; }

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="device">Device</param>
        public static void Initialize(Device device)
        {
            EffectNull = CreateEffect<EffectNull>(device, Resources.ShaderNullFxo, Resources.ShaderNullFx);

            EffectDefaultSprite = CreateEffect<EffectDefaultSprite>(device, Resources.ShaderDefaultSpriteFxo, Resources.ShaderDefaultSpriteFxo);
            EffectDefaultFont = CreateEffect<EffectDefaultFont>(device, Resources.ShaderDefaultFontFxo, Resources.ShaderDefaultFontFxo);
            EffectDefaultCubemap = CreateEffect<EffectDefaultCubemap>(device, Resources.ShaderDefaultCubemapFxo, Resources.ShaderDefaultCubemapFx);
            EffectDefaultBillboard = CreateEffect<EffectDefaultBillboard>(device, Resources.ShaderDefaultBillboardFxo, Resources.ShaderDefaultBillboardFx);
            EffectDefaultClouds = CreateEffect<EffectDefaultClouds>(device, Resources.ShaderDefaultCloudsFxo, Resources.ShaderDefaultCloudsFx);
            EffectDefaultBasic = CreateEffect<EffectDefaultBasic>(device, Resources.ShaderDefaultBasicFxo, Resources.ShaderDefaultBasicFx);
            EffectDefaultTerrain = CreateEffect<EffectDefaultTerrain>(device, Resources.ShaderDefaultTerrainFxo, Resources.ShaderDefaultTerrainFx);
            EffectDefaultSkyScattering = CreateEffect<EffectDefaultSkyScattering>(device, Resources.ShaderDefaultSkyScatteringFxo, Resources.ShaderDefaultSkyScatteringFx);
            EffectDefaultCPUParticles = CreateEffect<EffectDefaultCPUParticles>(device, Resources.ShaderDefaultCPUParticlesFxo, Resources.ShaderDefaultCPUParticlesFx);
            EffectDefaultGPUParticles = CreateEffect<EffectDefaultGPUParticles>(device, Resources.ShaderDefaultGPUParticlesFxo, Resources.ShaderDefaultGPUParticlesFx);

            EffectDeferredComposer = CreateEffect<EffectDeferredComposer>(device, Resources.ShaderDeferredComposerFxo, Resources.ShaderDeferredComposerFx);
            EffectDeferredBasic = CreateEffect<EffectDeferredBasic>(device, Resources.ShaderDeferredBasicFxo, Resources.ShaderDeferredBasicFxo);
            EffectDeferredTerrain = CreateEffect<EffectDeferredTerrain>(device, Resources.ShaderDeferredTerrainFxo, Resources.ShaderDeferredTerrainFx);

            EffectShadowBillboard = CreateEffect<EffectShadowBillboard>(device, Resources.ShaderShadowBillboardFxo, Resources.ShaderShadowBillboardFx);
            EffectShadowBasic = CreateEffect<EffectShadowBasic>(device, Resources.ShaderShadowBasicFxo, Resources.ShaderShadowBasicFx);
            EffectShadowTerrain = CreateEffect<EffectShadowTerrain>(device, Resources.ShaderShadowTerrainFxo, Resources.ShaderShadowTerrainFx);

            EffectPostBlur = CreateEffect<EffectPostBlur>(device, Resources.ShaderPostBlurFxo, Resources.ShaderPostBlurFx);
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void Dispose()
        {
            Helper.Dispose(EffectNull);

            Helper.Dispose(EffectDefaultSprite);
            Helper.Dispose(EffectDefaultFont);
            Helper.Dispose(EffectDefaultCubemap);
            Helper.Dispose(EffectDefaultBillboard);
            Helper.Dispose(EffectDefaultClouds);
            Helper.Dispose(EffectDefaultBasic);
            Helper.Dispose(EffectDefaultTerrain);
            Helper.Dispose(EffectDefaultSkyScattering);
            Helper.Dispose(EffectDefaultCPUParticles);
            Helper.Dispose(EffectDefaultGPUParticles);

            Helper.Dispose(EffectDeferredComposer);
            Helper.Dispose(EffectDeferredBasic);
            Helper.Dispose(EffectDeferredTerrain);

            Helper.Dispose(EffectShadowBillboard);
            Helper.Dispose(EffectShadowBasic);
            Helper.Dispose(EffectShadowTerrain);

            Helper.Dispose(EffectPostBlur);
        }

        /// <summary>
        /// Creates a new effect from resources
        /// </summary>
        /// <typeparam name="T">Effect type</typeparam>
        /// <param name="device">Graphics device</param>
        /// <param name="resFxo">Compiled resource</param>
        /// <param name="resFx">Source code resource</param>
        /// <returns>Returns the new generated effect instance</returns>
        private static T CreateEffect<T>(Device device, byte[] resFxo, byte[] resFx) where T : Drawer
        {
            var res = resFxo != null ? resFxo : resFx;

            return (T)Activator.CreateInstance(typeof(T), device, res, false);
        }

        /// <summary>
        /// Update scene globals
        /// </summary>
        /// <param name="materialPalette">Material palette</param>
        /// <param name="materialPaletteWidth">Material palette width</param>
        public static void UpdateSceneGlobals(
            ShaderResourceView materialPalette, uint materialPaletteWidth,
            ShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            EffectDefaultBillboard.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                GameEnvironment.LODDistanceHigh, GameEnvironment.LODDistanceMedium, GameEnvironment.LODDistanceLow);

            EffectDefaultBasic.UpdateGlobals(
                materialPalette, materialPaletteWidth, 
                animationPalette, animationPaletteWidth, 
                GameEnvironment.LODDistanceHigh, GameEnvironment.LODDistanceMedium, GameEnvironment.LODDistanceLow);
            EffectDefaultTerrain.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                GameEnvironment.LODDistanceHigh, GameEnvironment.LODDistanceMedium, GameEnvironment.LODDistanceLow);

            EffectDeferredBasic.UpdateGlobals(animationPalette, animationPaletteWidth);
            EffectDeferredComposer.UpdateGlobals(
                materialPalette, materialPaletteWidth,
                GameEnvironment.LODDistanceHigh, GameEnvironment.LODDistanceMedium, GameEnvironment.LODDistanceLow);

            EffectShadowBasic.UpdateGlobals(animationPalette, animationPaletteWidth);
        }
    }
}
