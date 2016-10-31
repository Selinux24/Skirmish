using SharpDX;
using System;
using Buffer = SharpDX.Direct3D11.Buffer;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    /// <summary>
    /// Particle emitter
    /// </summary>
    public class ParticleEmitter : IDisposable
    {
        public int ParticleCountMax;
        public float EmissionRate;

        public bool OrbitPosition;
        public bool OrbitVelocity;
        public bool OrbitAcceleration;
        public bool Ellipsoid;

        public Vector3 Position;
        public Vector3 PositionVariance;
        public Vector3 Velocity;
        public Vector3 VelocityVariance;
        public Vector3 Acceleration;
        public Vector3 AccelerationVariance;
        public Color4 ColorStart;
        public Color4 ColorStartVariance;
        public Color4 ColorEnd;
        public Color4 ColorEndVariance;
        public float RotationParticleSpeedMin;
        public float RotationParticleSpeedMax;
        public Vector3 RotationAxis;
        public Vector3 RotationAxisVariance;
        public float RotationSpeedMin;
        public float RotationSpeedMax;
        public float Angle;
        public float EnergyMin;
        public float EnergyMax;
        public float SizeStartMin;
        public float SizeStartMax;
        public float SizeEndMin;
        public float SizeEndMax;

        /// <summary>
        /// First stream out flag
        /// </summary>
        public bool FirstRun = true;

        /// <summary>
        /// Emitter initialization buffer
        /// </summary>
        public Buffer EmittersBuffer;
        /// <summary>
        /// Drawing buffer
        /// </summary>
        public Buffer DrawingBuffer;
        /// <summary>
        /// Stream out buffer
        /// </summary>
        public Buffer StreamOutBuffer;
        /// <summary>
        /// Stride
        /// </summary>
        public int InputStride;

        public uint TextureCount;
        /// <summary>
        /// Textures
        /// </summary>
        public ShaderResourceView TextureArray;

        /// <summary>
        /// Toggle stream out and drawing buffers
        /// </summary>
        public void ToggleBuffers()
        {
            Buffer temp = this.DrawingBuffer;
            this.DrawingBuffer = this.StreamOutBuffer;
            this.StreamOutBuffer = temp;
        }

        public void Dispose()
        {
            Helper.Dispose(this.EmittersBuffer);
            Helper.Dispose(this.DrawingBuffer);
            Helper.Dispose(this.StreamOutBuffer);
        }
    }
}
