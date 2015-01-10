using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Billboard drawer
    /// </summary>
    public class Billboard : ModelBase
    {
        /// <summary>
        /// Effect
        /// </summary>
        private EffectBillboard effect;

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="scene">Scene</param>
        /// <param name="content">Content</param>
        public Billboard(Game game, Scene3D scene, ModelContent content)
            : base(game, scene, content)
        {
            this.effect = new EffectBillboard(game.Graphics.Device);

            this.Manipulator = new Manipulator3D();
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

            this.Manipulator.Update(gameTime);
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                this.Game.Graphics.SetBlendTransparent();

                #region Per frame update

                this.effect.FrameBuffer.WorldViewProjection = this.Scene.World * this.Manipulator.LocalTransform * this.Scene.ViewProjectionPerspective;
                this.effect.FrameBuffer.Lights = new BufferLights(this.Scene.Camera.Position - this.Manipulator.Position, this.Scene.Lights);
                this.effect.UpdatePerFrame();

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
                            this.effect.ObjectBuffer.TextureCount = (uint)mat.DiffuseTexture.Description.Texture2DArray.ArraySize;
                            this.effect.UpdatePerObject(mat.DiffuseTexture);
                        }
                        else
                        {
                            this.effect.ObjectBuffer.Material.SetMaterial(Material.Default);
                            this.effect.ObjectBuffer.TextureCount = (uint)this.Textures.Count;
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
    }
}
