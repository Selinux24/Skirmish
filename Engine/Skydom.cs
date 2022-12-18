
namespace Engine
{
    /// <summary>
    /// Sky dom
    /// </summary>
    /// <remarks>
    /// It's a cubemap that fits his position with the eye camera position
    /// </remarks>
    public sealed class Skydom : Cubemap<SkydomDescription>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public Skydom(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }
    }
}
