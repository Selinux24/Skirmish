
namespace Engine.Common
{
    /// <summary>
    /// Mesh by material collection
    /// </summary>
    public class MeshByMaterialCollection : DrawingDataCollection<IMesh>
    {
        /// <summary>
        /// Initializes the mesh list
        /// </summary>
        /// <param name="name">Owner name</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        public void Initialize(string name, BufferManager bufferManager, bool dynamicBuffers, BufferDescriptor instancingBuffer)
        {
            if (bufferManager == null)
            {
                return;
            }

            Logger.WriteTrace(nameof(MeshByMaterialCollection), $"{name} Processing Mesh Collection => {this}");

            foreach (var mesh in GetValues())
            {
                mesh.Initialize(name, bufferManager, dynamicBuffers, instancingBuffer);
            }
        }
        /// <summary>
        /// Disposes internal meshes
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        public void DisposeResources(BufferManager bufferManager)
        {
            if (bufferManager == null)
            {
                return;
            }

            foreach (var mesh in GetValues())
            {
                //Remove data from buffer manager
                bufferManager.RemoveVertexData(mesh.VertexBuffer);
                bufferManager.RemoveIndexData(mesh.IndexBuffer);

                mesh.Dispose();
            }

            Clear();
        }
    }
}
