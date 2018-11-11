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
        private Flare[] flares;
        /// <summary>
        /// Draw flares flag
        /// </summary>
        private bool drawFlares = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public LensFlare(Scene scene, LensFlareDescription description)
            : base(scene, description)
        {
            this.glowSprite = new Sprite(scene, new SpriteDescription()
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
                        FlareSprite = new Sprite(scene, sprDesc),
                        Position = flareDesc.Position,
                        Scale = flareDesc.Scale,
                        Color = flareDesc.Color,
                    };
                }
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~LensFlare()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose of resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (glowSprite != null)
                {
                    glowSprite.Dispose();
                    glowSprite = null;
                }

                if (this.flares != null)
                {
                    for (int i = 0; i < this.flares.Length; i++)
                    {
                        this.flares[i]?.Dispose();
                        this.flares[i] = null;
                    }

                    this.flares = null;
                }
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="context">Updating context</param>
        public override void Update(UpdateContext context)
        {
            // Don't draw any flares by default
            this.drawFlares = false;

            var keyLight = context.Lights.KeyLight;
            if (keyLight?.Enabled == true)
            {
                if (!this.IsFlareVisible(keyLight, context.EyePosition))
                {
                    return;
                }

                float dot = Math.Max(0, Vector3.Dot(context.EyeDirection, -keyLight.Direction));
                float scale = dot * keyLight.Brightness;
                if (scale <= 0)
                {
                    return;
                }

                float transparency = dot;

                // Set view translation to Zero to simulate infinite
                var infiniteView = context.View;
                infiniteView.TranslationVector = Vector3.Zero;

                // Project the light position into 2D screen space.
                var projectedPosition = this.Game.Graphics.Viewport.Project(
                    -keyLight.Direction * (1f + context.NearPlaneDistance), //Move position into near and far plane projection bounds
                    context.Projection,
                    infiniteView,
                    Matrix.Identity);

                if (projectedPosition.Z >= 0 && projectedPosition.Z <= 1)
                {
                    //The light is in front of the camera.
                    this.drawFlares = true;

                    var lightProjectedPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
                    var lightProjectedDirection = lightProjectedPosition - this.Game.Form.RelativeCenter;

                    //Update glow sprite
                    float glowScale = 50f / this.glowSprite.Width;
                    this.glowSprite.Color = new Color4(keyLight.DiffuseColor.RGB(), 0.25f);
                    this.glowSprite.Manipulator.SetPosition(lightProjectedPosition - (this.glowSprite.RelativeCenter * glowScale * scale));
                    this.glowSprite.Manipulator.SetScale(glowScale * scale);
                    this.glowSprite.Update(context);

                    //Update flares
                    if (this.flares?.Length > 0)
                    {
                        for (int i = 0; i < this.flares.Length; i++)
                        {
                            var flare = this.flares[i];

                            // Compute the position of this flare sprite.
                            var flarePosition = (lightProjectedPosition + lightProjectedDirection * flare.Position);

                            // Set the flare alpha based on the angle with view and light directions.
                            flare.FlareSprite.Color = new Color4(flare.Color.RGB(), 0.5f * transparency);
                            flare.FlareSprite.Manipulator.SetPosition(flarePosition - (flare.FlareSprite.RelativeCenter * flare.Scale * scale));
                            flare.FlareSprite.Manipulator.SetScale(flare.Scale * scale);
                            flare.FlareSprite.Update(context);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Gets if the flare is visible
        /// </summary>
        /// <param name="light">Key light</param>
        /// <param name="eyePosition">Eye position</param>
        /// <returns>Returns true if the flare is visible</returns>
        private bool IsFlareVisible(SceneLightDirectional light, Vector3 eyePosition)
        {
            if (this.Scene != null)
            {
                Vector3 lightPosition = light.GetPosition(1000);
                Vector3 direction = Vector3.Normalize(eyePosition - lightPosition);

                Ray ray = new Ray(lightPosition, direction);

                if (this.Scene.PickNearest(ray, false, out PickingResult<Triangle> result) &&
                    Vector3.Distance(lightPosition, eyePosition) > result.Distance)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Draws flare
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            if (this.drawFlares)
            {
                // Draw the sprite using additive blending.
                this.Game.Graphics.SetBlendAdditive();

                // Draw glow
                this.glowSprite.Draw(context);

                //Draw flares if any
                if (this.flares != null && this.flares.Length > 0)
                {
                    for (int i = 0; i < this.flares.Length; i++)
                    {
                        this.flares[i].FlareSprite.Draw(context);
                    }
                }
            }
        }
    }
}
