using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
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
    public class SpriteTexture : Drawable
    {
        /// <summary>
        /// Minimap vertex buffer
        /// </summary>
        private Buffer vertexBuffer;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding vertexBufferBinding;
        /// <summary>
        /// Minimap index buffer
        /// </summary>
        private Buffer indexBuffer;
        /// <summary>
        /// Drawer
        /// </summary>
        private EffectBasic effect;
        /// <summary>
        /// Technique
        /// </summary>
        private EffectTechnique effectTechnique;
        /// <summary>
        /// Context to draw minimap
        /// </summary>
        private Context drawContext;
        
        /// <summary>
        /// Texture
        /// </summary>
        public ShaderResourceView Texture;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Minimap description</param>
        public SpriteTexture(Game game, SpriteTextureDescription description)
            : base(game)
        {
            VertexData[] cv;
            uint[] ci;
            VertexData.CreateSprite(
                Vector2.Zero,
                1, 1,
                0, 0,
                out cv,
                out ci);

            List<VertexPositionTexture> vertList = new List<VertexPositionTexture>();

            Array.ForEach(cv, (v) => { vertList.Add(VertexData.CreateVertexPositionTexture(v)); });

            this.vertexBuffer = this.Device.CreateVertexBufferImmutable(vertList.ToArray());
            this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, vertList[0].Stride, 0);
            this.indexBuffer = this.Device.CreateIndexBufferImmutable(ci);

            this.effect = DrawerPool.EffectBasic;
            this.effectTechnique = DrawerPool.EffectBasic.PositionTextureRED;

            this.InitializeContext(description.Left, description.Top, description.Width, description.Height);
        }
        /// <summary>
        /// Initialize minimap context
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        private void InitializeContext(int left, int top, int width, int height)
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

            this.drawContext = new Context()
            {
                EyePosition = eyePos,
                World = world,
                ViewProjection = view * proj,
            };
        }
        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {

        }
        /// <summary>
        /// Draw objects
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            #region Effect update

            this.DeviceContext.InputAssembler.InputLayout = this.effect.GetInputLayout(effectTechnique);
            this.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            this.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            this.effect.FrameBuffer.World = this.drawContext.World;
            this.effect.FrameBuffer.WorldInverse = Matrix.Invert(this.drawContext.World);
            this.effect.FrameBuffer.WorldViewProjection = this.drawContext.World * this.drawContext.ViewProjection;
            this.effect.UpdatePerFrame(null);

            this.effect.ObjectBuffer.Material.SetMaterial(Material.Default);
            this.effect.UpdatePerObject(this.Texture, null);

            this.effect.SkinningBuffer.FinalTransforms = null;
            this.effect.UpdatePerSkinning();

            this.effect.InstanceBuffer.TextureIndex = 0;
            this.effect.UpdatePerInstance();

            #endregion

            for (int p = 0; p < this.effectTechnique.Description.PassCount; p++)
            {
                this.effectTechnique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.DeviceContext.DrawIndexed(6, 0, 0);
            }
        }
        /// <summary>
        /// Dispose objects
        /// </summary>
        public override void Dispose()
        {
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
    public class SpriteTextureDescription
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
    }
}
