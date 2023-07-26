using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Command list interface
    /// </summary>
    public interface IEngineCommandList : IDisposable
    {
        /// <summary>
        /// Gets the internal command list
        /// </summary>
        /// <returns></returns>
        CommandList GetCommandList();
    }

    /// <summary>
    /// Command list
    /// </summary>
    public class EngineCommandList : IEngineCommandList
    {
        /// <summary>
        /// Internal command list
        /// </summary>
        private readonly CommandList commandList;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandList">Command list</param>
        internal EngineCommandList(CommandList commandList)
        {
            this.commandList = commandList ?? throw new ArgumentNullException(nameof(commandList));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineCommandList()
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
                commandList?.Dispose();
            }
        }

        /// <inheritdoc/>
        public CommandList GetCommandList()
        {
            return commandList;
        }
    }
}
