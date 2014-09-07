using System.IO;
using SharpDX;
using SharpDX.Direct3D11;

namespace Common
{
    using Common.Utils;

    public abstract class Scene
    {
        protected Device Device
        {
            get
            {
                return this.Game.Graphics.Device;
            }
        }
        protected DeviceContext DeviceContext
        {
            get
            {
                return this.Game.Graphics.DeviceContext;
            }
        }
        protected Game Game { get; private set; }
        public bool Active { get; set; }
        public int Order { get; set; }
        public bool UseZBuffer { get; set; }
        public Matrix World = Matrix.Identity;
        public Camera Camera { get; private set; }
        public SceneLights Lights = new SceneLights();
        public string ContentPath { get; set; }

        public Scene(Game game)
        {
            this.Game = game;

            this.Camera = Camera.CreateFree(
                new Vector3(0.0f, 0.0f, -10.0f),
                Vector3.Zero);

            this.Camera.SetLens(
                this.Game.Graphics.Width,
                this.Game.Graphics.Height);
        
            this.UseZBuffer = true;

            this.ContentPath = "Resources";
        }
        public virtual void Update()
        {
            this.Camera.Update();
        }
        public virtual void Draw()
        {

        }
        public virtual void Dispose()
        {
            if (this.Camera != null)
            {
                this.Camera.Dispose();
                this.Camera = null;
            }
        }

        /// <summary>
        /// Obtiene las matrices aplicando la transformación local
        /// </summary>
        /// <param name="localTransform">Transformación local</param>
        /// <returns>Devuelve las matrices transformadas</returns>
        public BufferMatrix GetMatrizes(Material material, Matrix localTransform)
        {
            Matrix worldLocal = this.World * localTransform;

            return new BufferMatrix()
            {
                World = worldLocal,
                WorldInverse = Matrix.Invert(worldLocal),
                WorldViewProjection = worldLocal * this.Camera.PerspectiveView * this.Camera.PerspectiveProjection,
                Material = new BufferMaterials()
                {
                    Ambient = material.Ambient,
                    Diffuse = material.Diffuse,
                    Reflect = material.Reflective,
                    Specular = material.Specular,
                    Padding = 4000.0f,
                },
            };
        }
        /// <summary>
        /// Obtiene las matrices aplicando la transformación local
        /// </summary>
        /// <param name="localTransform">Transformación local</param>
        /// <returns>Devuelve las matrices transformadas</returns>
        public BufferMatrix GetOrthoMatrizes(Material material, Matrix localTransform)
        {
            Matrix worldLocal = this.World * localTransform;

            return new BufferMatrix()
            {
                World = worldLocal,
                WorldInverse = Matrix.Invert(worldLocal),
                WorldViewProjection = worldLocal * this.Camera.OrthoView * this.Camera.OrthoProjection,
                Material = new BufferMaterials()
                {
                    Ambient = material.Ambient,
                    Diffuse = material.Diffuse,
                    Reflect = material.Reflective,
                    Specular = material.Specular,
                    Padding = 4000.0f,
                },
            };
        }
        /// <summary>
        /// Obtiene la información de luces
        /// </summary>
        /// <returns>Devuelve la información de luces</returns>
        public BufferLights GetLights()
        {
            Matrix world = Matrix.Transpose(this.World);

            Vector3 eyePosition;
            Vector3.Transform(
                ref this.Camera.Position,
                ref world,
                out eyePosition);

            return new BufferLights()
            {
                DirectionalLight1 = this.Lights.DirectionalLight1,
                DirectionalLight2 = this.Lights.DirectionalLight2,
                DirectionalLight3 = this.Lights.DirectionalLight3,
                PointLight = this.Lights.PointLight,
                SpotLight = this.Lights.SpotLight,
                FogStart = this.Lights.FogStart,
                FogRange = this.Lights.FogRange,
                FogColor = this.Lights.FogColor,
                EyePositionWorld = this.Camera.Position,
            };
        }

        public string FindContent(string resourcePath)
        {
            if (File.Exists(resourcePath))
            {
                return resourcePath;
            }
            else
            {
                return Path.Combine(this.ContentPath, resourcePath);
            }
        }
    
        public string[] FindContent(string[] resourcePaths)
        {
            if (resourcePaths != null && resourcePaths.Length > 0)
            {
                for (int i = 0; i < resourcePaths.Length; i++)
                {
                    if (!File.Exists(resourcePaths[i]))
                    {
                        resourcePaths[i] = Path.Combine(this.ContentPath, resourcePaths[i]);
                    }
                }
            }

            return resourcePaths;
        }
    }
}
