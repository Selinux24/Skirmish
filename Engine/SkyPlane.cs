using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Sky plane
    /// </summary>
    public class SkyPlane : Drawable
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private readonly BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private readonly BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Sky texture 1
        /// </summary>
        private readonly EngineShaderResourceView skyTexture1 = null;
        /// <summary>
        /// Sky texture 2
        /// </summary>
        private readonly EngineShaderResourceView skyTexture2 = null;
        /// <summary>
        /// Brightness
        /// </summary>
        private float brightness;
        /// <summary>
        /// Translation
        /// </summary>
        private float translation = 0;
        /// <summary>
        /// First layer translation
        /// </summary>
        private Vector2 firstLayerTranslation;
        /// <summary>
        /// Second layer translation
        /// </summary>
        private Vector2 secondLayerTranslation;
        /// <summary>
        /// Plane mode
        /// </summary>
        private readonly SkyPlaneModes skyMode;
        /// <summary>
        /// Traslation direction
        /// </summary>
        private Vector2 direction;
        /// <summary>
        /// Plane rotation (Y)
        /// </summary>
        private Matrix rotation;
        /// <summary>
        /// Clouds color
        /// </summary>
        private Color4 color;

        /// <summary>
        /// First layer translation
        /// </summary>
        public Vector2 FirstLayerTranslation { get; set; }
        /// <summary>
        /// Second layer translation
        /// </summary>
        public Vector2 SecondLayerTranslation { get; set; }
        /// <summary>
        /// Maximum brightness
        /// </summary>
        public float MaxBrightness { get; set; }
        /// <summary>
        /// Minimum brightness
        /// </summary>
        public float MinBrightness { get; set; }
        /// <summary>
        /// Fading distance
        /// </summary>
        public float FadingDistance { get; set; }
        /// <summary>
        /// Velocity
        /// </summary>
        public float Velocity { get; set; }
        /// <summary>
        /// Gets or sets the traslation direction
        /// </summary>
        public Vector2 Direction
        {
            get
            {
                return this.direction;
            }
            set
            {
                this.direction = value;

                if (this.direction != Vector2.Zero)
                {
                    float a = Helper.AngleSigned(Vector2.UnitX, Vector2.Normalize(this.direction));

                    this.rotation = Matrix.RotationY(a);
                }
            }
        }
        /// <summary>
        /// Perturbation scale
        /// </summary>
        public float PerturbationScale { get; set; }
        /// <summary>
        /// Gets or sets the clouds base color
        /// </summary>
        public Color4 CloudsBaseColor { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sky plane description class</param>
        public SkyPlane(Scene scene, SkyPlaneDescription description)
            : base(scene, description)
        {
            var img1 = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Texture1Name),
            };
            this.skyTexture1 = this.Game.ResourceManager.RequestResource(img1);

            var img2 = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Texture2Name),
            };
            this.skyTexture2 = this.Game.ResourceManager.RequestResource(img2);

            this.skyMode = description.SkyMode;
            this.rotation = Matrix.Identity;

            this.MaxBrightness = description.MaxBrightness;
            this.MinBrightness = description.MinBrightness;
            this.FadingDistance = description.FadingDistance;
            this.Velocity = description.Velocity;
            this.PerturbationScale = description.PerturbationScale;
            this.Direction = description.Direction;
            this.CloudsBaseColor = description.CloudBaseColor;

            //Create sky plane
            var cPlane = GeometryUtil.CreateCurvePlane(
                description.Size,
                description.Repeat,
                description.PlaneWidth,
                description.PlaneTop,
                description.PlaneBottom);

            var vertices = VertexPositionTexture.Generate(cPlane.Vertices, cPlane.Uvs);
            var indices = cPlane.Indices;

            this.vertexBuffer = this.BufferManager.AddVertexData(description.Name, false, vertices);
            this.indexBuffer = this.BufferManager.AddIndexData(description.Name, false, indices);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SkyPlane()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                this.BufferManager?.RemoveVertexData(this.vertexBuffer);
                this.BufferManager?.RemoveIndexData(this.indexBuffer);
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            float delta = context.GameTime.ElapsedSeconds * this.Velocity * 0.001f;

            this.firstLayerTranslation += this.FirstLayerTranslation * delta;
            this.secondLayerTranslation += this.SecondLayerTranslation * delta;

            this.translation += delta;
            this.translation %= 1f;

            if (context.Lights.KeyLight != null)
            {
                this.brightness = Math.Min(this.MaxBrightness, context.Lights.KeyLight.Brightness + this.MinBrightness);
            }

            this.color = (this.CloudsBaseColor + context.Lights.SunColor) * 0.5f;
        }
        /// <summary>
        /// Draws content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;
            var draw =
                (mode.HasFlag(DrawerModes.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModes.TransparentOnly) && this.Description.AlphaEnabled);

            if (draw && this.indexBuffer.Count > 0)
            {
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;

                this.BufferManager.SetIndexBuffer(this.indexBuffer);

                var effect = DrawerPool.EffectDefaultClouds;
                var technique = this.skyMode == SkyPlaneModes.Static ? effect.CloudsStatic : effect.CloudsPerturbed;

                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer, Topology.TriangleList);

                effect.UpdatePerFrame(
                    this.rotation * Matrix.Translation(context.EyePosition),
                    context.ViewProjection,
                    this.brightness,
                    this.color,
                    this.FadingDistance,
                    this.skyTexture1,
                    this.skyTexture2);

                if (this.skyMode == SkyPlaneModes.Static)
                {
                    effect.UpdatePerFrameStatic(
                        this.firstLayerTranslation,
                        this.secondLayerTranslation);
                }
                else
                {
                    effect.UpdatePerFramePerturbed(
                        this.translation,
                        this.PerturbationScale);
                }

                var graphics = this.Game.Graphics;

                graphics.SetBlendAdditive();

                for (int p = 0; p < technique.PassCount; p++)
                {
                    graphics.EffectPassApply(technique, p, 0);

                    graphics.DrawIndexed(
                        this.indexBuffer.Count,
                        this.indexBuffer.BufferOffset,
                        this.vertexBuffer.BufferOffset);
                }
            }
        }
    }

    /// <summary>
    /// Sky plane extensions
    /// </summary>
    public static class SkyPlaneExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<SkyPlane> AddComponentSkyPlane(this Scene scene, SkyPlaneDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            SkyPlane component = null;

            await Task.Run(() =>
            {
                component = new SkyPlane(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
