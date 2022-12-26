
namespace Engine.Windows
{
    /// <summary>
    /// Images helper factory
    /// </summary>
    class WindowsImagesFactory : IGameServiceFactory<IImages>
    {
        /// <inheritdoc/>
        public IImages Instance()
        {
            return new WindowsImages();
        }
    }
}
