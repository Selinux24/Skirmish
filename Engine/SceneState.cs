using Engine.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene state
    /// </summary>
    public class SceneState : IGameState
    {
        /// <summary>
        /// Scene name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Scene date
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// State version
        /// </summary>
        public Version Version { get; set; } = new Version(0, 1);
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Order
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Scene mode
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SceneModes SceneMode { get; set; } = SceneModes.Unknown;
        /// <summary>
        /// Game time ticks
        /// </summary>
        public long GameTime { get; set; }
        /// <summary>
        /// Grounding box minimum point
        /// </summary>
        public Position3? GroundBoundingBoxMin { get; set; }
        /// <summary>
        /// Grounding box maximum point
        /// </summary>
        public Position3? GroundBoundingBoxMax { get; set; }
        /// <summary>
        /// Navigation box minimum point
        /// </summary>
        public Position3? NavigationBoundingBoxMin { get; set; }
        /// <summary>
        /// Navigation box maximum point
        /// </summary>
        public Position3? NavigationBoundingBoxMax { get; set; }
        /// <summary>
        /// Game environment state
        /// </summary>
        public IGameState GameEnvironment { get; set; }
        /// <summary>
        /// Scene lights state
        /// </summary>
        public IGameState SceneLights { get; set; }
        /// <summary>
        /// Camera state
        /// </summary>
        public IGameState Camera { get; set; }
        /// <summary>
        /// Components state
        /// </summary>
        public IEnumerable<ISceneObjectState> Components { get; set; } = Enumerable.Empty<ISceneObjectState>();
    }
}
