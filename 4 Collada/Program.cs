using Engine;

namespace Collada
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("4 Collada", 400, 300, false))
            {
                cl.Form.ShowMouse = false;

                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
