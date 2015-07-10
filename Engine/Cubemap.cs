using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Cube-map drawer
    /// </summary>
    public class Cubemap : ModelBase
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
        public Cubemap(Game game, ModelContent content)
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
                    #region Per frame update

                    DrawerPool.EffectCubemap.FrameBuffer.WorldViewProjection = context.World * this.Manipulator.LocalTransform * context.ViewProjection;
                    DrawerPool.EffectCubemap.UpdatePerFrame();

                    #endregion

                    foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                    {
                        foreach (string material in dictionary.Keys)
                        {
                            Mesh mesh = dictionary[material];
                            MeshMaterial mat = this.Materials[material];
                            EffectTechnique technique = DrawerPool.EffectCubemap.GetTechnique(mesh.VertextType, DrawingStages.Drawing);

                            #region Per object update

                            DrawerPool.EffectCubemap.UpdatePerObject(mat.DiffuseTexture);

                            #endregion

                            mesh.SetInputAssembler(this.DeviceContext, DrawerPool.EffectCubemap.GetInputLayout(technique));

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

    /// <summary>
    /// Cube-map description
    /// </summary>
    public class CubemapDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath;
        /// <summary>
        /// Texture
        /// </summary>
        public string Texture;
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius;
    }
}
