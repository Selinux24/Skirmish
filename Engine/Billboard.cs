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
        /// Drawing radius from eye point
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        public Billboard(Game game, ModelContent content)
            : base(game, content, false, 0, false, false)
        {
            this.Manipulator = new Manipulator3D();
            this.Radius = 0;
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
                EffectBillboard effect = DrawerPool.EffectBillboard;
                EffectTechnique technique = null;
                if (context.DrawerMode == DrawerModesEnum.Default) { technique = effect.Billboard; }
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) { technique = effect.ShadowMapBillboard; }

                if (technique != null)
                {
                    this.Game.Graphics.SetBlendTransparent();

                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        effect.FrameBuffer.WorldViewProjection = context.World * this.Manipulator.LocalTransform * context.ViewProjection;
                        effect.UpdatePerFrame(null);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Default)
                    {
                        effect.FrameBuffer.WorldViewProjection = context.World * this.Manipulator.LocalTransform * context.ViewProjection;
                        effect.FrameBuffer.ShadowTransform = context.ShadowTransform;
                        effect.FrameBuffer.Lights = new BufferLights(context.EyePosition - this.Manipulator.Position, context.Lights);
                        effect.FrameBuffer.Radius = this.Radius;
                        effect.UpdatePerFrame(context.ShadowMap);
                    }

                    #endregion

                    foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                    {
                        foreach (string material in dictionary.Keys)
                        {
                            Mesh mesh = dictionary[material];
                            MeshMaterial mat = this.Materials[material];

                            #region Per object update

                            if (context.DrawerMode == DrawerModesEnum.Default)
                            {
                                if (mat != null)
                                {
                                    effect.ObjectBuffer.Material.SetMaterial(mat.Material);
                                    effect.ObjectBuffer.TextureCount = (uint)mat.DiffuseTexture.Description.Texture2DArray.ArraySize;
                                    effect.UpdatePerObject(mat.DiffuseTexture);
                                }
                                else
                                {
                                    effect.ObjectBuffer.Material.SetMaterial(Material.Default);
                                    effect.ObjectBuffer.TextureCount = (uint)this.Textures.Count;
                                    effect.UpdatePerObject(null);
                                }
                            }

                            #endregion

                            mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

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
