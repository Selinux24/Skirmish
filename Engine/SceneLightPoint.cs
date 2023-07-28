using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

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
                return new BoundingSphere(Position, Radius);
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
                return Matrix.Scaling(Radius) * Matrix.Translation(Position);
            }
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
        public SceneLightPoint(
            string name, bool castShadow, Color3 diffuse, Color3 specular, bool enabled,
            SceneLightPointDescription description)
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
            return Line3D.CreateFromVertices(GeometryUtil.CreateSphere(Topology.LineList, BoundingSphere, sliceCount, stackCount));
        }

        /// <inheritdoc/>
        public void SetShadowParameters(int assignedShadowMap)
        {
            ShadowMapIndex = assignedShadowMap;
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
                ShadowMapIndex = ShadowMapIndex,
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
            ShadowMapIndex = sceneLightsState.ShadowMapIndex;
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
