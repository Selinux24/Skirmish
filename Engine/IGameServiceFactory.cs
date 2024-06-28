
namespace Engine
{
    public interface IGameServiceFactory
    {

    }

    public interface IGameServiceFactory<out T> : IGameServiceFactory
    {
        T Instance();
    }
}
