using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace Engine.Common
{
    /// <summary>
    /// Particle data buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleData : IBufferData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                //TODO: Verify
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("VELOCITY", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("SIZE", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
                new InputElement("AGE", 0, SharpDX.DXGI.Format.R32_Float, 32, 0, InputClassification.PerVertexData, 0),
                new InputElement("CLASS", 0, SharpDX.DXGI.Format.R32_UInt, 36, 0, InputClassification.PerVertexData, 0),
                new InputElement("TYPE", 0, SharpDX.DXGI.Format.R32_UInt, 40, 0, InputClassification.PerVertexData, 0),
            };
        }

        public Vector3 InitialPositionWorld;
        public Vector3 InitialVelocityWorld;
        public Vector2 SizeWorld;
        public float Age;
        public uint Class;
        public uint Type;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(ParticleData));
            }
        }
    }
}
