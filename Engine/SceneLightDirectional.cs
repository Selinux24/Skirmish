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
        /// Initial light direction
        /// </summary>
        private Vector3 initialDirection = Vector3.Zero;

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
                return base.ParentTransform;
            }
            set
            {
                base.ParentTransform = value;

                this.UpdateLocalTransform();
            }
        }
        /// <summary>
        /// First shadow map index
        /// </summary>
        public uint ShadowMapIndex { get; set; }
        /// <summary>
        /// Shadow map count
        /// </summary>
        public uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix[] FromLightVP { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLightDirectional()
            : base()
        {
            this.initialDirection = Vector3.ForwardLH;
            this.BaseBrightness = this.Brightness = 1f;
            this.ShadowMapIndex = 0;
            this.ShadowMapCount = 0;

            this.UpdateLocalTransform();
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
            this.initialDirection = direction;
            this.BaseBrightness = this.Brightness = brigthness;

            this.UpdateLocalTransform();
        }

        /// <summary>
        /// Updates local transform
        /// </summary>
        private void UpdateLocalTransform()
        {
            this.Direction = Vector3.TransformNormal(this.initialDirection, base.ParentTransform);
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
        /// Clones current light
        /// </summary>
        /// <returns>Returns a new instante with same data</returns>
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

                ParentTransform = this.ParentTransform,

                initialDirection = this.initialDirection,
            };
        }
    }
}
