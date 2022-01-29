using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.UI;

    /// <summary>
    /// Lens flare
    /// </summary>
    public sealed class LensFlare : Drawable<LensFlareDescription>
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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public LensFlare(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~LensFlare()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                glowSprite?.Dispose();
                glowSprite = null;

                if (flares != null)
                {
                    for (int i = 0; i < flares.Length; i++)
                    {
                        flares[i]?.Dispose();
                        flares[i] = null;
                    }

                    flares = null;
                }
            }
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(LensFlareDescription description)
        {
            await base.InitializeAssets(description);

            var gl = await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.Glow",
                $"{Name}.Glow",
                new SpriteDescription()
                {
                    ContentPath = Description.ContentPath,
                    Height = 100,
                    Width = 100,
                    Textures = new string[] { Description.GlowTexture },
                    BlendMode = Description.BlendMode,
                });
            glowSprite = gl;

            if (Description.Flares != null && Description.Flares.Length > 0)
            {
                flares = new Flare[Description.Flares.Length];

                for (int i = 0; i < Description.Flares.Length; i++)
                {
                    var flareDesc = Description.Flares[i];

                    SpriteDescription sprDesc = new SpriteDescription()
                    {
                        ContentPath = Description.ContentPath,
                        Height = 100,
                        Width = 100,
                        Textures = new string[] { flareDesc.Texture },
                        BlendMode = Description.BlendMode,
                    };

                    flares[i] = new Flare()
                    {
                        FlareSprite = await Scene.CreateComponent<Sprite, SpriteDescription>(
                            $"{Id}.Flare_{i}",
                            $"{Name}.Flare_{i}",
                            sprDesc),
                        Distance = flareDesc.Distance,
                        Scale = flareDesc.Scale,
                        Color = flareDesc.Color,
                    };
                }
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            // Don't draw any flares by default
            drawFlares = false;

            var keyLight = context.Lights.KeyLight;
            if (keyLight?.Enabled == true)
            {
                if (!IsFlareVisible(keyLight, context.EyePosition))
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
                var projectedPosition = Game.Graphics.Viewport.Project(
                    -keyLight.Direction * (1f + context.NearPlaneDistance), //Move position into near and far plane projection bounds
                    context.Projection,
                    infiniteView,
                    Matrix.Identity);

                if (projectedPosition.Z >= 0 && projectedPosition.Z <= 1)
                {
                    //The light is in front of the camera.
                    drawFlares = true;

                    var lightProjectedPosition = new Vector2(projectedPosition.X, projectedPosition.Y);
                    var lightProjectedDirection = lightProjectedPosition - Game.Form.RenderCenter;

                    //Update glow sprite
                    float glowScale = scale;
                    var glowSpritePos = lightProjectedPosition;

                    glowSprite.BaseColor = new Color4(keyLight.DiffuseColor, 0.25f);
                    glowSprite.Scale = glowScale;
                    glowSprite.SetPosition(glowSpritePos - glowSprite.LocalCenter);
                    glowSprite.Update(context);

                    //Update flares
                    if (flares?.Length > 0)
                    {
                        for (int i = 0; i < flares.Length; i++)
                        {
                            var flare = flares[i];

                            // Compute the position of this flare sprite.
                            float flareScale = flare.Scale * scale;
                            var flareSpritePos = lightProjectedPosition + (lightProjectedDirection * flare.Distance);

                            // Set the flare alpha based on the angle with view and light directions.
                            flare.FlareSprite.BaseColor = new Color4(flare.Color.RGB(), 0.5f * transparency);
                            flare.FlareSprite.Scale = flareScale;
                            flare.FlareSprite.SetPosition(flareSpritePos - flare.FlareSprite.LocalCenter);
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
        private bool IsFlareVisible(ISceneLightDirectional light, Vector3 eyePosition)
        {
            if (Scene == null)
            {
                return false;
            }

            var frustum = Scene.Camera.Frustum;
            float maxZ = Scene.Camera.FarPlaneDistance;

            Vector3 lPositionUnit = eyePosition - light.Direction;

            //Is the light into the vision cone?
            if (frustum.Contains(lPositionUnit) != ContainmentType.Disjoint)
            {
                //Calculate the ray from light to position
                Vector3 lightPosition = light.GetPosition(maxZ);
                Ray ray = new Ray(lightPosition, -light.Direction);

                if (!Scene.PickNearest<Triangle>(ray, RayPickingParams.Coarse, out _))
                {
                    return true;
                }

                if (Scene.PickNearest(ray, RayPickingParams.Perfect, out PickingResult<Triangle> result) &&
                    Vector3.Distance(lightPosition, eyePosition) > result.Distance)
                {
                    return false;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (!drawFlares)
            {
                return;
            }

            bool draw = context.ValidateDraw(BlendMode, true);
            if (!draw)
            {
                return;
            }

            // Draw glow
            glowSprite?.Draw(context);

            //Draw flares if any
            if (flares?.Length > 0)
            {
                for (int i = 0; i < flares.Length; i++)
                {
                    flares[i].FlareSprite.Draw(context);
                }
            }
        }
    }
}
