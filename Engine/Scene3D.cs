using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// 3D scene
    /// </summary>
    public abstract class Scene3D : Scene
    {
        /// <summary>
        /// Scene component list
        /// </summary>
        private List<Drawable> components = new List<Drawable>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        public Scene3D(Game game)
            : base(game)
        {

        }
        /// <summary>
        /// Scene objects initialization
        /// </summary>
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
        /// <summary>
        /// Game objects updating
        /// </summary>
        /// <param name="gameTime">Game time</param>
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
        /// <summary>
        /// Game objects drawing
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            for (int i = 0; i < this.components.Count; i++)
            {
                if (this.components[i].Visible)
                {
                    this.Game.Graphics.SetDefaultRasterizer();
                    this.Game.Graphics.SetBlendAlphaToCoverage();

                    this.components[i].Draw(gameTime);
                }
            }
        }
        /// <summary>
        /// Window resize handling
        /// </summary>
        public override void HandleWindowResize()
        {
            base.HandleWindowResize();

            for (int i = 0; i < this.components.Count; i++)
            {
                this.components[i].HandleWindowResize();
            }
        }
        /// <summary>
        /// Dispose scene objects
        /// </summary>
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

        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="modelFilename">Model file name</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Model AddModel(string modelFilename, int order = 0)
        {
            return AddModel(modelFilename, Matrix.Identity, order);
        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="modelFilename">Model file name</param>
        /// <param name="transform">Initial transform to apply to loaded geometry</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Model AddModel(string modelFilename, Matrix transform, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(this.ContentPath, modelFilename, transform);

            Model newModel = new Model(this.Game, this, geo);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Model AddModel(ModelContent content, int order = 0)
        {
            Model newModel = new Model(this.Game, this, content);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="modelFilename">Model file name</param>
        /// <param name="instances">Number of instances for the model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public ModelInstanced AddInstancingModel(string modelFilename, int instances, int order = 0)
        {
            return AddInstancingModel(modelFilename, Matrix.Identity, instances, order);
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="modelFilename">Model file name</param>
        /// <param name="transform">Initial transform to apply to loaded geometry</param>
        /// <param name="instances">Number of instances for the model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public ModelInstanced AddInstancingModel(string modelFilename, Matrix transform, int instances, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(this.ContentPath, modelFilename, transform);

            ModelInstanced newModel = new ModelInstanced(this.Game, this, geo, instances);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new instanced model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="instances">Number of instances for the model</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public ModelInstanced AddInstancingModel(ModelContent content, int instances, int order = 0)
        {
            ModelInstanced newModel = new ModelInstanced(this.Game, this, content, instances);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="modelFilename">Model file name</param>
        /// <param name="description">Terrain description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Terrain AddTerrain(string modelFilename, TerrainDescription description, int order = 0)
        {
            return AddTerrain(modelFilename, Matrix.Identity, description, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="modelFilename">Model file name</param>
        /// <param name="transform">Initial transform to apply to loaded geometry</param>
        /// <param name="description">Terrain description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Terrain AddTerrain(string modelFilename, Matrix transform, TerrainDescription description, int order = 0)
        {
            ModelContent geo = LoaderCOLLADA.Load(this.ContentPath, modelFilename, transform);

            return AddTerrain(geo, description, order);
        }
        /// <summary>
        /// Adds new terrain model
        /// </summary>
        /// <param name="content">Content</param>
        /// <param name="description">Terrain description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Terrain AddTerrain(ModelContent content, TerrainDescription description, int order = 0)
        {
            Terrain newModel = new Terrain(this.Game, this, content, this.ContentPath, description);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new skydom
        /// </summary>
        /// <param name="texture">Skydom texture</param>
        /// <param name="radius">Skydom radius</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
        public Cubemap AddSkydom(string texture, int radius, int order = 0)
        {
            ModelContent skydom = ModelContent.GenerateSkydom(this.ContentPath, texture, radius);

            Cubemap newModel = new Cubemap(this.Game, this, skydom);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds new sprite
        /// </summary>
        /// <param name="texture">Srpite texture</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new model</returns>
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
        /// <summary>
        /// Adds text
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="color">Color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new text</returns>
        public TextDrawer AddText(string font, int fontSize, Color color, int order = 0)
        {
            TextDrawer newModel = new TextDrawer(this.Game, this, font, fontSize, color);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds text
        /// </summary>
        /// <param name="font">Font</param>
        /// <param name="fontSize">Font size</param>
        /// <param name="color">Color</param>
        /// <param name="shadowColor">Shadow color</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new text</returns>
        public TextDrawer AddText(string font, int fontSize, Color color, Color shadowColor, int order = 0)
        {
            TextDrawer newModel = new TextDrawer(this.Game, this, font, fontSize, color, shadowColor);

            this.AddComponent(newModel, order);

            return newModel;
        }
        /// <summary>
        /// Adds particle system
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns new particle system</returns>
        public ParticleSystem AddParticleSystem(ParticleSystemDescription description, int order = 0)
        {
            ParticleSystem newModel = new ParticleSystem(this.Game, this, description);

            this.AddComponent(newModel, order);

            return newModel;
        }

        /// <summary>
        /// Add component to collection
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="order">Processing order</param>
        private void AddComponent(Drawable component, int order)
        {
            if (!this.components.Contains(component))
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
        }
        /// <summary>
        /// Remove and dispose component
        /// </summary>
        /// <param name="component">Component</param>
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
