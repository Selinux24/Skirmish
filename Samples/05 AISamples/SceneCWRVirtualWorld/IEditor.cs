using Engine;
using System.Threading.Tasks;

namespace AISamples.SceneCWRVirtualWorld
{
    interface IEditor
    {
        bool Visible { get; set; }
        Task Initialize(Scene scene);
        void UpdateInputEditor(IGameTime gameTime);
        void Draw();
    }
}
