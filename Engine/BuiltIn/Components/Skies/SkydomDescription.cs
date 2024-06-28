
namespace Engine.BuiltIn.Components.Skies
{
    /// <summary>
    /// Skydom descriptor
    /// </summary>
    public class SkydomDescription : CubemapDescription
    {
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="radius">Cube radius</param>
        public static new SkydomDescription Box(string texture, float radius)
        {
            return new SkydomDescription
            {
                CubicTexture = texture,
                Geometry = CubeMapGeometry.Box,
                Radius = radius,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="faceSize">Cross texture face size</param>
        /// <param name="radius">Cube radius</param>
        public static new SkydomDescription Box(string texture, int faceSize, float radius)
        {
            return new SkydomDescription
            {
                CubicTexture = texture,
                Geometry = CubeMapGeometry.Box,
                Faces = ComputeCubemapFaces(faceSize),
                Radius = radius,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="radius">Cube radius</param>
        public static new SkydomDescription Sphere(string texture, float radius)
        {
            return new SkydomDescription
            {
                CubicTexture = texture,
                Geometry = CubeMapGeometry.Sphere,
                Radius = radius,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="faceSize">Cross texture face size</param>
        /// <param name="radius">Cube radius</param>
        public static new SkydomDescription Sphere(string texture, int faceSize, float radius)
        {
            return new SkydomDescription
            {
                CubicTexture = texture,
                Geometry = CubeMapGeometry.Sphere,
                Faces = ComputeCubemapFaces(faceSize),
                Radius = radius,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="radius">Radius</param>
        /// <returns></returns>
        public static new SkydomDescription Hemispheric(string texture, float radius)
        {
            return Hemispheric(new[] { texture }, radius);
        }
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="textures">Texture file names</param>
        /// <param name="radius">Radius</param>
        /// <returns></returns>
        public static new SkydomDescription Hemispheric(string[] textures, float radius)
        {
            return new SkydomDescription
            {
                PlainTextures = textures,
                Geometry = CubeMapGeometry.Hemispheric,
                Radius = radius,
                IsCubic = false,
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkydomDescription()
            : base()
        {
            BlendMode = BlendModes.Opaque;

            ReverseFaces = true;
        }
    }
}
