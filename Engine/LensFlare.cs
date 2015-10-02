using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine
{
    using Engine.Common;
    using Engine.Helpers;

    public class LensFlare : Drawable
    {
        SceneLightDirectional light;

        Matrix projection;

        Vector2 flareVector;

        public SceneLightDirectional Light
        {
            get
            {
                return this.light;
            }
            set
            {
                this.light = value;
                if (this.light != null)
                {
                    Vector3 lightPosition = this.light.GetPosition(1000000);

                    // Lensflare sprites are positioned at intervals along a line that runs from the 2D light position toward the center of the screen.
                    Vector2 screenCenter = new Vector2(this.Game.Graphics.Viewport.Width, this.Game.Graphics.Viewport.Height) * 0.5f;
                    Vector2 lightPosition2D = new Vector2(lightPosition.X, lightPosition.Y);

                    this.flareVector = screenCenter - lightPosition2D;
                }
                else
                {
                    this.flareVector = Vector2.Zero;
                }
            }
        }

        private Sprite glowSprite;
        private Flare[] flares = null;

        public LensFlare(Game game, LensFlareDescription description)
            : base(game)
        {
            this.projection = Matrix.OrthoOffCenterLH(
                0,
                game.Graphics.Viewport.Width,
                game.Graphics.Viewport.Height,
                0,
                0,
                1);

            this.glowSprite = new Sprite(game, new SpriteDescription()
            {
                ContentPath = description.ContentPath,
                Height = 100,
                Width = 100,
                Textures = new string[] { description.GlowTexture }
            });

            if (description.Flares != null && description.Flares.Length > 0)
            {
                this.flares = new Flare[description.Flares.Length];

                for (int i = 0; i < description.Flares.Length; i++)
                {
                    var flareDesc = description.Flares[i];

                    SpriteDescription sprDesc = new SpriteDescription()
                    {
                        ContentPath = description.ContentPath,
                        Height = 100,
                        Width = 100,
                        Textures = new string[] { flareDesc.Texture }
                    };

                    this.flares[i] = new Flare()
                    {
                        FlareSprite = new Sprite(game, sprDesc),
                        Position = flareDesc.Position,
                        Scale = flareDesc.Scale,
                        Color = flareDesc.Color,
                    };
                }
            }
        }

        public override void Dispose()
        {
            Helper.Dispose(this.glowSprite);
            Helper.Dispose(this.flares);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.light != null)
            {
                this.glowSprite.Update(gameTime);

                if (this.flares != null && this.flares.Length > 0)
                {
                    for (int i = 0; i < this.flares.Length; i++)
                    {
                        this.flares[i].FlareSprite.Update(gameTime);
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime, Context context)
        {
            if (this.light != null)
            {
                // The sun is infinitely distant, so it should not be affected by the
                // position of the camera. Floating point math doesn't support infinitely
                // distant vectors, but we can get the same result by making a copy of our
                // view matrix, then resetting the view translation to zero. Pretending the
                // camera has not moved position gives the same result as if the camera
                // was moving, but the light was infinitely far away. If our flares came
                // from a local object rather than the sun, we would use the original view
                // matrix here.
                Matrix infiniteView = context.View;

                infiniteView.TranslationVector = Vector3.Zero;

                // Project the light position into 2D screen space.
                ViewportF viewport = this.Game.Graphics.Viewport;

                Vector3 projectedPosition = viewport.Project(
                    -this.Light.Direction,
                    context.Projection,
                    infiniteView,
                    Matrix.Identity);

                // Don't draw any flares if the light is behind the camera.
                if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
                {
                    return;
                }

                Vector2 lightPosition = new Vector2(projectedPosition.X, projectedPosition.Y);

                Vector3 p = this.light.GetPosition(100);

                // View pos to light pos
                Vector3 v1 = Vector3.Normalize(p - context.EyePosition);

                // View direction
                Vector3 v2 = context.EyeTarget;

                float angle = Vector3.Dot(v1, v2);

                float alpha = Math.Min(1f, angle * angle);

                this.DrawGlow(gameTime, context, lightPosition, alpha);
                this.DrawFlares(gameTime, context, lightPosition, alpha);
            }
        }

        private void DrawGlow(GameTime gameTime, Context context, Vector2 lightPosition, float angle)
        {
            Color4 color = new Color4(1, 1, 1, angle);
            float scale = 100f / this.glowSprite.Width;

            this.glowSprite.Color = color;
            this.glowSprite.Manipulator.SetPosition(lightPosition);
            this.glowSprite.Manipulator.SetScale(scale);

            //Draw sprite with alpha
            this.Game.Graphics.SetBlendAdditive();
            this.glowSprite.Draw(gameTime, context);
        }

        private void DrawFlares(GameTime gameTime, Context context, Vector2 lightPosition, float angle)
        {
            if (this.flares != null && this.flares.Length > 0)
            {
                this.Game.Graphics.SetBlendAdditive();

                for (int i = 0; i < this.flares.Length; i++)
                {
                    Flare flare = this.flares[i];

                    // Set the flare alpha based on the previous occlusion query result.
                    Color4 flareColor = flare.Color;
                    flareColor.Alpha *= angle;

                    // Compute the position of this flare sprite.
                    Vector2 flarePosition = lightPosition + this.flareVector * flare.Position;

                    flare.FlareSprite.Color = flareColor;
                    flare.FlareSprite.Manipulator.SetPosition(flarePosition);

                    // Draw the flare sprite using additive blending.
                    flare.FlareSprite.Draw(gameTime, context);
                }
            }
        }
    }

    public class Flare : IDisposable
    {
        public Sprite FlareSprite;
        public float Position;
        public float Scale;
        public Color Color;

        public void Dispose()
        {
            Helper.Dispose(this.FlareSprite);
        }
    }

    public class LensFlareDescription
    {
        public string ContentPath;
        public string GlowTexture;
        public FlareDescription[] Flares;
    }

    public class FlareDescription
    {
        public float Position;
        public float Scale;
        public Color Color;
        public string Texture;

        public FlareDescription(float position, float scale, Color color, string texture)
        {
            this.Position = position;
            this.Scale = scale;
            this.Color = color;
            this.Texture = texture;
        }
    }
}
