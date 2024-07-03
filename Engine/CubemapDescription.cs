using SharpDX;

namespace Engine
{
    /// <summary>
    /// Cube-map description
    /// </summary>
    public class CubemapDescription : SceneObjectDescription
    {
        /// <summary>
        /// Gets the default cube map description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="radius">Radius</param>
        public static CubemapDescription Box(string texture, float radius)
        {
            return new CubemapDescription
            {
                CubicTexture = texture,
                Geometry = CubeMapGeometry.Box,
                Radius = radius,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Gets the default cube map description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="faceSize">Cross texture face size</param>
        /// <param name="radius">Radius</param>
        public static CubemapDescription Box(string texture, int faceSize, float radius)
        {
            return new CubemapDescription
            {
                CubicTexture = texture,
                Geometry = CubeMapGeometry.Box,
                Faces = ComputeCubemapFaces(faceSize),
                Radius = radius,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Gets the default cube map description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="radius">Radius</param>
        public static CubemapDescription Sphere(string texture, float radius)
        {
            return new CubemapDescription
            {
                CubicTexture = texture,
                Geometry = CubeMapGeometry.Sphere,
                Radius = radius,
                IsCubic = true,
            };
        }
        /// <summary>
        /// Gets the default cube map description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="faceSize">Cross texture face size</param>
        /// <param name="radius">Radius</param>
        public static CubemapDescription Sphere(string texture, int faceSize, float radius)
        {
            return new CubemapDescription
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
        public static CubemapDescription Hemispheric(string texture, float radius)
        {
            return Hemispheric([texture], radius);
        }
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="textures">Texture file names</param>
        /// <param name="radius">Radius</param>
        /// <returns></returns>
        public static CubemapDescription Hemispheric(string[] textures, float radius)
        {
            return new CubemapDescription
            {
                PlainTextures = textures,
                Geometry = CubeMapGeometry.Hemispheric,
                Radius = radius,
                IsCubic = false,
            };
        }
        /// <summary>
        /// Computes the cubmap faces
        /// </summary>
        /// <param name="faceSize">Face size</param>
        public static Rectangle[] ComputeCubemapFaces(int faceSize)
        {
            return
            [
                new (faceSize * 2, faceSize * 1, faceSize, faceSize), //Right
                new (faceSize * 0, faceSize * 1, faceSize, faceSize), //Left
                new (faceSize * 1, faceSize * 0, faceSize, faceSize), //Top
                new (faceSize * 1, faceSize * 2, faceSize, faceSize), //Bottom
                new (faceSize * 1, faceSize * 1, faceSize, faceSize), //Front
                new (faceSize * 3, faceSize * 1, faceSize, faceSize), //Back
            ];
        }

        /// <summary>
        /// Cubic texture
        /// </summary>
        public bool IsCubic { get; set; } = true;
        /// <summary>
        /// Texture
        /// </summary>
        public string CubicTexture { get; set; }
        /// <summary>
        /// Cube faces
        /// </summary>
        /// <remarks>
        /// Cube map texture faces defined as rectangles, for cross style texures:
        /// Index 0: Right face
        /// Index 1: Left face
        /// Index 2: Top face
        /// Index 3: Bottom face
        /// Index 4: Front face
        /// Index 5: Back face
        /// </remarks>
        public Rectangle[] Faces { get; set; } = [];

        /// <summary>
        /// Plain texture list
        /// </summary>
        public string[] PlainTextures { get; set; }

        /// <summary>
        /// Cubemap geometry
        /// </summary>
        public CubeMapGeometry Geometry { get; set; } = CubeMapGeometry.Sphere;
        /// <summary>
        /// Reverse geometry faces
        /// </summary>
        public bool ReverseFaces { get; set; } = false;

        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CubemapDescription()
            : base()
        {
            DepthEnabled = false;
        }
    }
}
