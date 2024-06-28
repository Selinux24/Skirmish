using SharpDX;
using System;

namespace Engine.BuiltIn.Components.Particles
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Particle emitter
    /// </summary>
    public class ParticleEmitter : ICullable
    {
        /// <summary>
        /// Samples a bounding box for the current emitter at the specified time
        /// </summary>
        /// <param name="emitter">Emitter</param>
        /// <param name="systemParams">Particle system parameters</param>
        /// <param name="time">Time</param>
        /// <returns>Returns the sampled bounding box</returns>
        private static BoundingBox SampleBBox(ParticleEmitter emitter, ParticleSystemParams systemParams, float time)
        {
            //Initial position
            var initialPos = Vector3.Zero;
            var velocity = emitter.Velocity * systemParams.EmitterVelocitySensitivity;
            float horizontalVelocity = MathF.Max(systemParams.HorizontalVelocity.X, systemParams.HorizontalVelocity.Y);

            //Max v velocity
            var vVelocity = velocity;
            vVelocity.Y *= MathF.Max(systemParams.VerticalVelocity.X, systemParams.VerticalVelocity.Y);

            //Max h velocity
            var hVelocity1 = velocity;
            hVelocity1.X *= horizontalVelocity * MathF.Cos(0);
            hVelocity1.Z *= horizontalVelocity * MathF.Sin(0);

            var hVelocity2 = velocity;
            hVelocity2.X *= horizontalVelocity * MathF.Cos(1);
            hVelocity2.Z *= horizontalVelocity * MathF.Sin(1);

            //Final positions
            var finalPosV = ComputeParticlePosition(
                initialPos,
                vVelocity,
                systemParams.EndVelocity,
                systemParams.MaxDuration * time,
                time,
                systemParams.Gravity);

            var finalPosH1 = ComputeParticlePosition(
                initialPos,
                hVelocity1,
                systemParams.EndVelocity,
                systemParams.MaxDuration * time,
                time,
                systemParams.Gravity);

            var finalPosH2 = ComputeParticlePosition(
                initialPos,
                hVelocity2,
                systemParams.EndVelocity,
                systemParams.MaxDuration * time,
                time,
                systemParams.Gravity);

            float startSize = systemParams.MaxStartSize * 0.5f;

            var initial = new BoundingSphere(initialPos + new Vector3(0, startSize, 0), startSize * 0.5f);

            float endSize = systemParams.MaxEndSize * 0.5f;

            var finalV = new BoundingSphere(finalPosV + new Vector3(0, endSize, 0), endSize * 0.5f);
            var finalH1 = new BoundingSphere(finalPosH1 + new Vector3(0, endSize, 0), endSize * 0.5f);
            var finalH2 = new BoundingSphere(finalPosH2 + new Vector3(0, endSize, 0), endSize * 0.5f);

            var bbox = BoundingBox.FromSphere(initial);
            bbox = BoundingBox.Merge(bbox, BoundingBox.FromSphere(finalV));
            bbox = BoundingBox.Merge(bbox, BoundingBox.FromSphere(finalH1));
            bbox = BoundingBox.Merge(bbox, BoundingBox.FromSphere(finalH2));

            return bbox;
        }
        /// <summary>
        /// Computes a particle position
        /// </summary>
        /// <param name="position">Initial position</param>
        /// <param name="velocity">Initial velocity</param>
        /// <param name="endVelocityMag">End velocity magnitude</param>
        /// <param name="age">Age</param>
        /// <param name="normalizedAge">Normalized age</param>
        /// <param name="gravity">Gravity force vector</param>
        /// <returns>Returns the resulting particle position</returns>
        private static Vector3 ComputeParticlePosition(Vector3 position, Vector3 velocity, float endVelocityMag, float age, float normalizedAge, Vector3 gravity)
        {
            float startVelocityMag = velocity.Length();
            float totalVelocityMag = startVelocityMag * endVelocityMag;
            float velocityIntegral = (startVelocityMag * normalizedAge) + (totalVelocityMag - startVelocityMag) * normalizedAge * normalizedAge * 0.5f;

            var finalVelocity = Vector3.Normalize(velocity) * velocityIntegral;
            var finalGravity = gravity * age * normalizedAge;

            var p = finalVelocity + finalGravity;

            return position + p;
        }

        /// <summary>
        /// Merged emitter bounding box
        /// </summary>
        /// <remarks>The box grows when position changes</remarks>
        private BoundingBox? mergedBoundingBox;
        /// <summary>
        /// Original bounding box
        /// </summary>
        /// <remarks>This box isn't transformed if position changes</remarks>
        private BoundingBox boundingBox;

        /// <summary>
        /// Particle name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Emitter position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Emitter velocity
        /// </summary>
        public Vector3 Velocity { get; set; }
        /// <summary>
        /// Emitter scale
        /// </summary>
        public float Scale { get; set; }
        /// <summary>
        /// Emission rate
        /// </summary>
        public float EmissionRate { get; set; }
        /// <summary>
        /// Emitter duration
        /// </summary>
        public float Duration { get; set; }
        /// <summary>
        /// Gets or sets whether the emitter duration is infinite
        /// </summary>
        public bool InfiniteDuration { get; set; }
        /// <summary>
        /// Gets or sets the maximum distance from camera
        /// </summary>
        public float MaximumDistance { get; set; }
        /// <summary>
        /// Distance from camera
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// Gets whether the emitter is active
        /// </summary>
        public bool Active
        {
            get
            {
                return (InfiniteDuration || Duration > 0);
            }
        }
        /// <summary>
        /// Gets or sets whether the emitter particles is visible
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the emitter particles is culled
        /// </summary>
        public bool Culled { get; set; } = false;
        /// <summary>
        /// Gets or sets the instance with the emitter is attached to
        /// </summary>
        public ITransformable3D Instance { get; set; }

        /// <summary>
        /// Total particle system time
        /// </summary>
        public float TotalTime { get; private set; }
        /// <summary>
        /// Elapsed time
        /// </summary>
        public float ElapsedTime { get; private set; }
        /// <summary>
        /// Returns true if the current emitter is drawable
        /// </summary>
        public bool IsDrawable
        {
            get
            {
                return Visible && !Culled && (Distance <= MaximumDistance);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleEmitter()
        {
            Position = Vector3.Zero;
            Velocity = Vector3.Up;
            Scale = 1f;
            EmissionRate = 1f;
            Duration = 0f;
            InfiniteDuration = false;
            MaximumDistance = 100f;
            Distance = 0f;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="desc">Description</param>
        public ParticleEmitter(ParticleEmitterDescription desc)
        {
            Position = desc.Position;
            Velocity = desc.Velocity;
            Scale = desc.Scale;
            EmissionRate = desc.EmissionRate;
            Duration = desc.Duration;
            InfiniteDuration = desc.InfiniteDuration;
            MaximumDistance = desc.MaximumDistance;
            Distance = desc.Distance;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="desc">Description</param>
        public ParticleEmitter(ParticleEmitterFile desc)
        {
            Position = desc.Position;
            Velocity = desc.Velocity;
            Scale = desc.Scale;
            EmissionRate = desc.EmissionRate;
            Duration = desc.Duration;
            InfiniteDuration = desc.InfiniteDuration;
            MaximumDistance = desc.MaximumDistance;
            Distance = desc.Distance;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="pointOfView">Point of view</param>
        public virtual void Update(IGameTime gameTime, Vector3 pointOfView)
        {
            ElapsedTime = gameTime.ElapsedSeconds;

            TotalTime += ElapsedTime;

            if (!InfiniteDuration)
            {
                Duration -= ElapsedTime;
            }

            Distance = Vector3.Distance(Position, pointOfView);

            UpdateBoundingBox();
        }
        /// <summary>
        /// Updates the internal bounding box
        /// </summary>
        protected virtual void UpdateBoundingBox()
        {
            var tmp = mergedBoundingBox;

            mergedBoundingBox = new BoundingBox(
                boundingBox.Minimum + Position,
                boundingBox.Maximum + Position);

            if (tmp != null)
            {
                mergedBoundingBox = BoundingBox.Merge(tmp.Value, mergedBoundingBox.Value);
            }
        }

        /// <summary>
        /// Calculates the initial velocity for a particle
        /// </summary>
        /// <param name="parameters">Particle system parameters</param>
        /// <param name="hVelVariance">Horizontal velocity variance</param>
        /// <param name="vVelVariance">Vertical velocity variance</param>
        /// <param name="hAngleVariance">Horizontal angle variance</param>
        /// <returns>Returns the initial velocity vector with magnitude</returns>
        public Vector3 CalcInitialVelocity(ParticleSystemParams parameters, float hVelVariance, float vVelVariance, float hAngleVariance)
        {
            Vector3 velocity = Velocity * parameters.EmitterVelocitySensitivity;

            float horizontalVelocity = MathUtil.Lerp(
                parameters.HorizontalVelocity.X,
                parameters.HorizontalVelocity.Y,
                hVelVariance);

            float horizontalAngle = hAngleVariance * MathUtil.TwoPi;

            velocity.X += horizontalVelocity * MathF.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * MathF.Sin(horizontalAngle);

            velocity.Y += MathUtil.Lerp(
                parameters.VerticalVelocity.X,
                parameters.VerticalVelocity.Y,
                vVelVariance);

            return velocity;
        }

        /// <summary>
        /// Gets the maximum number of particles running at the same time
        /// </summary>
        /// <param name="maxParticleDuration">Maximum particle duration</param>
        /// <returns>Returns the maximum number of particles running at the same time</returns>
        public int GetMaximumConcurrentParticles(float maxParticleDuration)
        {
            float maxActiveParticles = maxParticleDuration * (1f / EmissionRate);

            return (int)(maxActiveParticles > (int)maxActiveParticles ? maxActiveParticles + 1 : maxActiveParticles);
        }

        /// <inheritdoc/>
        public bool Cull(int cullIndex, ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            var bbox = GetBoundingBox();

            var inside = volume.Contains(bbox) != ContainmentType.Disjoint;
            if (inside)
            {
                distance = Vector3.DistanceSquared(volume.Position, Position);
            }

            return Culled = !inside;
        }

        /// <summary>
        /// Sets the internal bounding box for culling tests
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        public void SetBoundingBox(BoundingBox bbox)
        {
            boundingBox = bbox;
            mergedBoundingBox = null;
        }
        /// <summary>
        /// Gets the internal bounding box updated with position
        /// </summary>
        /// <returns>Returns the internal bounding box for culling tests</returns>
        public virtual BoundingBox GetBoundingBox()
        {
            return mergedBoundingBox ?? new BoundingBox(
                boundingBox.Minimum + Position,
                boundingBox.Maximum + Position);
        }
        /// <summary>
        /// Updates the particle emitter bounds
        /// </summary>
        /// <param name="systemParams">System parameters</param>
        public void UpdateBounds(ParticleSystemParams systemParams)
        {
            var bbox = SampleBBox(this, systemParams, 0.33f);
            bbox = BoundingBox.Merge(bbox, SampleBBox(this, systemParams, 0.66f));
            bbox = BoundingBox.Merge(bbox, SampleBBox(this, systemParams, 1f));

            SetBoundingBox(bbox);
        }
    }
}
