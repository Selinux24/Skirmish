﻿using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    public class Model : ModelBase
    {
        private EffectBasic effect;
        private VolumeBox volumeBox;
        private VolumeSphere volumeSphere;

        public Manipulator Manipulator { get; private set; }

        public Model(Game game, Scene3D scene, ModelContent model, bool debug = false)
            : base(game, scene, model)
        {
            this.effect = new EffectBasic(game.Graphics.Device);
            this.LoadEffectLayouts(this.effect);

            this.Manipulator = new Manipulator();

            if (debug)
            {
                this.volumeBox = new VolumeBox(game, scene, Color.Red);
                this.volumeSphere = new VolumeSphere(game, scene, 30, 10, Color.Yellow);
            }
        }
        public override void Dispose()
        {
            base.Dispose();

            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }

            if (this.volumeBox != null)
            {
                this.volumeBox.Dispose();
                this.volumeBox = null;
            }

            if (this.volumeSphere != null)
            {
                this.volumeSphere.Dispose();
                this.volumeSphere = null;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.Manipulator.Update(gameTime);

            if (this.volumeBox != null)
            {
                BoundingBox bbox = this.ComputeBoundingBox(this.Manipulator.LocalTransform);

                this.volumeBox.Manipulator.SetScale(this.Manipulator.Scaling * (bbox.Maximum - bbox.Minimum));
                this.volumeBox.Manipulator.SetPosition(this.Manipulator.Position + ((bbox.Maximum + bbox.Minimum) * 0.5f));
                this.volumeBox.Update(gameTime);
            }

            if (this.volumeSphere != null)
            {
                BoundingSphere bsphere = this.ComputeBoundingSphere(this.Manipulator.LocalTransform);

                this.volumeSphere.Manipulator.SetScale(this.Manipulator.Scaling * bsphere.Radius);
                this.volumeSphere.Manipulator.SetPosition(this.Manipulator.Position + bsphere.Center);
                this.volumeSphere.Update(gameTime);
            }
        }
        public override void Draw(GameTime gameTime)
        {
            if (this.Meshes != null)
            {
                #region Per frame update

                Matrix local = this.Manipulator.LocalTransform;
                Matrix world = this.Scene.World * local;
                Matrix worldInverse = Matrix.Invert(world);
                Matrix worldViewProjection = world * this.Scene.ViewProjectionPerspective;

                this.effect.FrameBuffer.World = world;
                this.effect.FrameBuffer.WorldInverse = worldInverse;
                this.effect.FrameBuffer.WorldViewProjection = worldViewProjection;
                this.effect.FrameBuffer.Lights = new BufferLights(this.Scene.Camera.Position, this.Scene.Lights);
                this.effect.UpdatePerFrame();

                #endregion

                #region Per skinning update

                if (this.SkinnedData != null)
                {
                    this.effect.SkinningBuffer.FinalTransforms = this.SkinnedData.FinalTransforms;
                    this.effect.UpdatePerSkinning();
                }

                #endregion

                foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                {
                    foreach (string material in dictionary.Keys)
                    {
                        Mesh mesh = dictionary[material];
                        MeshMaterial mat = material != NoMaterial ? this.Materials[material] : null;
                        string techniqueName = this.Techniques[mesh];

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

                        mesh.SetInputAssembler(this.DeviceContext, this.effect.GetInputLayout(techniqueName));

                        EffectTechnique technique = this.effect.GetTechnique(techniqueName);

                        for (int p = 0; p < technique.Description.PassCount; p++)
                        {
                            technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                            mesh.Draw(gameTime, this.DeviceContext);
                        }
                    }
                }
            }

            if (this.volumeBox != null) this.volumeBox.Draw(gameTime);
            if (this.volumeSphere != null) this.volumeSphere.Draw(gameTime);
        }
    }
}
