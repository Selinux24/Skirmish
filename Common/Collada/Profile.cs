using System;
using System.Xml.Serialization;
using SharpDX;

namespace Common.Collada
{
    using Common.Collada.Types;
    using Common.Utils;

    [Serializable]
    public class Profile
    {
        [XmlElement("asset")]
        public AssetType Asset { get; set; }
        [XmlElement("image", typeof(ImageType))]
        public ImageType[] Images { get; set; }
        [XmlElement("newparam", typeof(NewParamType))]
        public NewParamType[] Params { get; set; }
        [XmlElement("technique")]
        public Technique Technique { get; set; }
        [XmlAttribute("id")]
        public string Id { get; set; }

        public Material CreateMaterial()
        {
            Material geoMat = new Material();

            if (this.Technique.Description.Ambient != null) geoMat.Ambient = this.Technique.Description.Ambient.Color;
            if (this.Technique.Description.Diffuse != null)
            {
                if (this.Technique.Description.Diffuse.Texture != null)
                {
                    geoMat.Diffuse = Color4.White;
                    geoMat.Texture = new TextureDescription()
                    {
                        Name = this.Technique.Description.Diffuse.Texture.Texture,
                        TextureArray = new string[] { this.Technique.Description.Diffuse.Texture.Texture },
                    };
                }
                else
                {
                    geoMat.Diffuse = this.Technique.Description.Diffuse.Color;
                }
            }
            if (this.Technique.Description.Emission != null) geoMat.Emission = this.Technique.Description.Emission.Color;
            if (this.Technique.Description.Specular != null) geoMat.Specular = this.Technique.Description.Specular.Color;
            if (this.Technique.Description.Shininess != null) geoMat.Specular.Alpha = this.Technique.Description.Shininess.Float;
            if (this.Technique.Description.Reflective != null) geoMat.Reflective = this.Technique.Description.Reflective.Color;
            if (this.Technique.Description.Reflectivity != null) geoMat.Reflective.Alpha = this.Technique.Description.Reflectivity.Float;
            if (this.Technique.Description.Transparent != null) geoMat.Transparent = this.Technique.Description.Transparent.Color;
            if (this.Technique.Description.Transparency != null) geoMat.Transparent.Alpha = this.Technique.Description.Transparency.Float;
            if (this.Technique.Description.IndexOfRefraction != null) geoMat.IndexOfRefraction = this.Technique.Description.IndexOfRefraction.Float;

            return geoMat;
        }

        public string GetImage()
        {
            if (this.Technique.Description.Diffuse.Texture != null)
            {
                string sampleName = this.Technique.Description.Diffuse.Texture.Texture;

                NewParamType pSampler = Array.Find(this.Params, p => p.Id == sampleName);
                if (pSampler != null)
                {
                    NewParamType pSurface = Array.Find(this.Params, p => p.Id == pSampler.Sampler2D.Source);
                    if (pSurface != null)
                    {
                        return pSurface.Surface.InitFrom.Value;
                    }
                }
            }

            return null;
        }
    }
}
