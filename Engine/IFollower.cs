using SharpDX;

namespace Engine
{
    /// <summary>
    /// Follower interface
    /// </summary>
    public interface IFollower
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Interest
        /// </summary>
        Vector3 Interest { get; }
        /// <summary>
        /// Velocity
        /// </summary>
        float Velocity { get; set; }

        /// <summary>
        /// Updates the follower position and interest
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void Update(GameTime gameTime);
    }
}
