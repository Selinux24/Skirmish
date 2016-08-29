using SharpDX;
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
        /// Local transform
        /// </summary>
        private Matrix local = Matrix.Identity;
        /// <summary>
        /// Level of detail
        /// </summary>
        private LevelOfDetailEnum levelOfDetail = LevelOfDetailEnum.None;

        /// <summary>
        /// Datos renderización
        /// </summary>
        protected DrawingData DrawingData { get; private set; }
        
        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; set; }
        /// <summary>
        /// Level of detail
        /// </summary>
        public override LevelOfDetailEnum LevelOfDetail
        {
            get
            {
                return this.levelOfDetail;
            }
            set
            {
                this.levelOfDetail = this.GetLODDrawingData(value);
                this.DrawingData = this.ChangeDrawingData(this.DrawingData, this.levelOfDetail);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        public Cubemap(Game game, ModelContent content)
            : base(game, content, false, 0, false, false, false)
        {
            this.Manipulator = new Manipulator3D();
            this.Opaque = true;
            this.DeferredEnabled = true;
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            this.Manipulator.Update(context.GameTime);

            this.local = context.World * this.Manipulator.LocalTransform;
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.DrawingData != null)
            {
                EffectCubemap effect = DrawerPool.EffectCubemap;
                EffectTechnique technique = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) { technique = effect.ForwardCubemap; }
                else if (context.DrawerMode == DrawerModesEnum.Deferred) { technique = effect.DeferredCubemap; }

                if (technique != null)
                {
                    #region Per frame update

                    effect.UpdatePerFrame(
                        this.local,
                        context.ViewProjection,
                        context.EyePosition,
                        context.Frustum,
                        context.Lights);

                    #endregion

                    this.Game.Graphics.SetDepthStencilZDisabled();

                    foreach (MeshMaterialsDictionary dictionary in this.DrawingData.Meshes.Values)
                    {
                        foreach (string material in dictionary.Keys)
                        {
                            Mesh mesh = dictionary[material];
                            MeshMaterial mat = this.DrawingData.Materials[material];

                            #region Per object update

                            effect.UpdatePerObject(mat.DiffuseTexture);

                            #endregion

                            mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

                            for (int p = 0; p < technique.Description.PassCount; p++)
                            {
                                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                mesh.Draw(this.DeviceContext);
                            }
                        }
                    }
                }
            }
        }
    }
}
