
namespace Engine.Common
{
    using Vortice.Direct3D11;

    /// <summary>
    /// Geometry shader description
    /// </summary>
    public class EngineGeometryShader : EngineShader<ID3D11GeometryShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineGeometryShader(string name, ID3D11GeometryShader vertexShader, byte[] byteCode) : base(name, vertexShader, byteCode) { }
    }
}
