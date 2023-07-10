using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Command list interface
    /// </summary>
    public interface IEngineCommandList
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

        /// <inheritdoc/>
        public CommandList GetCommandList()
        {
            return commandList;
        }
    }
}
