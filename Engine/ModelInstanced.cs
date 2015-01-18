using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Instaced model
    /// </summary>
    public class ModelInstanced : ModelBase
    {
        /// <summary>
        /// Effect to draw
        /// </summary>
        private EffectInstancing effect;
        /// <summary>
        /// Instancing data per instance
        /// </summary>
        private VertexInstancingData[] instancingData = null;
        /// <summary>
        /// Manipulator list per instance
        /// </summary>
        private Manipulator3D[] instances = null;

        /// <summary>
        /// Indicates whether the draw call uses z-buffer if available
        /// </summary>
        public bool UseZBuffer { get; set; }
        /// <summary>
        /// Gets manipulator instance list
        /// </summary>
        /// <returns>Returns manipulator instance list</returns>
        public Manipulator3D[] Manipulators
        {
            get
            {
                return this.instances;
            }
        }
        /// <summary>
        /// Gets instance count
        /// </summary>
        public int Count
        {
            get
            {
                return this.instances.Length;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="scene">Scene</param>
        /// <param name="content">Content</param>
        /// <param name="instances">Number of instances</param>
        public ModelInstanced(Game game, Scene3D scene, ModelContent content, int instances)
            : base(game, scene, content, true, instances)
        {
            this.UseZBuffer = true;

            this.effect = new EffectInstancing(game.Graphics.Device);

            this.instancingData = new VertexInstancingData[instances];

            this.instances = new Manipulator3D[instances];
            for (int i = 0; i < instances; i++)
            {
                this.instances[i] = new Manipulator3D();
            }
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

            if (this.instances != null && this.instances.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    Manipulator3D man = this.instances[i];

                    man.Update(gameTime);

                    this.instancingData[i].Local = man.LocalTransform;
                }
            }
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                if (this.UseZBuffer)
                {
                    this.Game.Graphics.EnableZBuffer();
                }
                else
                {
                    this.Game.Graphics.DisableZBuffer();
                }

                #region Per frame update

                this.effect.FrameBuffer.World = this.Scene.World;
                this.effect.FrameBuffer.WorldInverse = this.Scene.WorldInverse;
                this.effect.FrameBuffer.WorldViewProjection = this.Scene.World * this.Scene.ViewProjectionPerspective;
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
                        MeshInstanced mesh = (MeshInstanced)dictionary[material];
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

                        mesh.WriteInstancingData(this.DeviceContext, this.instancingData);

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
