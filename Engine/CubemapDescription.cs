
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Cube-map description
    /// </summary>
    public class CubemapDescription : DrawableDescription
    {
        /// <summary>
        /// Cube map geometry enumeration
        /// </summary>
        public enum CubeMapGeometryEnum
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
        public string ContentPath = "Resources";
        /// <summary>
        /// Texture
        /// </summary>
        public string Texture;
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius;
        /// <summary>
        /// Cubemap geometry
        /// </summary>
        public CubeMapGeometryEnum Geometry = CubeMapGeometryEnum.Sphere;
        /// <summary>
        /// Reverse geometry faces
        /// </summary>
        public bool ReverseFaces = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public CubemapDescription()
            : base()
        {
            this.Static = true;
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.DepthEnabled = false;
            this.AlphaEnabled = false;
        }
    }
}
