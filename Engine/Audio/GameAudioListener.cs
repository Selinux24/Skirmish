using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio listener
    /// </summary>
    public class GameAudioListener : GameAudioSource
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Listener position</param>
        /// <param name="radius">Source maximum radius</param>
        /// <param name="cone">Source sound cone description</param>
        public GameAudioListener(Vector3 position, float radius = float.MaxValue, GameAudioConeDescription? cone = null) :
            base(radius, cone)
        {
            Manipulator3D source = new Manipulator3D();

            source.SetPosition(position);

            SetSource(source);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transform">Listener transform</param>
        /// <param name="radius">Source maximum radius</param>
        /// <param name="cone">Source sound cone description</param>
        public GameAudioListener(IManipulator transform, float radius = float.MaxValue, GameAudioConeDescription? cone = null) :
            base(radius, cone)
        {
            SetSource(transform);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Listener instance</param>
        /// <param name="radius">Source maximum radius</param>
        /// <param name="cone">Source sound cone description</param>
        public GameAudioListener(ITransformable3D source, float radius = float.MaxValue, GameAudioConeDescription? cone = null) :
            base(radius, cone)
        {
            SetSource(source);
        }
    }
}
