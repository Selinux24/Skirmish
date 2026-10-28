using System;
using System.Collections.Generic;
using System.Numerics;
using Vortice.Mathematics;

namespace Engine
{
    using Engine.Mathematics;

    /// <summary>
    /// Spot light
    /// </summary>
    public class SceneLightSpot : SceneLight, ISceneLightSpot
    {
        /// <summary>
        /// Initial transform
        /// </summary>
        private Matrix4x4 initialTransform = Matrix4x4.Identity;
        /// <summary>
        /// Initial radius
        /// </summary>
        private float initialRadius = 1f;
        /// <summary>
        /// Initial intensity
        /// </summary>
        private float initialIntensity = 1f;

        /// <inheritdoc/>
        public Vector3 Direction { get; set; }
        /// <inheritdoc/>
        public float FallOffAngle { get; set; }
        /// <inheritdoc/>
        public float FallOffAngleRadians
        {
            get
            {
                return Helper.DegreesToRadians(FallOffAngle);
            }
            set
            {
                FallOffAngle = Helper.RadiansToDegrees(value);
            }
        }
        /// <inheritdoc/>
        public float Radius { get; set; }
        /// <inheritdoc/>
        public float Intensity { get; set; }
        /// <inheritdoc/>
        public BoundingSphere BoundingSphere
        {
            get
            {
                float radius = Radius * 0.5f;
                Vector3 center = (Position + (Vector3.Normalize(Direction) * radius));

                return new BoundingSphere(center, radius);
            }
        }
        /// <inheritdoc/>
        public override Matrix4x4 ParentTransform
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
        /// <inheritdoc/>
        public Matrix4x4 Local
        {
            get
            {
                float radius = Radius * 0.5f;
                Vector3 center = (Position + (Vector3.Normalize(Direction) * radius));

                return Matrix4x4.CreateScale(radius) * Matrix4x4.CreateTranslation(center);
            }
        }

        /// <summary>
        /// Creates the transform matrix from the specified position and direction
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="direction">Direction</param>
        /// <returns>Returns the transform matrix</returns>
        public static Matrix4x4 CreateFromPositionDirection(Vector3 position, Vector3 direction)
        {
            float f = MathF.Abs(Vector3.Dot(direction, Vector3Extensions.Up));
            Vector3 up = Helper.IsOne(f) ? Vector3Extensions.ForwardLH : Vector3Extensions.Up;
            return Matrix4x4Extensions.CreateWorld(position, direction, up);
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
        public SceneLightSpot(string name, bool castShadow, Vector3 diffuse, Vector3 specular, bool enabled, SceneLightSpotDescription description)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            Position = description.Position;
            Direction = description.Direction;
            FallOffAngle = description.FallOffAngle;
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

            Matrix4x4.Decompose(trn, out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            Radius = initialRadius * scale.X;
            Intensity = initialIntensity * scale.X;
            Direction = Matrix4x4.CreateFromQuaternion(rotation).GetBackward();
            Position = translation;
        }

        /// <inheritdoc/>
        public bool MarkForShadowCasting(GameEnvironment environment, Vector3 eyePosition)
        {
            CastShadowsMarked = EvaluateLight(environment, eyePosition, CastShadow, Position, Radius);

            return CastShadowsMarked;
        }
        /// <inheritdoc/>
        public override void ClearShadowParameters()
        {
            ShadowMapIndex = -1;
            ShadowMapCount = 0;
            FromLightVP = [];
        }
        /// <inheritdoc/>
        public override void SetShadowParameters(Camera camera, int assignedShadowMap)
        {
            ShadowMapIndex = assignedShadowMap;
            ShadowMapCount = 1;
            FromLightVP = CalcFromLightViewProjection();
        }
        /// <summary>
        /// Gets the from light view * projection matrix
        /// </summary>
        private Matrix4x4[] CalcFromLightViewProjection()
        {
            var near = 1f;
            var projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(FallOffAngleRadians * 2f, 1f, near, Radius);

            var pos = Position;
            var look = Position + (Direction * Radius);
            var view = Matrix4x4.CreateLookAtLeftHanded(pos, look, Vector3Extensions.Up);

            var vp = view * projection;

            return [vp];
        }
        /// <inheritdoc/>
        public override ICullingVolume GetLightVolume()
        {
            return (IntersectionVolumeSphere)BoundingSphere;
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
                FallOffAngle = FallOffAngle,
                Direction = Direction,
                Intensity = Intensity,

                initialTransform = initialTransform,
                initialRadius = initialRadius,
                initialIntensity = initialIntensity,

                ParentTransform = ParentTransform,
            };
        }

        /// <inheritdoc/>
        public IEnumerable<Line3D> GetVolume(int sliceCount)
        {
            var coneLines = Line3D.CreateConeCupAngle(FallOffAngleRadians, Radius, sliceCount);

            //The wired cone has his basin on the XZ plane. Light points along the Z axis, we have to rotate 90 degrees around the X axis
            Matrix4x4 rot = Matrix4x4.CreateRotationX(Helper.PiOver2);

            //Then move and rotate the cone to light position and direction
            float f = MathF.Abs(Vector3.Dot(Direction, Vector3Extensions.Up));
            Matrix4x4 trn = Matrix4x4Extensions.CreateWorld(Position, Direction, Helper.IsOne(f) ? Vector3Extensions.ForwardLH : Vector3Extensions.Up);

            return Line3D.Transform(coneLines, rot * trn);
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new SceneLightSpotState
            {
                Name = Name,
                Enabled = Enabled,
                CastShadow = CastShadow,
                CastShadowsMarked = CastShadowsMarked,
                DiffuseColor = DiffuseColor,
                SpecularColor = SpecularColor,
                State = State,
                ParentTransform = ParentTransform,

                InitialTransform = initialTransform,
                InitialRadius = initialRadius,
                InitialIntensity = initialIntensity,
                Position = Position,
                Direction = Direction,
                Angle = FallOffAngle,
                Radius = Radius,
                Intensity = Intensity,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not SceneLightSpotState sceneLightsState)
            {
                return;
            }

            Name = sceneLightsState.Name;
            Enabled = sceneLightsState.Enabled;
            CastShadow = sceneLightsState.CastShadow;
            CastShadowsMarked = sceneLightsState.CastShadowsMarked;
            DiffuseColor = sceneLightsState.DiffuseColor;
            SpecularColor = sceneLightsState.SpecularColor;
            State = sceneLightsState.State;
            ParentTransform = sceneLightsState.ParentTransform;

            initialTransform = sceneLightsState.InitialTransform;
            initialRadius = sceneLightsState.InitialRadius;
            initialIntensity = sceneLightsState.InitialIntensity;
            Position = sceneLightsState.Position;
            Direction = sceneLightsState.Direction;
            FallOffAngle = sceneLightsState.Angle;
            Radius = sceneLightsState.Radius;
            Intensity = sceneLightsState.Intensity;
        }
    }
}
