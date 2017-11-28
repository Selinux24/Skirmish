using SharpDX;

namespace Engine
{
    /// <summary>
    /// Water description
    /// </summary>
    public class WaterDescription : SceneObjectDescription
    {
        /// <summary>
        /// Base color
        /// </summary>
        public Color BaseColor = new Color(0.1f, 0.19f, 0.22f, 1.0f);
        /// <summary>
        /// Water color
        /// </summary>
        public Color WaterColor = new Color(0.8f, 0.9f, 0.6f, 1.0f);
        /// <summary>
        /// Wave height
        /// </summary>
        public float WaveHeight = 0.6f;
        /// <summary>
        /// Wave choppy
        /// </summary>
        public float WaveChoppy = 4.0f;
        /// <summary>
        /// Wave speed
        /// </summary>
        public float WaveSpeed = 0.8f;
        /// <summary>
        /// Wave frequency
        /// </summary>
        public float WaveFrequency = 0.16f;
        /// <summary>
        /// Water plane size
        /// </summary>
        public float PlaneSize = 100f;
        /// <summary>
        /// Water plane height
        /// </summary>
        public float PlaneHeight = 0f;

        /// <summary>
        /// Constructor
        /// </summary>
        public WaterDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
