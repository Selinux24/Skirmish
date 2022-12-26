
namespace Engine.Windows
{
    public class WindowsEngineFormFactory : IGameServiceFactory<IEngineForm>
    {
        public IEngineForm Instance()
        {
            return new WindowsEngineForm();
        }
    }
}
