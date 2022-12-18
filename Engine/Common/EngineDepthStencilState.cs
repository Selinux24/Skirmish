using System;

namespace Engine.Common
{
    using DepthStencilState = SharpDX.Direct3D11.DepthStencilState;

    /// <summary>
    /// Engine depth stencil state
    /// </summary>
    public class EngineDepthStencilState : IDisposable
    {
        /// <summary>
        /// Internal depth stencil state
        /// </summary>
        private DepthStencilState state = null;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Description
        /// </summary>
        public EngineDepthStencilStateDescription Description { get; private set; }

        /// <summary>
        /// Creates a Z-buffer enabled for write depth-stencil state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Returns the Z-buffer enabled for write depth-stencil state</returns>
        public static EngineDepthStencilState WRzBufferEnabled(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,

                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,

                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                },
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(WRzBufferEnabled)}", desc);
        }
        /// <summary>
        /// Creates a Z-buffer disabled for write depth-stencil state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Returns the Z-buffer disabled for write depth-stencil state</returns>
        public static EngineDepthStencilState WRzBufferDisabled(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Never,

                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,

                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                },
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(WRzBufferDisabled)}", desc);
        }
        /// <summary>
        /// Creates a Z-buffer enabled for read depth-stencil state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the Z-buffer enabled for read depth-stencil state</returns>
        public static EngineDepthStencilState RDzBufferEnabled(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Less,
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(RDzBufferEnabled)}", desc);
        }
        /// <summary>
        /// Creates a Z-buffer disabled for read depth-stencil state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the Z-buffer disabled for read depth-stencil state</returns>
        public static EngineDepthStencilState RDzBufferDisabled(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Never,
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(RDzBufferDisabled)}", desc);
        }
        /// <summary>
        /// Creates a No depth, no stencil state
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the No depth, no stencil state</returns>
        public static EngineDepthStencilState None(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Never,

                IsStencilEnabled = false,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(None)}", desc);
        }
        /// <summary>
        /// Creates a depth state for shadow mapping
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the shadow mapping depth state</returns>
        public static EngineDepthStencilState ShadowMapping(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,

                IsStencilEnabled = false,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,

                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Equal,
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Equal,
                },
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(ShadowMapping)}", desc);
        }
        /// <summary>
        /// Creates a Depth-stencil state for volume marking
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the Depth-stencil state for volume marking</returns>
        /// <remarks>Value != 0 if object is inside of the current drawing volume</remarks>
        public static EngineDepthStencilState VolumeMarking(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Less,

                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0xFF,

                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                },
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(VolumeMarking)}", desc);
        }
        /// <summary>
        /// Creates a Depth-stencil state for volume drawing
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="name">Name</param>
        /// <returns>Creates the Depth-stencil state for volume drawing</returns>
        /// <remarks>Process pixels if stencil value != stencil reference</remarks>
        public static EngineDepthStencilState VolumeDrawing(Graphics graphics, string name)
        {
            var desc = new EngineDepthStencilStateDescription()
            {
                IsDepthEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Never,

                IsStencilEnabled = true,
                StencilReadMask = 0xFF,
                StencilWriteMask = 0x00,

                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.NotEqual,
                },

                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.NotEqual,
                },
            };

            return graphics.CreateDepthStencilState($"{name}.{nameof(VolumeDrawing)}", desc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="stencilState">Depth stencil state</param>
        internal EngineDepthStencilState(string name, DepthStencilState stencilState)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name), "A stencil state name must be specified.");
            state = stencilState ?? throw new ArgumentNullException(nameof(stencilState), "A stencil state must be specified.");
            state.DebugName = name;

            Description = (EngineDepthStencilStateDescription)state.Description;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineDepthStencilState()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                state?.Dispose();
                state = null;
            }
        }

        /// <summary>
        /// Gets the internal depth stencil state
        /// </summary>
        /// <returns>Returns the internal depth stencil state</returns>
        internal DepthStencilState GetDepthStencilState()
        {
            return state;
        }
    }
}
