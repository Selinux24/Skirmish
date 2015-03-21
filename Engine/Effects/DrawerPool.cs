using SharpDX.Direct3D11;

namespace Engine.Effects
{
    /// <summary>
    /// Pool of drawers
    /// </summary>
    static class DrawerPool
    {
        /// <summary>
        /// Device
        /// </summary>
        private static Device device;

        /// <summary>
        /// Basic effect
        /// </summary>
        private static EffectBasic effectBasic = null;
        /// <summary>
        /// Billboards effect
        /// </summary>
        private static EffectBillboard effectBillboard = null;
        /// <summary>
        /// Cube map effect
        /// </summary>
        private static EffectCubemap effectCubemap = null;
        /// <summary>
        /// Font drawing effect
        /// </summary>
        private static EffectFont effectFont = null;
        /// <summary>
        /// Instancing effect
        /// </summary>
        private static EffectInstancing effectInstancing = null;
        /// <summary>
        /// Particles drawing effect
        /// </summary>
        private static EffectParticles effectParticles = null;
        /// <summary>
        /// Shadows effect
        /// </summary>
        private static EffectShadow effectShadow = null;

        /// <summary>
        /// Gets basic effect
        /// </summary>
        /// <remarks>Creates it if not instantiated yet</remarks>
        public static EffectBasic EffectBasic
        {
            get
            {
                if (effectBasic == null)
                {
                    effectBasic = new EffectBasic(device);
                }

                return effectBasic;
            }
        }
        /// <summary>
        /// Gets billboards effect
        /// </summary>
        /// <remarks>Creates it if not instantiated yet</remarks>
        public static EffectBillboard EffectBillboard
        {
            get
            {
                if (effectBillboard == null)
                {
                    effectBillboard = new EffectBillboard(device);
                }

                return effectBillboard;
            }
        }
        /// <summary>
        /// Gets cube map effect
        /// </summary>
        /// <remarks>Creates it if not instantiated yet</remarks>
        public static EffectCubemap EffectCubemap
        {
            get
            {
                if (effectCubemap == null)
                {
                    effectCubemap = new EffectCubemap(device);
                }

                return effectCubemap;
            }
        }
        /// <summary>
        /// Gets font drawing effect
        /// </summary>
        /// <remarks>Creates it if not instantiated yet</remarks>
        public static EffectFont EffectFont
        {
            get
            {
                if (effectFont == null)
                {
                    effectFont = new EffectFont(device);
                }

                return effectFont;
            }
        }
        /// <summary>
        /// Gets instancing effect
        /// </summary>
        /// <remarks>Creates it if not instantiated yet</remarks>
        public static EffectInstancing EffectInstancing
        {
            get
            {
                if (effectInstancing == null)
                {
                    effectInstancing = new EffectInstancing(device);
                }

                return effectInstancing;
            }
        }
        /// <summary>
        /// Gets particles drawing effect
        /// </summary>
        /// <remarks>Creates it if not instantiated yet</remarks>
        public static EffectParticles EffectParticles
        {
            get
            {
                if (effectParticles == null)
                {
                    effectParticles = new EffectParticles(device);
                }

                return effectParticles;
            }
        }
        /// <summary>
        /// Gets shadow effect
        /// </summary>
        /// <remarks>Creates it if not instantiated yet</remarks>
        public static EffectShadow EffectShadow
        {
            get
            {
                if (effectShadow == null)
                {
                    effectShadow = new EffectShadow(device);
                }

                return effectShadow;
            }
        }

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="device">Device</param>
        public static void Initialize(Device device)
        {
            DrawerPool.device = device;
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void Dispose()
        {
            if (effectBasic != null)
            {
                effectBasic.Dispose();
                effectBasic = null;
            }

            if (effectBillboard != null)
            {
                effectBillboard.Dispose();
                effectBillboard = null;
            }

            if (effectCubemap != null)
            {
                effectCubemap.Dispose();
                effectCubemap = null;
            }

            if (effectFont != null)
            {
                effectFont.Dispose();
                effectFont = null;
            }

            if (effectInstancing != null)
            {
                effectInstancing.Dispose();
                effectInstancing = null;
            }

            if (effectParticles != null)
            {
                effectParticles.Dispose();
                effectParticles = null;
            }

            if (effectShadow != null)
            {
                effectShadow.Dispose();
                effectShadow = null;
            }
        }
    }
}
