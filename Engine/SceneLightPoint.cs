using SharpDX;

namespace Engine
{
    /// <summary>
    /// Point light
    /// </summary>
    public class SceneLightPoint : SceneLight
    {
        /// <summary>
        /// Position
        /// </summary>
        private Vector3 position = Vector3.Zero;
        /// <summary>
        /// Radius
        /// </summary>
        private float radius = 1f;
        /// <summary>
        /// Initial transform
        /// </summary>
        private Matrix offsetTransform = Matrix.Identity;
        /// <summary>
        /// Local transform
        /// </summary>
        private Matrix local = Matrix.Identity;

        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (this.position != value)
                {
                    this.position = value;

                    this.Update();
                }
            }
        }
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius
        {
            get
            {
                return this.radius;
            }
            set
            {
                if (this.radius != value)
                {
                    this.radius = value;

                    this.Update();
                }
            }
        }
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity = 1f;
        /// <summary>
        /// Gets the bounding sphere of the active light
        /// </summary>
        public BoundingSphere BoundingSphere { get; private set; }
        /// <summary>
        /// Transform matrix
        /// </summary>
        public Matrix Transform { get; protected set; }
        /// <summary>
        /// Local transform
        /// </summary>
        public override Matrix Local
        {
            get
            {
                return this.local;
            }
            set
            {
                this.local = value;

                var trn = this.offsetTransform * this.local;

                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                trn.Decompose(out scale, out rotation, out translation);
                this.position = translation;

                this.Update();
            }
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
            this.position = position;
            this.radius = radius;
            this.Intensity = intensity;

            this.Update();
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
            this.offsetTransform = transform;
            this.Local = Matrix.Identity;

            this.radius = radius;
            this.Intensity = intensity;

            this.Update();
        }

        /// <summary>
        /// Gets the light volume
        /// </summary>
        /// <returns>Returns a line list representing the light volume</returns>
        public Line3D[] GetVolume()
        {
            return Line3D.CreateWiredSphere(this.BoundingSphere, 10, 10);
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        private void Update()
        {
            this.BoundingSphere = new BoundingSphere(this.position, this.radius);
            this.Transform = Matrix.Scaling(this.radius) * Matrix.Translation(this.position);
        }
    }
}
