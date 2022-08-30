using System.Collections.Generic;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in drawer interface
    /// </summary>
    public interface IBuiltInDrawer
    {
        /// <summary>
        /// Updates drawer mesh state
        /// </summary>
        /// <param name="state">Drawer state</param>
        void UpdateMesh(BuiltInDrawerMeshState state);
        /// <summary>
        /// Updates drawer material state
        /// </summary>
        /// <param name="state">Drawer state</param>
        void UpdateMaterial(BuiltInDrawerMaterialState state);

        /// <summary>
        /// Draws the specified meshes
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        /// <param name="instances">Number of instances</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes, int instances = 0, int startInstanceLocation = 0);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="options">Draw options</param>
        void Draw(BufferManager bufferManager, DrawOptions options);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="buffer">Vertex buffer</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="drawCount">Draw count</param>
        void Draw(IEngineVertexBuffer buffer, Topology topology, int drawCount);
    }
}
