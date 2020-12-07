
namespace Engine
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
        public static new SkydomDescription Default(string texture, float radius)
        {
            return new SkydomDescription
            {
                Texture = texture,
                Radius = radius,
            };
        }
        /// <summary>
        /// Gets the default skydom description
        /// </summary>
        /// <param name="texture">Texture file name</param>
        /// <param name="faceSize">Cross texture face size</param>
        /// <param name="radius">Cube radius</param>
        public static new SkydomDescription FromCrossTexture(string texture, int faceSize, float radius)
        {
            return new SkydomDescription
            {
                Texture = texture,
                Faces = ComputeCubemapFaces(faceSize),
                Radius = radius,
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
