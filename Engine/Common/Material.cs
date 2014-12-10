using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Common
{
    using Engine.Content;

    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public static readonly Material Default = new Material(MaterialContent.Default);

        public Color4 AmbientColor;
        public Color4 DiffuseColor;
        public Color4 EmissionColor;
        public float IndexOfRefraction;
        public Color4 ReflectiveColor;
        public float Reflectivity;
        public float Shininess;
        public Color4 SpecularColor;
        public float Transparency;
        public Color4 Transparent;

        public Material(MaterialContent effect)
        {
            this.AmbientColor = effect.AmbientColor;
            this.DiffuseColor = effect.DiffuseColor;
            this.EmissionColor = effect.EmissionColor;
            this.IndexOfRefraction = effect.IndexOfRefraction;
            this.ReflectiveColor = effect.ReflectiveColor;
            this.Reflectivity = effect.Reflectivity;
            this.Shininess = effect.Shininess;
            this.SpecularColor = effect.SpecularColor;
            this.Transparency = effect.Transparency;
            this.Transparent = effect.Transparent;
        }
    };
}
