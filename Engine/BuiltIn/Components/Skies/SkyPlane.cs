﻿using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Skies
{
    using Engine.BuiltIn.Drawers;
    using Engine.BuiltIn.Drawers.Clouds;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Sky plane
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class SkyPlane(Scene scene, string id, string name) : Drawable<SkyPlaneDescription>(scene, id, name)
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Sky texture 1
        /// </summary>
        private EngineShaderResourceView skyTexture1 = null;
        /// <summary>
        /// Sky texture 2
        /// </summary>
        private EngineShaderResourceView skyTexture2 = null;
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
        private SkyPlaneModes skyMode;
        /// <summary>
        /// Clouds color
        /// </summary>
        private Color3 color;
        /// <summary>
        /// Clouds drawer
        /// </summary>
        private BuiltInClouds cloudsDrawer;

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
        /// Destructor
        /// </summary>
        ~SkyPlane()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                BufferManager?.RemoveVertexData(vertexBuffer);
                BufferManager?.RemoveIndexData(indexBuffer);
            }
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(SkyPlaneDescription description)
        {
            await base.ReadAssets(description);

            var img1 = new FileArrayImageContent(Description.ContentPath, Description.Texture1Name);
            skyTexture1 = Game.ResourceManager.RequestResource(img1);

            var img2 = new FileArrayImageContent(Description.ContentPath, Description.Texture2Name);
            skyTexture2 = Game.ResourceManager.RequestResource(img2);

            skyMode = Description.SkyMode;

            MaxBrightness = Description.MaxBrightness;
            MinBrightness = Description.MinBrightness;
            FadingDistance = Description.FadingDistance;
            Velocity = Description.Velocity;
            PerturbationScale = Description.PerturbationScale;
            CloudsBaseColor = Description.CloudBaseColor;

            //Create sky plane
            var cPlane = GeometryUtil.CreateCurvePlane(
                Description.Size,
                Description.Repeat,
                Description.PlaneWidth,
                Description.PlaneTop,
                Description.PlaneBottom);

            var vertices = VertexPositionTexture.Generate(cPlane.Vertices, cPlane.Uvs);
            var indices = cPlane.Indices;

            vertexBuffer = BufferManager.AddVertexData(Name, false, vertices);
            indexBuffer = BufferManager.AddIndexData(Name, false, indices);

            cloudsDrawer = BuiltInShaders.GetDrawer<BuiltInClouds>();
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            float delta = context.GameTime.TotalSeconds * Velocity * 0.001f;

            firstLayerTranslation = FirstLayerTranslation * delta;
            secondLayerTranslation = SecondLayerTranslation * delta;

            translation = delta;
            translation %= 1f;

            var lights = Scene.Lights;
            if (lights.KeyLight != null)
            {
                brightness = MathF.Min(MaxBrightness, lights.KeyLight.Brightness + MinBrightness);
            }

            color = (CloudsBaseColor + lights.SunColor) * 0.5f;
        }
        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!BuffersReady)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            var dc = context.DeviceContext;

            var state = new BuiltInCloudsState
            {
                Perturbed = skyMode == SkyPlaneModes.Perturbed,
                Translation = translation,
                Scale = PerturbationScale,
                FadingDistance = FadingDistance,
                FirstTranslation = firstLayerTranslation,
                SecondTranslation = secondLayerTranslation,
                Color = color,
                Brightness = brightness,
                Clouds1 = skyTexture1,
                Clouds2 = skyTexture2,
            };
            cloudsDrawer.UpdateClouds(dc, state);

            var drawOptions = new DrawOptions
            {
                IndexBuffer = indexBuffer,
                VertexBuffer = vertexBuffer,
                Topology = Topology.TriangleList,
            };
            return cloudsDrawer.Draw(dc, drawOptions);
        }
    }
}
