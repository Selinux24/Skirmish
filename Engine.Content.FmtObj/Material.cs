using SharpDX;

namespace Engine.Content.FmtObj
{
    struct Material
    {
        public string Name { get; set; }
        public string ShaderProfile { get; set; }

        public float Ns { get; set; }
        public string MapNs { get; set; }

        public Color3 Ka { get; set; }
        public string MapKa { get; set; }
        
        public Color3 Kd { get; set; }
        public string MapKd { get; set; }
        
        public Color3 Ks { get; set; }
        public string MapKs { get; set; }
        
        public Color3 Ke { get; set; }
        public float Ni { get; set; }
        
        public float D { get; set; }
        public string MapD { get; set; }
        
        public string MapBump { get; set; }
    }
}
