using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphic command management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Graphics deferred context
        /// </summary>
        private readonly List<IEngineDeferredContext> deferredContextList = new();

        /// <summary>
        /// Creates a new deferred context
        /// </summary>
        /// <param name="name">Deferred context name</param>
        internal IEngineDeferredContext CreateDeferredContext(string name)
        {
            var dc = deferredContextList.Find(d => d.Name == name);
            if (dc != null)
            {
                return dc;
            }

            var deferredContext = new DeviceContext3(device)
            {
                DebugName = name,
            };

            dc = new EngineDeferredContext(deferredContext, name);

            deferredContextList.Add(dc);

            return dc;
        }
     
        /// <summary>
        /// Executes a command list in the immediate context
        /// </summary>
        /// <param name="commandList">Command list</param>
        /// <param name="restoreState">Resore state</param>
        public void ExecuteCommandList(IEngineCommandList commandList, bool restoreState = false)
        {
            immediateContext.ExecuteCommandList(commandList.GetCommandList(), restoreState);
        }
    }
}
