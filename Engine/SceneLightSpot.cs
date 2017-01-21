using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public class SceneLightSpot : SceneLight
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
        /// Parent local transform
        /// </summary>
        private Matrix parentTransform = Matrix.Identity;

        /// <summary>
        /// Ligth position
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Ligth direction
        /// </summary>
        public Vector3 Direction { get; set; }
        /// <summary>
        /// Cone angle in degrees
        /// </summary>
        public float Angle { get; set; }
        /// <summary>
        /// Cone angle in radians
        /// </summary>
        public float AngleRadians
        {
            get
            {
                return MathUtil.DegreesToRadians(this.Angle);
            }
            set
            {
                this.Angle = MathUtil.RadiansToDegrees(value);
            }
        }
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
                Vector3 center = (this.Position + (Vector3.Normalize(this.Position) * this.Radius)) * 0.5f;

                return new BoundingSphere(center, this.Radius);
            }
        }
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
                this.Radius = this.initialRadius * scale.X;
                this.Intensity = this.initialIntensity * scale.X;
                this.Direction = Matrix.RotationQuaternion(rotation).Down;
                this.Position = translation;
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
        /// Constructor
        /// </summary>
        protected SceneLightSpot()
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
            if (direction == Vector3.Zero) throw new ArgumentException("Direction must have magnitude", "direction");

            float f = Math.Abs(Vector3.Dot(direction, Vector3.Up));
            this.initialTransform = Helper.CreateWorld(position, direction, f == 1 ? Vector3.ForwardLH : Vector3.Up);
            this.initialRadius = radius;
            this.initialIntensity = intensity;

            this.Angle = angle;
            this.ParentTransform = Matrix.Identity;
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
            this.initialTransform = transform;
            this.initialRadius = radius;
            this.initialIntensity = intensity;

            this.Angle = angle;
            this.ParentTransform = Matrix.Identity;
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
            float f = Math.Abs(Vector3.Dot(this.Direction, Vector3.Up));
            Matrix trn = Helper.CreateWorld(this.Position, this.Direction, f == 1 ? Vector3.ForwardLH : Vector3.Up);

            return Line3D.Transform(coneLines, rot * trn);
        }
        /// <summary>
        /// Gets the text representation of the light
        /// </summary>
        /// <returns>Returns the text representation of the light</returns>
        public override SceneLight Clone()
        {
            var l = new SceneLightSpot()
            {
                Name = this.Name,
                Enabled = this.Enabled,
                CastShadow = this.CastShadow,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                State = this.State,

                Position = this.Position,
                Radius = this.Radius,
                Angle = this.Angle,
                Direction = this.Direction,
                Intensity = this.Intensity,

                parentTransform = this.parentTransform,

                initialTransform = this.initialTransform,
                initialRadius = this.initialRadius,
                initialIntensity = this.initialIntensity,
            };

            return l;
        }
    }
}
