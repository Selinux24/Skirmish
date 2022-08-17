﻿using SharpDX;
using System.Collections.Generic;

namespace Engine.Effects
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
        /// <param name="animation">Animation information</param>
        /// <param name="material">Material information</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="tintColor">Tint color</param>
        void Update(
            AnimationDrawInfo animation,
            MaterialDrawInfo material,
            uint textureIndex,
            Color4 tintColor);

        /// <summary>
        /// Draws the specified meshes shadow maps
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="meshes">Mesh list</param>
        void DrawShadows(BufferManager bufferManager, IEnumerable<Mesh> meshes);
        /// <summary>
        /// Draws the specified vertex buffer shadow maps
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="vertexBuffer">Vertex buffer</param>
        /// <param name="drawCount">Primitive count</param>
        /// <param name="topology">Topology</param>
        void DrawShadows(BufferManager bufferManager, BufferDescriptor vertexBuffer, int drawCount, Topology topology);

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
