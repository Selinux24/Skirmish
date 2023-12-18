
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
        public IGameTime GameTime { get; set; }

        /// <summary>
        /// Clones the actual update context
        /// </summary>
        public UpdateContext Clone()
        {
            return new UpdateContext
            {
                GameTime = GameTime,
            };
        }
    }
}
