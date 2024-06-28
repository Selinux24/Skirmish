
namespace Engine.Windows
{
    /// <summary>
    /// Windows form factory
    /// </summary>
    class WindowsEngineFormFactory : IGameServiceFactory<IEngineForm>
    {
        /// <inheritdoc/>
        public IEngineForm Instance()
        {
            return new WindowsEngineForm();
        }
    }
}
