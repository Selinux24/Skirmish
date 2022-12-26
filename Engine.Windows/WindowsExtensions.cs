using Engine.UI;

namespace Engine.Windows
{
    /// <summary>
    /// Windows startup extensions
    /// </summary>
    public static class WindowsExtensions
    {
        /// <summary>
        /// Startup
        /// </summary>
        public static void Startup()
        {
            EngineServiceFactory.Register<IEngineForm, WindowsEngineFormFactory>();
            EngineServiceFactory.Register<IInput, WindowsInputFactory>();
            EngineServiceFactory.Register<IImages, WindowsImagesFactory>();
            EngineServiceFactory.Register<IFonts, WindowsFontsFactory>();
        }
    }
}
