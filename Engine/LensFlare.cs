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
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class LensFlare(Scene scene, string id, string name) : Drawable<LensFlareDescription>(scene, id, name)
    {
        /// <summary>
        /// Glow sprite
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
        /// Gets light position at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <param name="direction">Light direction</param>
        /// <returns>Returns light position at specified distance</returns>
        private static Vector3 GetPosition(float distance, Vector3 direction)
        {
            return distance * -2f * direction;
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
        public override async Task ReadAssets(LensFlareDescription description)
        {
            await base.ReadAssets(description);

            var gl = await Scene.CreateComponent<Sprite, SpriteDescription>(
                $"{Id}.Glow",
                $"{Name}.Glow",
                new SpriteDescription()
                {
                    ContentPath = Description.ContentPath,
                    Height = 100,
                    Width = 100,
                    Textures = [Description.GlowTexture],
                    BlendMode = Description.BlendMode,
                });
            glowSprite = gl;

            if (Description.Flares != null && Description.Flares.Length > 0)
            {
                flares = new Flare[Description.Flares.Length];

                for (int i = 0; i < Description.Flares.Length; i++)
                {
                    var flareDesc = Description.Flares[i];

                    var sprDesc = new SpriteDescription()
                    {
                        ContentPath = Description.ContentPath,
                        Height = 100,
                        Width = 100,
                        Textures = [flareDesc.Texture],
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

            var keyLight = Scene.Lights.KeyLight;
            if ((keyLight?.Enabled) != true)
            {
                return;
            }

            var camera = Scene.Camera;

            if (!IsFlareVisible(keyLight, camera.Position))
            {
                return;
            }

            float dot = MathF.Max(0f, Vector3.Dot(camera.Direction, -keyLight.Direction));
            float scale = dot * keyLight.Brightness;
            if (scale <= 0)
            {
                return;
            }

            float transparency = dot;

            // Set view translation to Zero to simulate infinite
            var infiniteView = camera.View;
            infiniteView.TranslationVector = Vector3.Zero;

            // Project the light position into 2D screen space.
            var projectedPosition = Game.Graphics.Viewport.Project(
                -keyLight.Direction * (1f + camera.NearPlaneDistance), //Move position into near and far plane projection bounds
                camera.Projection,
                infiniteView,
                Matrix.Identity);

            if (projectedPosition.Z < 0 || projectedPosition.Z > 1)
            {
                return;
            }

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
            if (!(flares?.Length > 0))
            {
                return;
            }

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
                Vector3 lightPosition = GetPosition(maxZ, light.Direction);
                var ray = new Ray(lightPosition, -light.Direction);

                var coarseRay = new PickingRay(ray, PickingHullTypes.Coarse);

                if (!Scene.PickNearest<Triangle>(coarseRay, SceneObjectUsages.None, out _))
                {
                    return true;
                }

                var perfectRay = new PickingRay(ray, PickingHullTypes.Geometry);

                if (Scene.PickNearest<Triangle>(perfectRay, SceneObjectUsages.None, out var result) &&
                    Vector3.Distance(lightPosition, eyePosition) > result.PickingResult.Distance)
                {
                    return false;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!drawFlares)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode, true);
            if (!draw)
            {
                return false;
            }

            // Draw glow
            bool drawn = glowSprite?.Draw(context) ?? false;

            //Draw flares if any
            if (flares?.Length > 0)
            {
                for (int i = 0; i < flares.Length; i++)
                {
                    drawn = flares[i].FlareSprite.Draw(context) || drawn;
                }
            }

            return drawn;
        }
    }
}
