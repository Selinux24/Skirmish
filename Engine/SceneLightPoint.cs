using SharpDX;

namespace Engine
{
    /// <summary>
    /// Point light
    /// </summary>
    public class SceneLightPoint : SceneLight, ISceneLightOmnidirectional
    {
        /// <summary>
        /// Initial transform
        /// </summary>
        private Matrix initialTransform = Matrix.Identity;
        /// <summary>
        /// Initial radius
        /// </summary>
        private float initialRadius = 1f;
        /// <summary>
        /// Initial intensity
        /// </summary>
        private float initialIntensity = 1f;

        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity { get; set; }
        /// <summary>
        /// Gets the bounding sphere of the active light
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get
            {
                return new BoundingSphere(this.Position, this.Radius);
            }
        }
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
        /// Local matrix
        /// </summary>
        public Matrix Local
        {
            get
            {
                return Matrix.Scaling(this.Radius) * Matrix.Translation(this.Position);
            }
        }
        /// <summary>
        /// Shadow map index
        /// </summary>
        public int ShadowMapIndex { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected SceneLightPoint()
            : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="castShadow">Light casts shadow</param>
        /// <param name="diffuse">Diffuse color contribution</param>
        /// <param name="specular">Specular color contribution</param>
        /// <param name="enabled">Lights is enabled</param>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="intensity">Intensity</param>
        public SceneLightPoint(
            string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled,
            Vector3 position, float radius, float intensity)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.initialTransform = Matrix.Translation(position);
            this.initialRadius = radius;
            this.initialIntensity = intensity;

            this.UpdateLocalTransform();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Light name</param>
        /// <param name="castShadow">Light casts shadow</param>
        /// <param name="diffuse">Diffuse color contribution</param>
        /// <param name="specular">Specular color contribution</param>
        /// <param name="enabled">Light is enabled</param>
        /// <param name="transform">Initial transform</param>
        /// <param name="radius">Radius</param>
        /// <param name="intensity">Intensity</param>
        public SceneLightPoint(
            string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled,
            Matrix transform, float radius, float intensity)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.initialTransform = transform;
            this.initialRadius = radius;
            this.initialIntensity = intensity;

            this.UpdateLocalTransform();
        }

        /// <summary>
        /// Updates local transform
        /// </summary>
        private void UpdateLocalTransform()
        {
            var trn = this.initialTransform * base.ParentTransform;

            trn.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            this.Radius = initialRadius * scale.X;
            this.Intensity = initialIntensity * scale.X;
            this.Position = translation;
        }

        /// <summary>
        /// Gets the perspective projection matrix
        /// </summary>
        /// <returns>Returns de perspective projection matrix for shadow mapping</returns>
        public Matrix GetProjection()
        {
            return Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1, 0.1f, this.Radius + 0.1f);
        }

        /// <summary>
        /// Gets the light volume
        /// </summary>
        /// <param name="sliceCount">Sphere slice count (vertical subdivisions - meridians)</param>
        /// <param name="stackCount">Sphere stack count (horizontal subdivisions - parallels)</param>
        /// <returns>Returns a line list representing the light volume</returns>
        public Line3D[] GetVolume(int sliceCount, int stackCount)
        {
            return Line3D.CreateWiredSphere(this.BoundingSphere, sliceCount, stackCount);
        }
        /// <summary>
        /// Clones current light
        /// </summary>
        /// <returns>Returns a new instante with same data</returns>
        public override SceneLight Clone()
        {
            var l = new SceneLightPoint()
            {
                Name = this.Name,
                Enabled = this.Enabled,
                CastShadow = this.CastShadow,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                State = this.State,

                Position = this.Position,
                Radius = this.Radius,
                Intensity = this.Intensity,

                ParentTransform = this.ParentTransform,

                initialTransform = this.initialTransform,
                initialRadius = this.initialRadius,
                initialIntensity = this.initialIntensity,
            };

            return l;
        }
    }
}
