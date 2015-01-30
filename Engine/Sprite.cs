using System;
using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Sprite drawer
    /// </summary>
    public class Sprite : ModelBase
    {
        /// <summary>
        /// Effect
        /// </summary>
        private EffectBasic effect = null;
        /// <summary>
        /// Source render width
        /// </summary>
        private float previousRenderWidth;
        /// <summary>
        /// Source render height
        /// </summary>
        private float previousRenderHeight;
        /// <summary>
        /// Source width
        /// </summary>
        private readonly float sourceWidth;
        /// <summary>
        /// Source height
        /// </summary>
        private readonly float sourceHeight;

        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        public bool FitScreen { get; set; }
        /// <summary>
        /// Gets or sets text left position in 2D screen
        /// </summary>
        public int Left
        {
            get
            {
                return (int)this.Manipulator.Position.X;
            }
            set
            {
                this.Manipulator.SetPosition(new Vector2(value, this.Manipulator.Position.Y));
            }
        }
        /// <summary>
        /// Gets or sets text top position in 2D screen
        /// </summary>
        public int Top
        {
            get
            {
                return (int)this.Manipulator.Position.Y;
            }
            set
            {
                this.Manipulator.SetPosition(new Vector2(this.Manipulator.Position.X, value));
            }
        }
        /// <summary>
        /// Width
        /// </summary>
        public float Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public float Height { get; set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Gets or sets the texture index to render
        /// </summary>
        public int TextureIndex { get; set; }
        /// <summary>
        /// Sprite rectangle
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(
                    (int)this.Manipulator.Position.X,
                    (int)this.Manipulator.Position.Y,
                    (int)this.Width,
                    (int)this.Height);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="scene">Scene</param>
        /// <param name="texture">Texture</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="fitScreen">Fit screen</param>
        public Sprite(Game game, Scene3D scene, string texture, float width, float height, bool fitScreen = false)
            : this(game, scene, new[] { texture }, width, height, fitScreen)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="scene">Scene</param>
        /// <param name="textures">Texture array</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="fitScreen">Fit screen</param>
        public Sprite(Game game, Scene3D scene, string[] textures, float width, float height, bool fitScreen = false)
            : base(game, scene, ModelContent.GenerateSprite(scene.ContentPath, textures), false, 0, false)
        {
            this.effect = new EffectBasic(game.Graphics.Device);

            int renderWidth = game.Form.RenderWidth;
            int renderHeight = game.Form.RenderHeight;

            this.Width = width <= 0 ? renderWidth : width;
            this.Height = height <= 0 ? renderHeight : height;
            this.FitScreen = fitScreen;

            this.previousRenderWidth = renderWidth;
            this.previousRenderHeight = renderHeight;
            this.sourceWidth = this.Width / renderWidth;
            this.sourceHeight = this.Height / renderHeight;

            this.TextureIndex = 0;

            this.Manipulator = new Manipulator2D();
        }
        /// <summary>
        /// Resource disposing
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.FitScreen)
            {
                if (this.previousRenderWidth != this.Game.Form.RenderWidth ||
                    this.previousRenderHeight != this.Game.Form.RenderHeight)
                {
                    float leftRelative = (float)this.Left / (float)this.previousRenderWidth;
                    float topRelative = (float)this.Top / (float)this.previousRenderHeight;
                    this.Left = (int)Math.Round(leftRelative * this.Game.Form.RenderWidth, 0);
                    this.Top = (int)Math.Round(topRelative * this.Game.Form.RenderHeight, 0);

                    this.Width = this.sourceWidth * this.Game.Form.RenderWidth;
                    this.Height = this.sourceHeight * this.Game.Form.RenderHeight;

                    this.previousRenderWidth = this.Game.Form.RenderWidth;
                    this.previousRenderHeight = this.Game.Form.RenderHeight;
                }
            }

            this.Manipulator.Update(gameTime, this.Game.Form.RelativeCenter, this.Width, this.Height);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                this.Game.Graphics.EnableZBuffer();

                #region Per frame update

                Matrix world = this.Scene.World * this.Manipulator.LocalTransform;
                Matrix worldInverse = Matrix.Invert(world);
                Matrix worldViewProjection = world * this.Scene.ViewProjectionOrthogonal;

                this.effect.FrameBuffer.World = world;
                this.effect.FrameBuffer.WorldInverse = worldInverse;
                this.effect.FrameBuffer.WorldViewProjection = worldViewProjection;
                this.effect.FrameBuffer.Lights = new BufferLights(this.Scene.Camera.Position);
                this.effect.UpdatePerFrame();

                #endregion

                #region Per skinning update

                if (this.SkinningData != null)
                {
                    this.effect.SkinningBuffer.FinalTransforms = this.SkinningData.FinalTransforms;
                    this.effect.UpdatePerSkinning();
                }

                #endregion

                foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                {
                    foreach (string material in dictionary.Keys)
                    {
                        Mesh mesh = dictionary[material];
                        MeshMaterial mat = this.Materials[material];
                        EffectTechnique technique = this.effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing);

                        #region Per object update

                        if (mat != null)
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(mat.Material);
                            this.effect.UpdatePerObject(mat.DiffuseTexture, this.TextureIndex);
                        }
                        else
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(Material.Default);
                            this.effect.UpdatePerObject(null, this.TextureIndex);
                        }

                        #endregion

                        mesh.SetInputAssembler(this.DeviceContext, this.effect.GetInputLayout(technique));

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                            mesh.Draw(gameTime, this.DeviceContext);
                        }
                    }
                }
            }
        }
    }
}
