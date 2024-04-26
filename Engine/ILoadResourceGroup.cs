using System;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Load resource group interface
    /// </summary>
    public interface ILoadResourceGroup
    {
        /// <summary>
        /// Group identifier
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Process the load resource group
        /// </summary>
        /// <param name="progress">Progress</param>
        Task Process(IProgress<LoadResourceProgress> progress);
        /// <summary>
        /// Ends the load resource group
        /// </summary>
        void End();
    }
}
