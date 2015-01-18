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
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="scene">Scene</param>
        /// <param name="texture">Texture</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Sprite(Game game, Scene3D scene, string texture, float width, float height)
            : base(game, scene, ModelContent.GenerateSprite(scene.ContentPath, texture))
        {
            this.effect = new EffectBasic(game.Graphics.Device);

            this.Width = width;
            this.Height = height;

            this.sourceWidth = width / game.Form.RenderWidth;
            this.sourceHeight = height / game.Form.RenderHeight;

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
                this.effect.FrameBuffer.Lights = new BufferLights(this.Scene.Camera.Position, this.Scene.Lights);
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
                            this.effect.UpdatePerObject(mat.DiffuseTexture);
                        }
                        else
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(Material.Default);
                            this.effect.UpdatePerObject(null);
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
        /// <summary>
        /// Resize handling
        /// </summary>
        public override void HandleWindowResize()
        {
            base.HandleWindowResize();

            if (this.FitScreen)
            {
                this.Width = this.sourceWidth * this.Game.Form.RenderWidth;
                this.Height = this.sourceHeight * this.Game.Form.RenderHeight;
            }
        }
    }
}
