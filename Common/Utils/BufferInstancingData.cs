using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferInstancingData
    {
        public Matrix Local;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferInstancingData));
            }
        }
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

        public BufferInstancingData(Matrix local)
        {
            this.Local = local;
        }

        public void SetPosition(Vector3 position)
        {
            this.Local = Matrix.Translation(position);
        }
    };
}
