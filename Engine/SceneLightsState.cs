using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Scene lights state
    /// </summary>
    public class SceneLightsState : IGameState
    {
        /// <summary>
        /// Directional lights
        /// </summary>
        public IEnumerable<IGameState> DirectionalLights { get; set; } = [];
        /// <summary>
        /// Point lights
        /// </summary>
        public IEnumerable<IGameState> PointLights { get; set; } = [];
        /// <summary>
        /// Spot lights
        /// </summary>
        public IEnumerable<IGameState> SpotLights { get; set; } = [];
        /// <summary>
        /// Hemispheric light
        /// </summary>
        public IGameState HemisphericLigth { get; set; }
    }
}
