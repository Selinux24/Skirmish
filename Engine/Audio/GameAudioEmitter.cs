
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
        /// <param name="emitterDescription">Emitter description</param>
        public GameAudioEmitter(GameAudioSourceDescription emitterDescription) : base(emitterDescription)
        {

        }

        /// <summary>
        /// Sets the game audio emitter's source
        /// </summary>
        /// <param name="manipulator">Transformable instance</param>
        public void SetSource(ITransformable3D source)
        {
            SetSource(source.Manipulator);
        }
    }
}
