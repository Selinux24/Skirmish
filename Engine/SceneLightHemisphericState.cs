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
        public ColorRgba AmbientDown { get; set; }
        /// <summary>
        /// Ambient up color
        /// </summary>
        public ColorRgba AmbientUp { get; set; }
    }
}
