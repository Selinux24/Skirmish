using Engine.BuiltIn;
using Engine.Common;
using System;

namespace Engine
{
    /// <summary>
    /// Foliage buffer
    /// </summary>
    class FoliageBuffer : IDisposable
    {
        /// <summary>
        /// Foliage buffer id static counter
        /// </summary>
        private static int ID = 0;
        /// <summary>
        /// Gets the next instance Id
        /// </summary>
        /// <returns>Returns the next Instance Id</returns>
        private static int GetID()
        {
            return ++ID;
        }

        /// <summary>
        /// Buffer name
        /// </summary>
        private readonly string name;
        /// <summary>
        /// Buffer manager
        /// </summary>
        private readonly BufferManager bufferManager = null;
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private readonly BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Vertex count
        /// </summary>
        private int vertexDrawCount = 0;

        /// <summary>
        /// Foliage attached to buffer flag
        /// </summary>
        public bool Attached { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="name">Name</param>
        public FoliageBuffer(BufferManager bufferManager, string name)
        {
            ArgumentNullException.ThrowIfNull(bufferManager);
            this.bufferManager = bufferManager;

            int id = GetID();
            this.name = $"{name ?? nameof(FoliageBuffer)}.{id}";

            Attached = false;
            vertexBuffer = this.bufferManager.AddVertexData(this.name, true, new VertexBillboard[FoliagePatch.MAX]);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~FoliageBuffer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Resource disposal
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            //Remove data from buffer manager
            bufferManager?.RemoveVertexData(vertexBuffer);
        }

        /// <summary>
        /// Attaches the specified patch to buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="data">Vertex data</param>
        public void WriteData(IEngineDeviceContext dc, BufferManager bufferManager, VertexBillboard[] data)
        {
            vertexDrawCount = 0;
            Attached = false;

            //Get the data
            if (data.Length <= 0)
            {
                return;
            }

            //Attach data to buffer
            if (!bufferManager.WriteVertexBuffer(dc, vertexBuffer, data, false))
            {
                return;
            }

            vertexDrawCount = data.Length;
            Attached = true;
        }
        /// <summary>
        /// Frees the buffer
        /// </summary>
        public void Free()
        {
            vertexDrawCount = 0;
            Attached = false;
        }
        /// <summary>
        /// Draws the foliage data
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="drawer">Drawer</param>
        public bool DrawFoliage(IEngineDeviceContext dc, BuiltInDrawer drawer)
        {
            if (vertexDrawCount <= 0)
            {
                return false;
            }

            return drawer.Draw(dc, bufferManager, new()
            {
                VertexBuffer = vertexBuffer,
                VertexDrawCount = vertexDrawCount,
                Topology = Topology.PointList,
            });
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{name} => Attached: {Attached}; {vertexBuffer}";
        }
    }
}
