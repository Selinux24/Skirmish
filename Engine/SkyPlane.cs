using SharpDX;
using SharpDX.Direct3D;
using System;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

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
        /// Vertex buffer offset
        /// </summary>
        private int vertexBufferOffset = -1;
        /// <summary>
        /// Vertex buffer slot
        /// </summary>
        private int vertexBufferSlot = -1;
        /// <summary>
        /// Vertex count
        /// </summary>
        private int vertexCount = 0;
        /// <summary>
        /// Index buffer offset
        /// </summary>
        private int indexBufferOffset = -1;
        /// <summary>
        /// Index buffer slot
        /// </summary>
        private int indexBufferSlot = -1;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexCount = 0;
        /// <summary>
        /// Sky texture 1
        /// </summary>
        private ShaderResourceView skyTexture1 = null;
        /// <summary>
        /// Sky texture 2
        /// </summary>
        private ShaderResourceView skyTexture2 = null;
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
        /// Maximum number of instances
        /// </summary>
        public override int MaxInstances
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Sky plane description class</param>
        public SkyPlane(Game game, BufferManager bufferManager, SkyPlaneDescription description)
            : base(game, bufferManager, description)
        {
            this.Cull = false;

            var img1 = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Texture1Name),
            };
            this.skyTexture1 = game.ResourceManager.CreateResource(img1);

            var img2 = new ImageContent()
            {
                Streams = ContentManager.FindContent(description.ContentPath, description.Texture2Name),
            };
            this.skyTexture2 = game.ResourceManager.CreateResource(img2);

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

            var indices = iData;

            this.BufferManager.Add(0, vertices, false, 0, out this.vertexBufferOffset, out this.vertexBufferSlot);
            this.BufferManager.Add(0, indices, false, out this.indexBufferOffset, out this.indexBufferSlot);

            this.vertexCount = vertices.Length;
            this.indexCount = indices.Length;
        }
        /// <summary>
        /// Resource releasing
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.skyTexture1);
            Helper.Dispose(this.skyTexture2);
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
            if (this.indexCount > 0)
            {
                this.BufferManager.SetIndexBuffer(this.Game.Graphics, this.indexBufferSlot);

                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame++;
                    Counters.PrimitivesPerFrame += this.indexCount / 3;
                }

                var effect = DrawerPool.EffectDefaultClouds;
                var technique = this.mode == SkyPlaneMode.Static ? effect.CloudsStatic : effect.CloudsPerturbed;

                this.BufferManager.SetInputAssembler(this.Game.Graphics, technique, VertexTypes.PositionTexture, false, PrimitiveTopology.TriangleList);

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

                this.Game.Graphics.SetBlendAdditive();

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.DrawIndexed(this.indexCount, this.indexBufferOffset, this.vertexBufferOffset);

                    Counters.DrawCallsPerFrame++;
                }
            }
        }
    }
}
