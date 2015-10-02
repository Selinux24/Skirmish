using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    public class LensFlare : Drawable
    {
        private Sprite glowSprite;
        private Flare[] flares = null;

        public SceneLightDirectional Light { get; set; }

        public LensFlare(Game game, LensFlareDescription description)
            : base(game)
        {
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
            if (this.Light != null)
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
            if (this.Light != null)
            {
                // Set view translation to Zero to simulate infinite
                Matrix infiniteView = context.View;
                infiniteView.TranslationVector = Vector3.Zero;

                // Project the light position into 2D screen space.
                Vector3 projectedPosition = this.Game.Graphics.Viewport.Project(
                    -this.Light.Direction,
                    context.Projection,
                    infiniteView,
                    Matrix.Identity);

                // Don't draw any flares if the light is behind the camera.
                if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
                {
                    return;
                }
                else
                {
                    Vector2 lightProjectedPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
                    Vector2 lightProjectedDirection = lightProjectedPosition - this.Game.Form.RelativeCenter;

                    this.DrawGlow(gameTime, context, lightProjectedPosition, lightProjectedDirection);
                    this.DrawFlares(gameTime, context, lightProjectedPosition, lightProjectedDirection);
                }
            }
        }

        private void DrawGlow(GameTime gameTime, Context context, Vector2 lightPosition, Vector2 lightDirection)
        {
            Color4 color = this.Light.LightColor;
            color.Alpha = 0.25f;
            
            float scale = 50f / this.glowSprite.Width;

            this.glowSprite.Color = color;
            this.glowSprite.Manipulator.SetPosition(lightPosition - (this.glowSprite.RelativeCenter * scale));
            this.glowSprite.Manipulator.SetScale(scale);

            //Draw sprite with alpha
            this.Game.Graphics.SetBlendAdditive();
            this.glowSprite.Draw(gameTime, context);
        }

        private void DrawFlares(GameTime gameTime, Context context, Vector2 lightPosition, Vector2 lightDirection)
        {
            if (this.flares != null && this.flares.Length > 0)
            {
                this.Game.Graphics.SetBlendAdditive();

                for (int i = 0; i < this.flares.Length; i++)
                {
                    Flare flare = this.flares[i];

                    // Set the flare alpha based on the previous occlusion query result.
                    Color4 flareColor = flare.Color;
                    flareColor.Alpha *= 0.5f;

                    // Compute the position of this flare sprite.
                    Vector2 flarePosition = (lightPosition + lightDirection * flare.Position);

                    flare.FlareSprite.Color = flareColor;
                    flare.FlareSprite.Manipulator.SetScale(flare.Scale);
                    flare.FlareSprite.Manipulator.SetPosition(flarePosition - (flare.FlareSprite.RelativeCenter * flare.Scale));

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
