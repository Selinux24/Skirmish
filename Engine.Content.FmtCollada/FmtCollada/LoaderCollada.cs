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
    using Engine.Content.Persistence;

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
        /// Gets the loader delegate
        /// </summary>
        /// <returns>Returns a delegate wich creates a loader</returns>
        public Func<ILoader> GetLoaderDelegate()
        {
            return () => { return new LoaderCollada(); };
        }

        /// <summary>
        /// Gets the extensions list which this loader is valid
        /// </summary>
        /// <returns>Returns a extension array list</returns>
        public IEnumerable<string> GetExtensions()
        {
            return new string[] { ".dae" };
        }

        /// <summary>
        /// Load a collada model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Conten description</param>
        /// <returns>Returns the loaded contents</returns>
        public IEnumerable<ContentData> Load(string contentFolder, ContentDataFile content)
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
            bool bakeTransforms = content.BakeTransforms;
            bool readAnimations = content.ReadAnimations;

            var modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList?.Any() == true)
            {
                List<ContentData> res = new List<ContentData>();

                foreach (var model in modelList)
                {
                    var dae = Collada.Load(model);

                    ContentData modelContent = new ContentData();

                    //Scene Objects
                    ProcessLibraryLights(dae, modelContent);
                    ProcessLibraryImages(dae, modelContent, contentFolder);
                    ProcessLibraryMaterial(dae, modelContent);
                    ProcessLibraryGeometries(dae, modelContent, volumes);
                    ProcessLibraryControllers(dae, modelContent);

                    //Scene Relations
                    ProcessVisualScene(dae, transform, useControllerTransform, bakeTransforms, modelContent);

                    if (readAnimations)
                    {
                        //Animations
                        ProcessLibraryAnimations(dae, modelContent, animation);
                    }

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
        private static IEnumerable<ContentData> FilterGeometry(ContentData modelContent, string armatureName, IEnumerable<string> meshesByLOD)
        {
            bool filterByArmature = !string.IsNullOrWhiteSpace(armatureName);
            bool filterByMeshes = meshesByLOD?.Any() == true;

            if (!filterByArmature && !filterByMeshes)
            {
                //Nothing to filter
                return new[] { modelContent };
            }

            List<ContentData> res = new List<ContentData>();

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
        private static void ProcessLibraryLights(Collada dae, ContentData modelContent)
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
                            Color = dirTechnique.Directional.Color.ToColor3(),
                        };
                    }

                    var pointTechnique = Array.Find(light.LightTechniqueCommon, l => l.Point != null);
                    if (pointTechnique != null)
                    {
                        info = new LightContent()
                        {
                            LightType = LightContentTypes.Point,
                            Color = pointTechnique.Point.Color.ToColor3(),
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
                            Color = spotTechnique.Spot.Color.ToColor3(),
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
        private static void ProcessLibraryImages(Collada dae, ContentData modelContent, string contentFolder)
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
        private static void ProcessLibraryMaterial(Collada dae, ContentData modelContent)
        {
            if (dae.LibraryMaterials?.Length > 0 && dae.LibraryEffects?.Length > 0)
            {
                foreach (var material in dae.LibraryMaterials)
                {
                    var info = MaterialBlinnPhongContent.Default;

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
        private static void ProcessLibraryGeometries(Collada dae, ContentData modelContent, IEnumerable<string> volumes)
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

                            modelContent.ImportMaterial(geometry.Id, subMesh.Material, subMesh);
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
        private static void ProcessLibraryControllers(Collada dae, ContentData modelContent)
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
        private static void ProcessLibraryAnimations(Collada dae, ContentData modelContent, AnimationFile animation)
        {
            if (dae.LibraryAnimations?.Length > 0)
            {
                for (int i = 0; i < dae.LibraryAnimations.Length; i++)
                {
                    var animationLib = dae.LibraryAnimations[i];

                    var info = LoaderAnimations.ProcessAnimation(animationLib);

                    modelContent.AddAnimationContent(animationLib.Id, info);
                }

                modelContent.AnimationDefinition = animation;
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
        private static MaterialBlinnPhongContent ProcessTechniqueFX(ProfileCommon profile)
        {
            var technique = profile.Technique;

            VarColorOrTexture diffuse = null;
            VarColorOrTexture emissive = null;
            VarColorOrTexture ambient = null;
            VarColorOrTexture specular = null;
            VarFloatOrParam shininess = null;
            BasicTransparent transparent = null;

            if (technique.Blinn != null || technique.Phong != null)
            {
                var algorithm = technique.Blinn ?? technique.Phong;

                diffuse = algorithm.Diffuse;
                emissive = algorithm.Emission;
                ambient = algorithm.Ambient;
                specular = algorithm.Specular;
                shininess = algorithm.Shininess;
                transparent = algorithm.Transparent;
            }
            else if (technique.Constant != null)
            {
                var algorithm = technique.Constant;

                emissive = algorithm.Emission;
                transparent = algorithm.Transparent;
            }
            else if (technique.Lambert != null)
            {
                var algorithm = technique.Lambert;

                diffuse = algorithm.Diffuse;
                emissive = algorithm.Emission;
                ambient = algorithm.Ambient;
                specular = algorithm.Specular;
                shininess = algorithm.Shininess;
                transparent = algorithm.Transparent;
            }

            string diffuseTexture = GetTexture(profile, diffuse);
            Color4 diffuseColor = GetColor(diffuse, MaterialConstants.DiffuseColor);

            string emissiveTexture = GetTexture(profile, emissive);
            Color3 emissiveColor = GetColor(emissive, MaterialConstants.EmissiveColor);

            string ambientTexture = GetTexture(profile, ambient);
            Color3 ambientColor = GetColor(ambient, MaterialConstants.AmbientColor);

            string specularTexture = GetTexture(profile, specular);
            Color3 specularColor = GetColor(specular, MaterialConstants.SpecularColor);

            float shininessValue = shininess?.Float?.Value ?? MaterialConstants.Shininess;

            bool isTransparent = transparent?.Opaque != null;

            //Look for bump mappings
            string normalMapTexture = FindBumpMap(profile, technique);

            return new MaterialBlinnPhongContent()
            {
                DiffuseTexture = diffuseTexture,
                DiffuseColor = diffuseColor,
                EmissiveTexture = emissiveTexture,
                EmissiveColor = emissiveColor,
                AmbientTexture = ambientTexture,
                AmbientColor = ambientColor,
                SpecularTexture = specularTexture,
                SpecularColor = specularColor,
                Shininess = shininessValue,

                IsTransparent = isTransparent,

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
        private static Color3 GetColor(VarColorOrTexture colorOrTexture, Color3 defaultColor)
        {
            if (colorOrTexture == null)
            {
                return defaultColor;
            }

            return colorOrTexture.Color?.ToColor3() ?? defaultColor;
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
        /// <param name="bakeTransforms">Bake transforms into sub-meshes</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessVisualScene(Collada dae, Matrix transform, bool useControllerTransform, bool bakeTransforms, ContentData modelContent)
        {
            if (dae.Scene.InstanceVisualScene == null)
            {
                return;
            }

            string sceneUrl = dae.Scene.InstanceVisualScene.Url;

            var vScene = dae.LibraryVisualScenes.FirstOrDefault(l => string.Equals("#" + l.Id, sceneUrl, StringComparison.OrdinalIgnoreCase));
            if (vScene?.Nodes.Any() != true)
            {
                return;
            }

            ProcessSceneNodes(
                vScene.Nodes.Where(n => !n.IsArmature && !n.HasController),
                transform,
                useControllerTransform,
                bakeTransforms,
                modelContent);

            ProcessSceneNodesArmature(
                vScene.Nodes.Where(n => n.IsArmature),
                vScene.Nodes.Where(n => n.HasController),
                modelContent);
        }
        /// <summary>
        /// Process a node from a visual scene node list
        /// </summary>
        /// <param name="nodes">Node list</param>
        /// <param name="transform">Parent transform</param>
        /// <param name="useControllerTransform">Use parent controller transform</param>
        /// <param name="bakeTransforms">Bake transforms into sub-meshes</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessSceneNodes(IEnumerable<Node> nodes, Matrix transform, bool useControllerTransform, bool bakeTransforms, ContentData modelContent)
        {
            if (nodes?.Any() != true)
            {
                return;
            }

            foreach (var node in nodes)
            {
                Matrix trn = useControllerTransform ? transform * node.ReadMatrix() : transform;

                bool procChilds = true;

                if (node.IsLight)
                {
                    //Lights
                    ProcessSceneNodesLight(trn, node.InstanceLight, modelContent);
                }
                else if (node.HasGeometry)
                {
                    //Geometry nodes
                    ProcessSceneNodesGeometry(trn, node.InstanceGeometry, modelContent, bakeTransforms);
                }
                else
                {
                    procChilds = false;

                    //Default node
                    ProcessSceneNodesDefault(trn, node.Nodes, modelContent, bakeTransforms);
                }

                if (procChilds && node.Nodes?.Length > 0)
                {
                    ProcessSceneNodes(node.Nodes, bakeTransforms ? trn : transform, true, bakeTransforms, modelContent);
                }
            }
        }
        /// <summary>
        /// Process a default node
        /// </summary>
        /// <param name="trn">Transform</param>
        /// <param name="nodes">Node list</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="bakeTransforms">Bake transforms into sub-meshes</param>
        private static void ProcessSceneNodesDefault(Matrix trn, IEnumerable<Node> nodes, ContentData modelContent, bool bakeTransforms)
        {
            if (nodes?.Any() != true)
            {
                return;
            }

            foreach (var child in nodes)
            {
                ProcessSceneNodesGeometry(trn * child.ReadMatrix(), child.InstanceGeometry, modelContent, bakeTransforms);
            }
        }
        /// <summary>
        /// Process a light node list
        /// </summary>
        /// <param name="trn">Transform</param>
        /// <param name="lights">Light nodes</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessSceneNodesLight(Matrix trn, IEnumerable<InstanceWithExtra> lights, ContentData modelContent)
        {
            if (lights?.Any() != true)
            {
                return;
            }

            foreach (var il in lights)
            {
                string lightName = il.Url.Replace("#", "");

                var light = modelContent.Lights[lightName];

                light.Name = lightName;
                light.Transform = trn;
            }
        }
        /// <summary>
        /// Process a geometry node list
        /// </summary>
        /// <param name="trn">Transform</param>
        /// <param name="geometry">Geometry nodes</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="bakeTransforms">Bake transforms into sub-meshes</param>
        private static void ProcessSceneNodesGeometry(Matrix trn, IEnumerable<InstanceGeometry> geometry, ContentData modelContent, bool bakeTransforms)
        {
            if (trn.IsIdentity)
            {
                return;
            }

            if (geometry?.Any() != true)
            {
                return;
            }

            foreach (var ig in geometry)
            {
                string meshName = ig.Url.Replace("#", "");

                foreach (var submesh in modelContent.Geometry[meshName].Values)
                {
                    if (bakeTransforms)
                    {
                        submesh.ApplyTransform(trn);
                    }
                    else
                    {
                        submesh.Transform = trn;
                    }
                }
            }
        }
        /// <summary>
        /// Process a node from a visual scene node list
        /// </summary>
        /// <param name="armatureNodes">Armature node list</param>
        /// <param name="controllerNodes">Controller node list</param>
        /// <param name="modelContent">Nodel content</param>
        /// <returns>Returns the resulting skinning content</returns>
        private static void ProcessSceneNodesArmature(IEnumerable<Node> armatureNodes, IEnumerable<Node> controllerNodes, ContentData modelContent)
        {
            if (armatureNodes?.Any() != true || controllerNodes?.Any() != true)
            {
                return;
            }

            foreach (var node in armatureNodes)
            {
                //Armatures (Skeletons)
                var skeleton = CreateSkeleton(node.Id, node.Nodes);
                if (skeleton == null)
                {
                    continue;
                }

                string skeletonName = $"#{skeleton.Root.Name}";

                //Armature controllers
                var lControllers = controllerNodes
                    .Where(n => n.HasController && string.Equals(n.SkeletonId, skeletonName, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(controller => controller.InstanceController.Select(c => c.Url.Replace("#", "")))
                    .ToArray();

                modelContent.SkinningInfo.Add(
                    node.Id,
                    new SkinningContent
                    {
                        Skeleton = skeleton,
                        Controllers = lControllers,
                    });
            }
        }
        /// <summary>
        /// Creates a skeleton from an armature node list
        /// </summary>
        /// <param name="skeletonName">Skeleton name</param>
        /// <param name="armatureNodes">Armature node list</param>
        /// <returns>Returns the resulting skeleton</returns>
        private static Skeleton CreateSkeleton(string skeletonName, IEnumerable<Node> armatureNodes)
        {
            if (armatureNodes?.Any() != true)
            {
                return null;
            }

            var root = ProcessJoints(skeletonName, null, armatureNodes.First());

            return new Skeleton(root);
        }

        #endregion
    }
}
