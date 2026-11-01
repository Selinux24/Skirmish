using Engine.Content;

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
        public CntColor AmbientDown { get; set; }
        /// <summary>
        /// Ambient up color
        /// </summary>
        public CntColor AmbientUp { get; set; }
    }
}
