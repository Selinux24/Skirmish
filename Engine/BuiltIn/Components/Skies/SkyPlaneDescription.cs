using SharpDX;

namespace Engine.BuiltIn.Components.Skies
{
    /// <summary>
    /// Sky plane description
    /// </summary>
    public class SkyPlaneDescription : SceneObjectDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";
        /// <summary>
        /// Texture 1 name
        /// </summary>
        public string Texture1Name { get; set; }
        /// <summary>
        /// Texture 2 name
        /// </summary>
        public string Texture2Name { get; set; }

        /// <summary>
        /// Maximum brightness value when animated with key light
        /// </summary>
        public float MaxBrightness { get; set; }
        /// <summary>
        /// Minimum brightness value when animated with key light
        /// </summary>
        public float MinBrightness { get; set; }
        /// <summary>
        /// Gets or sets the clouds base color
        /// </summary>
        public Color3 CloudBaseColor { get; set; }
        /// <summary>
        /// Clouds quad size
        /// </summary>
        public uint Size { get; set; }
        /// <summary>
        /// Texture repeat
        /// </summary>
        public int Repeat { get; set; }
        /// <summary>
        /// Plane width
        /// </summary>
        public float PlaneWidth { get; set; }
        /// <summary>
        /// Plane top
        /// </summary>
        public float PlaneTop { get; set; }
        /// <summary>
        /// Plane bottom
        /// </summary>
        public float PlaneBottom { get; set; }
        /// <summary>
        /// Fading distance
        /// </summary>
        public float FadingDistance { get; set; }
        /// <summary>
        /// Wind velocity
        /// </summary>
        public float Velocity { get; set; }
        /// <summary>
        /// Wind direction
        /// </summary>
        public Vector2 Direction { get; set; }
        /// <summary>
        /// Perturbation scale
        /// </summary>
        public float PerturbationScale { get; set; }
        /// <summary>
        /// Sky plane mode
        /// </summary>
        public SkyPlaneModes SkyMode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkyPlaneDescription()
            : base()
        {
            DepthEnabled = false;
            BlendMode = BlendModes.Opaque | BlendModes.Additive;

            MaxBrightness = 0.75f;
            MinBrightness = 0.15f;
            CloudBaseColor = Color3.White;
            Size = 100;
            Repeat = 2;
            PlaneWidth = 50;
            PlaneTop = 1f;
            PlaneBottom = -0.5f;
            FadingDistance = 20f;
            Velocity = 1f;
            Direction = Vector2.One;
            PerturbationScale = 0.3f;
            SkyMode = SkyPlaneModes.Static;
        }
    }
}
