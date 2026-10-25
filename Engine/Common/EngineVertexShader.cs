
namespace Engine.Common
{
    using Vortice.Direct3D11;

    /// <summary>
    /// Vertex shader description
    /// </summary>
    public class EngineVertexShader : EngineShader<ID3D11VertexShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineVertexShader(string name, ID3D11VertexShader vertexShader, byte[] byteCode) : base(name, vertexShader, byteCode) { }
    }
}
