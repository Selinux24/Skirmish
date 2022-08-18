using SharpDX;
using System.Collections.Generic;

namespace Engine.BuiltInEffects
{
    using Engine.Common;

    /// <summary>
    /// Geometry drawer interface
    /// </summary>
    public interface IGeometryDrawer2
    {
        /// <summary>
        /// Update object data
        /// </summary>
        /// <param name="material">Material information</param>
        /// <param name="tintColor">Tint color</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animation">Animation information</param>
        void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation);
        /// <summary>
        /// Draws the specified meshes
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        void Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="vertexBuffer">Vertex buffer</param>
        /// <param name="drawCount">Primitive count</param>
        /// <param name="topology">Topology</param>
        void Draw(BufferManager bufferManager, BufferDescriptor vertexBuffer, int drawCount, Topology topology);
    }
}
