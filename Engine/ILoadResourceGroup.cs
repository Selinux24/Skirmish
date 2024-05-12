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
        Task Process();
        /// <summary>
        /// Ends the load resource group
        /// </summary>
        void End();
    }
}
