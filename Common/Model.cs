using System.Collections.Generic;
using System.IO;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Common
{
    using Common.Utils;

    public abstract class Model : Drawable
    {
        protected Scene3D Scene { get; private set; }
        protected Dictionary<VertexTypes, Drawer> Drawers = new Dictionary<VertexTypes, Drawer>();
        protected Dictionary<string, ShaderResourceView> Textures = new Dictionary<string, ShaderResourceView>();
        protected Geometry[] Geometry = null;

        public Model(Game game, Scene3D scene, Geometry[] geometry, bool effectFramework = true)
            : base(game)
        {
            this.Scene = scene;

            this.Geometry = geometry;

            if (this.Geometry != null && this.Geometry.Length > 0)
            {
                this.ProcessGeometry(effectFramework);

                this.ProcessMaterials();
            }
        }
        public override void Dispose()
        {
            if (this.Textures != null)
            {
                foreach (ShaderResourceView res in this.Textures.Values)
                {
                    res.Dispose();
                }

                this.Textures.Clear();
                this.Textures = null;
            }

            if (this.Drawers != null)
            {
                foreach (VertexTypes type in this.Drawers.Keys)
                {
                    this.Drawers[type].Dispose();
                }
            }

            if (this.Geometry != null)
            {
                for (int i = 0; i < this.Geometry.Length; i++)
                {
                    this.Geometry[i].Dispose();
                }

                this.Geometry = null;
            }

            base.Dispose();
        }
        public override void Draw()
        {
            base.Draw();

            foreach (VertexTypes type in this.Drawers.Keys)
            {
                Drawer drawer = this.Drawers[type];

                if (drawer is EffectBase)
                {
                    this.DrawEffect((EffectBase)drawer, type);
                }
                else if (drawer is ShaderBase)
                {
                    this.DrawShader((ShaderBase)drawer, type);
                }
            }
        }
        protected abstract void DrawEffect(EffectBase effect, VertexTypes type);
        protected abstract void DrawShader(ShaderBase shader, VertexTypes type);

        protected abstract void ProcessGeometry(bool effectFramework);
        protected virtual void ProcessMaterials()
        {
            for (int i = 0; i < this.Geometry.Length; i++)
            {
                Material mat = this.Geometry[i].Material;
                if (mat.Textured)
                {
                    if (!this.Textures.ContainsKey(mat.Texture.Name))
                    {
                        ShaderResourceView resourceView = null;

                        if (!mat.Texture.IsArray)
                        {
                            resourceView = this.Game.Graphics.Device.LoadTexture(mat.Texture.Texture);
                        }
                        else
                        {
                            resourceView = this.Game.Graphics.Device.LoadTextureArray(mat.Texture.TextureArray);
                        }

                        this.Textures.Add(mat.Texture.Name, resourceView);
                    }
                }
            }
        }
    }
}
