using Engine;
using Engine.Common;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Terrain
{
    class PerlinNoiseScene : Scene
    {
        UITextureRenderer perlinRenderer;
        EngineShaderResourceView texture;
        int mapSize = 256;
        float mapScale = 0.5f;
        float mapLacunarity = 2;
        float mapPersistance = 0.5f;
        int mapOctaves = 5;
        Vector2 mapOffset = Vector2.One;
        int mapSeed = 0;

        public PerlinNoiseScene(Game game) : base(game)
        {
            GameEnvironment.Background = Color.AliceBlue;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await LoadResourcesAsync(InitializeTextureRenderer());
        }

        public async Task InitializeTextureRenderer()
        {
            texture = this.Game.ResourceManager.RequestResource(Guid.NewGuid(), GenerateMap(), mapSize, true);

            float size = this.Game.Form.RenderHeight * 0.8f;

            perlinRenderer = await this.AddComponentUITextureRenderer(UITextureRendererDescription.Default(size, size));
            perlinRenderer.CenterHorizontally = CenterTargets.Screen;
            perlinRenderer.CenterVertically = CenterTargets.Screen;
            perlinRenderer.Texture = texture;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            bool updateMap = false;

            float delta = gameTime.ElapsedSeconds;
            float scaleDelta = delta * 0.1f;

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                mapScale += scaleDelta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                mapScale -= scaleDelta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                mapOffset.Y -= delta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                mapOffset.Y += delta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                mapOffset.X -= delta;
                updateMap = true;
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                mapOffset.X += delta;
                updateMap = true;
            }

            if (this.Game.Input.KeyJustPressed(Keys.X))
            {
                mapSeed += 100 + (int)(delta * 10000f);
                updateMap = true;
            }

            if (!updateMap)
            {
                return;
            }

            mapScale = Math.Max(mapScale, 0);

            mapOffset.X = Math.Max(mapOffset.X, 1f);
            mapOffset.Y = Math.Max(mapOffset.Y, 1f);

            texture.Update(this.Game, GenerateMap());
        }

        private IEnumerable<Vector4> GenerateMap()
        {
            NoiseMapDescriptor nmDesc = new NoiseMapDescriptor
            {
                MapWidth = mapSize,
                MapHeight = mapSize,
                Scale = mapScale,
                Lacunarity = mapLacunarity,
                Persistance = mapPersistance,
                Octaves = mapOctaves,
                Offset = mapOffset,
                Seed = mapSeed,
            };

            return Perlin.CreateNoiseTexture(nmDesc).Select(c => c.ToVector4()).ToArray();
        }
    }
}
