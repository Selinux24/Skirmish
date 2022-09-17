﻿using System.Collections.Generic;

namespace Engine.BuiltIn
{
    using Engine.Common;

    /// <summary>
    /// Built-in drawer interface
    /// </summary>
    public interface IBuiltInDrawer
    {
        /// <summary>
        /// Updates drawer casting light
        /// </summary>
        /// <param name="context">Context</param>
        void UpdateCastingLight(DrawContextShadows context);

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
        bool Draw(BufferManager bufferManager, IEnumerable<Mesh> meshes, int instances = 0, int startInstanceLocation = 0);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="options">Draw options</param>
        bool Draw(BufferManager bufferManager, DrawOptions options);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="buffer">Vertex buffer</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="drawCount">Draw count</param>
        bool Draw(IEngineVertexBuffer buffer, Topology topology, int drawCount);

        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="buffer">Vertex buffer</param>
        /// <param name="topology">Primitive topology</param>
        bool DrawAuto(IEngineVertexBuffer buffer, Topology topology);

        /// <summary>
        /// Streams-out the specified buffer into the stream-out buffer
        /// </summary>
        /// <param name="firstRun">First run</param>
        /// <param name="buffer">Data buffer</param>
        /// <param name="streamOutBuffer">Stream-out target buffer</param>
        /// <param name="topology">Primitive topology</param>
        void StreamOut(bool firstRun, IEngineVertexBuffer buffer, IEngineVertexBuffer streamOutBuffer, Topology topology);
    }
}
