using SharpDX.Direct3D11;

namespace Engine.Effects
{
    using Properties;

    /// <summary>
    /// Pool of drawers
    /// </summary>
    static class DrawerPool
    {
        /// <summary>
        /// Basic effect
        /// </summary>
        public static EffectBasic EffectBasic { get; private set; }
        /// <summary>
        /// Billboards effect
        /// </summary>
        public static EffectBillboard EffectBillboard { get; private set; }
        /// <summary>
        /// Cube map effect
        /// </summary>
        public static EffectCubemap EffectCubemap { get; private set; }
        /// <summary>
        /// Font drawing effect
        /// </summary>
        public static EffectFont EffectFont { get; private set; }
        /// <summary>
        /// Instancing effect
        /// </summary>
        public static EffectInstancing EffectInstancing { get; private set; }
        /// <summary>
        /// Particles drawing effect
        /// </summary>
        public static EffectParticles EffectParticles { get; private set; }
        /// <summary>
        /// Shadows effect
        /// </summary>
        public static EffectBasicShadow EffectShadow { get; private set; }
        /// <summary>
        /// Instancing shadows effect
        /// </summary>
        public static EffectInstancingShadow EffectInstancingShadow { get; private set; }

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="device">Device</param>
        public static void Initialize(Device device)
        {
            if (Resources.ShaderBasicFxo != null)
            {
                EffectBasic = new EffectBasic(device, Resources.ShaderBasicFxo, false);
                EffectInstancing = new EffectInstancing(device, Resources.ShaderBasicFxo, false);
            }
            else
            {
                EffectBasic = new EffectBasic(device, Resources.ShaderBasicFx, true);
                EffectInstancing = new EffectInstancing(device, Resources.ShaderBasicFx, true);
            }

            if (Resources.ShaderShadowFxo != null)
            {
                EffectShadow = new EffectBasicShadow(device, Resources.ShaderShadowFxo, false);
                EffectInstancingShadow = new EffectInstancingShadow(device, Resources.ShaderShadowFxo, false);
            }
            else
            {
                EffectShadow = new EffectBasicShadow(device, Resources.ShaderShadowFx, true);
                EffectInstancingShadow = new EffectInstancingShadow(device, Resources.ShaderShadowFx, true);
            }

            if (Resources.ShaderFontFxo != null)
            {
                EffectFont = new EffectFont(device, Resources.ShaderFontFxo, false);
            }
            else
            {
                EffectFont = new EffectFont(device, Resources.ShaderFontFx, true);
            }

            if (Resources.ShaderCubemapFxo != null)
            {
                EffectCubemap = new EffectCubemap(device, Resources.ShaderCubemapFxo, false);
            }
            else
            {
                EffectCubemap = new EffectCubemap(device, Resources.ShaderCubemapFx, true);
            }

            if (Resources.ShaderBillboardFxo != null)
            {
                EffectBillboard = new EffectBillboard(device, Resources.ShaderBillboardFxo, false);
            }
            else
            {
                EffectBillboard = new EffectBillboard(device, Resources.ShaderBillboardFx, true);
            }

            if (Resources.ShaderParticlesFxo != null)
            {
                EffectParticles = new EffectParticles(device, Resources.ShaderParticlesFxo, false);
            }
            else
            {
                EffectParticles = new EffectParticles(device, Resources.ShaderParticlesFx, true);
            }

            
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void Dispose()
        {
            if (EffectBasic != null)
            {
                EffectBasic.Dispose();
                EffectBasic = null;
            }

            if (EffectBillboard != null)
            {
                EffectBillboard.Dispose();
                EffectBillboard = null;
            }

            if (EffectCubemap != null)
            {
                EffectCubemap.Dispose();
                EffectCubemap = null;
            }

            if (EffectFont != null)
            {
                EffectFont.Dispose();
                EffectFont = null;
            }

            if (EffectInstancing != null)
            {
                EffectInstancing.Dispose();
                EffectInstancing = null;
            }

            if (EffectParticles != null)
            {
                EffectParticles.Dispose();
                EffectParticles = null;
            }

            if (EffectShadow != null)
            {
                EffectShadow.Dispose();
                EffectShadow = null;
            }

            if (EffectInstancingShadow != null)
            {
                EffectInstancingShadow.Dispose();
                EffectInstancingShadow = null;
            }
        }
    }
}
