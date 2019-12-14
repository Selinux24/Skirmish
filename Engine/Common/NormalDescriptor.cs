using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Normal descriptor
    /// </summary>
    public class NormalDescriptor
    {
        /// <summary>
        /// Normal component
        /// </summary>
        public Vector3 Normal { get; set; }
        /// <summary>
        /// Tangent component
        /// </summary>
        public Vector3 Tangent { get; set; }
        /// <summary>
        /// Binormal component
        /// </summary>
        public Vector3 Binormal { get; set; }

        /// <summary>
        /// Transforms the normal data
        /// </summary>
        /// <param name="transform">Transform matrix</param>
        public void Transform(Matrix transform)
        {
            this.Normal = Vector3.TransformNormal(this.Normal, transform);
            this.Tangent = Vector3.TransformNormal(this.Tangent, transform);
            this.Binormal = Vector3.TransformNormal(this.Binormal, transform);
        }
    }
}
