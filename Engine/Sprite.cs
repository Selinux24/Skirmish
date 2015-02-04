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
        /// <param name="description">Description</param>
        public Sprite(Game game, Scene3D scene, SpriteDescription description)
            : base(game, scene, ModelContent.GenerateSprite(scene.ContentPath, description.Textures), false, 0, false, false)
        {
            this.effect = new EffectBasic(game.Graphics.Device);

            int renderWidth = game.Form.RenderWidth;
            int renderHeight = game.Form.RenderHeight;

            this.Width = description.Width <= 0 ? renderWidth : description.Width;
            this.Height = description.Height <= 0 ? renderHeight : description.Height;
            this.FitScreen = description.FitScreen;

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

                foreach (string meshName in this.Meshes.Keys)
                {
                    MeshMaterialsDictionary dictionary = this.Meshes[meshName];

                    #region Per skinning update

                    if (this.SkinningData != null)
                    {
                        this.effect.SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                        this.effect.UpdatePerSkinning();
                    }

                    #endregion

                    foreach (string material in dictionary.Keys)
                    {
                        Mesh mesh = dictionary[material];
                        MeshMaterial mat = this.Materials[material];
                        EffectTechnique technique = this.effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing);

                        #region Per object update

                        if (mat != null)
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(mat.Material);
                            this.effect.UpdatePerObject(mat.DiffuseTexture, mat.NormalMap, this.TextureIndex);
                        }
                        else
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(Material.Default);
                            this.effect.UpdatePerObject(null, null, this.TextureIndex);
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

    /// <summary>
    /// Sprite description
    /// </summary>
    public class SpriteDescription
    {
        /// <summary>
        /// Sprite textures
        /// </summary>
        public string[] Textures;
        /// <summary>
        /// Width
        /// </summary>
        public float Width;
        /// <summary>
        /// Height
        /// </summary>
        public float Height;
        /// <summary>
        /// Fit screen
        /// </summary>
        public bool FitScreen;
    }
}
