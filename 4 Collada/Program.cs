using Engine;

namespace Collada
{
    static class Program
    {
        static void Main()
        {
            using (Game cl = new Game("4 Collada", 1366, 720, true))
            {
                cl.Form.ShowMouse = false;

                cl.AddScene(new TestScene3D(cl) { Active = true, });

                cl.Run();
            }
        }
    }
}
