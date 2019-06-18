using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio agent
    /// </summary>
    public struct GameAudioAgent
    {
        /// <summary>
        /// Forward vector
        /// </summary>
        public Vector3 Forward { get; set; }
        /// <summary>
        /// Up vector
        /// </summary>
        public Vector3 Up { get; set; }
        /// <summary>
        /// Position coordinate
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// Applies the 2D parameters to the game audio agent.
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="velocity">Velocity</param>
        public void Apply2D(Vector2 position, Vector2 velocity)
        {
            Forward = Vector3.ForwardLH;
            Up = Vector3.Up;
            Position = new Vector3(position, 0);
            Velocity = new Vector3(velocity, 0);
        }
        /// <summary>
        /// Applies the 3D parameters to the game audio agent.
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="velocity">Velocity</param>
        public void Apply3D(Matrix world, Vector3 velocity)
        {
            Forward = world.Forward;
            Up = world.Up;
            Position = world.TranslationVector;
            Velocity = velocity;
        }
    }
}
