using SharpDX;
using System;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio effect parameters
    /// </summary>
    public class GameAudioEffectParameters
    {
        /// <summary>
        /// Sound name
        /// </summary>
        public string SoundName { get; set; }
        /// <summary>
        /// Gets a value indicating whether this instance is looped.
        /// </summary>
        public bool IsLooped { get; set; }
        /// <summary>
        /// Gets or sets the pan value of the sound effect.
        /// </summary>
        /// <remarks>The value is clamped to (-1f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public float Pan { get; set; }
        /// <summary>
        /// Gets or sets the pitch value of the sound effect.
        /// </summary>
        /// <remarks>The value is clamped to (-1f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public float Pitch { get; set; }
        /// <summary>
        /// Gets or sets the volume of the current sound effect instance.
        /// </summary>
        /// <remarks>The value is clamped to (0f, 1f) range.</remarks>
        /// <exception cref="ObjectDisposedException">Is thrown if the current instance was already disposed.</exception>
        public float Volume { get; set; } = 1;
        /// <summary>
        /// Gets or sets whether the master voice uses 3D audio or not
        /// </summary>
        public bool UseAudio3D { get; set; } = false;
        /// <summary>
        /// Gets or sets the current reverb preset configuration
        /// </summary>
        public ReverbPresets? ReverbPreset { get; set; } = ReverbPresets.Default;
        /// <summary>
        /// Destroy when finished
        /// </summary>
        public bool DestroyWhenFinished { get; set; } = true;

        /// <summary>
        /// Gets or sets the listener cone
        /// </summary>
        public GameAudioConeDescription? ListenerCone { get; set; } = null;

        /// <summary>
        /// Gets or sets the emitter radius
        /// </summary>
        public float EmitterRadius { get; set; } = float.MaxValue;
        /// <summary>
        /// Gets or sets the emitter cone
        /// </summary>
        public GameAudioConeDescription? EmitterCone { get; set; } = null;
        /// <summary>
        /// Gets or sets the emitter inner radius
        /// </summary>
        /// <remarks>From 0 to float.MaxValue. 2 by default</remarks>
        public float EmitterInnerRadius { get; set; } = 2;
        /// <summary>
        /// Gets or sets the emitter inner radius angle
        /// </summary>
        /// <remarks>From 0 to PI/4. PI/4 by default</remarks>
        public float EmitterInnerRadiusAngle { get; set; } = MathUtil.PiOverFour;
        /// <summary>
        /// Gets or sets the emitter volume curve
        /// </summary>
        /// <remarks>Linear curve by default</remarks>
        public GameAudioCurvePoint[] EmitterVolumeCurve { get; set; } = GameAudioCurvePoint.DefaultLinearCurve;
        /// <summary>
        /// Gets or sets the emitter lfe curve
        /// </summary>
        public GameAudioCurvePoint[] EmitterLfeCurve { get; set; } = GameAudioCurvePoint.DefaultLfeCurve;
        /// <summary>
        /// Gets or sets the emitter reverb curve
        /// </summary>
        public GameAudioCurvePoint[] EmitterReverbCurve { get; set; } = GameAudioCurvePoint.DefaultReverbCurve;
    }
}
