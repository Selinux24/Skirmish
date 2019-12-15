using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Content.FmtCollada
{
    using Engine.Animation;
    using Engine.Collada;
    using Engine.Collada.FX;
    using Engine.Collada.Types;
    using Engine.Common;

    /// <summary>
    /// Loader for collada
    /// </summary>
    public class LoaderCollada : ILoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LoaderCollada()
        {

        }

        /// <summary>
        /// Load a collada model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Conten description</param>
        /// <returns>Returns the loaded contents</returns>
        public IEnumerable<ModelContent> Load(string contentFolder, ModelContentDescription content)
        {
            Matrix transform = Matrix.Identity;

            if (content.Scale != 1f)
            {
                transform = Matrix.Scaling(content.Scale);
            }

            string fileName = content.ModelFileName;
            string armatureName = content.ArmatureName;
            string[] volumes = content.VolumeMeshes;
            string[] meshesByLOD = content.LODMeshes;
            var animation = content.Animation;
            bool useControllerTransform = content.UseControllerTransform;

            var modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList?.Any() == true)
            {
                List<ModelContent> res = new List<ModelContent>();

                foreach (var model in modelList)
                {
                    var dae = Collada.Load(model);

                    ModelContent modelContent = new ModelContent();

                    //Scene Objects
                    ProcessLibraryLights(dae, modelContent);
                    ProcessLibraryImages(dae, modelContent, contentFolder);
                    ProcessLibraryMaterial(dae, modelContent);
                    ProcessLibraryGeometries(dae, modelContent, volumes);
                    ProcessLibraryControllers(dae, modelContent);

                    //Scene Relations
                    ProcessVisualScene(dae, transform, useControllerTransform, modelContent);

                    //Animations
                    ProcessLibraryAnimations(dae, modelContent, animation);

                    //Filter the resulting model content
                    res.AddRange(FilterGeometry(modelContent, armatureName, meshesByLOD));

                    //Release the stream
                    model.Flush();
                    model.Dispose();
                }

                return res.ToArray();
            }
            else
            {
                throw new EngineException(string.Format("Model not found: {0}", fileName));
            }
        }
        /// <summary>
        /// Filters the loaded geometry by armature name and level of detail meshes (if any)
        /// </summary>
        /// <param name="modelContent">Model content to filter</param>
        /// <param name="armatureName">Armature name</param>
        /// <param name="meshesByLOD">Level of detail meshes</param>
        /// <returns>Returns the filtered model content parts</returns>
        private static IEnumerable<ModelContent> FilterGeometry(ModelContent modelContent, string armatureName, IEnumerable<string> meshesByLOD)
        {
            bool filterByArmature = !string.IsNullOrWhiteSpace(armatureName);
            bool filterByMeshes = meshesByLOD?.Any() == true;

            if (!filterByArmature && !filterByMeshes)
            {
                //Nothing to filter
                return new[] { modelContent };
            }

            List<ModelContent> res = new List<ModelContent>();

            //Filter by armature name
            if (filterByArmature &&
                modelContent.FilterByArmature(armatureName, out var armatureModel))
            {
                res.Add(armatureModel);
            }

            //Filter by level of detail meshes
            if (filterByMeshes)
            {
                var contents = meshesByLOD
                    .Select(lodMeshName => modelContent.Filter(lodMeshName.Replace('.', '_')))
                    .Where(mContent => mContent != null);

                res.AddRange(contents);
            }

            return res;
        }
        /// <summary>
        /// Gets whether the specified geometry name is a marked volume
        /// </summary>
        /// <param name="geometryName">Geometry name</param>
        /// <param name="volumes">List of volumen name prefixes</param>
        /// <returns>Returns true if the geometry name starts with any of the volume names in the collection</returns>
        private static bool IsVolume(string geometryName, IEnumerable<string> volumes)
        {
            return volumes?.Any(v => geometryName.StartsWith(v, StringComparison.OrdinalIgnoreCase)) == true;
        }

        #region Dictionary loaders

        /// <summary>
        /// Process lightd
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessLibraryLights(Collada dae, ModelContent modelContent)
        {
            if (dae.LibraryLights?.Length > 0)
            {
                foreach (var light in dae.LibraryLights)
                {
                    LightContent info = null;

                    var dirTechnique = Array.Find(light.LightTechniqueCommon, l => l.Directional != null);
                    if (dirTechnique != null)
                    {
                        info = new LightContent()
                        {
                            LightType = LightContentTypes.Directional,
                            Color = dirTechnique.Directional.Color.ToColor4(),
                        };
                    }

                    var pointTechnique = Array.Find(light.LightTechniqueCommon, l => l.Point != null);
                    if (pointTechnique != null)
                    {
                        info = new LightContent()
                        {
                            LightType = LightContentTypes.Point,
                            Color = pointTechnique.Point.Color.ToColor4(),
                            ConstantAttenuation = pointTechnique.Point.ConstantAttenuation.Value,
                            LinearAttenuation = pointTechnique.Point.LinearAttenuation.Value,
                            QuadraticAttenuation = pointTechnique.Point.QuadraticAttenuation.Value,
                        };
                    }

                    var spotTechnique = Array.Find(light.LightTechniqueCommon, l => l.Spot != null);
                    if (spotTechnique != null)
                    {
                        info = new LightContent()
                        {
                            LightType = LightContentTypes.Spot,
                            Color = spotTechnique.Spot.Color.ToColor4(),
                            ConstantAttenuation = spotTechnique.Spot.ConstantAttenuation.Value,
                            LinearAttenuation = spotTechnique.Spot.LinearAttenuation.Value,
                            QuadraticAttenuation = spotTechnique.Spot.QuadraticAttenuation.Value,
                            FallOffAngle = spotTechnique.Spot.FalloffAngle.Value,
                            FallOffExponent = spotTechnique.Spot.FalloffExponent.Value,
                        };
                    }

                    modelContent.Lights[light.Id] = info;
                }
            }
        }
        /// <summary>
        /// Process images
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="contentFolder">Content folder</param>
        private static void ProcessLibraryImages(Collada dae, ModelContent modelContent, string contentFolder)
        {
            if (dae.LibraryImages?.Length > 0)
            {
                foreach (var image in dae.LibraryImages)
                {
                    ImageContent info = null;

                    if (image.Data != null)
                    {
                        info = ImageContent.Texture(new MemoryStream((byte[])image.Data));
                    }
                    else if (!string.IsNullOrEmpty(image.InitFrom))
                    {
                        info = ImageContent.Texture(contentFolder, Uri.UnescapeDataString(image.InitFrom));
                    }

                    modelContent.Images[image.Id] = info;
                }
            }
        }
        /// <summary>
        /// Process materials
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessLibraryMaterial(Collada dae, ModelContent modelContent)
        {
            if (dae.LibraryMaterials?.Length > 0 && dae.LibraryEffects?.Length > 0)
            {
                foreach (var material in dae.LibraryMaterials)
                {
                    var info = MaterialContent.Default;

                    //Find effect
                    var effect = Array.Find(dae.LibraryEffects, e => e.Id == material.InstanceEffect.Url.Replace("#", ""));
                    if (effect != null)
                    {
                        if (effect.ProfileCG != null)
                        {
                            throw new NotImplementedException();
                        }
                        else if (effect.ProfileGles != null)
                        {
                            throw new NotImplementedException();
                        }
                        else if (effect.ProfileGlsl != null)
                        {
                            throw new NotImplementedException();
                        }
                        else if (effect.ProfileCommon != null)
                        {
                            info = ProcessTechniqueFX(effect.ProfileCommon);
                        }
                    }

                    modelContent.Materials[material.Id] = info;
                }
            }
        }
        /// <summary>
        /// Process geometry
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="volumes">Volume mesh names</param>
        private static void ProcessLibraryGeometries(Collada dae, ModelContent modelContent, IEnumerable<string> volumes)
        {
            if (dae.LibraryGeometries?.Length > 0)
            {
                foreach (var geometry in dae.LibraryGeometries)
                {
                    bool isVolume = IsVolume(geometry.Name, volumes);

                    var info = ProcessGeometry(geometry, isVolume);
                    if (info?.Length > 0)
                    {
                        foreach (var subMesh in info)
                        {
                            string materialName = FindMaterialTarget(subMesh.Material, dae.LibraryVisualScenes);
                            if (!string.IsNullOrWhiteSpace(materialName))
                            {
                                var mat = modelContent.Materials[materialName];

                                subMesh.Material = materialName;
                                subMesh.SetTextured(mat.DiffuseTexture != null);
                            }

                            modelContent.Geometry.Add(geometry.Id, subMesh.Material, subMesh);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Process controllers
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessLibraryControllers(Collada dae, ModelContent modelContent)
        {
            if (dae.LibraryControllers?.Length > 0)
            {
                foreach (var controller in dae.LibraryControllers)
                {
                    var info = ProcessController(controller);
                    if (info != null)
                    {
                        modelContent.Controllers[controller.Id] = info;
                    }
                }
            }
        }
        /// <summary>
        /// Process animations
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="animation">Animation description</param>
        private static void ProcessLibraryAnimations(Collada dae, ModelContent modelContent, AnimationDescription animation)
        {
            if (dae.LibraryAnimations?.Length > 0)
            {
                for (int i = 0; i < dae.LibraryAnimations.Length; i++)
                {
                    var animationLib = dae.LibraryAnimations[i];

                    var info = ProcessAnimation(modelContent, animationLib);
                    if (info?.Length > 0)
                    {
                        modelContent.Animations[animationLib.Id] = info;
                    }
                }

                modelContent.Animations.Definition = animation;
            }
        }

        #endregion

        #region Geometry

        /// <summary>
        /// Process geometry list
        /// </summary>
        /// <param name="geometry">Geometry info</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessGeometry(Geometry geometry, bool isVolume)
        {
            SubMeshContent[] info = null;

            if (geometry.Mesh != null)
            {
                info = ProcessMesh(geometry.Mesh, isVolume);
            }
            else if (geometry.Spline != null)
            {
                info = ProcessSpline(geometry.Spline, isVolume);
            }
            else if (geometry.ConvexMesh != null)
            {
                info = ProcessConvexMesh(geometry.ConvexMesh, isVolume);
            }

            return info;
        }
        /// <summary>
        /// Process mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessMesh(Engine.Collada.Mesh mesh, bool isVolume)
        {
            SubMeshContent[] res = null;

            //Procesar por topología
            if (mesh.Lines?.Length > 0)
            {
                res = ProcessLines(mesh.Lines, mesh.Sources, isVolume);
            }
            else if (mesh.LineStrips?.Length > 0)
            {
                res = ProcessLineStrips(mesh.LineStrips, mesh.Sources, isVolume);
            }
            else if (mesh.Triangles?.Length > 0)
            {
                res = ProcessTriangles(mesh.Triangles, mesh.Sources, isVolume);
            }
            else if (mesh.TriFans?.Length > 0)
            {
                res = ProcessTriFans(mesh.TriFans, mesh.Sources, isVolume);
            }
            else if (mesh.TriStrips?.Length > 0)
            {
                res = ProcessTriStrips(mesh.TriStrips, mesh.Sources, isVolume);
            }
            else if (mesh.PolyList?.Length > 0)
            {
                res = ProcessPolyLists(mesh.PolyList, mesh.Sources, isVolume);
            }
            else if (mesh.Polygons?.Length > 0)
            {
                res = ProcessPolygons(mesh.Polygons, mesh.Sources, isVolume);
            }

            return res;
        }
        /// <summary>
        /// Process spline
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessSpline(Spline spline, bool isVolume)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process convex mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessConvexMesh(ConvexMesh convexMesh, bool isVolume)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process lines
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessLines(Lines[] lines, Source[] meshSources, bool isVolume)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process line strips
        /// </summary>
        /// <param name="lines">Line strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessLineStrips(LineStrips[] lines, Source[] meshSources, bool isVolume)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangles
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriangles(Triangles[] triangles, Source[] meshSources, bool isVolume)
        {
            List<SubMeshContent> res = new List<SubMeshContent>();

            foreach (var triangle in triangles)
            {
                var verts = ProcessTriangle(triangle, meshSources);

                //Reorder vertices
                VertexData[] data = new VertexData[verts.Length];
                for (int i = 0; i < data.Length; i += 3)
                {
                    data[i + 0] = verts[i + 0];
                    data[i + 1] = verts[i + 2];
                    data[i + 2] = verts[i + 1];
                }

                SubMeshContent meshInfo = new SubMeshContent(Topology.TriangleList, triangle.Material, false, isVolume);

                meshInfo.SetVertices(data);

                res.Add(meshInfo);
            }

            return res.ToArray();
        }
        /// <summary>
        /// Process a single triangle
        /// </summary>
        /// <param name="triangle">Triangle</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <returns>Returns vertex data</returns>
        private static VertexData[] ProcessTriangle(Triangles triangle, Source[] meshSources)
        {
            List<VertexData> verts = new List<VertexData>();

            Input vertexInput = triangle[EnumSemantics.Vertex];
            Input normalInput = triangle[EnumSemantics.Normal];
            Input texCoordInput = triangle[EnumSemantics.TexCoord];

            Vector3[] positions = vertexInput != null ? meshSources[vertexInput.Offset].ReadVector3() : null;
            Vector3[] normals = normalInput != null ? meshSources[normalInput.Offset].ReadVector3() : null;
            Vector2[] texCoords = texCoordInput != null ? meshSources[texCoordInput.Offset].ReadVector2() : null;

            int inputCount = triangle.Inputs.Length;

            for (int i = 0; i < triangle.Count; i++)
            {
                for (int t = 0; t < 3; t++)
                {
                    int index = (i * inputCount * 3) + (t * inputCount);

                    VertexData vert = new VertexData()
                    {
                        FaceIndex = i,
                    };

                    vert = vert
                        .UpdateVertexInput(vertexInput, positions, triangle.P, index)
                        .UpdateNormalInput(normalInput, normals, triangle.P, index)
                        .UpdateTexCoordInput(texCoordInput, texCoords, triangle.P, index);

                    verts.Add(vert);
                }
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Process triangle fans
        /// </summary>
        /// <param name="triFans">Triangle fans</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriFans(TriFans[] triFans, Source[] meshSources, bool isVolume)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangle strips
        /// </summary>
        /// <param name="triStrips">Triangle strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriStrips(TriStrips[] triStrips, Source[] meshSources, bool isVolume)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process polygon lists
        /// </summary>
        /// <param name="polyLists">Polygon lists</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessPolyLists(PolyList[] polyLists, Source[] meshSources, bool isVolume)
        {
            List<SubMeshContent> res = new List<SubMeshContent>();

            foreach (var polyList in polyLists)
            {
                var verts = ProcessPolyList(polyList, meshSources);

                //Reorder vertices
                VertexData[] data = new VertexData[verts.Length];
                for (int i = 0; i < data.Length; i += 3)
                {
                    data[i + 0] = verts[i + 0];
                    data[i + 1] = verts[i + 2];
                    data[i + 2] = verts[i + 1];
                }

                SubMeshContent meshInfo = new SubMeshContent(Topology.TriangleList, polyList.Material, false, isVolume);

                meshInfo.SetVertices(data);

                res.Add(meshInfo);
            }

            return res.ToArray();
        }
        /// <summary>
        /// Process polygon list
        /// </summary>
        /// <param name="polyList">Polygon list</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <returns>Return vertext data</returns>
        private static VertexData[] ProcessPolyList(PolyList polyList, Source[] meshSources)
        {
            List<VertexData> verts = new List<VertexData>();

            Input vertexInput = polyList[EnumSemantics.Vertex];
            Input normalInput = polyList[EnumSemantics.Normal];
            Input texCoordInput = polyList[EnumSemantics.TexCoord];
            Input colorsInput = polyList[EnumSemantics.Color];

            Vector3[] positions = vertexInput != null ? meshSources[vertexInput.Offset].ReadVector3() : null;
            Vector3[] normals = normalInput != null ? meshSources[normalInput.Offset].ReadVector3() : null;
            Vector2[] texCoords = texCoordInput != null ? meshSources[texCoordInput.Offset].ReadVector2() : null;
            Color3[] colors = colorsInput != null ? meshSources[colorsInput.Offset].ReadColor3() : null;

            int index = 0;
            int inputCount = polyList.Inputs.Length;

            for (int i = 0; i < polyList.Count; i++)
            {
                int n = polyList.VCount[i];

                for (int v = 0; v < n; v++)
                {
                    VertexData vert = new VertexData()
                    {
                        FaceIndex = i,
                    };

                    vert = vert
                        .UpdateVertexInput(vertexInput, positions, polyList.P, index)
                        .UpdateNormalInput(normalInput, normals, polyList.P, index)
                        .UpdateTexCoordInput(texCoordInput, texCoords, polyList.P, index)
                        .UpdateColorsInput(colorsInput, colors, polyList.P, index);

                    verts.Add(vert);

                    index += inputCount;
                }
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Process polygons
        /// </summary>
        /// <param name="polygons">Polygons</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isVolume">Current geometry is a volume mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessPolygons(Polygons[] polygons, Source[] meshSources, bool isVolume)
        {
            List<SubMeshContent> res = new List<SubMeshContent>();

            foreach (var polygon in polygons)
            {
                var verts = ProcessPolygon(polygon, meshSources);

                //Reorder vertices
                VertexData[] data = new VertexData[verts.Length];
                for (int i = 0; i < data.Length; i += 3)
                {
                    data[i + 0] = verts[i + 0];
                    data[i + 1] = verts[i + 2];
                    data[i + 2] = verts[i + 1];
                }

                SubMeshContent meshInfo = new SubMeshContent(Topology.TriangleList, polygon.Material, false, isVolume);

                meshInfo.SetVertices(data);

                res.Add(meshInfo);
            }

            return res.ToArray();
        }
        /// <summary>
        /// Process polygon
        /// </summary>
        /// <param name="polygon">Polygon</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <returns>Returns vertex data</returns>
        private static VertexData[] ProcessPolygon(Polygons polygon, Source[] meshSources)
        {
            List<VertexData> verts = new List<VertexData>();

            Input vertexInput = polygon[EnumSemantics.Vertex];
            Input normalInput = polygon[EnumSemantics.Normal];
            Input texCoordInput = polygon[EnumSemantics.TexCoord];

            Vector3[] positions = vertexInput != null ? meshSources[vertexInput.Offset].ReadVector3() : null;
            Vector3[] normals = normalInput != null ? meshSources[normalInput.Offset].ReadVector3() : null;
            Vector2[] texCoords = texCoordInput != null ? meshSources[texCoordInput.Offset].ReadVector2() : null;

            int inputCount = polygon.Inputs.Length;

            for (int i = 0; i < polygon.Count; i++)
            {
                var indices = polygon.P[i];
                var n = indices.Values.Length / inputCount;

                for (int v = 0; v < n; v++)
                {
                    int index = (v * inputCount);

                    VertexData vert = new VertexData()
                    {
                        FaceIndex = i,
                    };

                    vert = vert
                        .UpdateVertexInput(vertexInput, positions, indices, index)
                        .UpdateNormalInput(normalInput, normals, indices, index)
                        .UpdateTexCoordInput(texCoordInput, texCoords, indices, index);

                    verts.Add(vert);
                }
            }

            return verts.ToArray();
        }

        #endregion

        #region Materials

        private static string FindMaterialTarget(string material, VisualScene[] libraryVisualScenes)
        {
            foreach (var vs in libraryVisualScenes)
            {
                string res = FindMaterialTarget(material, vs.Nodes);

                if (res != null) return res;
            }

            return material;
        }

        private static string FindMaterialTarget(string material, Node[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node.HasGeometry)
                {
                    //Look up on geometry
                    string res = FindMaterialTarget(material, node.InstanceGeometry);

                    if (res != null) return res;
                }

                if (node.Nodes != null)
                {
                    //Look up on child nodes
                    string res = FindMaterialTarget(material, node.Nodes);

                    if (res != null) return res;
                }
            }

            return null;
        }

        private static string FindMaterialTarget(string material, InstanceGeometry[] instances)
        {
            var instanceMaterial = instances
                .Where(g => g.BindMaterial?.TechniqueCommon?.Any(t => t.InstanceMaterial?.Any() == true) == true)
                .Select(g => g.BindMaterial.TechniqueCommon[0].InstanceMaterial[0])
                .FirstOrDefault(i => string.Equals(material, i.Symbol, StringComparison.OrdinalIgnoreCase));

            return instanceMaterial?.Target.Replace("#", "");
        }

        #endregion

        #region Controllers

        /// <summary>
        /// Process controller
        /// </summary>
        /// <param name="controller">Controller</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessController(Controller controller)
        {
            ControllerContent res = null;

            string armatureName = controller.Name.Replace(".", "_");

            if (controller.Skin != null)
            {
                res = ProcessSkin(armatureName, controller.Skin);
            }
            else if (controller.Morph != null)
            {
                res = ProcessMorph(armatureName, controller.Morph);
            }

            return res;
        }
        /// <summary>
        /// Process skin
        /// </summary>
        /// <param name="name">Skin name</param>
        /// <param name="skin">Skin information</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessSkin(string name, Skin skin)
        {
            ControllerContent res = new ControllerContent
            {
                BindShapeMatrix = Matrix.Transpose(skin.BindShapeMatrix.ToMatrix()),
                Skin = skin.SourceUri.Replace("#", ""),
                Armature = name,
            };

            if (skin.VertexWeights != null)
            {
                ProcessVertexWeights(name, skin, out Dictionary<string, Matrix> ibmList, out Weight[] wgList);

                res.InverseBindMatrix = ibmList;
                res.Weights = wgList;
            }

            return res;
        }
        /// <summary>
        /// Process morph
        /// </summary>
        /// <param name="name">Morph name</param>
        /// <param name="morph">Morph information</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessMorph(string name, Morph morph)
        {
            ControllerContent res = new ControllerContent()
            {
                Armature = name,
            };

            ProcessVertexWeights(morph, out Weight[] wgList);

            res.Weights = wgList;

            return res;
        }
        /// <summary>
        /// Process vertext weight information
        /// </summary>
        /// <param name="name">Armature name</param>
        /// <param name="skin">Skin information</param>
        /// <param name="inverseBindMatrixList">Inverse bind matrix list result</param>
        /// <param name="weightList">Weight list result</param>
        private static void ProcessVertexWeights(string name, Skin skin, out Dictionary<string, Matrix> inverseBindMatrixList, out Weight[] weightList)
        {
            //Joints & matrices
            int jointsOffset = -1;
            string[] joints = null;
            Matrix[] mats = null;
            var jointsInput = skin.VertexWeights[EnumSemantics.Joint];
            if (jointsInput != null)
            {
                jointsOffset = jointsInput.Offset;

                //Joint names
                var jInput = skin.Joints[EnumSemantics.Joint];
                if (jInput != null)
                {
                    joints = skin[jInput.Source].ReadNames();
                }

                //Inverse bind matrix for each joint
                var mInput = skin.Joints[EnumSemantics.InverseBindMatrix];
                if (mInput != null)
                {
                    mats = skin[mInput.Source].ReadMatrix();
                }
            }

            //Weights
            int weightsOffset = -1;
            float[] weights = null;
            var weightsInput = skin.VertexWeights[EnumSemantics.Weight];
            if (weightsInput != null)
            {
                weightsOffset = weightsInput.Offset;

                weights = skin[weightsInput.Source].ReadFloat();
            }

            weightList = BuildVertexWeigths(name, skin, jointsOffset, joints, weightsOffset, weights);
            inverseBindMatrixList = BuildInverseBindMatrixList(name, joints, mats);
        }
        /// <summary>
        /// Process vertext weight information
        /// </summary>
        /// <param name="morph">Morph information</param>
        /// <param name="weightList">Weight list result</param>
        private static void ProcessVertexWeights(Morph morph, out Weight[] weightList)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Creates the vertex weights list
        /// </summary>
        /// <param name="name">Armature name</param>
        /// <param name="skin">Skin data</param>
        /// <param name="jointsOffset">Joints offset</param>
        /// <param name="joints">Joints names list</param>
        /// <param name="weightsOffset">Weights offset</param>
        /// <param name="weights">Weight values list</param>
        /// <returns>Returns the weights list</returns>
        private static Weight[] BuildVertexWeigths(string name, Skin skin, int jointsOffset, string[] joints, int weightsOffset, float[] weights)
        {
            var wgList = new List<Weight>();

            int index = 0;
            int sources = skin.VertexWeights.Inputs.Length;

            for (int i = 0; i < skin.VertexWeights.Count; i++)
            {
                int n = skin.VertexWeights.VCount[i];

                for (int v = 0; v < n; v++)
                {
                    string jointName = null;
                    float weightValue = 0;

                    if (jointsOffset >= 0 && joints != null)
                    {
                        jointName = name + "_" + joints[skin.VertexWeights.V[index + jointsOffset]];
                    }

                    if (weightsOffset >= 0 && weights != null)
                    {
                        weightValue = weights[skin.VertexWeights.V[index + weightsOffset]];
                    }

                    if (weightValue != 0.0f)
                    {
                        //Adds weight only if has value
                        var wg = new Weight()
                        {
                            VertexIndex = i,
                            Joint = jointName,
                            WeightValue = weightValue,
                        };

                        wgList.Add(wg);
                    }

                    index += sources;
                }
            }

            return wgList.ToArray();
        }
        /// <summary>
        /// Creates the inverse bind matrix dictionary
        /// </summary>
        /// <param name="name">Armature name</param>
        /// <param name="joints">Joint names list</param>
        /// <param name="mats">Inverse bind matrix list</param>
        /// <returns>Returns the inverse bind matrix by joint name dictionary</returns>
        private static Dictionary<string, Matrix> BuildInverseBindMatrixList(string name, string[] joints, Matrix[] mats)
        {
            var ibmList = new Dictionary<string, Matrix>();

            if (mats != null)
            {
                for (int i = 0; i < joints?.Length; i++)
                {
                    ibmList.Add(name + "_" + joints[i], mats[i]);
                }
            }

            return ibmList;
        }

        #endregion

        #region Animation

        /// <summary>
        /// Process animation
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="animationLibrary">Animation library</param>
        /// <returns>Retuns animation content list</returns>
        private static AnimationContent[] ProcessAnimation(ModelContent modelContent, Animation animationLibrary)
        {
            List<AnimationContent> res = new List<AnimationContent>();

            foreach (var channel in animationLibrary.Channels)
            {
                string jointName = channel.Target.Split("/".ToCharArray())[0];

                if (!modelContent.SkinningInfo.HasJointData(jointName))
                {
                    //Process only joints in the skeleton
                    continue;
                }

                foreach (var sampler in animationLibrary.Samplers)
                {
                    var info = BuildAnimationContent(sampler, animationLibrary, jointName);

                    res.Add(info);
                }
            }

            return res.ToArray();
        }
        /// <summary>
        /// Reads animation data from the specified sampler and builds an AnimationContent instance
        /// </summary>
        /// <param name="sampler">Sampler</param>
        /// <param name="animationLibrary">Animation library</param>
        /// <param name="jointName">Joint name</param>
        /// <returns>Returns an AnimationContent instance for the Joint</returns>
        private static AnimationContent BuildAnimationContent(Sampler sampler, Animation animationLibrary, string jointName)
        {
            float[] inputs = null;
            Matrix[] outputs = null;
            string[] interpolations = null;

            //Keyframe times
            Input inputsInput = sampler[EnumSemantics.Input];
            if (inputsInput != null)
            {
                inputs = animationLibrary[inputsInput.Source].ReadFloat();
            }

            //Keyframe transform matrix
            Input outputsInput = sampler[EnumSemantics.Output];
            if (outputsInput != null)
            {
                outputs = animationLibrary[outputsInput.Source].ReadMatrix();
            }

            //Keyframe interpolation types
            Input interpolationsInput = sampler[EnumSemantics.Interpolation];
            if (interpolationsInput != null)
            {
                interpolations = animationLibrary[interpolationsInput.Source].ReadNames();
            }

            List<Keyframe> keyframes = new List<Keyframe>();

            for (int i = 0; i < inputs?.Length; i++)
            {
                Keyframe keyframe = new Keyframe()
                {
                    Time = inputs[i],
                    Transform = outputs != null ? outputs[i] : Matrix.Identity,
                    Interpolation = interpolations?[i],
                };

                keyframes.Add(keyframe);
            }

            AnimationContent info = new AnimationContent()
            {
                Joint = jointName,
                Keyframes = keyframes.ToArray(),
            };

            return info;
        }

        #endregion

        #region Armatures

        /// <summary>
        /// Process skeleton
        /// </summary>
        /// <param name="skeletonName">Skeleton name</param>
        /// <param name="parent">Parent joint</param>
        /// <param name="node">Armature node</param>
        /// <returns>Return skeleton joint hierarchy</returns>
        private static Joint ProcessJoints(string skeletonName, Joint parent, Node node)
        {
            Matrix localTransform = node.ReadMatrix();

            Joint jt = new Joint(skeletonName + "_" + node.SId, parent, localTransform, Matrix.Identity);

            Skeleton.UpdateToWorldTransform(jt);

            if (node.Nodes?.Length > 0)
            {
                List<Joint> childs = new List<Joint>();

                foreach (Node child in node.Nodes)
                {
                    childs.Add(ProcessJoints(skeletonName, jt, child));
                }

                jt.Childs = childs.ToArray();
            }

            return jt;
        }

        #endregion

        #region Effects

        /// <summary>
        /// Process effect information
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <returns>Returns material content</returns>
        private static MaterialContent ProcessTechniqueFX(ProfileCommon profile)
        {
            TechniqueCommon technique = profile.Technique;

            string algorithmName = null;

            VarColorOrTexture emission = null;
            VarColorOrTexture ambient = null;
            VarColorOrTexture diffuse = null;
            VarColorOrTexture specular = null;
            VarFloatOrParam shininess = null;
            VarColorOrTexture reflective = null;
            VarFloatOrParam reflectivity = null;
            BasicTransparent transparent = null;
            VarFloatOrParam transparency = null;
            VarFloatOrParam indexOfRefraction = null;

            if (technique.Blinn != null || technique.Phong != null)
            {
                BlinnPhong algorithm = technique.Blinn ?? technique.Phong;

                algorithmName = "BlinnPhong";

                emission = algorithm.Emission;
                ambient = algorithm.Ambient;
                diffuse = algorithm.Diffuse;
                specular = algorithm.Specular;
                shininess = algorithm.Shininess;
                reflective = algorithm.Reflective;
                reflectivity = algorithm.Reflectivity;
                transparent = algorithm.Transparent;
                transparency = algorithm.Transparency;
                indexOfRefraction = algorithm.IndexOfRefraction;
            }
            else if (technique.Constant != null)
            {
                Constant algorithm = technique.Constant;

                algorithmName = "Constant";

                emission = algorithm.Emission;
                reflective = algorithm.Reflective;
                reflectivity = algorithm.Reflectivity;
                transparent = algorithm.Transparent;
                transparency = algorithm.Transparency;
                indexOfRefraction = algorithm.IndexOfRefraction;
            }
            else if (technique.Lambert != null)
            {
                Lambert algorithm = technique.Lambert;

                algorithmName = "Lambert";

                emission = algorithm.Emission;
                ambient = algorithm.Ambient;
                diffuse = algorithm.Diffuse;
                specular = algorithm.Specular;
                shininess = algorithm.Shininess;
                reflective = algorithm.Reflective;
                reflectivity = algorithm.Reflectivity;
                transparent = algorithm.Transparent;
                transparency = algorithm.Transparency;
                indexOfRefraction = algorithm.IndexOfRefraction;
            }

            string emissionTexture = GetTexture(profile, emission);
            Color4 emissionColor = GetColor(emission, new Color4(0.0f, 0.0f, 0.0f, 1.0f));

            string ambientTexture = GetTexture(profile, ambient);
            Color4 ambientColor = GetColor(ambient, new Color4(0.0f, 0.0f, 0.0f, 1.0f));

            string diffuseTexture = GetTexture(profile, diffuse);
            Color4 diffuseColor = GetColor(diffuse, new Color4(0.8f, 0.8f, 0.8f, 1.0f));

            string reflectiveTexture = GetTexture(profile, reflective);
            Color4 reflectiveColor = GetColor(reflective, new Color4(0.0f, 0.0f, 0.0f, 0.0f));

            string specularTexture = GetTexture(profile, specular);
            Color4 specularColor = GetColor(specular, new Color4(0.5f, 0.5f, 0.5f, 1.0f));

            float indexOfRefractionValue = indexOfRefraction?.Float.Value ?? 1.0f;

            float reflectivityValue = reflectivity?.Float.Value ?? 0.0f;

            float shininessValue = shininess?.Float.Value ?? 50.0f;

            float transparencyValue = transparency?.Float.Value ?? 0.0f;

            Color4 transparentColor = transparent?.Opaque == EnumOpaque.AlphaOne ? new Color4(0.0f, 0.0f, 0.0f, 1.0f) : new Color4(0.0f, 0.0f, 0.0f, 0.0f);

            //Look for bump mappings
            string normalMapTexture = FindBumpMap(profile, technique);

            return new MaterialContent()
            {
                Algorithm = algorithmName,

                EmissionTexture = emissionTexture,
                EmissionColor = emissionColor,
                AmbientTexture = ambientTexture,
                AmbientColor = ambientColor,
                DiffuseTexture = diffuseTexture,
                DiffuseColor = diffuseColor,
                SpecularTexture = specularTexture,
                SpecularColor = specularColor,
                ReflectiveTexture = reflectiveTexture,
                ReflectiveColor = reflectiveColor,

                Shininess = shininessValue,
                Reflectivity = reflectivityValue,
                Transparency = transparencyValue,
                IndexOfRefraction = indexOfRefractionValue,

                Transparent = transparentColor,

                NormalMapTexture = normalMapTexture,
            };
        }
        /// <summary>
        /// Gets the texture name
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <param name="colorOrTexture">Color or texture value</param>
        /// <returns>Returns the texture name</returns>
        private static string GetTexture(ProfileCommon profile, VarColorOrTexture colorOrTexture)
        {
            if (colorOrTexture == null)
            {
                return null;
            }

            if (colorOrTexture.Texture != null)
            {
                return FindTexture(profile, colorOrTexture.Texture);
            }

            return null;
        }
        /// <summary>
        /// Gets the color value
        /// </summary>
        /// <param name="colorOrTexture">Color or texture value</param>
        /// <param name="defaultColor">Default color value</param>
        /// <returns>Returns the color value</returns>
        private static Color4 GetColor(VarColorOrTexture colorOrTexture, Color4 defaultColor)
        {
            if (colorOrTexture == null)
            {
                return defaultColor;
            }

            return colorOrTexture.Color?.ToColor4() ?? defaultColor;
        }
        /// <summary>
        /// Finds texture
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <param name="texture">Texture information</param>
        /// <returns>Returns texture name</returns>
        private static string FindTexture(ProfileCommon profile, BasicTexture texture)
        {
            if (texture == null)
            {
                return null;
            }

            var sampler = profile[texture.Texture].Sampler2D;
            if (sampler == null)
            {
                return null;
            }

            var surface = profile[sampler.Source].Surface;
            if (surface == null)
            {
                return null;
            }

            if (surface.InitFrom != null)
            {
                return surface.InitFrom.Value;
            }
            else if (surface.InitAsNull != null)
            {
                throw new NotImplementedException();
            }
            else if (surface.InitAsTarget != null)
            {
                throw new NotImplementedException();
            }
            else if (surface.InitPlanar != null)
            {
                throw new NotImplementedException();
            }
            else if (surface.InitCube != null)
            {
                throw new NotImplementedException();
            }
            else if (surface.InitVolume != null)
            {
                throw new NotImplementedException();
            }

            return null;
        }
        /// <summary>
        /// Finds bump map texture
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <param name="technique">Technique information</param>
        /// <returns>Returns texture name</returns>
        private static string FindBumpMap(ProfileCommon profile, TechniqueCommon technique)
        {
            if (technique.Extras?.Length > 0)
            {
                for (int i = 0; i < technique.Extras.Length; i++)
                {
                    var techniques = technique.Extras[i].Techniques;
                    if (techniques?.Length > 0)
                    {
                        for (int t = 0; t < techniques.Length; t++)
                        {
                            if (techniques[t].BumpMaps?.Length > 0)
                            {
                                return FindTexture(profile, techniques[t].BumpMaps[0].Texture);
                            }
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Visual Scene

        /// <summary>
        /// Process visual scene
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="transform">Parent transform</param>
        /// <param name="useControllerTransform">Use parent controller transform</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessVisualScene(Collada dae, Matrix transform, bool useControllerTransform, ModelContent modelContent)
        {
            if (dae.Scene.InstanceVisualScene != null)
            {
                string sceneUrl = dae.Scene.InstanceVisualScene.Url;

                var vScene = Array.Find(dae.LibraryVisualScenes, l => string.Equals("#" + l.Id, sceneUrl, StringComparison.OrdinalIgnoreCase));
                if (vScene?.Nodes.Length > 0)
                {
                    ProcessSceneNodes(
                        vScene.Nodes,
                        transform,
                        useControllerTransform,
                        modelContent);
                }
            }
        }
        /// <summary>
        /// Process a node list from a visual scene
        /// </summary>
        /// <param name="nodes">Node list</param>
        /// <param name="transform">Parent transform</param>
        /// <param name="useControllerTransform">Use parent controller transform</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessSceneNodes(IEnumerable<Node> nodes, Matrix transform, bool useControllerTransform, ModelContent modelContent)
        {
            foreach (var node in nodes.Where(n => !n.IsArmature && !n.HasController))
            {
                Matrix trn = useControllerTransform ? transform * node.ReadMatrix() : transform;

                bool procChilds = true;

                if (node.IsLight)
                {
                    //Lights
                    ProcessSceneNodeLight(trn, node, modelContent);
                }
                else if (node.HasGeometry)
                {
                    //Geometry nodes
                    ProcessSceneNodeGeometry(trn, node, modelContent);
                }
                else
                {
                    procChilds = false;

                    //Default node
                    ProcessSceneNodeDefault(trn, node, modelContent);
                }

                if (procChilds && node.Nodes?.Length > 0)
                {
                    ProcessSceneNodes(node.Nodes, trn, true, modelContent);
                }
            }

            if (nodes.Any(n => n.IsArmature || n.HasController))
            {
                ProcessSceneNodesArmature(nodes, modelContent);
            }
        }
        /// <summary>
        /// Process a light node
        /// </summary>
        /// <param name="trn">Transform</param>
        /// <param name="node">Node</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessSceneNodeLight(Matrix trn, Node node, ModelContent modelContent)
        {
            if (node.InstanceLight?.Length > 0)
            {
                foreach (var il in node.InstanceLight)
                {
                    string lightName = il.Url.Replace("#", "");

                    var light = modelContent.Lights[lightName];

                    light.Name = lightName;
                    light.Transform = trn;
                }
            }
        }
        /// <summary>
        /// Process a geometry node
        /// </summary>
        /// <param name="trn">Transform</param>
        /// <param name="node">Node</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessSceneNodeGeometry(Matrix trn, Node node, ModelContent modelContent)
        {
            if (!trn.IsIdentity && node.InstanceGeometry?.Length > 0)
            {
                foreach (var ig in node.InstanceGeometry)
                {
                    string meshName = ig.Url.Replace("#", "");

                    foreach (var submesh in modelContent.Geometry[meshName].Values)
                    {
                        submesh.Transform(trn);
                    }
                }
            }
        }
        /// <summary>
        /// Process a default node
        /// </summary>
        /// <param name="trn">Transform</param>
        /// <param name="node">Node</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessSceneNodeDefault(Matrix trn, Node node, ModelContent modelContent)
        {
            if (node.Nodes?.Any() != true)
            {
                return;
            }

            foreach (var child in node.Nodes)
            {
                Matrix childTrn = trn * child.ReadMatrix();

                if (!childTrn.IsIdentity && child.InstanceGeometry?.Length > 0)
                {
                    foreach (var ig in child.InstanceGeometry)
                    {
                        string meshName = ig.Url.Replace("#", "");

                        foreach (var submesh in modelContent.Geometry[meshName].Values)
                        {
                            submesh.Transform(childTrn);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Process a node from a visual scene node list
        /// </summary>
        /// <param name="nodes">Node list</param>
        /// <returns>Returns the resulting skinning content</returns>
        private static void ProcessSceneNodesArmature(IEnumerable<Node> nodes, ModelContent modelContent)
        {
            foreach (var node in nodes.Where(n => n.IsArmature))
            {
                //Armatures (Skeletons)
                var skeleton = ProcessSceneNodeArmature(node);
                if (skeleton != null)
                {
                    List<string> lControllers = new List<string>();

                    string skeletonName = $"#{skeleton.Root.Name}";

                    //Armature controllers
                    var controllerNodes = nodes.Where(n => n.HasController && string.Equals(n.SkeletonId, skeletonName, StringComparison.OrdinalIgnoreCase));
                    foreach (var controller in controllerNodes)
                    {
                        //Controllers
                        var nControllers = ProcessSceneNodeController(controller);

                        lControllers.AddRange(nControllers);
                    }

                    modelContent.SkinningInfo.Add(
                        node.Id,
                        new SkinningContent
                        {
                            Skeleton = skeleton,
                            Controllers = lControllers.ToArray(),
                        });
                }
            }
        }
        /// <summary>
        /// Process an armature node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the resulting skeleton</returns>
        private static Skeleton ProcessSceneNodeArmature(Node node)
        {
            if (node.Nodes?.Length > 0)
            {
                var root = ProcessJoints(node.Id, null, node.Nodes[0]);

                return new Skeleton(root);
            }

            return null;
        }
        /// <summary>
        /// Process a controller node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns a list of controller names</returns>
        private static string[] ProcessSceneNodeController(Node node)
        {
            List<string> lControllers = new List<string>();

            if (node.InstanceController?.Length > 0)
            {
                foreach (var ic in node.InstanceController)
                {
                    string controllerName = ic.Url.Replace("#", "");

                    lControllers.Add(controllerName);
                }
            }

            return lControllers.ToArray();
        }

        #endregion
    }

    /// <summary>
    /// Extensions for collada to sharpDX data parse
    /// </summary>
    static class LoaderColladaExtensions
    {
        /// <summary>
        /// Reads a Vector2 from BasicFloat2
        /// </summary>
        /// <param name="vector">BasicFloat2 vector</param>
        /// <returns>Returns the parsed Vector2 from BasicFloat2</returns>
        public static Vector2 ToVector2(this BasicFloat2 vector)
        {
            if (vector.Values != null && vector.Values.Length == 2)
            {
                return new Vector2(vector.Values[0], vector.Values[1]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Vector2.");
            }
        }
        /// <summary>
        /// Reads a Vector3 from BasicFloat3
        /// </summary>
        /// <param name="vector">BasicFloat3 vector</param>
        /// <returns>Returns the parsed Vector3 from BasicFloat3</returns>
        public static Vector3 ToVector3(this BasicFloat3 vector)
        {
            if (vector.Values != null && vector.Values.Length == 3)
            {
                return new Vector3(vector.Values[0], vector.Values[2], vector.Values[1]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Vector3.");
            }
        }
        /// <summary>
        /// Reads a Vector4 from BasicFloat4
        /// </summary>
        /// <param name="vector">BasicFloat4 vector</param>
        /// <returns>Returns the parsed Vector4 from BasicFloat4</returns>
        public static Vector4 ToVector4(this BasicFloat4 vector)
        {
            if (vector.Values != null && vector.Values.Length == 4)
            {
                return new Vector4(vector.Values[0], vector.Values[2], vector.Values[1], vector.Values[3]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Vector4.");
            }
        }
        /// <summary>
        /// Reads a Color4 from BasicColor
        /// </summary>
        /// <param name="color">BasicColor color</param>
        /// <returns>Returns the parsed Color4 from BasicColor</returns>
        public static Color4 ToColor4(this BasicColor color)
        {
            if (color.Values != null && color.Values.Length == 3)
            {
                return new Color4(color.Values[0], color.Values[1], color.Values[2], 1f);
            }
            else if (color.Values != null && color.Values.Length == 4)
            {
                return new Color4(color.Values[0], color.Values[1], color.Values[2], color.Values[3]);
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Color4.");
            }
        }
        /// <summary>
        /// Reads a Matrix from BasicFloat4x4
        /// </summary>
        /// <param name="matrix">BasicFloat4x4 matrix</param>
        /// <returns>Returns the parsed Matrix from BasicFloat4x4</returns>
        /// <remarks>
        /// From right handed
        /// { rx, ry, rz, 0 }
        /// { ux, uy, uz, 0 }
        /// { lx, ly, lz, 0 }
        /// { px, py, pz, 1 }
        /// To left handed
        /// { rx, rz, ry, 0 }
        /// { lx, lz, ly, 0 }
        /// { ux, uz, uy, 0 }
        /// { px, pz, py, 1 }
        /// </remarks>
        public static Matrix ToMatrix(this BasicFloat4X4 matrix)
        {
            if (matrix.Values != null && matrix.Values.Length == 16)
            {
                Matrix m = new Matrix()
                {
                    M11 = matrix.Values[0],
                    M12 = matrix.Values[2],
                    M13 = matrix.Values[1],
                    M14 = matrix.Values[3],

                    M31 = matrix.Values[4],
                    M32 = matrix.Values[6],
                    M33 = matrix.Values[5],
                    M34 = matrix.Values[7],

                    M21 = matrix.Values[8],
                    M22 = matrix.Values[10],
                    M23 = matrix.Values[9],
                    M24 = matrix.Values[11],

                    M41 = matrix.Values[12],
                    M42 = matrix.Values[14],
                    M43 = matrix.Values[13],
                    M44 = matrix.Values[15],
                };

                return m;
            }
            else
            {
                throw new EngineException("Value cannot be parsed to Matrix 4x4.");
            }
        }

        /// <summary>
        /// Reads a float array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the float array</returns>
        public static float[] ReadFloat(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 1)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(float)));
            }

            int length = source.TechniqueCommon.Accessor.Count;

            List<float> n = new List<float>();

            for (int i = 0; i < length * stride; i += stride)
            {
                float v = source.FloatArray[i];

                n.Add(v);
            }

            return n.ToArray();
        }
        /// <summary>
        /// Reads a string array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the string array</returns>
        public static string[] ReadNames(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 1)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(string)));
            }

            int length = source.TechniqueCommon.Accessor.Count;

            List<string> names = new List<string>();

            for (int i = 0; i < length * stride; i += stride)
            {
                string v = source.NameArray[i];

                names.Add(v);
            }

            return names.ToArray();
        }
        /// <summary>
        /// Reads a string array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the string array</returns>
        public static string[] ReadIDRefs(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 1)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(string)));
            }

            int length = source.TechniqueCommon.Accessor.Count;

            List<string> names = new List<string>();

            for (int i = 0; i < length * stride; i += stride)
            {
                string v = source.IdRefArray[i];

                names.Add(v);
            }

            return names.ToArray();
        }
        /// <summary>
        /// Reads a Vector2 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Vector2 array</returns>
        public static Vector2[] ReadVector2(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 2)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector2)));
            }

            int length = source.TechniqueCommon.Accessor.Count;

            int s = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "S");
            int t = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "T");

            List<Vector2> verts = new List<Vector2>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Vector2 v = new Vector2(
                    source.FloatArray[i + s],
                    source.FloatArray[i + t]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Reads a Vector3 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Vector3 array</returns>
        public static Vector3[] ReadVector3(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 3)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector3)));
            }

            int x = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "X");
            int y = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Y");
            int z = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Z");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Vector3> verts = new List<Vector3>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Vector3 v = new Vector3(
                    source.FloatArray[i + x],
                    source.FloatArray[i + z],
                    source.FloatArray[i + y]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Reads a Vector4 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Vector4 array</returns>
        public static Vector4[] ReadVector4(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 4)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(Vector3)));
            }

            int x = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "X");
            int y = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Y");
            int z = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "Z");
            int w = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "W");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Vector4> verts = new List<Vector4>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Vector4 v = new Vector4(
                    source.FloatArray[i + x],
                    source.FloatArray[i + z],
                    source.FloatArray[i + y],
                    source.FloatArray[i + w]);

                verts.Add(v);
            }

            return verts.ToArray();
        }
        /// <summary>
        /// Reads a Color3 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Color3 array</returns>
        public static Color3[] ReadColor3(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 3)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(Color3)));
            }

            int r = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "R");
            int g = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "G");
            int b = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "B");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Color3> colors = new List<Color3>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Color3 v = new Color3(
                    source.FloatArray[i + r],
                    source.FloatArray[i + g],
                    source.FloatArray[i + b]);

                colors.Add(v);
            }

            return colors.ToArray();
        }
        /// <summary>
        /// Reads a Color4 array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Color4 array</returns>
        public static Color4[] ReadColor4(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 4)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(Color4)));
            }

            int r = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "R");
            int g = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "G");
            int b = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "B");
            int a = Array.FindIndex(source.TechniqueCommon.Accessor.Params, p => p.Name == "A");

            int length = source.TechniqueCommon.Accessor.Count;

            List<Color4> colors = new List<Color4>();

            for (int i = 0; i < length * stride; i += stride)
            {
                //To left handed -> Z flipped to Y
                Color4 v = new Color4(
                    source.FloatArray[i + r],
                    source.FloatArray[i + g],
                    source.FloatArray[i + b],
                    source.FloatArray[i + a]);

                colors.Add(v);
            }

            return colors.ToArray();
        }
        /// <summary>
        /// Reads a Matrix array from a source
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Returns the Matrix array</returns>
        /// <remarks>
        /// From right handed
        /// { rx, ry, rz, 0 }
        /// { ux, uy, uz, 0 }
        /// { lx, ly, lz, 0 }
        /// { px, py, pz, 1 }
        /// To left handed
        /// { rx, rz, ry, 0 }
        /// { lx, lz, ly, 0 }
        /// { ux, uz, uy, 0 }
        /// { px, pz, py, 1 }
        /// </remarks>
        public static Matrix[] ReadMatrix(this Source source)
        {
            int stride = source.TechniqueCommon.Accessor.Stride;
            if (stride != 16)
            {
                throw new EngineException(string.Format("Stride not supported for {1}: {0}", stride, typeof(Matrix)));
            }

            int length = source.TechniqueCommon.Accessor.Count;

            List<Matrix> mats = new List<Matrix>();

            for (int i = 0; i < length * stride; i += stride)
            {
                Matrix m = new Matrix()
                {
                    M11 = source.FloatArray[i + 0],
                    M12 = source.FloatArray[i + 8],
                    M13 = source.FloatArray[i + 4],
                    M14 = source.FloatArray[i + 12],

                    M21 = source.FloatArray[i + 2],
                    M22 = source.FloatArray[i + 10],
                    M23 = source.FloatArray[i + 6],
                    M24 = source.FloatArray[i + 14],

                    M31 = source.FloatArray[i + 1],
                    M32 = source.FloatArray[i + 9],
                    M33 = source.FloatArray[i + 5],
                    M34 = source.FloatArray[i + 13],

                    M41 = source.FloatArray[i + 3],
                    M43 = source.FloatArray[i + 7],
                    M42 = source.FloatArray[i + 11],
                    M44 = source.FloatArray[i + 15],
                };

                mats.Add(m);
            }

            return mats.ToArray();
        }

        /// <summary>
        /// Reads a transform matrix (SRT) from a Node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>Returns the parsed matrix</returns>
        public static Matrix ReadMatrix(this Node node)
        {
            if (node.Matrix != null)
            {
                return ReadMatrixFromTransform(node.Matrix);
            }
            else
            {
                return ReadMatrixLocRotScale(node.Translate, node.Rotate, node.Scale);
            }
        }
        /// <summary>
        /// Reads a transform matrix (SRT) from a matrix in the Node
        /// </summary>
        /// <param name="matrix">Node matrix</param>
        /// <returns>Returns the parsed matrix</returns>
        public static Matrix ReadMatrixFromTransform(BasicFloat4X4[] matrix)
        {
            if (matrix != null)
            {
                BasicFloat4X4 trn = Array.Find(matrix, t => string.Equals(t.SId, "transform"));
                if (trn != null)
                {
                    Matrix m = trn.ToMatrix();

                    return Matrix.Transpose(m);
                }
            }

            return Matrix.Identity;
        }
        /// <summary>
        /// Reads a transform matrix (SRT) from the specified transforms in the Node
        /// </summary>
        /// <param name="translate">Translate</param>
        /// <param name="rotate">Rotate</param>
        /// <param name="scale">Scale</param>
        /// <returns>Returns the parsed matrix</returns>
        public static Matrix ReadMatrixLocRotScale(BasicFloat3[] translate, BasicFloat4[] rotate, BasicFloat3[] scale)
        {
            Matrix finalTranslation = Matrix.Identity;
            Matrix finalRotation = Matrix.Identity;
            Matrix finalScale = Matrix.Identity;

            if (translate != null)
            {
                BasicFloat3 loc = Array.Find(translate, t => string.Equals(t.SId, "location"));
                if (loc != null) finalTranslation *= Matrix.Translation(loc.ToVector3());
            }

            if (rotate != null)
            {
                BasicFloat4 rotX = Array.Find(rotate, t => string.Equals(t.SId, "rotationX"));
                if (rotX != null)
                {
                    Vector4 r = rotX.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotY = Array.Find(rotate, t => string.Equals(t.SId, "rotationY"));
                if (rotY != null)
                {
                    Vector4 r = rotY.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }

                BasicFloat4 rotZ = Array.Find(rotate, t => string.Equals(t.SId, "rotationZ"));
                if (rotZ != null)
                {
                    Vector4 r = rotZ.ToVector4();
                    finalRotation *= Matrix.RotationAxis(new Vector3(r.X, r.Y, r.Z), r.W);
                }
            }

            if (scale != null)
            {
                BasicFloat3 sca = Array.Find(scale, t => string.Equals(t.SId, "scale"));
                if (sca != null) finalScale *= Matrix.Scaling(sca.ToVector3());
            }

            return finalScale * finalRotation * finalTranslation;
        }

        /// <summary>
        /// Adds vertex input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="vertexInput">Input</param>
        /// <param name="positions">Positions list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateVertexInput(this VertexData vert, Input vertexInput, Vector3[] positions, BasicIntArray indices, int index)
        {
            if (vertexInput != null)
            {
                var vIndex = indices[index + vertexInput.Offset];
                vert.VertexIndex = vIndex;
                vert.Position = positions[vIndex];
            }

            return vert;
        }
        /// <summary>
        /// Adds normal input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="normalInput">Input</param>
        /// <param name="normals">Normals list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateNormalInput(this VertexData vert, Input normalInput, Vector3[] normals, BasicIntArray indices, int index)
        {
            if (normalInput != null)
            {
                var nIndex = indices[index + normalInput.Offset];
                vert.Normal = normals[nIndex];
            }

            return vert;
        }
        /// <summary>
        /// Adds texture coordinates input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="texCoordInput">Input</param>
        /// <param name="texCoords">Coordinates list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateTexCoordInput(this VertexData vert, Input texCoordInput, Vector2[] texCoords, BasicIntArray indices, int index)
        {
            if (texCoordInput != null)
            {
                var tIndex = indices[index + texCoordInput.Offset];
                Vector2 tex = texCoords[tIndex];

                //Invert Vertical coordinate
                tex.Y = -tex.Y;

                vert.Texture = tex;
            }

            return vert;
        }
        /// <summary>
        /// Adds color input to vertex data
        /// </summary>
        /// <param name="vert">Vertex data instance</param>
        /// <param name="colorsInput">Input</param>
        /// <param name="colors">Colors list</param>
        /// <param name="indices">Index list</param>
        /// <param name="index">Current index</param>
        /// <returns>Returns the updated vertex data in a new instance</returns>
        internal static VertexData UpdateColorsInput(this VertexData vert, Input colorsInput, Color3[] colors, BasicIntArray indices, int index)
        {
            if (colorsInput != null)
            {
                int cIndex = indices[index + colorsInput.Offset];
                vert.Color = new Color4(colors[cIndex], 1);
            }

            return vert;
        }
    }
}
