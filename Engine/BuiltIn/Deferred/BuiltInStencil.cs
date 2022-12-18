
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
        /// <param name="graphics">Graphics</param>
        public BuiltInStencil(Graphics graphics) : base(graphics)
        {
            SetVertexShader<DeferredStencilVs>();
        }
    }
}
