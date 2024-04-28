using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Point light
    /// </summary>
    public class SceneLightPoint : SceneLight, ISceneLightPoint
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

        /// <inheritdoc/>
        public float Radius { get; set; }
        /// <inheritdoc/>
        public float Intensity { get; set; }
        /// <inheritdoc/>
        public BoundingSphere BoundingSphere
        {
            get
            {
                return new BoundingSphere(Position, Radius);
            }
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public Matrix Local
        {
            get
            {
                return Matrix.Scaling(Radius) * Matrix.Translation(Position);
            }
        }

        /// <summary>
        /// Gets the point light from light view matrix
        /// </summary>
        /// <param name="lightPosition">Light position</param>
        /// <param name="direction">Direction</param>
        /// <param name="up">Up vector</param>
        /// <returns>Returns the point light from light view matrix</returns>
        private static Matrix GetFromPointLightViewProjection(Vector3 lightPosition, Vector3 direction, Vector3 up)
        {
            // View from light to scene center position
            return Matrix.LookAtLH(lightPosition, lightPosition + direction, up);
        }

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
        /// <param name="enabled">Light is enabled</param>
        /// <param name="description">Light description</param>
        public SceneLightPoint(string name, bool castShadow, Color3 diffuse, Color3 specular, bool enabled, SceneLightPointDescription description)
            : base(name, castShadow, diffuse, specular, enabled)
        {
            initialTransform = description.Transform;
            initialRadius = description.Radius;
            initialIntensity = description.Intensity;

            UpdateLocalTransform();
        }

        /// <summary>
        /// Updates local transform
        /// </summary>
        private void UpdateLocalTransform()
        {
            var trn = initialTransform * ParentTransform;

            trn.Decompose(out Vector3 scale, out _, out Vector3 translation);
            Radius = initialRadius * scale.X;
            Intensity = initialIntensity * scale.X;
            Position = translation;
        }

        /// <inheritdoc/>
        public override bool MarkForShadowCasting(GameEnvironment environment, Vector3 eyePosition)
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
            FromLightVP = UpdateFromLightViewProjection();
        }
        /// <summary>
        /// Gets from light view * projection matrix cube
        /// </summary>
        /// <returns>Returns the from light view * projection matrix cube</returns>
        private Matrix[] UpdateFromLightViewProjection()
        {
            // Orthogonal projection from center
            var projection = Matrix.PerspectiveFovLH(MathUtil.PiOverTwo, 1f, 0.1f, Radius);

            return
            [
                GetFromPointLightViewProjection(Position, Vector3.Right,      Vector3.Up)         * projection,
                GetFromPointLightViewProjection(Position, Vector3.Left,       Vector3.Up)         * projection,
                GetFromPointLightViewProjection(Position, Vector3.Up,         Vector3.BackwardLH) * projection,
                GetFromPointLightViewProjection(Position, Vector3.Down,       Vector3.ForwardLH)  * projection,
                GetFromPointLightViewProjection(Position, Vector3.ForwardLH,  Vector3.Up)         * projection,
                GetFromPointLightViewProjection(Position, Vector3.BackwardLH, Vector3.Up)         * projection,
            ];
        }
        /// <inheritdoc/>
        public override ICullingVolume GetLightVolume()
        {
            return (IntersectionVolumeSphere)BoundingSphere;
        }

        /// <inheritdoc/>
        public override ISceneLight Clone()
        {
            return new SceneLightPoint()
            {
                Name = Name,
                Enabled = Enabled,
                CastShadow = CastShadow,
                DiffuseColor = DiffuseColor,
                SpecularColor = SpecularColor,
                State = State,

                Position = Position,
                Radius = Radius,
                Intensity = Intensity,

                initialTransform = initialTransform,
                initialRadius = initialRadius,
                initialIntensity = initialIntensity,

                ParentTransform = ParentTransform,
            };
        }

        /// <inheritdoc/>
        public IEnumerable<Line3D> GetVolume(int sliceCount, int stackCount)
        {
            return Line3D.CreateSphere(BoundingSphere, sliceCount, stackCount);
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new SceneLightPointState
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
                Radius = Radius,
                Intensity = Intensity,
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (state is not SceneLightPointState sceneLightsState)
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
            Radius = sceneLightsState.Radius;
            Intensity = sceneLightsState.Intensity;
        }
    }
}
