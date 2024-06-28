using SharpDX;
using System;

namespace Engine.Physics
{
    /// <summary>
    /// Explosion in two phases: Implosion and Expansive wave
    /// </summary>
    public class Explosion : IGlobalForceGenerator
    {
        /// <summary>
        /// Explosion total elapsed time
        /// </summary>
        public float TotalElapsedTime { get; private set; } = 0f;

        /// <summary>
        /// The location of the detonation
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Radius in which the objects are not attracted in the implosion, because they are trapped in the epicenter
        /// </summary>
        public float ImplosionMinRadius { get; private set; }
        /// <summary>
        /// Radius at which objects are attracted in the first phase of the explosion.
        /// </summary>
        public float ImplosionMaxRadius { get; private set; }
        /// <summary>
        /// Implosion Phase Duration
        /// </summary>
        public float ImplosionDuration { get; private set; }
        /// <summary>
        /// The maximum force that is applied in the implosion
        /// </summary>
        public float ImplosionForce { get; private set; }

        /// <summary>
        /// Speed at which the shock wave travels
        /// </summary>
        /// <remarks>
        /// The width of the shock wave must be greater than or equal to the maximum distance it can reach. Width >= Speed * Duration
        /// </remarks>
        public float ShockwaveSpeed { get; private set; }
        /// <summary>
        /// Shock wave width
        /// </summary>
        /// <remarks>
        /// The faster the wave, the wider the wave should be
        /// </remarks>
        public float ShockwaveThickness { get; private set; }
        /// <summary>
        /// Attenuation when the shock wave overtakes an object
        /// </summary>
        public float ShockwaveOvertakenAttenuation { get; private set; }
        /// <summary>
        /// Force applied at the center of the blast wave on a stationary object.
        /// </summary>
        /// <remarks>
        /// Objects in front of or behind the wave gain proportionally less force.
        /// </remarks>
        public float PeakConcussionForce { get; private set; }
        /// <summary>
        /// The duration of the shock wave phase
        /// </summary>
        /// <remarks>
        /// The closer to the end, the less powerful the wave.
        /// </remarks>
        public float ConcussionDuration { get; private set; }

        /// <summary>
        /// Gets the explosion maximum distance
        /// </summary>
        public float MaximumDistance { get => (ShockwaveSpeed * ConcussionDuration) + ShockwaveThickness; }
        /// <summary>
        /// Gets the explosion total duration
        /// </summary>
        public float TotalDuration { get => ImplosionDuration + ConcussionDuration; }
        /// <summary>
        /// Gets the implosion area distance
        /// </summary>
        public float ImplosionArea { get => ImplosionMaxRadius - ImplosionMinRadius; }
        /// <summary>
        /// Explosion phase
        /// </summary>
        public ExplosionPhases CurrentPhase { get; private set; }

        /// <inheritdoc/>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public Explosion(Vector3 position, ExplosionDescription description)
        {
            Position = position;

            float implosionArea = description.ImplosionMaxRadius - description.ImplosionMinRadius;
            if (MathUtil.IsZero(implosionArea) || implosionArea <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(description), $"{nameof(description.ImplosionMinRadius)} - {nameof(description.ImplosionMaxRadius)} must be major than 0.");
            }
            ImplosionMinRadius = description.ImplosionMinRadius;
            ImplosionMaxRadius = description.ImplosionMaxRadius;

            if (MathUtil.IsZero(description.ImplosionDuration) || description.ImplosionDuration <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(description), $"{nameof(description.ImplosionDuration)} must be major than 0.");
            }
            ImplosionDuration = description.ImplosionDuration;

            if (MathUtil.IsZero(description.ImplosionForce) || description.ImplosionForce <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(description), $"{nameof(description.ImplosionForce)} must be major than 0.");
            }
            ImplosionForce = description.ImplosionForce;

            float maxDistance = (description.ShockwaveSpeed * description.ConcussionDuration) + description.ShockwaveThickness;
            if (MathUtil.IsZero(maxDistance) || maxDistance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(description), $"({nameof(description.ShockwaveSpeed)} * {nameof(description.ConcussionDuration)}) - {nameof(description.ShockwaveThickness)} must be major than 0.");
            }
            ShockwaveSpeed = description.ShockwaveSpeed;
            ShockwaveThickness = description.ShockwaveThickness;
            ConcussionDuration = description.ConcussionDuration;

            if (MathUtil.IsZero(description.PeakConcussionForce) || description.PeakConcussionForce <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(description), $"{nameof(description.PeakConcussionForce)} must be major than 0.");
            }
            PeakConcussionForce = description.PeakConcussionForce;

            if (MathUtil.IsZero(description.ShockwaveOvertakenAttenuation) || description.ShockwaveOvertakenAttenuation <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(description), $"{nameof(description.ShockwaveOvertakenAttenuation)} must be major than 0.");
            }
            ShockwaveOvertakenAttenuation = description.ShockwaveOvertakenAttenuation;
        }

        /// <inheritdoc/>
        public void UpdateForce(float time)
        {
            if (MathUtil.IsZero(time))
            {
                return;
            }

            TotalElapsedTime += time;
        }
        /// <inheritdoc/>
        public void ApplyForce(IRigidBody rigidBody)
        {
            if (rigidBody == null)
            {
                return;
            }

            if (!IsActive)
            {
                return;
            }

            // Detect the phase of the explosion in which we are
            CurrentPhase = DetectPhase();

            float forceMagnitude = 0;
            Vector3 forceNormal = Vector3.Zero;
            switch (CurrentPhase)
            {
                case ExplosionPhases.Implosion:
                    forceMagnitude = CalculateImplosionPhaseForce(rigidBody);
                    forceNormal = Vector3.Normalize(Position - rigidBody.Position);
                    break;
                case ExplosionPhases.ExpansiveWave:
                    forceMagnitude = CalculateExpansiveWavePhaseForce(rigidBody);
                    forceNormal = Vector3.Normalize(rigidBody.Position - Position);
                    break;
                default:
                    IsActive = false;
                    break;
            }

            if (MathUtil.IsZero(forceMagnitude))
            {
                return;
            }

            rigidBody.AddForce(forceNormal * forceMagnitude);
        }
        /// <summary>
        /// Detects the explosion phase
        /// </summary>
        /// <returns>Returns the current explosion phase</returns>
        private ExplosionPhases DetectPhase()
        {
            if (TotalElapsedTime <= ImplosionDuration) return ExplosionPhases.Implosion;
            if (TotalElapsedTime <= TotalDuration) return ExplosionPhases.ExpansiveWave;

            return ExplosionPhases.None;
        }
        /// <summary>
        /// Calculates the implosion phase force magnitude
        /// </summary>
        /// <param name="body">Rigid body</param>
        private float CalculateImplosionPhaseForce(IRigidBody body)
        {
            float distance = Vector3.Distance(body.Position, Position);
            if (distance > ImplosionMinRadius && distance <= ImplosionMaxRadius)
            {
                // The body is in the implosion area
                return (distance - ImplosionMinRadius) / ImplosionArea * ImplosionForce;
            }

            return 0;
        }
        /// <summary>
        /// Executes expansive wave phase
        /// </summary>
        /// <param name="body">Rigid body</param>
        private float CalculateExpansiveWavePhaseForce(IRigidBody body)
        {
            // Distance to the center of the object
            float distance = Vector3.Distance(body.Position, Position);

            // Current position of the shock wave
            float minShockwave = ShockwaveSpeed * TotalElapsedTime;
            float maxShockwave = (ShockwaveSpeed * TotalElapsedTime) + ShockwaveThickness;

            if (distance < minShockwave)
            {
                // The object has been overtaken by the shock wave.
                float concussionAttenuation = CalculateAttenuation(TotalElapsedTime, TotalDuration);

                return PeakConcussionForce * concussionAttenuation * ShockwaveOvertakenAttenuation;
            }

            if (distance < maxShockwave)
            {
                // The object is in full expansive wave.
                float concussionAttenuation = CalculateAttenuation(TotalElapsedTime, TotalDuration);

                return PeakConcussionForce * concussionAttenuation;
            }

            float maxDistance = MaximumDistance;
            if (distance < maxDistance)
            {
                // The object has not been hit by the blast wave. Strength attenuated by time and distance
                float concussionAttenuation = CalculateAttenuation(TotalElapsedTime, TotalDuration);
                float distanceAttenuation = CalculateAttenuation(distance, maxDistance);

                return PeakConcussionForce * concussionAttenuation * distanceAttenuation;
            }

            return 0;
        }
        /// <summary>
        /// Gets the attenuation from 0 to 1
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="max">Max value</param>
        private static float CalculateAttenuation(float current, float max)
        {
            if (MathUtil.IsZero(max) || max < 0)
            {
                return 0;
            }

            return current < max ? 1f - (current / max) : 0f;
        }
    }
}
