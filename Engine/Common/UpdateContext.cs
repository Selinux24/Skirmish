
namespace Engine.Common
{
    /// <summary>
    /// Updating context
    /// </summary>
    public struct UpdateContext
    {
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime { get; set; }
        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera { get; set; }
        /// <summary>
        /// Lights
        /// </summary>
        public SceneLights Lights { get; set; }

        /// <summary>
        /// Clones the actual update context
        /// </summary>
        public UpdateContext Clone()
        {
            return new UpdateContext
            {
                GameTime = GameTime,
                Camera = Camera,
                Lights = Lights,
            };
        }
    }
}
