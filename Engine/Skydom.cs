
namespace Engine
{
    using Engine.Common;

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

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            // Translates the box with the camera position
            Manipulator.SetPosition(context.EyePosition);

            base.Update(context);
        }
    }
}
