using SharpDX;

namespace Engine.Effects
{
    /// <summary>
    /// Effect water state
    /// </summary>
    public struct EffectWaterState
    {
        /// <summary>
        /// Base color
        /// </summary>
        public Color BaseColor { get; set; }
        /// <summary>
        /// Water color
        /// </summary>
        public Color WaterColor { get; set; }
        /// <summary>
        /// Wave heigth
        /// </summary>
        public float WaveHeight { get; set; }
        /// <summary>
        /// Wave choppy
        /// </summary>
        public float WaveChoppy { get; set; }
        /// <summary>
        /// Wave speed
        /// </summary>
        public float WaveSpeed { get; set; }
        /// <summary>
        /// Wave frequency
        /// </summary>
        public float WaveFrequency { get; set; }
        /// <summary>
        /// Total time
        /// </summary>
        public float TotalTime { get; set; }
        /// <summary>
        /// Shader steps
        /// </summary>
        public int Steps { get; set; }
        /// <summary>
        /// Geometry iterations
        /// </summary>
        public int GeometryIterations { get; set; }
        /// <summary>
        /// Color iterations
        /// </summary>
        public int ColorIterations { get; set; }
    }
}
