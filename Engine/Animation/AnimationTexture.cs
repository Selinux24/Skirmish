using SharpDX;
using System.Collections.Generic;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Animation
{
    using Engine.Helpers;

    public class AnimationTexture
    {
        public static ShaderResourceView Create(Game game, SkinningData skData)
        {
            List<Matrix> matList = new List<Matrix>();

            const float timestep = 1.0f / 30.0f;

            for (float t = 0; t < skData.Duration; t += timestep)
            {
                skData.Test(t);

                matList.AddRange(skData.GetFinalTransforms());
            }

            Vector4[] values = new Vector4[] { };

            var texture = game.Graphics.Device.CreateTexture(1024, values);

            return null;
        }
    }
}
