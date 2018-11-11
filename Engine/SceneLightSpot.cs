using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public class SceneLightSpot : SceneLight, ISceneLightSpot
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
                float radius = this.Radius * 0.5f;
                Vector3 center = (this.Position + (Vector3.Normalize(this.Direction) * radius));

                return new BoundingSphere(center, radius);
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
                float radius = this.Radius * 0.5f;
                Vector3 center = (this.Position + (Vector3.Normalize(this.Direction) * radius));

                return Matrix.Scaling(radius) * Matrix.Translation(center);
            }
        }
        /// <summary>
        /// Shadow map index
        /// </summary>
        public int ShadowMapIndex { get; set; }
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
        /// <param name="description">Light description</param>
        public SceneLightSpot(
            string name, bool castShadow, Color4 diffuse, Color4 specular, bool enabled,
            SceneLightSpotDescription description)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            this.initialTransform = description.Transform;
            this.initialRadius = description.Radius;
            this.initialIntensity = description.Intensity;

            this.Angle = description.Angle;

            this.UpdateLocalTransform();
        }

        /// <summary>
        /// Updates local transform
        /// </summary>
        private void UpdateLocalTransform()
        {
            var trn = this.initialTransform * base.ParentTransform;

            trn.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            this.Radius = this.initialRadius * scale.X;
            this.Intensity = this.initialIntensity * scale.X;
            this.Direction = Matrix.RotationQuaternion(rotation).Down;
            this.Position = translation;
        }

        /// <summary>
        /// Gets the light volume
        /// </summary>
        /// <param name="sliceCount">Cone slice count</param>
        /// <returns>Returns a line list representing the light volume</returns>
        public IEnumerable<Line3D> GetVolume(int sliceCount)
        {
            var coneLines = Line3D.CreateWiredConeAngle(this.AngleRadians, this.Radius, sliceCount);

            //The wired cone has his basin on the XZ plane. Light points along the Z axis, we have to rotate 90 degrees around the X axis
            Matrix rot = Matrix.RotationX(MathUtil.PiOverTwo);

            //Then move and rotate the cone to light position and direction
            float f = Math.Abs(Vector3.Dot(this.Direction, Vector3.Up));
            Matrix trn = Helper.CreateWorld(this.Position, this.Direction, f == 1 ? Vector3.ForwardLH : Vector3.Up);

            return Line3D.Transform(coneLines, rot * trn);
        }
        /// <summary>
        /// Clones current light
        /// </summary>
        /// <returns>Returns a new instante with same data</returns>
        public override SceneLight Clone()
        {
            return new SceneLightSpot()
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

                initialTransform = this.initialTransform,
                initialRadius = this.initialRadius,
                initialIntensity = this.initialIntensity,

                ParentTransform = this.ParentTransform,
            };
        }
    }
}
