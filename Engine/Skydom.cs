using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Sky dom
    /// </summary>
    /// <remarks>
    /// It's a cubemap that fits his position with the eye camera position
    /// </remarks>
    public class Skydom : Cubemap
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Skydom description</param>
        public Skydom(string id, string name, Scene scene, SkydomDescription description)
            : base(id, name, scene, description)
        {

        }

        /// <summary>
        /// Updates object state
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            Manipulator.SetPosition(context.EyePosition);

            base.Update(context);
        }
    }

    /// <summary>
    /// Skydom extensions
    /// </summary>
    public static class SkydomExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Skydom> AddComponentSkydom(this Scene scene, string id, string name, SkydomDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerSky)
        {
            Skydom component = null;

            await Task.Run(() =>
            {
                component = new Skydom(id, name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
