using SharpDX;

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
        /// Directional light who flares
        /// </summary>
        public SceneLightDirectional Light { get; set; }

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
            if (this.Light != null)
            {
                this.glowSprite.Update(context);

                if (this.flares != null && this.flares.Length > 0)
                {
                    for (int i = 0; i < this.flares.Length; i++)
                    {
                        this.flares[i].FlareSprite.Update(context);
                    }
                }

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
                    this.drawFlares = false;
                }
                else
                {
                    this.lightProjectedPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
                    this.lightProjectedDirection = lightProjectedPosition - this.Game.Form.RelativeCenter;

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
            Color4 color = this.Light.LightColor;
            color.Alpha = 0.25f;

            float scale = 50f / this.glowSprite.Width;

            this.glowSprite.Color = color;
            this.glowSprite.Manipulator.SetPosition(this.lightProjectedPosition - (this.glowSprite.RelativeCenter * scale));
            this.glowSprite.Manipulator.SetScale(scale);

            //Draw sprite with alpha
            this.Game.Graphics.SetBlendAdditive();
            this.glowSprite.Draw(context);
        }
        /// <summary>
        /// Draws the flare list sprites
        /// </summary>
        /// <param name="context">Drawing context</param>
        private void DrawFlares(DrawContext context)
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
                    Vector2 flarePosition = (this.lightProjectedPosition + this.lightProjectedDirection * flare.Position);

                    flare.FlareSprite.Color = flareColor;
                    flare.FlareSprite.Manipulator.SetScale(flare.Scale);
                    flare.FlareSprite.Manipulator.SetPosition(flarePosition - (flare.FlareSprite.RelativeCenter * flare.Scale));

                    // Draw the flare sprite using additive blending.
                    flare.FlareSprite.Draw(context);
                }
            }
        }
    }
}
