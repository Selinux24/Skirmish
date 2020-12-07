using SharpDX;

namespace Engine
{
    /// <summary>
    /// Cube-map description
    /// </summary>
    public class CubemapDescription : SceneObjectDescription
    {
        /// <summary>
        /// Cube map geometry enumeration
        /// </summary>
        public enum CubeMapGeometry
        {
            /// <summary>
            /// Box
            /// </summary>
            Box,
            /// <summary>
            /// Sphere
            /// </summary>
            Sphere,
        }

        /// <summary>
        /// Gets the default cube map description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="radius">Cube radius</param>
        public static CubemapDescription Default(string texture, float radius)
        {
            return new CubemapDescription
            {
                Texture = texture,
                Radius = radius,
            };
        }
        /// <summary>
        /// Gets the default cube map description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="faceSize">Cross texture face size</param>
        /// <param name="radius">Cube radius</param>
        public static CubemapDescription FromCrossTexture(string texture, int faceSize, float radius)
        {
            return new CubemapDescription
            {
                Texture = texture,
                Faces = ComputeCubemapFaces(faceSize),
                Radius = radius,
            };
        }
        /// <summary>
        /// Computes the cubmap faces
        /// </summary>
        /// <param name="faceSize">Face size</param>
        public static Rectangle[] ComputeCubemapFaces(int faceSize)
        {
            return new[]
            {
                new Rectangle(faceSize * 2, faceSize * 1, faceSize, faceSize), //Right
                new Rectangle(faceSize * 0, faceSize * 1, faceSize, faceSize), //Left
                new Rectangle(faceSize * 1, faceSize * 0, faceSize, faceSize), //Top
                new Rectangle(faceSize * 1, faceSize * 2, faceSize, faceSize), //Bottom
                new Rectangle(faceSize * 1, faceSize * 1, faceSize, faceSize), //Front
                new Rectangle(faceSize * 3, faceSize * 1, faceSize, faceSize), //Back
            };
        }

        /// <summary>
        /// Texture
        /// </summary>
        public string Texture { get; set; }
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
        public Rectangle[] Faces { get; set; } = new Rectangle[] { };
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Cubemap geometry
        /// </summary>
        public CubeMapGeometry Geometry { get; set; } = CubeMapGeometry.Sphere;
        /// <summary>
        /// Reverse geometry faces
        /// </summary>
        public bool ReverseFaces { get; set; } = false;

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
