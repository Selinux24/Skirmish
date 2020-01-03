using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio emitter
    /// </summary>
    public class GameAudioEmitter : GameAudioSource
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Emitter position</param>
        /// <param name="radius">Source maximum radius</param>
        /// <param name="cone">Source sound cone description</param>
        public GameAudioEmitter(Vector3 position, float radius = float.MaxValue, GameAudioConeDescription? cone = null) :
            base(radius, cone)
        {
            Manipulator3D source = new Manipulator3D();

            source.SetPosition(position);

            SetSource(source);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="transform">Emitter transform</param>
        /// <param name="radius">Source maximum radius</param>
        /// <param name="cone">Source sound cone description</param>
        public GameAudioEmitter(IManipulator transform, float radius = float.MaxValue, GameAudioConeDescription? cone = null) :
            base(radius, cone)
        {
            SetSource(transform);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Emitter instance</param>
        /// <param name="radius">Source maximum radius</param>
        /// <param name="cone">Source sound cone description</param>
        public GameAudioEmitter(ITransformable3D source, float radius = float.MaxValue, GameAudioConeDescription? cone = null) :
            base(radius, cone)
        {
            SetSource(source);
        }
    }

    /// <summary>
    /// Game audio emitter
    /// </summary>
    public class GameAudioEmitter<T> : GameAudioEmitter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Emitter instance</param>
        /// <param name="radius">Source maximum radius</param>
        /// <param name="cone">Source sound cone description</param>
        public GameAudioEmitter(SceneObject<T> source, float radius = float.MaxValue, GameAudioConeDescription? cone = null) :
            base(source.Transform, radius, cone)
        {

        }
    }
}
