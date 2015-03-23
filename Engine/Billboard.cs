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
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        public Billboard(Game game, ModelContent content)
            : base(game, content, false, 0, false, false)
        {
            this.Manipulator = new Manipulator3D();
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);

            this.Manipulator.Update(gameTime);
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
                if (context.DrawerMode == DrawerModesEnum.Default)
                {
                    this.Game.Graphics.SetBlendTransparent();

                    #region Per frame update

                    DrawerPool.EffectBillboard.FrameBuffer.WorldViewProjection = context.World * this.Manipulator.LocalTransform * context.ViewProjection;
                    DrawerPool.EffectBillboard.FrameBuffer.ShadowTransform = context.ShadowTransform;
                    DrawerPool.EffectBillboard.FrameBuffer.Lights = new BufferLights(context.EyePosition - this.Manipulator.Position, context.Lights);
                    DrawerPool.EffectBillboard.UpdatePerFrame(context.ShadowMap);

                    #endregion

                    foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                    {
                        foreach (string material in dictionary.Keys)
                        {
                            Mesh mesh = dictionary[material];
                            MeshMaterial mat = this.Materials[material];
                            EffectTechnique technique = DrawerPool.EffectBillboard.GetTechnique(mesh.VertextType, DrawingStages.Drawing);

                            #region Per object update

                            if (mat != null)
                            {
                                DrawerPool.EffectBillboard.ObjectBuffer.Material.SetMaterial(mat.Material);
                                DrawerPool.EffectBillboard.ObjectBuffer.TextureCount = (uint)mat.DiffuseTexture.Description.Texture2DArray.ArraySize;
                                DrawerPool.EffectBillboard.UpdatePerObject(mat.DiffuseTexture);
                            }
                            else
                            {
                                DrawerPool.EffectBillboard.ObjectBuffer.Material.SetMaterial(Material.Default);
                                DrawerPool.EffectBillboard.ObjectBuffer.TextureCount = (uint)this.Textures.Count;
                                DrawerPool.EffectBillboard.UpdatePerObject(null);
                            }

                            #endregion

                            mesh.SetInputAssembler(this.DeviceContext, DrawerPool.EffectBillboard.GetInputLayout(technique));

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
}
