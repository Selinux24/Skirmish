using System;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;
    using Engine.Helpers;

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
        private ModelInstance[] instances = null;

        /// <summary>
        /// Indicates whether the draw call uses z-buffer if available
        /// </summary>
        public bool UseZBuffer { get; set; }
        /// <summary>
        /// Gets manipulator per instance list
        /// </summary>
        /// <returns>Gets manipulator per instance list</returns>
        public ModelInstance[] Instances
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
        /// Gets visible instance count
        /// </summary>
        public int VisibleCount
        {
            get
            {
                return Array.FindAll(this.instances, i => i.Visible == true).Length;
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

            this.instances = Helper.CreateArray(instances, () => new ModelInstance());
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
                    if (this.instances[i].Active)
                    {
                        this.instances[i].Manipulator.Update(gameTime);
                    }
                }
            }
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null && this.VisibleCount > 0)
            {
                if (this.UseZBuffer)
                {
                    this.Game.Graphics.EnableZBuffer();
                }
                else
                {
                    this.Game.Graphics.DisableZBuffer();
                }

                if (this.instances != null && this.instances.Length > 0)
                {
                    int instanceIndex = 0;
                    for (int i = 0; i < this.instances.Length; i++)
                    {
                        if (this.instances[i].Visible)
                        {
                            this.instancingData[instanceIndex].Local = this.instances[i].Manipulator.LocalTransform;
                            this.instancingData[instanceIndex].TextureIndex = this.instances[i].TextureIndex;

                            instanceIndex++;
                        }
                    }
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

                            mesh.Draw(gameTime, this.DeviceContext, this.VisibleCount);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Model instance
    /// </summary>
    public class ModelInstance
    {
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator = new Manipulator3D();
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex = 0;
        /// <summary>
        /// Active
        /// </summary>
        public bool Active = true;
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible = true;
    }
}
