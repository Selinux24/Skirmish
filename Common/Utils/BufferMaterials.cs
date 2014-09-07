using System.Runtime.InteropServices;
using SharpDX;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferMaterials
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Color4 Reflect;
        public float Padding;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(BufferMaterials));
            }
        }
    };
}
