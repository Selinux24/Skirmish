using SharpDX;

namespace Engine
{
    /// <summary>
    /// Hemispheric light
    /// </summary>
    public class SceneLightHemispheric : SceneLight
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
        /// Parent local transform matrix
        /// </summary>
        public override Matrix ParentTransform
        {
            get
            {
                return Matrix.Identity;
            }
            set
            {

            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLightHemispheric()
            : base()
        {
            this.AmbientDown = Color.White;
            this.AmbientUp = Color.White;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="ambientDown">Ambient down color contribution</param>
        /// <param name="ambientUp">Ambient up color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        public SceneLightHemispheric(string name, Color4 ambientDown, Color4 ambientUp, bool enabled)
            : base(name, false, Color.Transparent, Color.Transparent, enabled)
        {
            this.AmbientDown = ambientDown;
            this.AmbientUp = ambientUp;
        }

        /// <summary>
        /// Clones current light
        /// </summary>
        /// <returns>Returns a new instante with same data</returns>
        public override SceneLight Clone()
        {
            return new SceneLightHemispheric()
            {
                Name = this.Name,
                Enabled = this.Enabled,
                CastShadow = this.CastShadow,
                AmbientDown = this.AmbientDown,
                AmbientUp = this.AmbientUp,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                State = this.State,
            };
        }
    }
}
