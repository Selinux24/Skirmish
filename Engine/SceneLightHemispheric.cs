using SharpDX;

namespace Engine
{
    /// <summary>
    /// Hemispheric light
    /// </summary>
    public class SceneLightHemispheric : SceneLight, ISceneLightHemispheric
    {
        /// <summary>
        /// Ambient down color
        /// </summary>
        public Color4 AmbientDown { get; set; }
        /// <summary>
        /// Ambient up color
        /// </summary>
        public Color4 AmbientUp { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLightHemispheric()
            : base()
        {
            AmbientDown = Color.White;
            AmbientUp = Color.White;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="ambientDown">Ambient down color contribution</param>
        /// <param name="ambientUp">Ambient up color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        public SceneLightHemispheric(string name, Color4 ambientDown, Color4 ambientUp, bool enabled)
            : this(name, ambientDown, 1f, ambientUp, 1f, enabled)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="ambientDown">Ambient down color contribution</param>
        /// <param name="brightnessDown">Brightness down</param>
        /// <param name="ambientUp">Ambient up color contribution</param>
        /// <param name="brightnessUp">Brightness up</param>
        /// <param name="enabled">Lights is enabled</param>
        public SceneLightHemispheric(string name, Color4 ambientDown, float brightnessDown, Color4 ambientUp, float brightnessUp, bool enabled)
            : base(name, false, Color.Transparent, Color.Transparent, enabled)
        {
            AmbientDown = new Color4(ambientDown.RGB() * brightnessDown, 1);
            AmbientUp = new Color4(ambientUp.RGB() * brightnessUp, 1);
        }

        /// <inheritdoc/>
        public override bool MarkForShadowCasting(Vector3 eyePosition)
        {
            CastShadowsMarked = false;

            return CastShadowsMarked;
        }
        /// <inheritdoc/>
        public override ISceneLight Clone()
        {
            return new SceneLightHemispheric()
            {
                Name = Name,
                Enabled = Enabled,
                CastShadow = CastShadow,
                AmbientDown = AmbientDown,
                AmbientUp = AmbientUp,
                DiffuseColor = DiffuseColor,
                SpecularColor = SpecularColor,
                State = State,
            };
        }
    }
}
