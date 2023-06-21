
using Engine.Content;

namespace Engine
{
    /// <summary>
    /// Spot light state
    /// </summary>
    public class SceneLightSpotState : SceneLightState, IGameState
    {
        /// <summary>
        /// Initial transform
        /// </summary>
        public Matrix4X4 InitialTransform { get; set; }
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
        /// Ligth direction
        /// </summary>
        public Position3 Direction { get; set; }
        /// <summary>
        /// Cone angle in degrees
        /// </summary>
        public float Angle { get; set; }
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity { get; set; }
        /// <summary>
        /// Shadow map count
        /// </summary>
        public uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix4X4[] FromLightVP { get; set; }
    }
}
