using Engine.Mathematics;

namespace Engine
{
    /// <summary>
    /// Hemispheric lights state
    /// </summary>
    public class SceneLightHemisphericState : SceneLightState, IGameState
    {
        /// <summary>
        /// Ambient down color
        /// </summary>
        public Color3 AmbientDown { get; set; }
        /// <summary>
        /// Ambient up color
        /// </summary>
        public Color3 AmbientUp { get; set; }
    }
}
