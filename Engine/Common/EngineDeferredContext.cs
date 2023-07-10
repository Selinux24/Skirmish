using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Deferred context interface
    /// </summary>
    public interface IEngineDeferredContext : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Starts a command list
        /// </summary>
        /// <remarks>
        /// Clears the deferred context state
        /// </remarks>
        void StartCommandList();
        /// <summary>
        /// Finish a command list
        /// </summary>
        /// <param name="restoreState">Resore state</param>
        IEngineCommandList FinishCommandList(bool restoreState = false);
    }

    /// <summary>
    /// Deferred context
    /// </summary>
    public class EngineDeferredContext : IEngineDeferredContext
    {
        /// <summary>
        /// Internal deferred context
        /// </summary>
        private readonly DeviceContext3 deviceContext;

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="deviceContext">Deferred device context</param>
        /// <param name="name">Name</param>
        internal EngineDeferredContext(DeviceContext3 deviceContext, string name)
        {
            this.deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext), "A deferred device context must be specified.");
            Name = name ?? throw new ArgumentNullException(nameof(name), "A deferred device name must be specified.");
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineDeferredContext()
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
                deviceContext?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void StartCommandList()
        {
            deviceContext.ClearState();
        }
        /// <inheritdoc/>
        public IEngineCommandList FinishCommandList(bool restoreState = false)
        {
            var cmdList = deviceContext.FinishCommandList(restoreState);

            return new EngineCommandList(cmdList);
        }
    }
}
