using System.Collections.Generic;
using System.Linq;

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
        public IEnumerable<IGameState> DirectionalLights { get; set; } = Enumerable.Empty<IGameState>();
        /// <summary>
        /// Point lights
        /// </summary>
        public IEnumerable<IGameState> PointLights { get; set; } = Enumerable.Empty<IGameState>();
        /// <summary>
        /// Spot lights
        /// </summary>
        public IEnumerable<IGameState> SpotLights { get; set; } = Enumerable.Empty<IGameState>();
        /// <summary>
        /// Hemispheric light
        /// </summary>
        public IGameState HemisphericLigth { get; set; }
    }
}
