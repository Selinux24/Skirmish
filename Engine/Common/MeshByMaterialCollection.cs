
namespace Engine.Common
{
    /// <summary>
    /// Mesh by material collection
    /// </summary>
    public class MeshByMaterialCollection : DrawingDataCollection<Mesh>
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

            foreach ((_, Mesh mesh) in this)
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

            foreach ((_, Mesh mesh) in this)
            {
                //Remove data from buffer manager
                bufferManager?.RemoveVertexData(mesh.VertexBuffer);
                bufferManager?.RemoveIndexData(mesh.IndexBuffer);

                mesh.Dispose();
            }

            Clear();
        }
    }
}
