
namespace Engine.BuiltIn.Drawers.Deferred
{
    /// <summary>
    /// Stencil drawer
    /// </summary>
    public class BuiltInStencil : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInStencil(Game game) : base(game)
        {
            SetVertexShader<DeferredStencilVs>(false);
        }
    }
}
