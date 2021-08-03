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
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public LensFlare(string id, string name, Scene scene, LensFlareDescription description)
            : base(id, name, scene, description)
        {
            glowSprite = new Sprite(
                $"{id}.Glow",
                $"{name}.Glow",
                scene,
                new SpriteDescription()
                {
                    ContentPath = description.ContentPath,
                    Height = 100,
                    Width = 100,
                    Textures = new string[] { description.GlowTexture },
                    BlendMode = description.BlendMode,
                });

            if (description.Flares != null && description.Flares.Length > 0)
            {
                flares = new Flare[description.Flares.Length];

                for (int i = 0; i < description.Flares.Length; i++)
                {
                    var flareDesc = description.Flares[i];

                    SpriteDescription sprDesc = new SpriteDescription()
                    {
                        ContentPath = description.ContentPath,
                        Height = 100,
                        Width = 100,
                        Textures = new string[] { flareDesc.Texture },
                        BlendMode = description.BlendMode,
                    };

                    flares[i] = new Flare()
                    {
                        FlareSprite = new Sprite(
                            $"{id}.Flare_{i}",
                            $"{name}.Flare_{i}",
                            scene,
                            sprDesc),
                        Distance = flareDesc.Distance,
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
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (glowSprite != null)
                {
                    glowSprite.Dispose();
                    glowSprite = null;
                }

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

    /// <summary>
    /// Lens flare extensions
    /// </summary>
    public static class LensFlareExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<LensFlare> AddComponentLensFlare(this Scene scene, string id, string name, LensFlareDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerEffects)
        {
            LensFlare component = null;

            await Task.Run(() =>
            {
                component = new LensFlare(id, name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
