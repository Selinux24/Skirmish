using Engine;
using SharpDX;
using System;

namespace TerrainSamples.SceneModularDungeon
{
    /// <summary>
    /// Light basic controller
    /// </summary>
    internal class LightController
    {
        /// <summary>
        /// Random generator
        /// </summary>
        private readonly Random rnd = Helper.NewGenerator();
        /// <summary>
        /// Initial controller time
        /// </summary>
        private readonly float initialTime = Helper.RandomGenerator.NextFloat(0, 100);

        /// <summary>
        /// Spot light
        /// </summary>
        public ISceneLightPoint Light { get; set; }
        /// <summary>
        /// Light position function
        /// </summary>
        public Func<Vector3> PositionFnc { get; set; }

        /// <summary>
        /// Updates the light position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(IGameTime gameTime)
        {
            float r = 0.01f;
            float h = 0.005f;
            float v = 5f;

            float totalSeconds = gameTime.TotalSeconds + initialTime;

            float cos = MathF.Cos(v * totalSeconds);
            float sin = MathF.Sin(v * totalSeconds);

            float x = r * cos * rnd.NextFloat(0.15f, 9f);
            float z = r * sin * rnd.NextFloat(0.85f, 9f);

            float y = h * sin * rnd.NextFloat(0.35f, 9f);

            Light.Position = PositionFnc() + new Vector3(x, y, z);
        }
    }
}
