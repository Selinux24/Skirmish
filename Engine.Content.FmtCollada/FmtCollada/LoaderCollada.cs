using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IEnumerable<ContentData>> Load(string contentFolder, ContentDataFile content)
        {
            string fileName = content.ModelFileName;

#if DEBUG
            var currentThread = System.Threading.Thread.CurrentThread;
            if (currentThread == null)
            {
                currentThread.Name = $"LoaderCollada_{contentFolder}/{fileName}";
            }
#endif

            var modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList?.Any() != true)
            {
                throw new EngineException(string.Format("Model not found: {0}", fileName));
            }

            var taskList = modelList
                .Select(model =>
                {
                    return Task.Run(() =>
                    {
                        var dae = Collada.Load(model);

                        return ReadContentData(dae, contentFolder, content);
                    });
                })
                .ToList();

            List<ContentData> res = new List<ContentData>();

            while (taskList.Any())
            {
                var t = await Task.WhenAny(taskList);

                taskList.Remove(t);

                res.AddRange(await t);
            }

            foreach (var model in modelList)
            {
                model.Flush();
                model.Dispose();
            }

            return res.ToArray();
        }
        /// <summary>
        /// Reads the collada file into a content data instance
        /// </summary>
        /// <param name="dae">Collada file</param>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Content data file</param>
        /// <returns>Returns a collection of content data instances</returns>
        private static IEnumerable<ContentData> ReadContentData(Collada dae, string contentFolder, ContentDataFile content)
        {
            ContentData modelContent = new ContentData();

            Matrix transform = Matrix.Identity;
            if (content.Scale != 1f)
            {
                transform = Matrix.Scaling(content.Scale);
            }

            string armatureName = content.ArmatureName;
            var hulls = content.HullMeshes;
            var meshesByLOD = content.LODMeshes;
            var animation = content.Animation;
            bool useControllerTransform = content.UseControllerTransform;
            bool bakeTransforms = content.BakeTransforms;
            bool readAnimations = content.ReadAnimations;

            //Scene Objects
            ProcessLibraryLights(dae, modelContent);
            ProcessLibraryImages(dae, modelContent, contentFolder);
            ProcessLibraryMaterial(dae, modelContent);
            ProcessLibraryGeometries(dae, modelContent, hulls);
            ProcessLibraryControllers(dae, modelContent);

            //Scene Relations
            ProcessVisualScene(dae, transform, useControllerTransform, bakeTransforms, modelContent);

            if (readAnimations)
            {
                //Animations
                ProcessLibraryAnimations(dae, modelContent, animation);
            }

            //Filter the resulting model content
            return FilterGeometry(modelContent, armatureName, meshesByLOD);
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
        /// Gets whether the specified geometry name is a marked hull
        /// </summary>
        /// <param name="geometryName">Geometry name</param>
        /// <param name="hulls">List of hull name prefixes</param>
        /// <returns>Returns true if the geometry name starts with any of the hull names in the collection</returns>
        private static bool IsHull(string geometryName, IEnumerable<string> hulls)
        {
            return hulls?.Any(v => geometryName.StartsWith(v, StringComparison.OrdinalIgnoreCase)) == true;
        }

        #region Dictionary loaders

        /// <summary>
        /// Process lightd
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        private static void ProcessLibraryLights(Collada dae, ContentData modelContent)
        {
            if (dae.LibraryLights?.Any() != true)
            {
                return;
            }

            foreach (var light in dae.LibraryLights)
            {
                var dirTechnique = light.LightTechniqueCommon?.FirstOrDefault(l => l.Directional != null);
                if (dirTechnique != null)
                {
                    modelContent.Lights[light.Id] = new LightContent()
                    {
                        LightType = LightContentTypes.Directional,
                        Color = dirTechnique.Directional.Color.ToColor3(),
                    };
                }

                var pointTechnique = light.LightTechniqueCommon?.FirstOrDefault(l => l.Point != null);
                if (pointTechnique != null)
                {
                    modelContent.Lights[light.Id] = new LightContent()
                    {
                        LightType = LightContentTypes.Point,
                        Color = pointTechnique.Point.Color.ToColor3(),
                        ConstantAttenuation = pointTechnique.Point.ConstantAttenuation.Value,
                        LinearAttenuation = pointTechnique.Point.LinearAttenuation.Value,
                        QuadraticAttenuation = pointTechnique.Point.QuadraticAttenuation.Value,
                    };
                }

                var spotTechnique = light.LightTechniqueCommon?.FirstOrDefault(l => l.Spot != null);
                if (spotTechnique != null)
                {
                    modelContent.Lights[light.Id] = new LightContent()
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
            if (dae.LibraryImages?.Any() != true)
            {
                return;
            }

            foreach (var image in dae.LibraryImages)
            {
                if (image.Data != null)
                {
                    modelContent.Images[image.Id] = new MemoryImageContent(new MemoryStream((byte[])image.Data));
                }
                else if (!string.IsNullOrEmpty(image.InitFrom))
                {
                    modelContent.Images[image.Id] = new FileArrayImageContent(contentFolder, Uri.UnescapeDataString(image.InitFrom));
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
            if (dae.LibraryMaterials?.Any() != true || dae.LibraryEffects?.Any() != true)
            {
                return;
            }

            foreach (var material in dae.LibraryMaterials)
            {
                //Find effect
                var effect = dae.LibraryEffects?.FirstOrDefault(e => e.Id == material.InstanceEffect.Url.Replace("#", ""));
                if (effect == null)
                {
                    continue;
                }

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
                    modelContent.Materials[material.Id] = ProcessTechniqueFX(effect.ProfileCommon);
                }
            }
        }
        /// <summary>
        /// Process geometry
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="hulls">Hull mesh names</param>
        private static void ProcessLibraryGeometries(Collada dae, ContentData modelContent, IEnumerable<string> hulls)
        {
            if (dae.LibraryGeometries?.Any() != true)
            {
                return;
            }

            foreach (var geometry in dae.LibraryGeometries)
            {
                bool isHull = IsHull(geometry.Name, hulls);

                var info = ProcessGeometry(geometry, isHull);
                if (info?.Any() != true)
                {
                    continue;
                }

                foreach (var subMesh in info)
                {
                    string materialName = FindMaterialTarget(subMesh.Material, dae.LibraryVisualScenes);
                    if (!string.IsNullOrWhiteSpace(materialName) && modelContent.Materials.TryGetValue(materialName, out var value))
                    {
                        var mat = value;

                        subMesh.Material = materialName;
                        subMesh.SetTextured(mat.DiffuseTexture != null);
                    }

                    modelContent.ImportMaterial(geometry.Id, subMesh.Material, subMesh);
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
            if (dae.LibraryControllers?.Any() != true)
            {
                return;
            }

            foreach (var controller in dae.LibraryControllers)
            {
                var info = ProcessController(controller);
                if (info != null)
                {
                    modelContent.Controllers[controller.Id] = info;
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
            if (dae.LibraryAnimations?.Any() != true)
            {
                return;
            }

            foreach (var animationLib in dae.LibraryAnimations)
            {
                var info = LoaderAnimations.ProcessAnimation(animationLib);

                modelContent.AddAnimationContent(animationLib.Id, info);
            }

            modelContent.AnimationDefinition = animation;
        }

        #endregion

        #region Geometry

        /// <summary>
        /// Process geometry list
        /// </summary>
        /// <param name="geometry">Geometry info</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessGeometry(Geometry geometry, bool isHull)
        {
            if (geometry.Mesh != null)
            {
                return ProcessMesh(geometry.Mesh, isHull);
            }
            else if (geometry.Spline != null)
            {
                return ProcessSpline(geometry.Spline, isHull);
            }
            else if (geometry.ConvexMesh != null)
            {
                return ProcessConvexMesh(geometry.ConvexMesh, isHull);
            }

            return Enumerable.Empty<SubMeshContent>();
        }
        /// <summary>
        /// Process mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessMesh(Engine.Collada.Mesh mesh, bool isHull)
        {
            //Procesar por topología
            if (mesh.Lines?.Any() == true)
            {
                return ProcessLines(mesh.Lines, mesh.Sources, isHull);
            }
            else if (mesh.LineStrips?.Any() == true)
            {
                return ProcessLineStrips(mesh.LineStrips, mesh.Sources, isHull);
            }
            else if (mesh.Triangles?.Any() == true)
            {
                return ProcessTriangles(mesh.Triangles, mesh.Sources, isHull);
            }
            else if (mesh.TriFans?.Any() == true)
            {
                return ProcessTriFans(mesh.TriFans, mesh.Sources, isHull);
            }
            else if (mesh.TriStrips?.Any() == true)
            {
                return ProcessTriStrips(mesh.TriStrips, mesh.Sources, isHull);
            }
            else if (mesh.PolyList?.Any() == true)
            {
                return ProcessPolyLists(mesh.PolyList, mesh.Sources, isHull);
            }
            else if (mesh.Polygons?.Any() == true)
            {
                return ProcessPolygons(mesh.Polygons, mesh.Sources, isHull);
            }

            return Enumerable.Empty<SubMeshContent>();
        }
        /// <summary>
        /// Process spline
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessSpline(Spline spline, bool isHull)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process convex mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessConvexMesh(ConvexMesh convexMesh, bool isHull)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process lines
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessLines(IEnumerable<Lines> lines, IEnumerable<Source> meshSources, bool isHull)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process line strips
        /// </summary>
        /// <param name="lines">Line strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessLineStrips(IEnumerable<LineStrips> lines, IEnumerable<Source> meshSources, bool isHull)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangles
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessTriangles(IEnumerable<Triangles> triangles, IEnumerable<Source> meshSources, bool isHull)
        {
            if (triangles?.Any() != true)
            {
                return Enumerable.Empty<SubMeshContent>();
            }

            List<SubMeshContent> res = new List<SubMeshContent>();

            foreach (var triangle in triangles)
            {
                var verts = ProcessTriangle(triangle, meshSources);
                if (!verts.Any())
                {
                    continue;
                }

                //Reorder vertices
                VertexData[] data = new VertexData[verts.Count()];
                for (int i = 0; i < data.Length; i += 3)
                {
                    data[i + 0] = verts.ElementAt(i + 0);
                    data[i + 1] = verts.ElementAt(i + 2);
                    data[i + 2] = verts.ElementAt(i + 1);
                }

                SubMeshContent meshInfo = new SubMeshContent(Topology.TriangleList, triangle.Material, false, isHull);

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
        private static IEnumerable<VertexData> ProcessTriangle(Triangles triangle, IEnumerable<Source> meshSources)
        {
            List<VertexData> verts = new List<VertexData>();

            var vertexInput = triangle[EnumSemantics.Vertex];
            var normalInput = triangle[EnumSemantics.Normal];
            var texCoordInput = triangle[EnumSemantics.TexCoord];

            var positions = vertexInput != null ? meshSources.ElementAt(vertexInput.Offset).ReadVector3() : null;
            var normals = normalInput != null ? meshSources.ElementAt(normalInput.Offset).ReadVector3() : null;
            var texCoords = texCoordInput != null ? meshSources.ElementAt(texCoordInput.Offset).ReadVector2() : null;

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
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessTriFans(IEnumerable<TriFans> triFans, IEnumerable<Source> meshSources, bool isHull)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangle strips
        /// </summary>
        /// <param name="triStrips">Triangle strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessTriStrips(IEnumerable<TriStrips> triStrips, IEnumerable<Source> meshSources, bool isHull)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process polygon lists
        /// </summary>
        /// <param name="polyLists">Polygon lists</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessPolyLists(IEnumerable<PolyList> polyLists, IEnumerable<Source> meshSources, bool isHull)
        {
            if (polyLists?.Any() != true)
            {
                return Enumerable.Empty<SubMeshContent>();
            }

            List<SubMeshContent> res = new List<SubMeshContent>();

            foreach (var polyList in polyLists)
            {
                ProcessPolyList(polyList, meshSources, out var verts, out var indices);
                if (!verts.Any() || !indices.Any())
                {
                    continue;
                }

                SubMeshContent meshInfo = new SubMeshContent(Topology.TriangleList, polyList.Material, false, isHull);

                meshInfo.SetVertices(verts);
                meshInfo.SetIndices(indices);

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
        private static void ProcessPolyList(PolyList polyList, IEnumerable<Source> meshSources, out IEnumerable<VertexData> vertices, out IEnumerable<uint> indices)
        {
            List<VertexData> verts = new List<VertexData>();
            List<uint> idx = new List<uint>();

            var vertexInput = polyList[EnumSemantics.Vertex];
            var normalInput = polyList[EnumSemantics.Normal];
            var texCoordInput = polyList[EnumSemantics.TexCoord];
            var colorsInput = polyList[EnumSemantics.Color];

            var positions = vertexInput != null ? meshSources.ElementAt(vertexInput.Offset).ReadVector3() : null;
            var normals = normalInput != null ? meshSources.ElementAt(normalInput.Offset).ReadVector3() : null;
            var texCoords = texCoordInput != null ? meshSources.ElementAt(texCoordInput.Offset).ReadVector2() : null;
            var colors = colorsInput != null ? meshSources.ElementAt(colorsInput.Offset).ReadColor3() : null;

            int index = 0;
            int inputCount = polyList.Inputs.Length;

            for (int i = 0; i < polyList.Count; i++)
            {
                int n = polyList.VCount[i];

                idx.AddRange(BuildFace(n, verts.Count));

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

            vertices = verts.ToArray();
            indices = idx.ToArray();
        }
        /// <summary>
        /// Builds a poligon face
        /// </summary>
        /// <param name="vertCount">Vertex count</param>
        /// <param name="startIndex">Start index</param>
        /// <returns>Returns a face index list</returns>
        private static IEnumerable<uint> BuildFace(int vertCount, int startIndex)
        {
            if (vertCount == 3)
            {
                return new uint[]
                {
                    (uint)startIndex,
                    (uint)startIndex + 2,
                    (uint)startIndex + 1
                };
            }

            if (vertCount == 4)
            {
                return new uint[]
                {
                    (uint)startIndex,
                    (uint)startIndex + 2,
                    (uint)startIndex + 1,

                    (uint)startIndex,
                    (uint)startIndex + 3,
                    (uint)startIndex + 2,
                };
            }

            return Enumerable.Empty<uint>();
        }
        /// <summary>
        /// Process polygons
        /// </summary>
        /// <param name="polygons">Polygons</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="isHull">Current geometry is a hull mesh</param>
        /// <returns>Returns sub mesh content</returns>
        private static IEnumerable<SubMeshContent> ProcessPolygons(IEnumerable<Polygons> polygons, IEnumerable<Source> meshSources, bool isHull)
        {
            if (polygons?.Any() != true)
            {
                return Enumerable.Empty<SubMeshContent>();
            }

            List<SubMeshContent> res = new List<SubMeshContent>();

            foreach (var polygon in polygons)
            {
                var verts = ProcessPolygon(polygon, meshSources);
                if (!verts.Any())
                {
                    continue;
                }

                //Reorder vertices
                VertexData[] data = new VertexData[verts.Count()];
                for (int i = 0; i < data.Length; i += 3)
                {
                    data[i + 0] = verts.ElementAt(i + 0);
                    data[i + 1] = verts.ElementAt(i + 2);
                    data[i + 2] = verts.ElementAt(i + 1);
                }

                SubMeshContent meshInfo = new SubMeshContent(Topology.TriangleList, polygon.Material, false, isHull);

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
        private static IEnumerable<VertexData> ProcessPolygon(Polygons polygon, IEnumerable<Source> meshSources)
        {
            List<VertexData> verts = new List<VertexData>();

            var vertexInput = polygon[EnumSemantics.Vertex];
            var normalInput = polygon[EnumSemantics.Normal];
            var texCoordInput = polygon[EnumSemantics.TexCoord];

            var positions = vertexInput != null ? meshSources.ElementAt(vertexInput.Offset).ReadVector3() : null;
            var normals = normalInput != null ? meshSources.ElementAt(normalInput.Offset).ReadVector3() : null;
            var texCoords = texCoordInput != null ? meshSources.ElementAt(texCoordInput.Offset).ReadVector2() : null;

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

        private static string FindMaterialTarget(string material, IEnumerable<VisualScene> libraryVisualScenes)
        {
            if (libraryVisualScenes?.Any() != true)
            {
                return material;
            }

            foreach (var vs in libraryVisualScenes)
            {
                if (FindMaterialTarget(material, vs.Nodes, out var target))
                {
                    return target;
                }
            }

            return material;
        }

        private static bool FindMaterialTarget(string material, IEnumerable<Node> nodes, out string target)
        {
            target = null;

            if (nodes?.Any() != true)
            {
                return false;
            }

            foreach (var node in nodes)
            {
                if (FindMaterialTarget(material, node, out var nodeTarget))
                {
                    target = nodeTarget;

                    return true;
                }
            }

            return false;
        }

        private static bool FindMaterialTarget(string material, Node node, out string target)
        {
            //Look up on geometry
            if (FindMaterialTarget(material, node.InstanceGeometry, out var geomTarget))
            {
                target = geomTarget;

                return true;
            }

            //Look up on instance controller
            if (FindMaterialTarget(material, node.InstanceController, out var instanceTarget))
            {
                target = instanceTarget;

                return true;
            }

            //Look up on child nodes
            if (FindMaterialTarget(material, node.Nodes, out var nodesTarget))
            {
                target = nodesTarget;

                return true;
            }

            target = null;

            return false;
        }

        private static bool FindMaterialTarget(string material, IEnumerable<InstanceGeometry> instances, out string target)
        {
            if (instances?.Any() != true)
            {
                target = null;

                return false;
            }

            var instanceMaterial = instances
                .Where(g => g.BindMaterial?.TechniqueCommon?.Any(t => t.InstanceMaterial?.Any() == true) == true)
                .Select(g => g.BindMaterial.TechniqueCommon.First().InstanceMaterial.First())
                .FirstOrDefault(i => string.Equals(material, i.Symbol, StringComparison.OrdinalIgnoreCase));

            target = instanceMaterial?.Target?.Replace("#", "");

            return target != null;
        }

        private static bool FindMaterialTarget(string material, IEnumerable<InstanceController> instances, out string target)
        {
            if (instances?.Any() != true)
            {
                target = null;

                return false;
            }

            var instanceMaterial = instances
                .Where(g => g.BindMaterial?.TechniqueCommon?.Any(t => t.InstanceMaterial?.Any() == true) == true)
                .Select(g => g.BindMaterial.TechniqueCommon.First().InstanceMaterial.First())
                .FirstOrDefault(i => string.Equals(material, i.Symbol, StringComparison.OrdinalIgnoreCase));

            target = instanceMaterial?.Target?.Replace("#", "");

            return target != null;
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
            if (controller.Skin != null)
            {
                return ProcessSkin(controller.Skin);
            }
            else if (controller.Morph != null)
            {
                return ProcessMorph(controller.Morph);
            }

            return null;
        }
        /// <summary>
        /// Process skin
        /// </summary>
        /// <param name="skin">Skin information</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessSkin(Skin skin)
        {
            ControllerContent res = new ControllerContent
            {
                BindShapeMatrix = Matrix.Transpose(skin.BindShapeMatrix.ToMatrix()),
                Skin = skin.SourceUri.Replace("#", ""),
            };

            if (skin.VertexWeights != null)
            {
                ProcessVertexWeights(skin, out var ibmList, out var wgList);

                res.InverseBindMatrix = ibmList;
                res.Weights = wgList.ToArray();
            }

            return res;
        }
        /// <summary>
        /// Process morph
        /// </summary>
        /// <param name="morph">Morph information</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessMorph(Morph morph)
        {
            ControllerContent res = new ControllerContent();

            ProcessVertexWeights(morph, out var wgList);

            res.Weights = wgList.ToArray();

            return res;
        }
        /// <summary>
        /// Process vertext weight information
        /// </summary>
        /// <param name="skin">Skin information</param>
        /// <param name="inverseBindMatrixList">Inverse bind matrix list result</param>
        /// <param name="weightList">Weight list result</param>
        private static void ProcessVertexWeights(Skin skin, out Dictionary<string, Matrix> inverseBindMatrixList, out IEnumerable<Weight> weightList)
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

            weightList = BuildVertexWeigths(skin, jointsOffset, joints, weightsOffset, weights);
            inverseBindMatrixList = BuildInverseBindMatrixList(joints, mats);
        }
        /// <summary>
        /// Process vertext weight information
        /// </summary>
        /// <param name="morph">Morph information</param>
        /// <param name="weightList">Weight list result</param>
        private static void ProcessVertexWeights(Morph morph, out IEnumerable<Weight> weightList)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Creates the vertex weights list
        /// </summary>
        /// <param name="skin">Skin data</param>
        /// <param name="jointsOffset">Joints offset</param>
        /// <param name="joints">Joints names list</param>
        /// <param name="weightsOffset">Weights offset</param>
        /// <param name="weights">Weight values list</param>
        /// <returns>Returns the weights list</returns>
        private static IEnumerable<Weight> BuildVertexWeigths(Skin skin, int jointsOffset, IEnumerable<string> joints, int weightsOffset, IEnumerable<float> weights)
        {
            if (skin?.VertexWeights == null || skin.VertexWeights.Count <= 0)
            {
                return Enumerable.Empty<Weight>();
            }

            if (jointsOffset < 0 || joints?.Any() != true)
            {
                return Enumerable.Empty<Weight>();
            }

            if (weightsOffset < 0 || weights?.Any() != true)
            {
                return Enumerable.Empty<Weight>();
            }

            var wgList = new List<Weight>();

            int index = 0;
            int sources = skin.VertexWeights.Inputs.Length;

            for (int i = 0; i < skin.VertexWeights.Count; i++)
            {
                int n = skin.VertexWeights.VCount[i];

                for (int v = 0; v < n; v++)
                {
                    float weightValue = weights.ElementAt(skin.VertexWeights.V[index + weightsOffset]);
                    if (weightValue != 0f)
                    {
                        string jointName = joints.ElementAt(skin.VertexWeights.V[index + jointsOffset]);

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
        /// <param name="joints">Joint names list</param>
        /// <param name="mats">Inverse bind matrix list</param>
        /// <returns>Returns the inverse bind matrix by joint name dictionary</returns>
        private static Dictionary<string, Matrix> BuildInverseBindMatrixList(IEnumerable<string> joints, IEnumerable<Matrix> mats)
        {
            if (mats?.Any() != true || joints?.Any() != true)
            {
                return new Dictionary<string, Matrix>();
            }

            if (mats.Count() != joints.Count())
            {
                return new Dictionary<string, Matrix>();
            }

            var ibmList = new Dictionary<string, Matrix>();

            for (int i = 0; i < joints.Count(); i++)
            {
                ibmList.Add(joints.ElementAt(i), mats.ElementAt(i));
            }

            return ibmList;
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
            if (technique.Extras?.Any() != true)
            {
                return null;
            }

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

            ProcessSceneNodesSkeleton(vScene.Nodes, out var skeletons);
            if (skeletons.Any())
            {
                ProcessVisualSceneSkins(vScene, modelContent, skeletons);
            }

            var otherNodes = vScene.Nodes.Where(n => !n.IsArmature && !n.HasController).ToArray();
            ProcessSceneNodes(
                otherNodes,
                transform,
                useControllerTransform,
                bakeTransforms,
                modelContent);
        }
        /// <summary>
        /// Process the specified visual scene node for skinned data configuration
        /// </summary>
        /// <param name="vScene">Visual scene node</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="skeletons">Skeleton list</param>
        private static void ProcessVisualSceneSkins(VisualScene vScene, ContentData modelContent, IEnumerable<Skeleton> skeletons)
        {
            ProcessSceneNodesInstanceController(vScene.Nodes, out var instanceControllers);

            foreach (var skeleton in skeletons)
            {
                var skeletonControllers = instanceControllers
                    .Where(ic => string.Equals(ic.Skeleton.FirstOrDefault()?.Replace("#", ""), skeleton.Name))?
                    .ToArray();

                if (skeletonControllers?.Any() != true)
                {
                    continue;
                }

                var controllerNames = skeletonControllers.Select(sc => sc.Url.Replace("#", "")).ToArray();
                foreach (var controller in controllerNames)
                {
                    modelContent.Controllers[controller].Armature = skeleton.Name;
                }

                modelContent.SkinningInfo.Add(
                    skeleton.Name,
                    new SkinningContent
                    {
                        Skeleton = skeleton,
                        Controllers = controllerNames,
                    });
            }
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
                if (!modelContent.Lights.ContainsKey(lightName))
                {
                    continue;
                }

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
            if (geometry?.Any() != true)
            {
                return;
            }

            if (trn.IsIdentity)
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
        /// Processs the specified node list, looking for skeletons
        /// </summary>
        /// <param name="nodes">Node list to process</param>
        /// <param name="skeletons">Returns a skeleton list, if any</param>
        private static void ProcessSceneNodesSkeleton(IEnumerable<Node> nodes, out IEnumerable<Skeleton> skeletons)
        {
            if (nodes?.Any() != true)
            {
                //No child nodes
                skeletons = Enumerable.Empty<Skeleton>();

                return;
            }

            List<Skeleton> res = new List<Skeleton>();

            foreach (var node in nodes)
            {
                ProcessSceneNodeSkeleton(node, out var nodeSkeletons);
                if (nodeSkeletons.Any())
                {
                    res.AddRange(nodeSkeletons);
                }
            }

            skeletons = res.ToArray();
        }
        /// <summary>
        /// Process the specified node, lookgin for a skeleton
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="skeletons">Returns a skeleton list, if any</param>
        private static void ProcessSceneNodeSkeleton(Node node, out IEnumerable<Skeleton> skeletons)
        {
            if (!node.IsArmature)
            {
                List<Skeleton> res = new List<Skeleton>();

                ProcessSceneNodesSkeleton(node.Nodes, out var childSkeletons);
                if (childSkeletons.Any())
                {
                    res.AddRange(childSkeletons);
                }

                skeletons = res.ToArray();

                return;
            }

            skeletons = new[] { CreateSkeleton(node) };
        }
        /// <summary>
        /// Process the specified node list, looking for instance controllers
        /// </summary>
        /// <param name="nodes">Node list to process</param>
        /// <param name="instanceControllers">Returns a instance controller list, if any</param>
        private static void ProcessSceneNodesInstanceController(IEnumerable<Node> nodes, out IEnumerable<InstanceController> instanceControllers)
        {
            if (nodes?.Any() != true)
            {
                //No child nodes
                instanceControllers = Enumerable.Empty<InstanceController>();

                return;
            }

            List<InstanceController> res = new List<InstanceController>();

            foreach (var node in nodes)
            {
                ProcessSceneNodeInstanceController(node, out var nodeInstanceControllers);
                if (nodeInstanceControllers.Any())
                {
                    res.AddRange(nodeInstanceControllers);
                }
            }

            instanceControllers = res.ToArray();
        }
        /// <summary>
        /// Process the specified node, lookgin for a instance controller
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="instanceControllers">Returns a instance controller list, if any</param>
        private static void ProcessSceneNodeInstanceController(Node node, out IEnumerable<InstanceController> instanceControllers)
        {
            if (!node.HasController)
            {
                List<InstanceController> res = new List<InstanceController>();

                ProcessSceneNodesInstanceController(node.Nodes, out var childInstanceControllers);
                if (childInstanceControllers.Any())
                {
                    res.AddRange(childInstanceControllers);
                }

                instanceControllers = res.ToArray();

                return;
            }

            instanceControllers = new[] { node.InstanceController.First() };
        }
        /// <summary>
        /// Creates a skeleton from an armature node
        /// </summary>
        /// <param name="armatureNode">Armature node</param>
        /// <returns>Returns the resulting skeleton</returns>
        private static Skeleton CreateSkeleton(Node armatureNode)
        {
            if (armatureNode == null)
            {
                return null;
            }

            var root = ProcessJoints(null, armatureNode);

            return new Skeleton(armatureNode.Id, root);
        }

        #endregion

        #region Armatures

        /// <summary>
        /// Process skeleton
        /// </summary>
        /// <param name="parent">Parent joint</param>
        /// <param name="node">Armature node</param>
        /// <returns>Return skeleton joint hierarchy</returns>
        private static Joint ProcessJoints(Joint parent, Node node)
        {
            Matrix localTransform = node.ReadMatrix();

            Joint jt = new Joint(node.Id, node.SId, parent, localTransform, Matrix.Identity);

            Skeleton.UpdateToWorldTransform(jt);

            if (node.Nodes?.Any() != true)
            {
                return jt;
            }

            List<Joint> childs = new List<Joint>();

            foreach (var child in node.Nodes)
            {
                childs.Add(ProcessJoints(jt, child));
            }

            jt.Childs = childs.ToArray();

            return jt;
        }

        #endregion
    }
}
