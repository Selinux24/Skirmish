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
    public class SpriteTexture : Drawable, IScreenFitted
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
        /// Drawing channels
        /// </summary>
        private SpriteTextureChannelsEnum channels = SpriteTextureChannelsEnum.None;

        /// <summary>
        /// Texture
        /// </summary>
        public ShaderResourceView Texture;
        /// <summary>
        /// Drawing channels
        /// </summary>
        public SpriteTextureChannelsEnum Channels
        {
            get
            {
                return this.channels;
            }
            set
            {
                if (this.channels != value)
                {
                    this.channels = value;

                    if (this.effect != null)
                    {
                        if (value == SpriteTextureChannelsEnum.All) this.effectTechnique = this.effect.PositionTexture;
                        else if (value == SpriteTextureChannelsEnum.Red) this.effectTechnique = this.effect.PositionTextureRED;
                        else if (value == SpriteTextureChannelsEnum.Green) this.effectTechnique = this.effect.PositionTextureGREEN;
                        else if (value == SpriteTextureChannelsEnum.Blue) this.effectTechnique = this.effect.PositionTextureBLUE;
                        else if (value == SpriteTextureChannelsEnum.Alpha) this.effectTechnique = this.effect.PositionTextureALPHA;
                        else if (value == SpriteTextureChannelsEnum.NoAlpha) this.effectTechnique = this.effect.PositionTextureNOALPHA;
                    }
                }
            }
        }

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
            this.Channels = description.Channel;

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
            Manipulator2D man = new Manipulator2D();
            man.SetPosition(left, top);
            man.Update(new GameTime(), this.Game.Form.RelativeCenter, width, height);

            Matrix viewProjection = Sprite.CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);

            this.drawContext = new Context()
            {
                EyePosition = new Vector3(0, 0, -1),
                World = man.LocalTransform,
                ViewProjection = viewProjection,
                Lights = SceneLights.Default,
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
            this.DeviceContext.InputAssembler.InputLayout = this.effect.GetInputLayout(effectTechnique);
            this.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            this.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            this.DeviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, Format.R32_UInt, 0);

            this.effect.UpdatePerFrame(this.drawContext.World, this.drawContext.ViewProjection);
            this.effect.UpdatePerObject(Material.Default, this.Texture, null, 0);
            this.effect.UpdatePerSkinning(null);

            for (int p = 0; p < this.effectTechnique.Description.PassCount; p++)
            {
                this.effectTechnique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                this.DeviceContext.DrawIndexed(6, 0, 0);
            }
        }
        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            this.drawContext.ViewProjection = Sprite.CreateViewOrthoProjection(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
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
    /// Channel color
    /// </summary>
    public enum SpriteTextureChannelsEnum
    {
        /// <summary>
        /// No channel selected
        /// </summary>
        None,
        /// <summary>
        /// All
        /// </summary>
        All,
        /// <summary>
        /// Red channel
        /// </summary>
        Red,
        /// <summary>
        /// Green channel
        /// </summary>
        Green,
        /// <summary>
        /// Blue channel
        /// </summary>
        Blue,
        /// <summary>
        /// Alpha channel
        /// </summary>
        Alpha,
        /// <summary>
        /// Without Alpha Channel
        /// </summary>
        NoAlpha,
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
        /// <summary>
        /// Channel color
        /// </summary>
        public SpriteTextureChannelsEnum Channel = SpriteTextureChannelsEnum.All;
    }
}
