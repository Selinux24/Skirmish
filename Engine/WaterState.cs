using SharpDX;

namespace Engine
{
    /// <summary>
    /// Water state
    /// </summary>
    public class WaterState
    {
        /// <summary>
        /// Base color
        /// </summary>
        public Color3 BaseColor { get; set; }
        /// <summary>
        /// Water color
        /// </summary>
        public Color4 WaterColor { get; set; }
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
        /// Shader steps
        /// </summary>
        public uint Steps { get; set; }
        /// <summary>
        /// Geometry iterations
        /// </summary>
        public uint GeometryIterations { get; set; }
        /// <summary>
        /// Color iterations
        /// </summary>
        public uint ColorIterations { get; set; }
    }
}
