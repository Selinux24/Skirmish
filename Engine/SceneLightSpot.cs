using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public class SceneLightSpot : SceneLight
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
        /// Cone angle
        /// </summary>
        private float angle = 0f;
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
        /// Ligth direction
        /// </summary>
        public Vector3 Direction = Vector3.Zero;
        /// <summary>
        /// Cone angle in degrees
        /// </summary>
        public float Angle
        {
            get
            {
                return this.angle;
            }
            set
            {
                this.angle = value;
            }
        }
        /// <summary>
        /// Cone angle in radians
        /// </summary>
        public float AngleRadians
        {
            get
            {
                return MathUtil.DegreesToRadians(this.angle);
            }
            set
            {
                this.angle = MathUtil.RadiansToDegrees(value);
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
        /// Gets the bounding box of the active light
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }
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
                this.Direction = Matrix.RotationQuaternion(rotation).Down;

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
        /// <param name="enabled">Light is enabled</param>
        /// <param name="position">Position</param>
        /// <param name="direction">Direction</param>
        /// <param name="angle">Angle in degrees</param>
        /// <param name="radius">Radius</param>
        /// <param name="intensity">Intensity</param>
        public SceneLightSpot(
            string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled,
            Vector3 position, Vector3 direction, float angle, float radius, float intensity)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.position = position;
            this.Direction = direction;
            this.angle = angle;
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
        /// <param name="angle">Angle in degrees</param>
        /// <param name="radius">Radius</param>
        /// <param name="intensity">Intensity</param>
        public SceneLightSpot(
            string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled,
            Matrix transform, float angle, float radius, float intensity)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.offsetTransform = transform;
            this.Local = Matrix.Identity;

            this.angle = angle;
            this.radius = radius;
            this.Intensity = intensity;

            this.Local = Matrix.Identity;

            this.Update();
        }

        /// <summary>
        /// Gets the light volume
        /// </summary>
        /// <returns>Returns a line list representing the light volume</returns>
        public Line3D[] GetVolume()
        {
            var coneLines = Line3D.CreateWiredConeAngle(this.AngleRadians, this.Radius, 10);

            //The wired cone has his basin on XZ plane. Light points along the Z axis, we have to rotate 90 degrees around the X axis
            Matrix rot = Matrix.RotationX(MathUtil.PiOverTwo);

            //Then move and rotate the cone to light position and direction
            Matrix trn = Helper.CreateWorld(this.Position, this.Direction, Vector3.Up);

            return Line3D.Transform(coneLines, rot * trn);
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        private void Update()
        {
            var lines = GetVolume();

            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < lines.Length; i++)
            {
                points.Add(lines[i].Point1);
                points.Add(lines[i].Point2);
            }

            this.BoundingBox = BoundingBox.FromPoints(points.ToArray());

            this.Transform = Matrix.Scaling(this.radius) * Matrix.Translation(this.position);
        }
    }
}
