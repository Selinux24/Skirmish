
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Geometry shader description
    /// </summary>
    public class EngineGeometryShader : EngineShader<GeometryShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineGeometryShader(string name, GeometryShader vertexShader, byte[] byteCode) : base(name, vertexShader, byteCode) { }
    }
}
