using Engine.BuiltIn;
using Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Vertex count
        /// </summary>
        private int vertexDrawCount = 0;

        /// <summary>
        /// Buffer manager
        /// </summary>
        protected BufferManager BufferManager = null;

        /// <summary>
        /// Buffer id
        /// </summary>
        public readonly int Id = 0;
        /// <summary>
        /// Foliage attached to buffer flag
        /// </summary>
        public bool Attached { get; protected set; }
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VertexBuffer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="name">Name</param>
        public FoliageBuffer(BufferManager bufferManager, string name)
        {
            BufferManager = bufferManager;
            Id = GetID();
            Attached = false;
            VertexBuffer = bufferManager.AddVertexData(string.Format("{1}.{0}", Id, name), true, new VertexBillboard[FoliagePatch.MAX]);
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
            BufferManager?.RemoveVertexData(VertexBuffer);
        }

        /// <summary>
        /// Attaches the specified patch to buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="data">Vertex data</param>
        public void WriteData(IEngineDeviceContext dc, BufferManager bufferManager, IEnumerable<VertexBillboard> data)
        {
            vertexDrawCount = 0;
            Attached = false;

            //Get the data
            if (!data.Any())
            {
                return;
            }

            //Attach data to buffer
            if (!bufferManager.WriteVertexBuffer(dc, VertexBuffer, data))
            {
                return;
            }

            vertexDrawCount = data.Count();
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

            return drawer.Draw(dc, BufferManager, new DrawOptions
            {
                VertexBuffer = VertexBuffer,
                VertexDrawCount = vertexDrawCount,
                Topology = Topology.PointList,
            });
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => Attached: {Attached}; {VertexBuffer}";
        }
    }
}
