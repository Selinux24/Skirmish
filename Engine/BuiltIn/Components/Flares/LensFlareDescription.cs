
namespace Engine.BuiltIn.Components.Flares
{
    /// <summary>
    /// Lens flare description
    /// </summary>
    public class LensFlareDescription : SceneObjectDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; }
        /// <summary>
        /// Glow texture
        /// </summary>
        public string GlowTexture { get; set; }
        /// <summary>
        /// Flare collection
        /// </summary>
        public LensFlareArtifact[] Flares { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public LensFlareDescription()
            : base()
        {
            DeferredEnabled = false;
            DepthEnabled = false;
            BlendMode = BlendModes.Alpha | BlendModes.Additive;
        }
    }
}
