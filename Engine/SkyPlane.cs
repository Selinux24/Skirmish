using SharpDX;
using SharpDX.Direct3D;
using System;

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
        private SkyPlaneMode mode;
        /// <summary>
        /// Plane rotation (Y)
        /// </summary>
        private Matrix rotation;

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
        /// Direction
        /// </summary>
        public Vector2 Direction { get; set; }
        /// <summary>
        /// Perturbation scale
        /// </summary>
        public float PerturbationScale { get; set; }

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
            this.skyTexture1 = this.Game.ResourceManager.CreateResource(img1);

            var img2 = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Texture2Name),
            };
            this.skyTexture2 = this.Game.ResourceManager.CreateResource(img2);

            this.mode = description.Mode;
            this.MaxBrightness = description.MaxBrightness;
            this.MinBrightness = description.MinBrightness;
            this.FadingDistance = description.FadingDistance;
            this.Velocity = description.Velocity;
            this.PerturbationScale = description.PerturbationScale;
            this.Direction = description.Direction;
            this.rotation = Matrix.Identity;

            if (this.Direction != Vector2.Zero)
            {
                float a = Helper.AngleSigned(Vector2.UnitY, Vector2.Normalize(this.Direction));

                this.rotation = Matrix.RotationY(a);
            }

            //Create sky plane
            Vector3[] vData;
            Vector2[] uvs;
            uint[] iData;
            GeometryUtil.CreateCurvePlane(
                description.Size,
                description.Repeat,
                description.PlaneWidth,
                description.PlaneTop,
                description.PlaneBottom,
                out vData, out uvs, out iData);

            VertexPositionTexture[] vertices = VertexPositionTexture.Generate(vData, uvs);

            this.vertexBuffer = this.BufferManager.Add(description.Name, vertices, false, 0);
            this.indexBuffer = this.BufferManager.Add(description.Name, iData, false);
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        public override void Dispose()
        {

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
        }
        /// <summary>
        /// Draws content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;

            if (mode.HasFlag(DrawerModesEnum.ShadowMap) ||
                (mode.HasFlag(DrawerModesEnum.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModesEnum.TransparentOnly) && this.Description.AlphaEnabled))
            {
                if (this.indexBuffer.Count > 0)
                {
                    if (!mode.HasFlag(DrawerModesEnum.ShadowMap))
                    {
                        Counters.InstancesPerFrame++;
                        Counters.PrimitivesPerFrame += this.indexBuffer.Count / 3;
                    }

                    this.BufferManager.SetIndexBuffer(this.indexBuffer.Slot);

                    var effect = DrawerPool.EffectDefaultClouds;
                    var technique = this.mode == SkyPlaneMode.Static ? effect.CloudsStatic : effect.CloudsPerturbed;

                    this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

                    effect.UpdatePerFrame(
                        this.rotation * Matrix.Translation(context.EyePosition),
                        context.ViewProjection,
                        this.brightness,
                        this.FadingDistance,
                        this.skyTexture1,
                        this.skyTexture2);

                    if (this.mode == SkyPlaneMode.Static)
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
                            this.indexBuffer.Offset,
                            this.vertexBuffer.Offset);
                    }
                }
            }
        }
    }
}
