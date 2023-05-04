using SharpDX;

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
        private float totalElapsedTime = 0f;

        /// <summary>
        /// The location of the detonation
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Radius in which the objects are not attracted in the implosion, because they are trapped in the epicenter
        /// </summary>
        public float ImplosionMinRadius { get; set; }
        /// <summary>
        /// Radius at which objects are attracted in the first phase of the explosion.
        /// </summary>
        public float ImplosionMaxRadius { get; set; }
        /// <summary>
        /// Implosion Phase Duration
        /// </summary>
        public float ImplosionDuration { get; set; }
        /// <summary>
        /// The maximum force that is applied in the implosion
        /// </summary>
        public float ImplosionForce { get; set; }

        /// <summary>
        /// Speed at which the shock wave travels
        /// </summary>
        /// <remarks>
        /// The width of the shock wave must be greater than or equal to the maximum distance it can reach. Width >= Speed * Duration
        /// </remarks>
        public float ShockwaveSpeed { get; set; }
        /// <summary>
        /// Shock wave width
        /// </summary>
        /// <remarks>
        /// The faster the wave, the wider the wave should be
        /// </remarks>
        public float ShockwaveThickness { get; set; }
        /// <summary>
        /// Force applied at the center of the blast wave on a stationary object.
        /// </summary>
        /// <remarks>
        /// Objects in front of or behind the wave gain proportionally less force.
        /// </remarks>
        public float PeakConcussionForce { get; set; }
        /// <summary>
        /// The duration of the shock wave phase
        /// </summary>
        /// <remarks>
        /// The closer to the end, the less powerful the wave.
        /// </remarks>
        public float ConcussionDuration { get; set; }

        /// <inheritdoc/>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public Explosion(Vector3 position, ExplosionDescription description)
        {
            Position = position;

            ImplosionMinRadius = description.ImplosionMinRadius;
            ImplosionMaxRadius = description.ImplosionMaxRadius;
            ImplosionDuration = description.ImplosionDuration;
            ImplosionForce = description.ImplosionForce;

            ShockwaveSpeed = description.ShockwaveSpeed;
            ShockwaveThickness = description.ShockwaveThickness;
            PeakConcussionForce = description.PeakConcussionForce;
            ConcussionDuration = description.ConcussionDuration;
        }

        /// <inheritdoc/>
        public void UpdateForce(IRigidBody body, float time)
        {
            if (body == null)
            {
                return;
            }

            if (MathUtil.IsZero(time))
            {
                return;
            }

            if (!IsActive)
            {
                return;
            }

            // Detect the phase of the explosion in which we are
            var phase = DetectPhase();

            switch (phase)
            {
                case ExplosionPhases.Implosion:
                    ImplosionPhase(body);
                    break;
                case ExplosionPhases.ExpansiveWave:
                    ExpansiveWavePhase(body, time);
                    break;
                case ExplosionPhases.None:
                default:
                    IsActive = false;
                    break;
            }

            totalElapsedTime += time;
        }
        /// <summary>
        /// Detects the explosion phase
        /// </summary>
        /// <returns>Returns the current explosion phase</returns>
        private ExplosionPhases DetectPhase()
        {
            if (totalElapsedTime <= ImplosionDuration) return ExplosionPhases.Implosion;
            if (totalElapsedTime <= (ImplosionDuration + ConcussionDuration)) return ExplosionPhases.ExpansiveWave;

            return ExplosionPhases.None;
        }
        /// <summary>
        /// Executes implosion phase
        /// </summary>
        /// <param name="body">Rigid body</param>
        private void ImplosionPhase(IRigidBody body)
        {
            float distance = Vector3.Distance(body.Position, Position);
            if (distance > ImplosionMinRadius && distance <= ImplosionMaxRadius)
            {
                // The body is in the implosion area. Apply forces
                float max = ImplosionMaxRadius - ImplosionMinRadius;
                if (MathUtil.IsZero(max))
                {
                    return;
                }

                float forceMagnitude = (distance - ImplosionMinRadius) / max;

                Vector3 force = Vector3.Normalize(Position - body.Position) * ImplosionForce * forceMagnitude;

                body.AddForce(force);
            }
        }
        /// <summary>
        /// Executes expansive wave phase
        /// </summary>
        /// <param name="body">Rigid body</param>
        /// <param name="time">Time</param>
        private void ExpansiveWavePhase(IRigidBody body, float time)
        {
            float totalDuration = ConcussionDuration + ImplosionDuration;
            if (MathUtil.IsZero(totalDuration))
            {
                return;
            }

            float maxDistance = (ShockwaveSpeed * ConcussionDuration) + ShockwaveThickness;
            if (MathUtil.IsZero(maxDistance))
            {
                return;
            }

            // Current explosion time, from 0 to 1
            float relativeTime = totalElapsedTime < totalDuration ? 1f - (totalElapsedTime / totalDuration) : 0f;

            // Current interval of maximum action of the wave
            float min = ShockwaveSpeed * totalElapsedTime;
            float max = ShockwaveSpeed * (totalElapsedTime + time);

            // Distance to the center of the object
            float distance = Vector3.Distance(body.Position, Position);

            float forceMagnitude = 0f;
            if (distance >= min && distance <= max)
            {
                // In full expansive wave. Attenuated forces are applied only for the duration
                forceMagnitude = PeakConcussionForce * relativeTime;
            }
            else if (distance < min)
            {
                // The object has been overtaken by the shock wave. Minimally attenuated force
                forceMagnitude = PeakConcussionForce * relativeTime;
            }
            else if (distance > max && distance <= maxDistance)
            {
                // The object has not been hit by the blast wave. Strength attenuated by time and distance

                // Distance to center, from 0 to 1
                float relativeDistance = distance < maxDistance ? 1f - (distance / maxDistance) : 0f;

                forceMagnitude = PeakConcussionForce * relativeDistance * relativeTime;
            }

            if (MathUtil.IsZero(forceMagnitude))
            {
                return;
            }

            Vector3 force = Vector3.Normalize(body.Position - Position) * forceMagnitude;

            body.AddForce(force);

        }
    }

    /// <summary>
    /// Explosion description
    /// </summary>
    public struct ExplosionDescription
    {
        /// <summary>
        /// Radius at which objects are attracted in the first phase of the explosion.
        /// </summary>
        public float ImplosionMaxRadius { get; set; }
        /// <summary>
        /// Radius in which the objects are not attracted in the implosion, because they are trapped in the epicenter
        /// </summary>
        public float ImplosionMinRadius { get; set; }
        /// <summary>
        /// Implosion Phase Duration
        /// </summary>
        public float ImplosionDuration { get; set; }
        /// <summary>
        /// The maximum force that is applied in the implosion
        /// </summary>
        public float ImplosionForce { get; set; }

        /// <summary>
        /// Speed at which the shock wave travels
        /// </summary>
        /// <remarks>
        /// The width of the shock wave must be greater than or equal to the maximum distance it can reach. Width >= Speed * Duration
        /// </remarks>
        public float ShockwaveSpeed { get; set; }
        /// <summary>
        /// Shock wave width
        /// </summary>
        /// <remarks>
        /// The faster the wave, the wider the wave should be
        /// </remarks>
        public float ShockwaveThickness { get; set; }
        /// <summary>
        /// Force applied at the center of the blast wave on a stationary object.
        /// </summary>
        /// <remarks>
        /// Objects in front of or behind the wave gain proportionally less force.
        /// </remarks>
        public float PeakConcussionForce { get; set; }
        /// <summary>
        /// The duration of the shock wave phase
        /// </summary>
        /// <remarks>
        /// The closer to the end, the less powerful the wave.
        /// </remarks>
        public float ConcussionDuration { get; set; }

        /// <summary>
        /// Creates an explosion at the specified position
        /// </summary>
        /// <returns>Gets the explosion force generator</returns>
        public static ExplosionDescription CreateExplosion()
        {
            return new ExplosionDescription
            {
                ImplosionMaxRadius = 3f,
                ImplosionMinRadius = 1f,
                ImplosionDuration = 0.2f,
                ImplosionForce = 1000f,
                ShockwaveSpeed = 50f,
                ShockwaveThickness = 2f,
                PeakConcussionForce = 100000f,
                ConcussionDuration = 0.5f
            };
        }
    }
}
