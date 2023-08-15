
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Vertex shader description
    /// </summary>
    public class EngineVertexShader : EngineShader<VertexShader>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexShader">Vertex shader</param>
        /// <param name="byteCode">Shader byte code</param>
        internal EngineVertexShader(string name, VertexShader vertexShader, byte[] byteCode) : base(name, vertexShader, byteCode) { }
    }
}
