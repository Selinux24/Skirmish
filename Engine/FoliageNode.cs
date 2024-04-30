using Engine.Common;
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Foliage node data
    /// </summary>
    class FoliageNode
    {
        /// <summary>
        /// Patch
        /// </summary>
        public FoliagePatch Patch { get; private set; } = new();
        /// <summary>
        /// Assigned buffer
        /// </summary>
        public FoliageBuffer Buffer { get; private set; }

        /// <summary>
        /// The node is ready to assign a buffer
        /// </summary>
        public bool IsReadyToAssign()
        {
            if (Patch == null || !Patch.Planted || !Patch.HasData)
            {
                //The patch is not ready
                return false;
            }

            if (Buffer != null && Buffer.Attached)
            {
                //The buffer is already atached
                return false;
            }

            return true;
        }
        /// <summary>
        /// The node is ready to write data into device
        /// </summary>
        public bool IsReadyToWrite()
        {
            if (!Patch.HasData)
            {
                return false;
            }

            if (Buffer == null || Buffer.Attached)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// The node is ready to draw
        /// </summary>
        public bool IsReadyToDraw()
        {
            if (!Patch.HasData)
            {
                return false;
            }

            if (Buffer == null || !Buffer.Attached)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the assigned buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public void SetBuffer(FoliageBuffer buffer)
        {
            Buffer = buffer;
        }
        /// <summary>
        /// Writes the data into the specified device context
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="transparent">Transparent data</param>
        public void WriteData(IEngineDeviceContext dc, BufferManager bufferManager, Vector3 eyePosition, bool transparent)
        {
            if (!IsReadyToWrite())
            {
                return;
            }

            var data = Patch.GetData(eyePosition, transparent);
            Buffer.WriteData(dc, bufferManager, data);
        }
        /// <summary>
        /// Frees the assigned buffer
        /// </summary>
        /// <param name="remove">Removes the buffer from the node</param>
        public void FreeBuffer(bool remove)
        {
            Buffer?.Free();
            if (remove)
            {
                Buffer = null;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Patch.Id} => {Buffer?.Attached ?? false}";
        }
    }
}
