using Engine.Common;

namespace Engine
{
    /// <summary>
    /// Drawer interface
    /// </summary>
    public interface IDrawer
    {
        /// <summary>
        /// Updates drawer casting light
        /// </summary>
        /// <param name="context">Context</param>
        void UpdateCastingLight(DrawContextShadows context);
        /// <summary>
        /// Updates drawer mesh state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Drawer state</param>
        void UpdateMesh(IEngineDeviceContext dc, IDrawerMeshState state);
        /// <summary>
        /// Updates drawer material state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Drawer state</param>
        void UpdateMaterial(IEngineDeviceContext dc, IDrawerMaterialState state);

        /// <summary>
        /// Draws the specified meshes
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="meshes">Mesh list</param>
        /// <param name="instances">Number of instances</param>
        /// <param name="startInstanceLocation">Start instance location</param>
        bool Draw(IEngineDeviceContext dc, Mesh[] meshes, int instances = 0, int startInstanceLocation = 0);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="options">Draw options</param>
        bool Draw(IEngineDeviceContext dc, DrawOptions options);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="buffer">Vertex buffer</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="drawCount">Draw count</param>
        bool Draw(IEngineDeviceContext dc, IEngineVertexBuffer buffer, Topology topology, int drawCount);
        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="bufferSlot">Buffer slot</param>
        /// <param name="vertexBufferBinding">Vertex binding</param>
        /// <param name="indexBuffer">Index buffer</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="count">Draw count</param>
        /// <param name="startLocation">Start location</param>
        bool Draw(IEngineDeviceContext dc, int bufferSlot, EngineVertexBufferBinding vertexBufferBinding, EngineBuffer indexBuffer, Topology topology, int count, int startLocation);

        /// <summary>
        /// Draws the specified vertex buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="buffer">Vertex buffer</param>
        /// <param name="topology">Primitive topology</param>
        bool DrawAuto(IEngineDeviceContext dc, IEngineVertexBuffer buffer, Topology topology);

        /// <summary>
        /// Streams-out the specified buffer into the stream-out buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="firstRun">First run</param>
        /// <param name="buffer">Data buffer</param>
        /// <param name="streamOutBuffer">Stream-out target buffer</param>
        /// <param name="topology">Primitive topology</param>
        void StreamOut(IEngineDeviceContext dc, bool firstRun, IEngineVertexBuffer buffer, IEngineVertexBuffer streamOutBuffer, Topology topology);
    }
}
