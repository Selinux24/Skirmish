using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Minimap
    /// </summary>
    public class Minimap : Drawable
    {
        /// <summary>
        /// Reference to the terrain that we render in the minimap
        /// </summary>
        private readonly Terrain terrain;
        /// <summary>
        /// Viewport to match the minimap texture size
        /// </summary>
        private readonly Viewport viewport;
        /// <summary>
        /// Minimap render target
        /// </summary>
        private RenderTargetView renderTarget;
        /// <summary>
        /// Minimap texture
        /// </summary>
        private ShaderResourceView renderTexture;
        /// <summary>
        /// Minimap vertex buffer
        /// </summary>
        private Buffer vertexBuffer;
        /// <summary>
        /// Minimap index buffer
        /// </summary>
        private Buffer indexBuffer;
        /// <summary>
        /// Line drawer for viewer frustum
        /// </summary>
        private LineListDrawer lineDrawer;
        /// <summary>
        /// Context to draw terrain
        /// </summary>
        private Context terrainDrawContext;
        /// <summary>
        /// Context to draw minimap
        /// </summary>
        private Context minimapDrawContext;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Minimap description</param>
        public Minimap(Game game, MinimapDescription description)
            : base(game)
        {
            this.terrain = description.Terrain;

            this.viewport = new Viewport(0, 0, description.Width, description.Height);

            using (var texture = this.Device.CreateRenderTargetTexture(description.Width, description.Height))
            {
                this.renderTarget = new RenderTargetView(this.Device, texture);
                this.renderTexture = new ShaderResourceView(this.Device, texture);
            }

            VertexData[] cv;
            uint[] ci;
            VertexData.CreateSprite(
                Vector2.Zero,
                1, 1,
                0, 0,
                out cv,
                out ci);

            List<VertexPositionNormalTexture> vertList = new List<VertexPositionNormalTexture>();

            Array.ForEach(cv, (v) => { vertList.Add(VertexData.CreateVertexPositionNormalTexture(v)); });

            this.vertexBuffer = this.Device.CreateVertexBufferImmutable(vertList.ToArray());
            this.indexBuffer = this.Device.CreateIndexBufferImmutable(ci);

            this.lineDrawer = new LineListDrawer(game, 12);
            this.lineDrawer.UseZBuffer = false;

            this.InitializeTerrainContext();
            this.InitializeMinimapContext(description.Left, description.Top, description.Width, description.Height);
        }
        /// <summary>
        /// Initialize terrain context
        /// </summary>
        private void InitializeTerrainContext()
        {
            BoundingBox bbox = this.terrain.GetBoundingBox();

            float x = bbox.Maximum.X - bbox.Minimum.X;
            float y = bbox.Maximum.Y - bbox.Minimum.Y;
            float z = bbox.Maximum.Z - bbox.Minimum.Z;

            Vector3 eyePos = new Vector3(0, y + 5f, 0);
            Vector3 target = Vector3.Zero;
            Vector3 dir = Vector3.Normalize(target - eyePos);

            Matrix view = Matrix.LookAtLH(
                eyePos,
                target,
                Vector3.UnitZ);

            Matrix proj = Matrix.OrthoLH(
                x,
                z,
                0.1f,
                2000f);

            this.terrainDrawContext = new Context()
            {
                EyePosition = eyePos,
                World = Matrix.Identity,
                ViewProjection = view * proj,
                Lights = new SceneLight()
                {
                    DirectionalLight1 = new SceneLightDirectional()
                    {
                        Ambient = new Color4(0.4f, 0.4f, 0.4f, 1.0f),
                        Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                        Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Direction = dir,
                    },
                    DirectionalLight1Enabled = true,
                },
            };
        }
        /// <summary>
        /// Initialize minimap context
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        private void InitializeMinimapContext(int left, int top, int width, int height)
        {
            Vector3 eyePos = new Vector3(0, 0, -1);
            Vector3 target = Vector3.Zero;
            Vector3 dir = Vector3.Normalize(target - eyePos);

            Manipulator2D man = new Manipulator2D();
            man.SetPosition(left, top);
            man.Update(new GameTime(), this.Game.Form.RelativeCenter, width, height);

            Matrix world = man.LocalTransform;

            Matrix view = Matrix.LookAtLH(
                eyePos,
                target,
                Vector3.Up);

            Matrix proj = Matrix.OrthoLH(
                this.Game.Form.RenderWidth,
                this.Game.Form.RenderHeight,
                0.1f,
                100f);

            this.minimapDrawContext = new Context()
            {
                EyePosition = eyePos,
                World = world,
                ViewProjection = view * proj,
                Lights = new SceneLight()
                {
                    DirectionalLight1 = new SceneLightDirectional()
                    {
                        Ambient = new Color4(0.4f, 0.4f, 0.4f, 1.0f),
                        Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f),
                        Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f),
                        Direction = dir,
                    },
                    DirectionalLight1Enabled = true,
                },
            };
        }
        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {
            this.lineDrawer.SetLines(Color.Red, GeometryUtil.CreateWiredFrustum(new BoundingFrustum(context.ViewProjection)));
        }
        /// <summary>
        /// Draw objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            this.DrawTerrain(gameTime, context);

            this.DrawMinimap(gameTime, context);
        }
        /// <summary>
        /// Draw terrain
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        private void DrawTerrain(GameTime gameTime, Context context)
        {
            this.Game.Graphics.SetRenderTarget(this.viewport, null, this.renderTarget, true, Color.Silver);

            this.terrain.Draw(gameTime, this.terrainDrawContext);

            this.lineDrawer.Draw(gameTime, this.terrainDrawContext);

            this.Game.Graphics.SetDefaultRenderTarget(false);
        }
        /// <summary>
        /// Draw minimap
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        private void DrawMinimap(GameTime gameTime, Context context)
        {
            #region Effect update

            this.DeviceContext.InputAssembler.InputLayout = DrawerPool.EffectBasic.GetInputLayout(DrawerPool.EffectBasic.PositionNormalTexture);
            this.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, new VertexPositionNormalTexture().Stride, 0));
            this.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            DrawerPool.EffectBasic.FrameBuffer.World = this.minimapDrawContext.World;
            DrawerPool.EffectBasic.FrameBuffer.WorldInverse = Matrix.Invert(this.minimapDrawContext.World);
            DrawerPool.EffectBasic.FrameBuffer.WorldViewProjection = this.minimapDrawContext.World * this.minimapDrawContext.ViewProjection;
            DrawerPool.EffectBasic.FrameBuffer.Lights = new BufferLights(this.minimapDrawContext.EyePosition, this.minimapDrawContext.Lights);
            DrawerPool.EffectBasic.UpdatePerFrame();

            DrawerPool.EffectBasic.ObjectBuffer.Material.SetMaterial(Material.Default);
            DrawerPool.EffectBasic.UpdatePerObject(this.renderTexture, null, 0);

            DrawerPool.EffectBasic.SkinningBuffer.FinalTransforms = null;
            DrawerPool.EffectBasic.UpdatePerSkinning();

            #endregion

            for (int p = 0; p < DrawerPool.EffectBasic.PositionNormalTexture.Description.PassCount; p++)
            {
                DrawerPool.EffectBasic.PositionNormalTexture.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.DeviceContext.DrawIndexed(6, 0, 0);
            }
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {
            if (this.renderTarget != null)
            {
                this.renderTarget.Dispose();
                this.renderTarget = null;
            }

            if (this.renderTexture != null)
            {
                this.renderTexture.Dispose();
                this.renderTexture = null;
            }

            if (this.lineDrawer != null)
            {
                this.lineDrawer.Dispose();
                this.lineDrawer = null;
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

    /// <summary>
    /// Minimap description
    /// </summary>
    public class MinimapDescription
    {
        /// <summary>
        /// Top position
        /// </summary>
        public int Top;
        /// <summary>
        /// Left position
        /// </summary>
        public int Left;
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Terrain to draw
        /// </summary>
        public Terrain Terrain;
    }
}
