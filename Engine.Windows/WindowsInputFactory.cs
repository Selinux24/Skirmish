
namespace Engine.Windows
{
    /// <summary>
    /// Input helper factory
    /// </summary>
    class WindowsInputFactory : IGameServiceFactory<IInput>
    {
        /// <inheritdoc/>
        public IInput Instance()
        {
            return new WindowsInput();
        }
    }
}
