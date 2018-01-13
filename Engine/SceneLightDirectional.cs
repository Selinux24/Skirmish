using SharpDX;

namespace Engine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public class SceneLightDirectional : SceneLight, ISceneLightDirectional
    {
        /// <summary>
        /// Primary default light source
        /// </summary>
        public static SceneLightDirectional KeyLight
        {
            get
            {
                return new SceneLightDirectional(
                    "Key light",
                    true,
                    new Color4(1, 0.9607844f, 0.8078432f, 1f),
                    new Color4(1, 0.9607844f, 0.8078432f, 1f) * 0.5f,
                    true,
                    new Vector3(-0.5265408f, -0.5735765f, -0.6275069f),
                    1f);
            }
        }
        /// <summary>
        /// Secondary default light source
        /// </summary>
        public static SceneLightDirectional FillLight
        {
            get
            {
                return new SceneLightDirectional(
                    "Fill light",
                    false,
                    new Color4(0.9647059f, 0.7607844f, 0.4078432f, 1f),
                    new Color4(0, 0, 0, 0),
                    true,
                    new Vector3(0.7198464f, 0.3420201f, 0.6040227f),
                    1f);
            }
        }
        /// <summary>
        /// Tertiary default light source
        /// </summary>
        public static SceneLightDirectional BackLight
        {
            get
            {
                return new SceneLightDirectional(
                    "Back light",
                    false,
                    new Color4(0.3231373f, 0.3607844f, 0.3937255f, 1f),
                    new Color4(0.3231373f, 0.3607844f, 0.3937255f, 1f) * 0.25f,
                    true,
                    new Vector3(0.4545195f, -0.7660444f, 0.4545195f),
                    1f);
            }
        }

        /// <summary>
        /// Initial transform
        /// </summary>
        private Matrix initialTransform = Matrix.Identity;
        /// <summary>
        /// Parent local transform
        /// </summary>
        private Matrix parentTransform = Matrix.Identity;

        /// <summary>
        /// Light direction
        /// </summary>
        public Vector3 Direction { get; set; }
        /// <summary>
        /// Base brightness
        /// </summary>
        public float BaseBrightness { get; protected set; }
        /// <summary>
        /// Light brightness
        /// </summary>
        public float Brightness { get; set; }
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        public override Matrix ParentTransform
        {
            get
            {
                return this.parentTransform;
            }
            set
            {
                this.parentTransform = value;

                var trn = this.initialTransform * this.parentTransform;

                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                trn.Decompose(out scale, out rotation, out translation);
                this.Direction = Matrix.RotationQuaternion(rotation).Down;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLightDirectional()
            : base()
        {
            this.Direction = Vector3.Zero;
            this.BaseBrightness = this.Brightness = 1f;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="castShadow">Light casts shadow</param>
        /// <param name="diffuse">Diffuse color contribution</param>
        /// <param name="specular">Specular color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        /// <param name="direction">Direction</param>
        /// <param name="brigthness">Brightness</param>
        public SceneLightDirectional(string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled, Vector3 direction, float brigthness)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.Direction = direction;
            this.BaseBrightness = this.Brightness = brigthness;
        }

        /// <summary>
        /// Gets light position at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns light position at specified distance</returns>
        public Vector3 GetPosition(float distance)
        {
            return distance * -2f * this.Direction;
        }

        /// <summary>
        /// Gets the text representation of the light
        /// </summary>
        /// <returns>Returns the text representation of the light</returns>
        public override SceneLight Clone()
        {
            return new SceneLightDirectional()
            {
                Name = this.Name,
                Enabled = this.Enabled,
                CastShadow = this.CastShadow,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                State = this.State,

                Direction = this.Direction,
                Brightness = this.Brightness,

                parentTransform = this.parentTransform,

                initialTransform = this.initialTransform,
            };
        }
    }
}
