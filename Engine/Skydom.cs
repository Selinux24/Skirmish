
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
}
