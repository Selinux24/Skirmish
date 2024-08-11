using Engine.Common;

namespace Engine
{
    /// <summary>
    /// Instancing data interface
    /// </summary>
    public interface IInstacingData : IBufferData
    {
        /// <summary>
        /// Gets the input element colection
        /// </summary>
        EngineInputElement[] GetInput(int slot);
    }
}
