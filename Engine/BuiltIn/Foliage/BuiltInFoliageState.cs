using SharpDX;

namespace Engine.BuiltIn.Foliage
{
    using Engine.Common;

    /// <summary>
    /// Built-in foliage state
    /// </summary>
    public struct BuiltInFoliageState
    {
        /// <summary>
        /// Drawing start radius
        /// </summary>
        public float StartRadius { get; set; }
        /// <summary>
        /// Drawing end radius
        /// </summary>
        public float EndRadius { get; set; }
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor { get; set; }
        /// <summary>
        /// Material index
        /// </summary>
        public uint MaterialIndex { get; set; }
        /// <summary>
        /// Texture count
        /// </summary>
        public uint TextureCount { get; set; }
        /// <summary>
        /// Normal map count
        /// </summary>
        public uint NormalMapCount { get; set; }
        /// <summary>
        /// Random texture
        /// </summary>
        public EngineShaderResourceView RandomTexture { get; set; }
        /// <summary>
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Normal maps
        /// </summary>
        public EngineShaderResourceView NormalMaps { get; set; }
        /// <summary>
        /// Point of view
        /// </summary>
        public Vector3 PointOfView { get; set; }
        /// <summary>
        /// Wind direction
        /// </summary>
        public Vector3 WindDirection { get; set; }
        /// <summary>
        /// Wind strength
        /// </summary>
        public float WindStrength { get; set; }
        /// <summary>
        /// Wind effect
        /// </summary>
        public float WindEffect { get; set; }
        /// <summary>
        /// Foliage instances per position
        /// </summary>
        public BuiltInFoliageInstances Instances { get; set; }
        /// <summary>
        /// Delta added to position for additional instances
        /// </summary>
        public Vector3 Delta { get; set; }
    }
}
