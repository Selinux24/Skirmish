using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using RenderTargetView = SharpDX.Direct3D11.RenderTargetView;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    public class Minimap : Drawable
    {
        /// <summary>
        /// Viewport to match the minimap texture size
        /// </summary>
        private readonly Viewport viewport;
        /// <summary>
        /// Reference to the terrain that we render in the minimap
        /// </summary>
        private readonly Terrain terrain;
        /// <summary>
        /// array of planes defining the "box" that surrounds the terrain
        /// </summary>
        private readonly Plane[] edgePlanes;
        /// <summary>
        /// Minimap render target
        /// </summary>
        private RenderTargetView renderTarget;

        private Buffer vertexBuffer;

        private Buffer indexBuffer;

        private Matrix viewProjection;

        private EffectBasic effect;

        private LineListDrawer lineDrawer;

        /// <summary>
        /// Minimap texture
        /// </summary>
        public ShaderResourceView Texture { get; private set; }
        /// <summary>
        /// Gets or sets the screen position
        /// </summary>
        public Vector2 Position { get; set; }
        /// <summary>
        /// Gets or sets the minimap size
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Minimap description</param>
        public Minimap(Game game, Scene3D scene, MinimapDescription description)
            : base(game, scene)
        {
            this.terrain = description.Terrain;
            BoundingBox bbox = this.terrain.GetBoundingBox();

            this.viewport = new Viewport(0, 0, description.Width, description.Height);

            using (var texture = this.Device.CreateRenderTargetTexture((int)description.Width, (int)description.Height))
            {
                this.renderTarget = new RenderTargetView(this.Device, texture);
                this.Texture = new ShaderResourceView(this.Device, texture);
            }

            this.Position = new Vector2(description.Left, description.Top);
            this.Size = new Vector2(description.Width, description.Height);

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[]
            {
                new VertexPositionNormalTexture(){ Position = new Vector3(-1, -1, 0), Normal = new Vector3(0, 0, 1), Texture = new Vector2(0, 1) },
                new VertexPositionNormalTexture(){ Position = new Vector3(-1,  1, 0), Normal = new Vector3(0, 0, 1), Texture = new Vector2(0, 0) },
                new VertexPositionNormalTexture(){ Position = new Vector3( 1,  1, 0), Normal = new Vector3(0, 0, 1), Texture = new Vector2(1, 0) },
                new VertexPositionNormalTexture(){ Position = new Vector3( 1, -1, 0), Normal = new Vector3(0, 0, 1), Texture = new Vector2(1, 1) },
            };
            this.vertexBuffer = this.Device.CreateVertexBufferImmutable(vertices);
            this.indexBuffer = this.Device.CreateIndexBufferImmutable(new[] { 0, 1, 2, 0, 2, 3 });

            this.edgePlanes = new[]
            {
                new Plane(1, 0, 0, bbox.Minimum.X),
                new Plane(-1, 0, 0, bbox.Maximum.X),
                new Plane(0, 1, 0, bbox.Minimum.Z),
                new Plane(0, -1, 0, bbox.Maximum.Z),
            };

            float width = bbox.Maximum.X - bbox.Minimum.X;
            float depth = bbox.Maximum.Z - bbox.Minimum.Z;

            Matrix projection = Matrix.OrthoLH(width, depth, 0.1f, 2000);
            Matrix view = Matrix.LookAtLH(new Vector3(0, depth, 0), Vector3.Zero, Vector3.UnitZ);

            this.viewProjection = view * projection;

            this.lineDrawer = new LineListDrawer(game, scene, 5);

            this.effect = new EffectBasic(this.Device);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(GameTime gameTime)
        {
            this.Game.Graphics.DisableZBuffer();

            this.Game.Graphics.SetRenderTarget(this.renderTarget, this.viewport);

            this.DeviceContext.ClearRenderTargetView(this.renderTarget, Color.White);

            //TODO: Draw terrain for minimap, using internal camera
            this.terrain.Draw(gameTime);

            //TODO: Draw frustum lines for minimap, using internal camera
            this.lineDrawer.Draw(gameTime);

            #region Effect update

            this.DeviceContext.InputAssembler.InputLayout = this.effect.GetInputLayout(this.effect.PositionNormalTexture);
            this.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, new VertexPositionNormalTexture().Stride, 0));
            this.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            Matrix world = Matrix.Scaling(100, 100, 1);

            this.effect.FrameBuffer.World = world;
            this.effect.FrameBuffer.WorldInverse = Matrix.Invert(world);
            this.effect.FrameBuffer.WorldViewProjection = world * this.Scene.ViewProjectionOrthogonal;
            this.effect.UpdatePerFrame();

            this.effect.ObjectBuffer.Material.SetMaterial(Material.Default);
            this.effect.UpdatePerObject(this.Texture, null, 0);

            this.effect.SkinningBuffer.FinalTransforms = null;
            this.effect.UpdatePerSkinning();

            #endregion

            for (int p = 0; p < this.effect.PositionNormalTexture.Description.PassCount; p++)
            {
                this.effect.PositionNormalTexture.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.DeviceContext.DrawIndexed(6, 0, 0);
            }

            this.Game.Graphics.SetDefaultRenderTarget();
        }

        public override void Dispose()
        {
            if (this.renderTarget != null)
            {
                this.renderTarget.Dispose();
                this.renderTarget = null;
            }

            if (this.Texture != null)
            {
                this.Texture.Dispose();
                this.Texture = null;
            }

            if (this.lineDrawer != null)
            {
                this.lineDrawer.Dispose();
                this.lineDrawer = null;
            }

            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }

            if (this.vertexBuffer != null)
            {
                this.vertexBuffer.Dispose();
                this.vertexBuffer = null;
            }

            if (this.indexBuffer != null)
            {
                this.indexBuffer.Dispose();
                this.indexBuffer = null;
            }
        }
    }

    public class MinimapDescription
    {
        public int Top;
        public int Left;
        public int Width;
        public int Height;
        public Terrain Terrain;
    }
}
