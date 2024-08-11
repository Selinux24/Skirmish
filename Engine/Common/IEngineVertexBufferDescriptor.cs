
namespace Engine.Common
{
    public interface IEngineVertexBufferDescriptor : IEngineDescriptor
    {
        /// <summary>
        /// Vertex buffer binding index in the manager list
        /// </summary>
        int BufferBindingIndex { get; set; }
        /// <summary>
        /// Instancing buffer descriptor
        /// </summary>
        BufferDescriptor InstancingDescriptor { get; set; }

        /// <summary>
        /// Gets whether the vertex data is of the specified type
        /// </summary>
        /// <param name="vertexData">Vertex data</param>
        bool OfType<TData>() where TData : struct, IVertexData;

        /// <summary>
        /// Adds the input element to the internal input list
        /// </summary>
        void AddInputs();
        /// <summary>
        /// Clears the internal input list
        /// </summary>
        void ClearInputs();
        /// <summary>
        /// Sets the specified instancing input elements to the internal list
        /// </summary>
        /// <param name="instancingInputs">Instancing inputs</param>
        void SetInstancingInputs(EngineInputElement[] instancingInputs);

        /// <summary>
        /// Gets the input element collection
        /// </summary>
        /// <param name="instanced">Is instanced</param>
        EngineInputElement[] GetInput(bool instanced);

        /// <summary>
        /// Copies the descriptor
        /// </summary>
        IEngineVertexBufferDescriptor Copy();
    }
}
