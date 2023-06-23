﻿using SharpDX;
using System.Collections.Generic;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Material content
    /// </summary>
    public struct MaterialBlinnPhongContent : IMaterialContent
    {
        /// <summary>
        /// Default material content
        /// </summary>
        public static MaterialBlinnPhongContent Default
        {
            get
            {
                return new MaterialBlinnPhongContent()
                {
                    DiffuseColor = MaterialConstants.DiffuseColor,
                    EmissiveColor = MaterialConstants.EmissiveColor,
                    AmbientColor = MaterialConstants.AmbientColor,
                    SpecularColor = MaterialConstants.SpecularColor,
                    Shininess = MaterialConstants.Shininess,
                    IsTransparent = false,
                };
            }
        }

        /// <summary>
        /// Diffuse texture name
        /// </summary>
        public string DiffuseTexture { get; set; }
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor { get; set; }
        /// <summary>
        /// Emissive texture name
        /// </summary>
        public string EmissiveTexture { get; set; }
        /// <summary>
        /// Emissive color
        /// </summary>
        public Color3 EmissiveColor { get; set; }
        /// <summary>
        /// Ambient texture name
        /// </summary>
        public string AmbientTexture { get; set; }
        /// <summary>
        /// Ambient color
        /// </summary>
        public Color3 AmbientColor { get; set; }
        /// <summary>
        /// Specular texture name
        /// </summary>
        public string SpecularTexture { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        public Color3 SpecularColor { get; set; }
        /// <summary>
        /// Shininess factor
        /// </summary>
        public float Shininess { get; set; }
        /// <summary>
        /// Normal map texture
        /// </summary>
        public string NormalMapTexture { get; set; }
        /// <summary>
        /// Use transparency
        /// </summary>
        public bool IsTransparent { get; set; }

        /// <inheritdoc/>
        public IMeshMaterial CreateMeshMaterial(IDictionary<string, EngineShaderResourceView> textures)
        {
            return new MeshMaterial
            {
                Material = new MaterialBlinnPhong
                {
                    DiffuseColor = DiffuseColor,
                    EmissiveColor = EmissiveColor,
                    AmbientColor = AmbientColor,
                    SpecularColor = SpecularColor,
                    Shininess = Shininess,
                    IsTransparent = IsTransparent,
                },
                EmissionTexture = string.IsNullOrWhiteSpace(EmissiveTexture) ? null : textures[EmissiveTexture],
                AmbientTexture = string.IsNullOrWhiteSpace(AmbientTexture) ? null : textures[AmbientTexture],
                DiffuseTexture = string.IsNullOrWhiteSpace(DiffuseTexture) ? null : textures[DiffuseTexture],
                NormalMap = string.IsNullOrWhiteSpace(NormalMapTexture) ? null : textures[NormalMapTexture],
            };
        }
        /// <inheritdoc/>
        public override readonly string ToString()
        {
            var emissive = string.IsNullOrWhiteSpace(EmissiveTexture) ? "[]" : $"[{EmissiveColor}]";
            var ambient = string.IsNullOrWhiteSpace(AmbientTexture) ? "[]" : $"[{AmbientColor}]";
            var diffuse = string.IsNullOrWhiteSpace(DiffuseTexture) ? "[]" : $"[{DiffuseColor}]";
            var specular = string.IsNullOrWhiteSpace(NormalMapTexture) ? "[]" : $"[{SpecularColor}]";

            return $"Blinn-Phong. Emissive: {emissive}; Ambient: {ambient}; Diffuse: {diffuse}; Specular: {specular}; Shininess: {Shininess};";
        }
    }
}
