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
        /// Previous emitter bounding box
        /// </summary>
        private BoundingBox? previousBoundingBox;
        /// <summary>
        /// Current emitter bounding box
        /// </summary>
        private BoundingBox currentBoundingBox;
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
            this.EmissionRate = 1f;
            this.Duration = 0f;
            this.InfiniteDuration = false;
            this.MaximumDistance = GameEnvironment.LODDistanceLow;
            this.Distance = 0f;
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
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns true if the emitter is outside the frustum</returns>
        public bool Cull(BoundingFrustum frustum)
        {
            var bbox = this.currentBoundingBox;

            return frustum.Contains(ref bbox) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Performs a culling test against the specified box
        /// </summary>
        /// <param name="box">box</param>
        /// <returns>Returns true if the emitter is outside the box</returns>
        public bool Cull(BoundingBox box)
        {
            var bbox = this.currentBoundingBox;

            return box.Contains(ref bbox) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Performs a culling test against the specified sphere
        /// </summary>
        /// <param name="sphere">sphere</param>
        /// <returns>Returns true if the emitter is outside the sphere</returns>
        public bool Cull(BoundingSphere sphere)
        {
            var bbox = this.currentBoundingBox;

            return sphere.Contains(ref bbox) != ContainmentType.Disjoint;
        }

        /// <summary>
        /// Sets the internal bounding box for culling tests
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        public void SetBoundingBox(BoundingBox bbox)
        {
            this.previousBoundingBox = null;
            this.currentBoundingBox = bbox;
        }
        /// <summary>
        /// Gets the internal bounding box updated with position
        /// </summary>
        /// <returns>Returns the internal bounding box for culling tests</returns>
        public virtual BoundingBox GetBoundingBox()
        {
            var nBbox = new BoundingBox(this.currentBoundingBox.Minimum + this.Position, this.currentBoundingBox.Maximum + this.Position);
            if (previousBoundingBox == null)
            {
                previousBoundingBox = nBbox;
            }
            else
            {
                previousBoundingBox = BoundingBox.Merge(previousBoundingBox.Value, nBbox);
            }

            return previousBoundingBox.Value;
        }
    }
}
