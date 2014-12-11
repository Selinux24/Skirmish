using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    [StructLayout(LayoutKind.Sequential)]
    public struct BufferMaterials : IBuffer
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

        public void SetMaterial(Material material)
        {
            this.Ambient = material.AmbientColor;
            this.Diffuse = material.DiffuseColor;
            this.Specular = material.SpecularColor;
            this.Specular.Alpha = material.Shininess;
            this.Reflect = material.ReflectiveColor;
            this.Padding = 1000f;
        }

        public int Stride
        {
            get
            {
                return SizeInBytes;
            }
        }
    };
}
