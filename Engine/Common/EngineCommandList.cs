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
        /// Name
        /// </summary>
        string Name { get; }
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

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="commandList">Command list</param>
        internal EngineCommandList(string name, CommandList commandList)
        {
            Name = name;
            this.commandList = commandList ?? throw new ArgumentNullException(nameof(commandList));

            this.commandList.DebugName = name;
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name ?? nameof(EngineCommandList)}";
        }
    }
}
