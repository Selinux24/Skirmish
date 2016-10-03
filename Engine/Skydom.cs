
namespace Engine
{
    using Engine.Common;
    using Engine.Content;

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
        /// <param name="game">Game</param>
        /// <param name="content">Content</param>
        public Skydom(Game game, ModelContent content)
            : base(game, content)
        {
            this.Cull = false;
            this.CastShadow = true;
            this.Static = true;
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
