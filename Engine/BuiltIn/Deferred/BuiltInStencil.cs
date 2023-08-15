
namespace Engine.BuiltIn.Deferred
{
    /// <summary>
    /// Stencil drawer
    /// </summary>
    public class BuiltInStencil : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInStencil() : base()
        {
            SetVertexShader<DeferredStencilVs>(false);
        }
    }
}
