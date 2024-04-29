using Engine.BuiltIn.Foliage;
using Engine.Common;
using SharpDX;
using System;

namespace Engine
{
    /// <summary>
    /// Foliage map channel
    /// </summary>
    class FoliageMapChannel : IDisposable
    {
        /// <summary>
        /// Channel index
        /// </summary>
        public int Index;
        /// <summary>
        /// Random seed
        /// </summary>
        public int Seed;
        /// <summary>
        /// Point density
        /// </summary>
        public float Density;
        /// <summary>
        /// Billboard minimum size
        /// </summary>
        public Vector2 MinSize;
        /// <summary>
        /// Billboard maximum size
        /// </summary>
        public Vector2 MaxSize;
        /// <summary>
        /// Delta
        /// </summary>
        public Vector3 Delta;
        /// <summary>
        /// Foliage textures
        /// </summary>
        public EngineShaderResourceView Textures;
        /// <summary>
        /// Foliage normal maps
        /// </summary>
        public EngineShaderResourceView NormalMaps;
        /// <summary>
        /// Foliage texture count
        /// </summary>
        public uint TextureCount;
        /// <summary>
        /// Foliage normal map count
        /// </summary>
        public uint NormalMapCount;
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor;
        /// <summary>
        /// Foliage start radius
        /// </summary>
        public float StartRadius;
        /// <summary>
        /// Foliage end radius
        /// </summary>
        public float EndRadius;
        /// <summary>
        /// Wind effect
        /// </summary>
        public float WindEffect;
        /// <summary>
        /// Geometry output count
        /// </summary>
        public BuiltInFoliageInstances Count;

        /// <summary>
        /// Destructor
        /// </summary>
        ~FoliageMapChannel()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Resource disposal
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Textures?.Dispose();
            Textures = null;

            NormalMaps?.Dispose();
            NormalMaps = null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Channel_{Index}; Seed: {Seed}; StartRadius: {StartRadius}; EndRadius: {EndRadius}; Instances: {Count}";
        }
    }
}
