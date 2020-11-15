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
                return MathUtil.DegreesToRadians(Angle);
            }
            set
            {
                Angle = MathUtil.RadiansToDegrees(value);
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
                float radius = Radius * 0.5f;
                Vector3 center = (Position + (Vector3.Normalize(Direction) * radius));

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

                UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Local matrix
        /// </summary>
        public Matrix Local
        {
            get
            {
                float radius = Radius * 0.5f;
                Vector3 center = (Position + (Vector3.Normalize(Direction) * radius));

                return Matrix.Scaling(radius) * Matrix.Translation(center);
            }
        }
        /// <summary>
        /// Shadow map count
        /// </summary>
        public uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        public Matrix[] FromLightVP { get; set; }

        /// <summary>
        /// Creates the transform matrix from the specified position and direction
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="direction">Direction</param>
        /// <returns>Returns the transform matrix</returns>
        public static Matrix CreateFromPositionDirection(Vector3 position, Vector3 direction)
        {
            float f = Math.Abs(Vector3.Dot(direction, Vector3.Up));
            Vector3 up = f == 1 ? Vector3.ForwardLH : Vector3.Up;
            return Helper.CreateWorld(position, direction, up);
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
        /// <param name="description">Light description</param>
        public SceneLightSpot(
            string name, bool castShadow, Color3 diffuse, Color3 specular, bool enabled,
            SceneLightSpotDescription description)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            Position = description.Position;
            Direction = description.Direction;
            Angle = description.Angle;
            Radius = description.Radius;
            Intensity = description.Intensity;

            initialTransform = CreateFromPositionDirection(Position, Direction);
            initialRadius = description.Radius;
            initialIntensity = description.Intensity;
        }

        /// <summary>
        /// Updates local transform
        /// </summary>
        private void UpdateLocalTransform()
        {
            var trn = initialTransform * ParentTransform;

            trn.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            Radius = initialRadius * scale.X;
            Intensity = initialIntensity * scale.X;
            Direction = Matrix.RotationQuaternion(rotation).Backward;
            Position = translation;
        }

        /// <inheritdoc/>
        public override void ClearShadowParameters()
        {
            base.ClearShadowParameters();

            ShadowMapCount = 0;
            FromLightVP = new Matrix[1];
        }
        /// <inheritdoc/>
        public override bool MarkForShadowCasting(Vector3 eyePosition)
        {
            CastShadowsMarked = EvaluateLight(eyePosition, CastShadow, Position, Radius);

            return CastShadowsMarked;
        }
        /// <inheritdoc/>
        public override ISceneLight Clone()
        {
            return new SceneLightSpot()
            {
                Name = Name,
                Enabled = Enabled,
                CastShadow = CastShadow,
                DiffuseColor = DiffuseColor,
                SpecularColor = SpecularColor,
                State = State,

                Position = Position,
                Radius = Radius,
                Angle = Angle,
                Direction = Direction,
                Intensity = Intensity,

                initialTransform = initialTransform,
                initialRadius = initialRadius,
                initialIntensity = initialIntensity,

                ParentTransform = ParentTransform,
            };
        }

        /// <summary>
        /// Gets the light volume
        /// </summary>
        /// <param name="sliceCount">Cone slice count</param>
        /// <returns>Returns a line list representing the light volume</returns>
        public IEnumerable<Line3D> GetVolume(int sliceCount)
        {
            var coneLines = Line3D.CreateWiredConeAngle(AngleRadians, Radius, sliceCount);

            //The wired cone has his basin on the XZ plane. Light points along the Z axis, we have to rotate 90 degrees around the X axis
            Matrix rot = Matrix.RotationX(MathUtil.PiOverTwo);

            //Then move and rotate the cone to light position and direction
            float f = Math.Abs(Vector3.Dot(Direction, Vector3.Up));
            Matrix trn = Helper.CreateWorld(Position, Direction, f == 1 ? Vector3.ForwardLH : Vector3.Up);

            return Line3D.Transform(coneLines, rot * trn);
        }
    }
}
