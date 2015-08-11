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
            Vector3 pos = new Vector3(0, 0, -1);

            Matrix view = Matrix.LookAtLH(
                pos,
                pos + Vector3.ForwardLH,
                Vector3.Up);

            Matrix orthoProj = Matrix.OrthoLH(
                width,
                height,
                0f, 100f);

            return view * orthoProj;
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
        /// Manipulator
        /// </summary>
        public Manipulator2D Manipulator { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Description</param>
        public Sprite(Game game, SpriteDescription description)
            : base(game, ModelContent.GenerateSprite(description.ContentPath, description.Textures), false, 0, false, false)
        {
            this.renderWidth = game.Form.RenderWidth.Pair();
            this.renderHeight = game.Form.RenderHeight.Pair();
            this.sourceWidth = description.Width <= 0 ? this.renderWidth : description.Width.Pair();
            this.sourceHeight = description.Height <= 0 ? this.renderHeight : description.Height.Pair();
            this.viewProjection = Sprite.CreateViewOrthoProjection(this.renderWidth, this.renderHeight);

            this.Width = this.sourceWidth;
            this.Height = this.sourceHeight;
            this.FitScreen = description.FitScreen;
            this.TextureIndex = 0;

            this.Manipulator = new Manipulator2D();
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);

            this.Manipulator.Update(
                gameTime, 
                this.Game.Form.RelativeCenter, 
                this.Width, 
                this.Height);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            if (this.Meshes != null)
            {
                #region Per frame update

                Matrix world = this.Manipulator.LocalTransform;
                Matrix worldInverse = Matrix.Invert(world);
                Matrix worldViewProjection = world * this.viewProjection;

                DrawerPool.EffectBasic.FrameBuffer.World = world;
                DrawerPool.EffectBasic.FrameBuffer.WorldInverse = worldInverse;
                DrawerPool.EffectBasic.FrameBuffer.WorldViewProjection = worldViewProjection;
                DrawerPool.EffectBasic.UpdatePerFrame(null);

                #endregion

                foreach (string meshName in this.Meshes.Keys)
                {
                    MeshMaterialsDictionary dictionary = this.Meshes[meshName];

                    #region Per skinning update

                    if (this.SkinningData != null)
                    {
                        DrawerPool.EffectBasic.SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                        DrawerPool.EffectBasic.UpdatePerSkinning();
                    }

                    #endregion

                    foreach (string material in dictionary.Keys)
                    {
                        Mesh mesh = dictionary[material];
                        MeshMaterial mat = this.Materials[material];
                        EffectTechnique technique = DrawerPool.EffectBasic.GetTechnique(mesh.VertextType, DrawingStages.Drawing);

                        #region Per object update

                        if (mat != null)
                        {
                            DrawerPool.EffectBasic.ObjectBuffer.Material.SetMaterial(mat.Material);
                            DrawerPool.EffectBasic.UpdatePerObject(mat.DiffuseTexture, mat.NormalMap);
                        }
                        else
                        {
                            DrawerPool.EffectBasic.ObjectBuffer.Material.SetMaterial(Material.Default);
                            DrawerPool.EffectBasic.UpdatePerObject(null, null);
                        }

                        #endregion

                        #region Per instance update

                        DrawerPool.EffectBasic.InstanceBuffer.TextureIndex = this.TextureIndex;
                        DrawerPool.EffectBasic.UpdatePerInstance();

                        #endregion

                        mesh.SetInputAssembler(this.DeviceContext, DrawerPool.EffectBasic.GetInputLayout(technique));

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                            mesh.Draw(gameTime, this.DeviceContext);
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
            int width = this.Game.Form.RenderWidth.Pair();
            int height = this.Game.Form.RenderHeight.Pair();

            this.viewProjection = Sprite.CreateViewOrthoProjection(width, height);

            if (this.FitScreen)
            {
                float w = width / (float)this.renderWidth;
                float h = height / (float)this.renderHeight;

                this.Width = ((int)(this.sourceWidth * w)).Pair();
                this.Height = ((int)(this.sourceHeight * h)).Pair();
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
