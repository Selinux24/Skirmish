using SharpDX;
using System;
using System.Collections.Generic;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    public class ParticleSystem2
    {
        public const int PARTICLE_COUNT_MAX = 100;
        public const int PARTICLE_EMISSION_RATE = 10;
        public const float PARTICLE_EMISSION_RATE_TIME_INTERVAL = 1000.0f / (float)PARTICLE_EMISSION_RATE;
        public const float PARTICLE_UPDATE_RATE_MAX = 8;

        public enum BlendModes
        {
            BLEND_NONE,
            BLEND_ALPHA,
            BLEND_ADDITIVE,
            BLEND_MULTIPLIED,
        }

        public class Particle
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public Vector3 Acceleration;
            public Color4 ColorStart;
            public Color4 ColorEnd;
            public Color4 Color;
            public float RotationPerParticleSpeed;
            public Vector3 RotationAxis;
            public float RotationSpeed;
            public float Angle;
            public float EnergyStart;
            public float Energy;
            public float SizeStart;
            public float SizeEnd;
            public float Size;
            public int Frame;
            public float TimeOnCurrentFrame;
        }

        private List<Particle> particles = null;
        private Random rnd = new Random();

        public ShaderResourceView Texture { get; set; }
        public BlendModes BlendMode { get; set; }

        public int ParticleCountMax { get; set; }
        public int EmissionRate { get; private set; }
        public float TimePerEmission { get; set; }
        public float EmitTime { get; set; }
        public float LastUpdated { get; set; }
        public bool Ellipsoid { get; set; }
        public bool Started { get; private set; }
        public bool Active
        {
            get
            {
                if (this.Started) return true;

                return (this.ParticleCount > 0);
            }
        }
        public int ParticleCount { get; private set; }
        public float SizeStartMin { get; private set; }
        public float SizeStartMax { get; private set; }
        public float SizeEndMin { get; private set; }
        public float SizeEndMax { get; private set; }
        public Color4 ColorStart { get; private set; }
        public Color4 ColorStartVariance { get; private set; }
        public Color4 ColorEnd { get; private set; }
        public Color4 ColorEndVariance { get; private set; }
        public float EnergyMin { get; private set; }
        public float EnergyMax { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 PositionVariance { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 VelocityVariance { get; private set; }
        public Vector3 Acceleration { get; private set; }
        public Vector3 AccelerationVariance { get; private set; }
        public float RotationPerParticleSpeedMin { get; private set; }
        public float RotationPerParticleSpeedMax { get; private set; }
        public float RotationSpeedMin { get; private set; }
        public float RotationSpeedMax { get; private set; }
        public Vector3 RotationAxis { get; private set; }
        public Vector3 RotationAxisVariance { get; private set; }
        public Matrix Rotation { get; private set; }
        public bool OrbitPosition { get; private set; }
        public bool OrbitVelocity { get; private set; }
        public bool OrbitAcceleration { get; private set; }

        public ParticleSystem2(int particleCountMax)
        {
            this.ParticleCountMax = particleCountMax;
            this.ParticleCount = 0;
            this.EmissionRate = (PARTICLE_EMISSION_RATE);
            this.Started = false;
            this.Ellipsoid = false;
            this.SizeStartMin = (1.0f);
            this.SizeStartMax = (1.0f);
            this.SizeEndMin = (1.0f);
            this.SizeEndMax = (1.0f);
            this.EnergyMin = (1000f);
            this.EnergyMax = (1000f);
            this.ColorStart = new Color4(0);
            this.ColorStartVariance = new Color4(0);
            this.ColorEnd = new Color4(1);
            this.ColorEndVariance = new Color4(0);
            this.Position = Vector3.Zero;
            this.PositionVariance = Vector3.Zero;
            this.Velocity = Vector3.Zero;
            this.VelocityVariance = Vector3.One;
            this.Acceleration = Vector3.Zero;
            this.AccelerationVariance = Vector3.Zero;
            this.RotationPerParticleSpeedMin = (0.0f);
            this.RotationPerParticleSpeedMax = (0.0f);
            this.RotationSpeedMin = (0.0f);
            this.RotationSpeedMax = (0.0f);
            this.RotationAxis = Vector3.Zero;
            this.Rotation = Matrix.Identity;
            this.OrbitPosition = false;
            this.OrbitVelocity = false;
            this.OrbitAcceleration = false;
            this.TimePerEmission = (PARTICLE_EMISSION_RATE_TIME_INTERVAL);
            this.EmitTime = 0;
            this.LastUpdated = 0;

            this.particles = new List<Particle>(particleCountMax);
        }

        public void Start()
        {
            this.Started = true;
            this.LastUpdated = 0;
        }
        public void Stop()
        {
            this.Started = false;
        }
        public void Update(GameTime gameTime, Vector3 translation, Matrix rotation)
        {
            if (!this.Active)
            {
                return;
            }

            // Cap particle updates at a maximum rate. This saves processing
            // and also improves precision since updating with very small
            // time increments is more lossy.
            if (gameTime.ElapsedMilliseconds < PARTICLE_UPDATE_RATE_MAX)
            {
                return;
            }

            if (this.Started && this.EmissionRate > 0)
            {
                // Calculate how much time has passed since we last emitted particles.
                this.EmitTime += gameTime.ElapsedMilliseconds;

                // How many particles should we emit this frame?
                int emitCount = (int)(this.EmitTime / this.TimePerEmission);
                if (emitCount > 0)
                {
                    if ((int)this.TimePerEmission > 0)
                    {
                        this.EmitTime = this.EmitTime % this.TimePerEmission;
                    }
                    this.EmitOnce(translation, rotation, emitCount);
                }
            }

            // Now update all currently living particles.
            for (int particlesIndex = 0; particlesIndex < this.ParticleCount; ++particlesIndex)
            {
                Particle p = this.particles[particlesIndex];

                p.Energy -= gameTime.ElapsedMilliseconds;
                if (p.Energy > 0.0f)
                {
                    if (p.RotationSpeed != 0.0f && !p.RotationAxis.IsZero)
                    {
                        Matrix pRotation;
                        Matrix.RotationAxis(ref p.RotationAxis, p.RotationSpeed * gameTime.ElapsedSeconds, out pRotation);

                        Vector3.TransformCoordinate(ref p.Velocity, ref pRotation, out p.Velocity);
                        Vector3.TransformCoordinate(ref p.Acceleration, ref pRotation, out p.Acceleration);
                    }

                    // Particle is still alive.
                    p.Velocity += p.Acceleration * gameTime.ElapsedSeconds;
                    p.Position += p.Velocity * gameTime.ElapsedSeconds;
                    p.Angle += p.RotationPerParticleSpeed * gameTime.ElapsedSeconds;

                    // Simple linear interpolation of color and size.
                    float percent = 1.0f - (p.Energy / p.EnergyStart);

                    p.Color = p.ColorStart + (p.ColorEnd - p.ColorStart) * percent;
                    p.Size = p.SizeStart + (p.SizeEnd - p.SizeStart) * percent;
                }
                else
                {
                    // Particle is dead.
                    // Move the particle furthest from the start of the array down to take its place, and re-use the slot at the end of the list of living particles.
                    if (particlesIndex != this.ParticleCount - 1)
                    {
                        this.particles[particlesIndex] = this.particles[this.ParticleCount - 1];
                    }
                    --this.ParticleCount;
                }
            }
        }
        public void Draw()
        {

        }

        public void EmitOnce(Vector3 translation, Matrix rotation, int particleCount)
        {
            // Limit particleCount so as not to go over particleCountMax.
            if (particleCount + this.ParticleCount > this.ParticleCountMax)
            {
                particleCount = this.ParticleCountMax - this.ParticleCount;
            }

            // Emit the new particles.
            for (int i = 0; i < particleCount; i++)
            {
                Particle p = this.particles[this.ParticleCount];

                this.GenerateColor(this.ColorStart, this.ColorStartVariance, ref p.ColorStart);
                this.GenerateColor(this.ColorEnd, this.ColorEndVariance, ref p.ColorEnd);
                p.Color = p.ColorStart;

                p.Energy = p.EnergyStart = this.rnd.NextFloat(this.EnergyMin, this.EnergyMax);
                p.Size = p.SizeStart = this.rnd.NextFloat(this.SizeStartMin, this.SizeStartMax);
                p.SizeEnd = this.rnd.NextFloat(this.SizeEndMin, this.SizeEndMax);
                p.RotationPerParticleSpeed = this.rnd.NextFloat(this.RotationPerParticleSpeedMin, this.RotationPerParticleSpeedMax);
                p.Angle = this.rnd.NextFloat(0.0f, p.RotationPerParticleSpeed);
                p.RotationSpeed = this.rnd.NextFloat(this.RotationSpeedMin, this.RotationSpeedMax);

                // Only initial position can be generated within an ellipsoidal domain.
                this.GenerateVector(this.Position, this.PositionVariance, ref p.Position, this.Ellipsoid);
                this.GenerateVector(this.Velocity, this.VelocityVariance, ref p.Velocity, false);
                this.GenerateVector(this.Acceleration, this.AccelerationVariance, ref p.Acceleration, false);
                this.GenerateVector(this.RotationAxis, this.RotationAxisVariance, ref p.RotationAxis, false);

                // Initial position, velocity and acceleration can all be relative to the emitter's transform.
                // Rotate specified properties by the node's rotation.
                if (this.OrbitPosition)
                {
                    Vector3.TransformCoordinate(ref p.Position, ref rotation, out p.Position);
                }

                if (this.OrbitVelocity)
                {
                    Vector3.TransformCoordinate(ref p.Velocity, ref rotation, out p.Velocity);
                }

                if (this.OrbitAcceleration)
                {
                    Vector3.TransformCoordinate(ref p.Acceleration, ref rotation, out p.Acceleration);
                }

                // The rotation axis always orbits the node.
                if (p.RotationSpeed != 0.0f && !p.RotationAxis.IsZero)
                {
                    Vector3.TransformCoordinate(ref p.RotationAxis, ref rotation, out p.RotationAxis);
                }

                // Translate position relative to the node's world space.
                p.Position += translation;

                p.TimeOnCurrentFrame = 0.0f;

                this.ParticleCount++;
            }
        }
        public void SetEmissionRate(int rate)
        {
            this.EmissionRate = rate;
            this.TimePerEmission = 1000.0f / (float)this.EmissionRate;
        }
        public void SetSize(float startMin, float startMax, float endMin, float endMax)
        {
            this.SizeStartMin = startMin;
            this.SizeStartMax = startMax;
            this.SizeEndMin = endMin;
            this.SizeEndMax = endMax;
        }
        public void SetColor(Color start, Color startVariance, Color end, Color endVariance)
        {
            this.ColorStart = start;
            this.ColorStartVariance = startVariance;
            this.ColorEnd = end;
            this.ColorEndVariance = endVariance;
        }
        public void SetEnergy(float energyMin, float energyMax)
        {
            this.EnergyMin = energyMin;
            this.EnergyMax = energyMax;
        }
        public void SetPosition(Vector3 position, Vector3 positionVariance)
        {
            this.Position = position;
            this.PositionVariance = positionVariance;
        }
        public void SetVelocity(Vector3 velocity, Vector3 velocityVariance)
        {
            this.Velocity = velocity;
            this.VelocityVariance = velocityVariance;
        }
        public void SetAcceleration(Vector3 acceleration, Vector3 accelerationVariance)
        {
            this.Acceleration = acceleration;
            this.AccelerationVariance = accelerationVariance;
        }
        public void SetRotationPerParticle(float speedMin, float speedMax)
        {
            this.RotationPerParticleSpeedMin = speedMin;
            this.RotationPerParticleSpeedMax = speedMax;
        }
        public void SetRotation(float speedMin, float speedMax, Vector3 axis, Vector3 axisVariance)
        {
            this.RotationSpeedMin = speedMin;
            this.RotationSpeedMax = speedMax;
            this.RotationAxis = axis;
            this.RotationAxisVariance = axisVariance;
        }
        public void SetOrbit(bool orbitPosition, bool orbitVelocity, bool orbitAcceleration)
        {
            this.OrbitPosition = orbitPosition;
            this.OrbitVelocity = orbitVelocity;
            this.OrbitAcceleration = orbitAcceleration;
        }

        private void GenerateVector(Vector3 bse, Vector3 variance, ref Vector3 dst, bool ellipsoid)
        {
            if (ellipsoid)
            {
                this.GenerateVectorInEllipsoid(bse, variance, ref  dst);
            }
            else
            {
                this.GenerateVectorInRect(bse, variance, ref  dst);
            }
        }
        private void GenerateVectorInRect(Vector3 bse, Vector3 variance, ref Vector3 dst)
        {
            dst.X = bse.X + variance.X * rnd.NextFloat(-1, 1);
            dst.Y = bse.Y + variance.Y * rnd.NextFloat(-1, 1);
            dst.Z = bse.Z + variance.Z * rnd.NextFloat(-1, 1);
        }
        private void GenerateVectorInEllipsoid(Vector3 center, Vector3 scale, ref Vector3 dst)
        {
            // Generate a point within a unit cube, then reject if the point is not in a unit sphere.
            do
            {
                dst.X = rnd.NextFloat(-1, 1);
                dst.Y = rnd.NextFloat(-1, 1);
                dst.Z = rnd.NextFloat(-1, 1);
            } while (dst.Length() > 1.0f);

            // Scale this point by the scaling vector.
            dst.X *= scale.X;
            dst.Y *= scale.Y;
            dst.Z *= scale.Z;

            // Translate by the center point.
            dst += center;
        }
        private void GenerateColor(Color4 bse, Color4 variance, ref Color4 dst)
        {
            dst.Red = bse.Red + variance.Red * rnd.NextFloat(-1, 1);
            dst.Green = bse.Green + variance.Green * rnd.NextFloat(-1, 1);
            dst.Blue = bse.Blue + variance.Blue * rnd.NextFloat(-1, 1);
            dst.Alpha = bse.Alpha + variance.Alpha * rnd.NextFloat(-1, 1);
        }
    }
}
