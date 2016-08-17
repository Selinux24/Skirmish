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
    public class Sprite : ModelBase, IScreenFitted
    {
        /// <summary>
        /// Creates view and orthoprojection from specified size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns view * orthoprojection matrix</returns>
        public static Matrix CreateViewOrthoProjection(int width, int height)
        {
            Matrix view;
            Matrix projection;
            CreateViewOrthoProjection(width, height, out view, out projection);

            return view * projection;
        }
        /// <summary>
        /// Creates view and orthoprojection from specified size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Ortho projection matrix</param>
        public static void CreateViewOrthoProjection(int width, int height, out Matrix view, out Matrix projection)
        {
            Vector3 pos = new Vector3(0, 0, -1);

            view = Matrix.LookAtLH(
                pos,
                pos + Vector3.ForwardLH,
                Vector3.Up);

            projection = Matrix.OrthoLH(
                width,
                height,
                0f, 100f);
        }

        /// <summary>
        /// Source render width
        /// </summary>
        private readonly int renderWidth;
        /// <summary>
        /// Source render height
        /// </summary>
        private readonly int renderHeight;
        /// <summary>
        /// Source width
        /// </summary>
        private readonly int sourceWidth;
        /// <summary>
        /// Source height
        /// </summary>
        private readonly int sourceHeight;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        private Matrix viewProjection;

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
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Relative center
        /// </summary>
        public Vector2 RelativeCenter
        {
            get
            {
                return (new Vector2(this.Width, this.Height)) * 0.5f;
            }
        }
        /// <summary>
        /// Sprite rectangle
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle(
                    this.Left,
                    this.Top,
                    this.Width,
                    this.Height);
            }
        }
        /// <summary>
        /// Indicates whether the sprite has to maintain proportion with window size
        /// </summary>
        public bool FitScreen { get; set; }
        /// <summary>
        /// Gets or sets the texture index to render
        /// </summary>
        public int TextureIndex { get; set; }
        /// <summary>
        /// Base color
        /// </summary>
        public Color4 Color { get; set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Description</param>
        public Sprite(Game game, SpriteDescription description)
            : base(game, ModelContent.GenerateSprite(description.ContentPath, description.Textures), false, 0, false, false, false)
        {
            this.renderWidth = game.Form.RenderWidth.NextPair();
            this.renderHeight = game.Form.RenderHeight.NextPair();
            this.sourceWidth = description.Width <= 0 ? this.renderWidth : description.Width.NextPair();
            this.sourceHeight = description.Height <= 0 ? this.renderHeight : description.Height.NextPair();
            this.viewProjection = Sprite.CreateViewOrthoProjection(this.renderWidth, this.renderHeight);

            this.Width = this.sourceWidth;
            this.Height = this.sourceHeight;
            this.FitScreen = description.FitScreen;
            this.TextureIndex = 0;
            this.Color = Color4.White;

            this.Manipulator = new Manipulator2D();
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            this.Manipulator.Update(context.GameTime, this.Game.Form.RelativeCenter, this.Width, this.Height);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.Meshes != null)
            {
                var effect = DrawerPool.EffectSprite;

                #region Per frame update

                effect.UpdatePerFrame(this.Manipulator.LocalTransform, this.viewProjection);

                #endregion

                this.Game.Graphics.SetDepthStencilZDisabled();

                foreach (string meshName in this.Meshes.Keys)
                {
                    MeshMaterialsDictionary dictionary = this.Meshes[meshName];

                    foreach (string material in dictionary.Keys)
                    {
                        Mesh mesh = dictionary[material];
                        MeshMaterial mat = this.Materials[material];
                        EffectTechnique technique = effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing, context.DrawerMode);

                        #region Per object update

                        if (mat != null)
                        {
                            effect.UpdatePerObject(this.Color, mat.DiffuseTexture, this.TextureIndex);
                        }
                        else
                        {
                            effect.UpdatePerObject(this.Color, null, this.TextureIndex);
                        }

                        #endregion

                        mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                            mesh.Draw(this.DeviceContext);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Resize
        /// </summary>
        public virtual void Resize()
        {
            int width = this.Game.Form.RenderWidth.NextPair();
            int height = this.Game.Form.RenderHeight.NextPair();

            this.viewProjection = Sprite.CreateViewOrthoProjection(width, height);

            if (this.FitScreen)
            {
                float w = width / (float)this.renderWidth;
                float h = height / (float)this.renderHeight;

                this.Width = ((int)(this.sourceWidth * w)).NextPair();
                this.Height = ((int)(this.sourceHeight * h)).NextPair();
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
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Fit screen
        /// </summary>
        public bool FitScreen;
    }

    /// <summary>
    /// Background description
    /// </summary>
    public class BackgroundDescription : SpriteDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="contentPath">Content path</param>
        /// <param name="texture">Tetxure</param>
        public BackgroundDescription()
        {
            this.Width = 0;
            this.Height = 0;
            this.FitScreen = true;
        }
    }
}
