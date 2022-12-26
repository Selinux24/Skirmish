using Engine.UI;

namespace Engine.Windows
{
    /// <summary>
    /// Fonts helper factory
    /// </summary>
    class WindowsFontsFactory : IGameServiceFactory<IFonts>
    {
        /// <inheritdoc/>
        public IFonts Instance()
        {
            return new WindowsFonts();
        }
    }
}
