using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    public abstract class Scene3D : Scene
    {
        private List<Drawable> components = new List<Drawable>();
        private bool debugMode = false;

        public Scene3D(Game game)
            : base(game)
        {
#if DEBUG
            this.debugMode = true;
#endif
        }
        public override void Initialize()
        {
            base.Initialize();

            this.Lights.DirectionalLight1.Ambient = new Color4(0.4f, 0.4f, 0.4f, 1.0f);
            this.Lights.DirectionalLight1.Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.Lights.DirectionalLight1.Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight1.Direction = Vector3.Normalize(new Vector3(0.57735f, -0.57735f, 0.57735f));
            this.Lights.DirectionalLight1Enabled = true;

            this.Lights.DirectionalLight2.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight2.Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight2.Specular = new Color4(0.25f, 0.25f, 0.25f, 1.0f);
            this.Lights.DirectionalLight2.Direction = Vector3.Normalize(new Vector3(-0.57735f, -0.57735f, 0.57735f));
            this.Lights.DirectionalLight2Enabled = true;

            this.Lights.DirectionalLight3.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight3.Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            this.Lights.DirectionalLight3.Specular = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.DirectionalLight3.Direction = Vector3.Normalize(new Vector3(0.0f, -0.707f, -0.707f));
            this.Lights.DirectionalLight3Enabled = true;

            this.Lights.PointLightEnabled = false;

            this.Lights.SpotLightEnabled = false;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = 0; i < this.components.Count; i++)
            {
                if (this.components[i].Active)
                {
                    this.components[i].Update(gameTime);
                }
            }
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            for (int i = 0; i < this.components.Count; i++)
            {
                if (this.components[i].Visible)
                {
                    this.components[i].Draw(gameTime);
                }
            }
        }
        public override void HandleResizing()
        {
            base.HandleResizing();

            for (int i = 0; i < this.components.Count; i++)
            {
                this.components[i].HandleResizing();
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

        public Model AddModel(string modelFilename, int order = 0)
        {
            return AddModel(modelFilename, Matrix.Identity, order);
        }
        public Model AddModel(string modelFilename, Matrix transform, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(this.ContentPath, modelFilename, transform);

            Model newModel = new Model(this.Game, this, geo, debugMode);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public Model AddModel(ModelContent geometry, int order = 0)
        {
            Model newModel = new Model(this.Game, this, geometry, debugMode);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public ModelInstanced AddInstancingModel(string modelFilename, int instances, int order = 0)
        {
            return AddInstancingModel(modelFilename, Matrix.Identity, instances, order);
        }
        public ModelInstanced AddInstancingModel(string modelFilename, Matrix transform, int instances, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(this.ContentPath, modelFilename, transform);

            ModelInstanced newModel = new ModelInstanced(this.Game, this, geo, instances, debugMode);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public ModelInstanced AddInstancingModel(ModelContent geometry, int instances, int order = 0)
        {
            ModelInstanced newModel = new ModelInstanced(this.Game, this, geometry, instances, debugMode);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public Terrain AddTerrain(string modelFilename, TerrainDescription description, int order = 0)
        {
            return AddTerrain(modelFilename, Matrix.Identity, description, order);
        }
        public Terrain AddTerrain(string modelFilename, Matrix transform, TerrainDescription description, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(this.ContentPath, modelFilename, transform);

            return AddTerrain(geo, description, order);
        }
        public Terrain AddTerrain(ModelContent geometry, TerrainDescription description, int order = 0)
        {
            Terrain newModel = new Terrain(this.Game, this, geometry, this.ContentPath, description, debugMode);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public Cubemap AddSkydom(string texture, int radius, int order = 0)
        {
            ModelContent skydom = ModelContent.GenerateSkydom(this.ContentPath, texture, radius);

            Cubemap newModel = new Cubemap(this.Game, this, skydom);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public Sprite AddSprite(string texture, int width, int height, int order = 0)
        {
            Sprite newModel = new Sprite(
                this.Game,
                this,
                texture,
                width,
                height);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public TextControl AddText(string font, int fontSize, Color color, int order = 0)
        {
            TextControl newModel = new TextControl(this.Game, this, font, fontSize, color);

            this.AddComponent(newModel, order);

            return newModel;
        }
        public TextControl AddText(string font, int fontSize, Color color, Color backColor, int order = 0)
        {
            TextControl newModel = new TextControl(this.Game, this, font, fontSize, color, backColor);

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
            this.components.Sort((p1, p2) =>
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
