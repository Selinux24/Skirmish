using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Content
{
    public class LightContent
    {
        public LightContentTypeEnum LightType = LightContentTypeEnum.Unknown;
        public Color4 Color;
        public float ConstantAttenuation;
        public float LinearAttenuation;
        public float QuadraticAttenuation;
        public float FallOffAngle;
        public float FallOffExponent;

        public void AddLight(Collada.AmbientDirectional ambientDirectional)
        {
            
        }

        public void AddLight(Collada.Spot spot)
        {
            throw new NotImplementedException();
        }

        public void AddLight(Collada.Point point)
        {
            throw new NotImplementedException();
        }
    }

    public enum LightContentTypeEnum
    {
        Unknown,
        Directional,
        Point,
        Spot,
    }
}
