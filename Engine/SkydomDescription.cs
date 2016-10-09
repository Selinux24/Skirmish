
namespace Engine
{
    /// <summary>
    /// Skydom descriptor
    /// </summary>
    public class SkydomDescription : CubemapDescription
    {
        public SkydomDescription()
            : base()
        {
            this.Static = true;
            this.AlwaysVisible = false;
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.EnableDepthStencil = false;
            this.EnableAlphaBlending = false;
        }
    }
}
