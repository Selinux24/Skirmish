using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Cubemap drawer
    /// </summary>
    public class Cubemap : ModelBase
    {
        /// <summary>
        /// Effect
        /// </summary>
        private EffectCubemap effect = null;

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
        public Cubemap(Game game, Scene3D scene, ModelContent content)
            : base(game, scene, content)
        {
            this.effect = new EffectCubemap(game.Graphics.Device);

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
                #region Per frame update

                this.effect.FrameBuffer.WorldViewProjection = this.Scene.World * this.Manipulator.LocalTransform * this.Scene.ViewProjectionPerspective;
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

                        this.effect.UpdatePerObject(mat.DiffuseTexture);

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
