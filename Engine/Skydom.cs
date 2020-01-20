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
        /// <param name="scene">Scene</param>
        /// <param name="description">Skydom description</param>
        public Skydom(Scene scene, SkydomDescription description)
            : base(scene, description)
        {

        }

        /// <summary>
        /// Updates object state
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.SetPosition(context.EyePosition);

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
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Skydom> AddComponentSkydom(this Scene scene, SkydomDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Skydom component = null;

            await Task.Run(() =>
            {
                component = new Skydom(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
