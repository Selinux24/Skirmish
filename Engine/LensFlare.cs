using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Lens flare
    /// </summary>
    public class LensFlare : Drawable
    {
        /// <summary>
        /// Glow sprote
        /// </summary>
        private Sprite glowSprite;
        /// <summary>
        /// Flares
        /// </summary>
        private Flare[] flares = null;
        /// <summary>
        /// Draw flares flag
        /// </summary>
        private bool drawFlares = false;
        /// <summary>
        /// Light projected position
        /// </summary>
        private Vector2 lightProjectedPosition;
        /// <summary>
        /// Light projected direction
        /// </summary>
        private Vector2 lightProjectedDirection;
        /// <summary>
        /// Flare scale
        /// </summary>
        private float scale = 1f;
        /// <summary>
        /// Flare transparency
        /// </summary>
        private float transparency = 1f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="description">Description</param>
        public LensFlare(Game game, LensFlareDescription description)
            : base(game, description)
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
        /// <summary>
        /// Dispose of resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.glowSprite);
            Helper.Dispose(this.flares);
        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            var keyLight = context.Lights.KeyLight;
            if (keyLight != null)
            {
                float dot = Math.Max(0, Vector3.Dot(context.EyeDirection, -keyLight.Direction));

                this.transparency = dot;
                this.scale = dot * keyLight.Brightness;

                // Set view translation to Zero to simulate infinite
                Matrix infiniteView = context.View;
                infiniteView.TranslationVector = Vector3.Zero;

                // Project the light position into 2D screen space.
                Vector3 projectedPosition = this.Game.Graphics.Viewport.Project(
                    -keyLight.Direction * (1f + context.NearPlaneDistance), //Move position into near and far plane projection bounds
                    context.Projection,
                    infiniteView,
                    Matrix.Identity);

                // Don't draw any flares if the light is behind the camera.
                if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
                {
                    this.drawFlares = false;
                }
                else
                {
                    this.lightProjectedPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
                    this.lightProjectedDirection = lightProjectedPosition - this.Game.Form.RelativeCenter;

                    this.glowSprite.Update(context);

                    if (this.flares != null && this.flares.Length > 0)
                    {
                        for (int i = 0; i < this.flares.Length; i++)
                        {
                            this.flares[i].FlareSprite.Update(context);
                        }
                    }

                    this.drawFlares = true;
                }
            }
        }
        /// <summary>
        /// Draws flare
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            if (this.drawFlares)
            {
                this.DrawGlow(context);
                this.DrawFlares(context);
            }
        }
        /// <summary>
        /// Draws the glowing sprite
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawGlow(DrawContext context)
        {
            var keyLight = context.Lights.KeyLight;
            if (keyLight != null)
            {
                Color4 color = keyLight.DiffuseColor;
                color.Alpha = 0.25f;

                if (this.scale > 0)
                {
                    float gScale = 50f / this.glowSprite.Width;

                    this.glowSprite.Color = color;
                    this.glowSprite.Manipulator.SetPosition(this.lightProjectedPosition - (this.glowSprite.RelativeCenter * gScale * this.scale));
                    this.glowSprite.Manipulator.SetScale(gScale * this.scale);

                    // Draw the sprite using additive blending.
                    this.Game.Graphics.SetBlendAdditive();
                    this.glowSprite.Draw(context);
                }
            }
        }
        /// <summary>
        /// Draws the flare list sprites
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawFlares(DrawContext context)
        {
            if (this.scale > 0)
            {
                if (this.flares != null && this.flares.Length > 0)
                {
                    for (int i = 0; i < this.flares.Length; i++)
                    {
                        Flare flare = this.flares[i];

                        // Set the flare alpha based on the previous occlusion query result.
                        Color4 flareColor = flare.Color;
                        flareColor.Alpha *= 0.5f * this.transparency;

                        // Compute the position of this flare sprite.
                        Vector2 flarePosition = (this.lightProjectedPosition + this.lightProjectedDirection * flare.Position);

                        flare.FlareSprite.Color = flareColor;
                        flare.FlareSprite.Manipulator.SetPosition(flarePosition - (flare.FlareSprite.RelativeCenter * flare.Scale * this.scale));
                        flare.FlareSprite.Manipulator.SetScale(flare.Scale * this.scale);

                        // Draw the flare sprite using additive blending.
                        this.Game.Graphics.SetBlendAdditive();
                        flare.FlareSprite.Draw(context);
                    }
                }
            }
        }
    }
}
