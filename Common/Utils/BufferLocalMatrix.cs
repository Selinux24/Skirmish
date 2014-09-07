using System.Runtime.InteropServices;
using SharpDX;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferLocalMatrix
    {
        public Matrix LocalTransform;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferLocalMatrix));
            }
        }

        public BufferLocalMatrix(Matrix localTransform)
        {
            this.LocalTransform = Matrix.Transpose(localTransform);
        }
    }
}
