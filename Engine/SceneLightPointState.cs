
namespace Engine
{
    /// <summary>
    /// Scene light point state
    /// </summary>
    public class SceneLightPointState : SceneLightState, IGameState
    {
        /// <summary>
        /// Initial transform
        /// </summary>
        public Matrix4x4 InitialTransform { get; set; }
        /// <summary>
        /// Initial radius
        /// </summary>
        public float InitialRadius { get; set; }
        /// <summary>
        /// Initial intensity
        /// </summary>
        public float InitialIntensity { get; set; }

        /// <summary>
        /// Ligth position
        /// </summary>
        public Position3 Position { get; set; }
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity { get; set; }
    }
}
