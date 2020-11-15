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
        private Color3 color;

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
                return direction;
            }
            set
            {
                direction = value;

                if (direction != Vector2.Zero)
                {
                    float a = Helper.AngleSigned(Vector2.UnitX, Vector2.Normalize(direction));

                    rotation = Matrix.RotationY(a);
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
        public Color3 CloudsBaseColor { get; set; }
        /// <summary>
        /// Returns true if the buffers were ready
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                if (vertexBuffer?.Ready != true)
                {
                    return false;
                }

                if (indexBuffer?.Ready != true)
                {
                    return false;
                }

                if (indexBuffer.Count <= 0)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Sky plane description class</param>
        public SkyPlane(string name, Scene scene, SkyPlaneDescription description)
            : base(name, scene, description)
        {
            var img1 = ImageContent.Texture(description.ContentPath, description.Texture1Name);
            skyTexture1 = Game.ResourceManager.RequestResource(img1);

            var img2 = ImageContent.Texture(description.ContentPath, description.Texture2Name);
            skyTexture2 = Game.ResourceManager.RequestResource(img2);

            skyMode = description.SkyMode;
            rotation = Matrix.Identity;

            MaxBrightness = description.MaxBrightness;
            MinBrightness = description.MinBrightness;
            FadingDistance = description.FadingDistance;
            Velocity = description.Velocity;
            PerturbationScale = description.PerturbationScale;
            Direction = description.Direction;
            CloudsBaseColor = description.CloudBaseColor;

            //Create sky plane
            var cPlane = GeometryUtil.CreateCurvePlane(
                description.Size,
                description.Repeat,
                description.PlaneWidth,
                description.PlaneTop,
                description.PlaneBottom);

            var vertices = VertexPositionTexture.Generate(cPlane.Vertices, cPlane.Uvs);
            var indices = cPlane.Indices;

            vertexBuffer = BufferManager.AddVertexData(name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(name, false, indices);
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
                BufferManager?.RemoveVertexData(vertexBuffer);
                BufferManager?.RemoveIndexData(indexBuffer);
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            float delta = context.GameTime.ElapsedSeconds * Velocity * 0.001f;

            firstLayerTranslation += FirstLayerTranslation * delta;
            secondLayerTranslation += SecondLayerTranslation * delta;

            translation += delta;
            translation %= 1f;

            if (context.Lights.KeyLight != null)
            {
                brightness = Math.Min(MaxBrightness, context.Lights.KeyLight.Brightness + MinBrightness);
            }

            color = (CloudsBaseColor + context.Lights.SunColor) * 0.5f;
        }
        /// <summary>
        /// Draws content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (!BuffersReady)
            {
                return;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return;
            }

            Counters.InstancesPerFrame++;
            Counters.PrimitivesPerFrame += indexBuffer.Count / 3;

            var effect = DrawerPool.EffectDefaultClouds;
            var technique = skyMode == SkyPlaneModes.Static ? effect.CloudsStatic : effect.CloudsPerturbed;

            BufferManager.SetIndexBuffer(indexBuffer);
            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            effect.UpdatePerFrame(
                rotation * Matrix.Translation(context.EyePosition),
                context.ViewProjection,
                brightness,
                color,
                FadingDistance,
                skyTexture1,
                skyTexture2);

            if (skyMode == SkyPlaneModes.Static)
            {
                effect.UpdatePerFrameStatic(
                    firstLayerTranslation,
                    secondLayerTranslation);
            }
            else
            {
                effect.UpdatePerFramePerturbed(
                    translation,
                    PerturbationScale);
            }

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(
                    indexBuffer.Count,
                    indexBuffer.BufferOffset,
                    vertexBuffer.BufferOffset);
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
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<SkyPlane> AddComponentSkyPlane(this Scene scene, string name, SkyPlaneDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerSky)
        {
            SkyPlane component = null;

            await Task.Run(() =>
            {
                component = new SkyPlane(name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
