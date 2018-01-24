using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Particle emitter
    /// </summary>
    public class ParticleEmitter : ICullable
    {
        /// <summary>
        /// Creates a maximum bounding box for a particle emitter
        /// </summary>
        /// <param name="maxParticleDuration">Particle duration</param>
        /// <param name="size">Particle size</param>
        /// <param name="hVel">Particle horizontal velocity</param>
        /// <param name="vVel">Particle vertical velocity</param>
        /// <returns>Returns the created bounding box</returns>
        public static BoundingBox GenerateBBox(float maxParticleDuration, Vector2 size, Vector2 hVel, Vector2 vVel)
        {
            var hDist = Math.Max(Math.Abs(hVel.X), Math.Abs(hVel.Y)) * maxParticleDuration;
            var vDist = (Math.Max(Math.Abs(vVel.X), Math.Abs(vVel.Y)) * maxParticleDuration) + (size.Y * 0.5f);

            var dMax = new Vector3(hDist, vDist, hDist);
            var dMin = new Vector3(-hDist, 0, -hDist);

            return new BoundingBox(dMin, dMax);
        }

        /// <summary>
        /// Current emitter bounding box
        /// </summary>
        /// <remarks>The box grows when position changes</remarks>
        private BoundingBox? currentBoundingBox;
        /// <summary>
        /// Original bounding box
        /// </summary>
        /// <remarks>This box isn't transformed if position changes</remarks>
        private BoundingBox boundingBox;
        /// <summary>
        /// Visible flag
        /// </summary>
        private bool visible = false;

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
        /// Gets or sets wheter the emitter duration is infinite
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
        /// Gets wheter the emitter is active
        /// </summary>
        public bool Active
        {
            get
            {
                return (this.InfiniteDuration || this.Duration > 0);
            }
        }
        /// <summary>
        /// Gets or sets wheter the emitter particles is visible
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.visible && (this.Distance <= this.MaximumDistance);
            }
            set
            {
                this.visible = value;
            }
        }

        /// <summary>
        /// Total particle system time
        /// </summary>
        public float TotalTime { get; private set; }
        /// <summary>
        /// Elapsed time
        /// </summary>
        public float ElapsedTime { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleEmitter()
        {
            this.Position = Vector3.Zero;
            this.Velocity = Vector3.Up;
            this.Scale = 1f;
            this.EmissionRate = 1f;
            this.Duration = 0f;
            this.InfiniteDuration = false;
            this.MaximumDistance = GameEnvironment.LODDistanceLow;
            this.Distance = 0f;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="desc">Description</param>
        public ParticleEmitter(ParticleEmitterDescription desc)
        {
            this.Position = desc.Position;
            this.Velocity = desc.Velocity;
            this.Scale = desc.Scale;
            this.EmissionRate = desc.EmissionRate;
            this.Duration = desc.Duration;
            this.InfiniteDuration = desc.InfiniteDuration;
            this.MaximumDistance = desc.MaximumDistance;
            this.Distance = desc.Distance;
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public virtual void Update(UpdateContext context)
        {
            this.ElapsedTime = context.GameTime.ElapsedSeconds;

            this.TotalTime += this.ElapsedTime;

            if (!this.InfiniteDuration)
            {
                this.Duration -= this.ElapsedTime;
            }

            this.Distance = Vector3.Distance(this.Position, context.EyePosition);

            this.UpdateBoundingBox();
        }
        /// <summary>
        /// Updates the internal bounding box
        /// </summary>
        protected virtual void UpdateBoundingBox()
        {
            var tmp = currentBoundingBox;

            currentBoundingBox = new BoundingBox(
                this.boundingBox.Minimum + this.Position,
                this.boundingBox.Maximum + this.Position);

            if (tmp != null)
            {
                currentBoundingBox = BoundingBox.Merge(tmp.Value, currentBoundingBox.Value);
            }
        }

        /// <summary>
        /// Gets the maximum number of particles running at the same time
        /// </summary>
        /// <param name="maxParticleDuration">Maximum particle duration</param>
        /// <returns>Returns the maximum number of particles running at the same time</returns>
        public int GetMaximumConcurrentParticles(float maxParticleDuration)
        {
            float maxActiveParticles = maxParticleDuration * (1f / this.EmissionRate);

            return (int)(maxActiveParticles != (int)maxActiveParticles ? maxActiveParticles + 1 : maxActiveParticles);
        }

        /// <summary>
        /// Performs a culling test against the specified frustum
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the emitter is outside of the frustum</returns>
        public bool Cull(ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            var bbox = this.GetBoundingBox();

            var inside = volume.Contains(bbox) != ContainmentType.Disjoint;
            if (inside)
            {
                distance = Vector3.DistanceSquared(volume.Position, this.Position);
            }

            return !inside;
        }

        /// <summary>
        /// Sets the internal bounding box for culling tests
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        public void SetBoundingBox(BoundingBox bbox)
        {
            this.boundingBox = bbox;
            this.currentBoundingBox = null;
        }
        /// <summary>
        /// Gets the internal bounding box updated with position
        /// </summary>
        /// <returns>Returns the internal bounding box for culling tests</returns>
        public virtual BoundingBox GetBoundingBox()
        {
            return currentBoundingBox.HasValue ? currentBoundingBox.Value : new BoundingBox();
        }
    }
}
