using SharpDX;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Helpers;

    public class CPUParticleSystem : IDisposable
    {
        protected CPUParticleSystemDescription Settings = new CPUParticleSystemDescription();

        private VertexCPUParticle[] particles;
        private Buffer vertexBuffer;
        private VertexBufferBinding[] vertexBufferBinding;

        private int firstActiveParticle;
        private int firstNewParticle;
        private int firstFreeParticle;
        private int firstRetiredParticle;
        private Random rnd = new Random();

        public CPUParticleSystemTypes ParticleType
        {
            get
            {
                return this.Settings.ParticleType;
            }
        }
        public float TotalTime;
        public ShaderResourceView Texture;
        public uint TextureCount;
        public int VertexCount
        {
            get
            {
                return this.particles.Length;
            }
        }

        public float MaxDuration { get { return this.Settings.MaxDuration; } }
        public float MaxDurationRandomness { get { return this.Settings.MaxDurationRandomness; } }
        public float EndVelocity { get { return this.Settings.EndVelocity; } }
        public Vector3 Gravity { get { return this.Settings.Gravity; } }
        public Vector2 StartSize { get { return new Vector2(this.Settings.MinStartSize, this.Settings.MaxStartSize); } }
        public Vector2 EndSize { get { return new Vector2(this.Settings.MinEndSize, this.Settings.MaxEndSize); } }
        public Color MinColor { get { return this.Settings.MinColor; } }
        public Color MaxColor { get { return this.Settings.MaxColor; } }
        public Vector2 RotateSpeed { get { return new Vector2(this.Settings.MinRotateSpeed, this.Settings.MaxRotateSpeed); } }

        public CPUParticleSystem(Game game, CPUParticleSystemDescription description)
        {
            this.Settings = description;

            ImageContent imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.TextureName),
            };
            this.Texture = game.ResourceManager.CreateResource(imgContent);
            this.TextureCount = (uint)imgContent.Count;

            this.particles = new VertexCPUParticle[description.MaxParticles];

            this.vertexBuffer = game.Graphics.Device.CreateVertexBufferWrite(this.particles);
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, default(VertexCPUParticle).Stride, 0),
            };
        }
        public void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
        }

        public void Update()
        {

        }
        public void Draw(Game game)
        {
            game.Graphics.DeviceContext.Draw(1, 0);

            Counters.DrawCallsPerFrame++;
            Counters.InstancesPerFrame++;
            Counters.TrianglesPerFrame += 2 * 1;
        }

        public void AddParticle(Game game, Vector3 position, Vector3 velocity)
        {
            int nextFreeParticle = this.firstFreeParticle + 1;

            if (nextFreeParticle >= this.particles.Length)
            {
                nextFreeParticle = 0;
            }

            if (nextFreeParticle == this.firstRetiredParticle)
            {
                return;
            }

            velocity *= this.Settings.EmitterVelocitySensitivity;

            float horizontalVelocity = MathUtil.Lerp(
                this.Settings.MinHorizontalVelocity,
                this.Settings.MaxHorizontalVelocity,
                this.rnd.NextFloat(0, 1));

            double horizontalAngle = this.rnd.NextDouble() * MathUtil.TwoPi;

            velocity.X += horizontalVelocity * (float)Math.Cos(horizontalAngle);
            velocity.Z += horizontalVelocity * (float)Math.Sin(horizontalAngle);

            velocity.Y += MathUtil.Lerp(
                this.Settings.MinVerticalVelocity,
                this.Settings.MaxVerticalVelocity,
                this.rnd.NextFloat(0, 1));

            Color randomValues = new Color(
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1),
                this.rnd.NextFloat(0, 1));

            this.particles[this.firstFreeParticle].Position = position;
            this.particles[this.firstFreeParticle].Velocity = velocity;
            this.particles[this.firstFreeParticle].Color = randomValues;
            this.particles[this.firstFreeParticle].MaxAge = this.TotalTime;

            this.firstFreeParticle = nextFreeParticle;

            game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, this.particles);
        }
        private void RetireActiveParticles()
        {
            float particleDuration = this.Settings.MaxDuration;

            while (this.firstActiveParticle != this.firstNewParticle)
            {
                float particleAge = this.TotalTime - this.particles[this.firstActiveParticle].MaxAge;

                if (particleAge < particleDuration)
                {
                    break;
                }

                this.particles[this.firstActiveParticle].MaxAge = this.TotalTime;

                this.firstActiveParticle++;

                if (this.firstActiveParticle >= this.particles.Length)
                {
                    this.firstActiveParticle = 0;
                }
            }
        }
        private void FreeRetiredParticles()
        {
            while (this.firstRetiredParticle != this.firstActiveParticle)
            {
                float age = this.TotalTime - (int)this.particles[this.firstRetiredParticle].MaxAge;

                if (age < 3)
                {
                    break;
                }

                this.firstRetiredParticle++;

                if (this.firstRetiredParticle >= this.particles.Length)
                {
                    this.firstRetiredParticle = 0;
                }
            }
        }

        public void SetBuffer(Game game)
        {
            game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            Counters.IAVertexBuffersSets++;
            game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, SharpDX.DXGI.Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;
        }

        private Vector3 ComputeParticlePosition(Vector3 position, Vector3 velocity, float age, float normalizedAge, float gEndVelocity, float gDuration, Vector3 gGravity)
        {
            float startVelocity = velocity.Length();
            float endVelocity = startVelocity * gEndVelocity;
            float velocityIntegral = startVelocity * normalizedAge + (endVelocity - startVelocity) * normalizedAge * normalizedAge * 0.5f;

            position += Vector3.Normalize(velocity) * velocityIntegral;
            position += gGravity * age * normalizedAge;

            return position;
        }
        private Vector2 ComputeParticleSize(Vector3 position, float randomValue, float normalizedAge, Vector2 gStartSize, Vector2 gEndSize, Matrix gWorldViewProjection, float gViewportHeight)
        {
            float startSize = MathUtil.Lerp(gStartSize.X, gStartSize.Y, randomValue);
            float endSize = MathUtil.Lerp(gEndSize.X, gEndSize.Y, randomValue);

            float size = MathUtil.Lerp(startSize, endSize, normalizedAge);

            return new Vector2(size, size);
        }
        private Color4 ComputeParticleColor(float randomValue, float normalizedAge, Color4 gMinColor, Color4 gMaxColor)
        {
            Color4 color = Color4.Lerp(gMinColor, gMaxColor, randomValue);

            color.Alpha *= normalizedAge * (1 - normalizedAge) * (1 - normalizedAge) * 6.7f;

            return color;
        }
        private Vector4 ComputeParticleRotation(float randomValue, float age, Vector2 gRotateSpeed)
        {
            float rotateSpeed = MathUtil.Lerp(gRotateSpeed.X, gRotateSpeed.Y, randomValue);

            float rotation = rotateSpeed * age;

            float c = (float)Math.Cos(rotation);
            float s = (float)Math.Sin(rotation);

            Vector4 rotationMatrix = new Vector4(c, -s, s, c);

            rotationMatrix *= 0.5f;
            rotationMatrix += 0.5f;

            return rotationMatrix;
        }
        public void Test(int index, Matrix world, Matrix viewProj, Vector3 eyePos, float vpHeight)
        {
            Matrix gWorld = world;
            Matrix gWorldViewProjection = world * viewProj;
            float gViewportHeight = vpHeight;
            Vector3 gEyePositionWorld = eyePos;
            float gTotalTime = this.TotalTime;
            //uint gTextureCount = 1;

            float gMaxDuration = this.MaxDuration;
            float gMaxDurationRandomness = this.MaxDurationRandomness;
            float gEndVelocity = this.EndVelocity;
            Vector3 gGravity = this.Gravity;
            Vector2 gStartSize = this.StartSize;
            Vector2 gEndSize = this.EndSize;
            Color4 gMinColor = this.MinColor;
            Color4 gMaxColor = this.MaxColor;
            Vector2 gRotateSpeed = this.RotateSpeed;

            var input = this.particles;

            float age = gTotalTime - input[0].MaxAge;
            age *= 1 + input[0].Color.Red * gMaxDurationRandomness;
            float normalizedAge = Math.Min(age / gMaxDuration, 1f);

            Vector3 centerWorld = ComputeParticlePosition(input[0].Position, input[0].Velocity, age, normalizedAge, gEndVelocity, gMaxDuration, gGravity);
            Vector2 sizeWorld = ComputeParticleSize(input[0].Position, input[0].Color.Green, normalizedAge, gStartSize, gEndSize, gWorldViewProjection, gViewportHeight);
            Color4 color = ComputeParticleColor(input[0].Color.Blue, normalizedAge, gMinColor, gMaxColor);
            Vector4 rotationWorld = ComputeParticleRotation(input[0].Color.Alpha, age, gRotateSpeed);

            Vector3 look = gEyePositionWorld - centerWorld;
            look.Y = 0.0f; // y-axis aligned, so project to xz-plane
            look = Vector3.Normalize(look);
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 right = Vector3.Cross(up, look);

            //Compute triangle strip vertices (quad) in world space.
            float halfWidth = 0.5f * sizeWorld.X;
            float halfHeight = 0.5f * sizeWorld.Y;
            Vector4[] v = new Vector4[4];
            v[0] = new Vector4(centerWorld + halfWidth * right - halfHeight * up, 1.0f);
            v[1] = new Vector4(centerWorld + halfWidth * right + halfHeight * up, 1.0f);
            v[2] = new Vector4(centerWorld - halfWidth * right - halfHeight * up, 1.0f);
            v[3] = new Vector4(centerWorld - halfWidth * right + halfHeight * up, 1.0f);
        }
    }
}
