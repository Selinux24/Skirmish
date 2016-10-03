using SharpDX;

namespace Engine
{
    /// <summary>
    /// Particle emitter
    /// </summary>
    public class ParticleEmitter
    {
        /// <summary>
        /// Particle size
        /// </summary>
        public float Size = 1f;
        /// <summary>
        /// Particle color
        /// </summary>
        public Color Color = Color.White;
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position = Vector3.Zero;
    }
}
