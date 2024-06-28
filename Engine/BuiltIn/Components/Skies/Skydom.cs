
namespace Engine.BuiltIn.Components.Skies
{
    /// <summary>
    /// Skydom
    /// </summary>
    /// <remarks>
    /// It's a cubemap that fits his position with the eye camera position
    /// </remarks>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class Skydom(Scene scene, string id, string name) : Cubemap<SkydomDescription>(scene, id, name)
    {
    }
}
