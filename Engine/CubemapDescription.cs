
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
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Texture
        /// </summary>
        public string Texture { get; set; }
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
            this.DepthEnabled = false;
        }
    }
}
