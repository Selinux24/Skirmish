using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace Engine.Common
{
    /// <summary>
    /// Instancing data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexInstancingData : IBufferData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("localTransform", 0, Format.R32G32B32A32_Float, 0, 1, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 1, Format.R32G32B32A32_Float, 16, 1, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 2, Format.R32G32B32A32_Float, 32, 1, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 3, Format.R32G32B32A32_Float, 48, 1, InputClassification.PerInstanceData, 1),
            };
        }

        /// <summary>
        /// Local transformation for the instance
        /// </summary>
        public Matrix Local;
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexInstancingData));
            }
        }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="local">Local transform</param>
        public VertexInstancingData(Matrix local)
        {
            this.Local = local;
        }
    };
}
