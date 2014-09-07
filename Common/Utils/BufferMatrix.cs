using System.Runtime.InteropServices;
using SharpDX;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferMatrix
    {
        public Matrix World;
        public Matrix WorldInverse;
        public Matrix WorldViewProjection;
        public BufferMaterials Material;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferMatrix));
            }
        }
    }
}
