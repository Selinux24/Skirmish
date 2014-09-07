using System.Collections.Generic;
using SharpDX;

namespace Common
{
    using Common.Utils;

    public abstract class Scene3D : Scene
    {
        private List<Drawable> components = new List<Drawable>();

        public Scene3D(Game game)
            : base(game)
        {
            this.Lights.DirectionalLight1.Ambient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            this.Lights.DirectionalLight1.Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight1.Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0.57735f, -0.57735f, 0.57735f));
            this.Lights.DirectionalLight1Enabled = true;

            this.Lights.DirectionalLight2.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight2.Diffuse = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            this.Lights.DirectionalLight2.Specular = new Color4(0.25f, 0.25f, 0.25f, 1.0f);
            this.Lights.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, 0.57735f));
            this.Lights.DirectionalLight2Enabled = true;

            this.Lights.DirectionalLight3.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight3.Diffuse = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            this.Lights.DirectionalLight3.Specular = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight3.Direction = Vector3.Normalize(new Vector3(0.0f, -0.707f, -0.707f));
            this.Lights.DirectionalLight3Enabled = true;

            this.Lights.PointLightEnabled = false;

            this.Lights.SpotLightEnabled = false;
        }
        public override void Update()
        {
            base.Update();

            for (int i = 0; i < this.components.Count; i++)
            {
                if (this.components[i].Active)
                {
                    this.components[i].Update();
                }
            }
        }
        public override void Draw()
        {
            base.Draw();

            for (int i = 0; i < this.components.Count; i++)
            {
                if (this.components[i].Visible)
                {
                    this.components[i].Draw();
                }
            }
        }
        public override void Dispose()
        {
            base.Dispose();

            for (int i = 0; i < this.components.Count; i++)
            {
                this.components[i].Dispose();
            }

            this.components.Clear();
            this.components = null;
        }

        public BasicModel AddModel(string modelFilename, int order = 0)
        {
            return AddModel(
                modelFilename, 
                Matrix.Identity, 
                Matrix.Identity, 
                Matrix.Identity, 
                order);
        }
        public BasicModel AddModel(string modelFilename, Matrix translation, Matrix rotation, Matrix scale, int order = 0)
        {
            Geometry[] geo = this.Device.LoadCollada(
                this.FindContent(modelFilename),
                translation,
                rotation,
                scale);

            BasicModel newModel = new BasicModel(this.Game, this, geo);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public BasicModel AddModel(Geometry geometry, int order = 0)
        {
            BasicModel newModel = new BasicModel(this.Game, this, new Geometry[] { geometry });

            this.AddComponent(newModel, order);

            return newModel;
        }
        public BasicModel AddModel(Geometry[] geometry, int order = 0)
        {
            BasicModel newModel = new BasicModel(this.Game, this, geometry);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public InstancingModel AddInstancingModel(string modelFilename, int instances, int order = 0)
        {
            return AddInstancingModel(
                modelFilename, 
                Matrix.Identity, 
                Matrix.Identity, 
                Matrix.Identity, 
                instances, 
                order);
        }
        public InstancingModel AddInstancingModel(string modelFilename, Matrix translation, Matrix rotation, Matrix scale, int instances, int order = 0)
        {
            Geometry[] geo = this.Device.LoadColladaInstanced(
                this.FindContent(modelFilename),
                translation, 
                rotation, 
                scale, 
                instances);

            InstancingModel newModel = new InstancingModel(this.Game, this, geo, instances);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public InstancingModel AddInstancingModel(Geometry geometry, int instances, int order = 0)
        {
            InstancingModel newModel = new InstancingModel(this.Game, this, new Geometry[] { geometry }, instances);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public InstancingModel AddInstancingModel(Geometry[] geometry, int instances, int order = 0)
        {
            InstancingModel newModel = new InstancingModel(this.Game, this, geometry, instances);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public Billboard AddBillboard(string modelFilename, string[] textures, int seed = 0, int order = 0)
        {
            return AddBillboard(
                modelFilename,
                Matrix.Identity, 
                Matrix.Identity, 
                Matrix.Identity,
                textures, 
                seed, 
                order);
        }
        public Billboard AddBillboard(string modelFilename, Matrix translation, Matrix rotation, Matrix scale, string[] textures, float saturation, int seed = 0, int order = 0)
        {
            Geometry[] geo = this.Device.PopulateBillboard(
                this.FindContent(modelFilename),
                translation, 
                rotation, 
                scale,
                this.FindContent(textures),
                saturation, 
                seed);

            Billboard newModel = new Billboard(this.Game, this, geo);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public Cubemap AddCubemap(string texture, int radius, int order = 0)
        {
            Material mat = Material.CreateTextured(this.FindContent(texture));

            Geometry skydom = this.Device.GenerateSkydom(mat, radius);

            Cubemap newModel = new Cubemap(this.Game, this, new Geometry[] { skydom });

            this.AddComponent(newModel, order);

            return newModel;
        }
        public BasicSprite AddSprite(string modelFilename, int width, int height, int order = 0)
        {
            BasicSprite newModel = new BasicSprite(
                this.Game, 
                this,
                this.FindContent(modelFilename), 
                width, 
                height);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public TextControl AddText(string font, int size, int order = 0)
        {
            TextControl newModel = new TextControl(this.Game, this, font, size);

            this.AddComponent(newModel, order);

            return newModel;
        }
        private void AddComponent(Drawable component, int order)
        {
            if (order == 0)
            {
                component.Order = this.components.Count + 1;
            }
            else
            {
                component.Order = order;
            }

            this.components.Add(component);
            this.components.Sort(
                delegate(Drawable p1, Drawable p2)
                {
                    return p1.Order.CompareTo(p2.Order);
                });
        }
        public void RemoveComponent(Drawable component)
        {
            if (this.components.Contains(component))
            {
                this.components.Remove(component);

                component.Dispose();
                component = null;
            }
        }
    }
}
