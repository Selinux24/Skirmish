using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine.Content
{
    using Engine.Animation;
    using Engine.Collada;
    using Engine.Collada.FX;
    using Engine.Collada.Types;
    using Engine.Common;

    /// <summary>
    /// Loader for collada
    /// </summary>
    public static class LoaderCOLLADA
    {
        /// <summary>
        /// Load a collada model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">Collada model</param>
        /// <param name="coordinate">Coordinate system</param>
        /// <param name="orientation">Up axis orientation</param>
        /// <returns>Returns the loaded contents</returns>
        public static ModelContent[] Load(string contentFolder, string fileName, CoordinateSystems coordinate = CoordinateSystems.LeftHanded, GeometryOrientations orientation = GeometryOrientations.YUp)
        {
            return Load(contentFolder, fileName, Matrix.Identity, coordinate, orientation);
        }
        /// <summary>
        /// Load a collada model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">Collada model</param>
        /// <param name="transform">Global geometry transform</param>
        /// <param name="coordinate">Coordinate system</param>
        /// <param name="orientation">Up axis orientation</param>
        /// <returns>Returns the loaded contents</returns>
        public static ModelContent[] Load(string contentFolder, string fileName, Matrix transform, CoordinateSystems coordinate = CoordinateSystems.LeftHanded, GeometryOrientations orientation = GeometryOrientations.YUp)
        {
            MemoryStream[] modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList != null && modelList.Length > 0)
            {
                ModelContent[] res = new ModelContent[modelList.Length];

                for (int i = 0; i < modelList.Length; i++)
                {
                    COLLADA dae = COLLADA.Load(modelList[i]);

                    GeometryOrientations daeUp = GeometryOrientations.YUp;
                    if (dae.Asset.UpAxis == EnumAxis.XUp) daeUp = GeometryOrientations.XUp;
                    else if (dae.Asset.UpAxis == EnumAxis.YUp) daeUp = GeometryOrientations.YUp;
                    else if (dae.Asset.UpAxis == EnumAxis.ZUp) daeUp = GeometryOrientations.ZUp;

                    LoaderConversion conversion = LoaderConversion.Compute(
                        transform,
                        CoordinateSystems.RightHanded,
                        coordinate,
                        daeUp,
                        orientation);

                    ModelContent modelContent = new ModelContent();

                    #region Scene Objects

                    ProcessLibraryLights(dae, modelContent);

                    ProcessLibraryImages(dae, modelContent, contentFolder);
                    ProcessLibraryMaterial(dae, modelContent);

                    ProcessLibraryGeometries(dae, modelContent, conversion);
                    ProcessLibraryControllers(dae, modelContent, conversion);

                    #endregion

                    #region Scene Relations

                    if (dae.Scene.InstanceVisualScene != null)
                    {
                        Skeleton skeleton = null;
                        List<string> controllers = new List<string>();

                        string sceneUrl = dae.Scene.InstanceVisualScene.Url;

                        VisualScene vScene = Array.Find(dae.LibraryVisualScenes, l => string.Equals("#" + l.Id, sceneUrl));
                        if (vScene != null)
                        {
                            if (vScene.Nodes != null && vScene.Nodes.Length > 0)
                            {
                                foreach (Node node in vScene.Nodes)
                                {
                                    #region Lights

                                    if (node.IsLight)
                                    {
                                        Matrix trn = node.ReadMatrix();


                                    }

                                    #endregion

                                    #region Armatures (Skeletons)

                                    if (node.IsArmature)
                                    {
                                        if (skeleton != null)
                                        {
                                            throw new Exception("Only one armature definition per file!");
                                        }

                                        Matrix trn = Matrix.Identity;
                                        if (node.Matrix != null)
                                        {
                                            trn = node.ReadMatrix();
                                        }
                                        else
                                        {
                                            trn = node.ReadTransforms().Matrix;
                                        }

                                        trn = Matrix.Transpose(conversion.ChangeGeometryOrientation(trn));

                                        if (node.Nodes != null && node.Nodes.Length > 0)
                                        {
                                            Joint root = ProcessJoints(trn, null, node.Nodes[0], conversion);

                                            skeleton = new Skeleton(root);
                                        }
                                    }

                                    #endregion

                                    #region Geometry nodes

                                    if (node.HasGeometry)
                                    {
                                        Matrix trn = Matrix.Identity;
                                        if (node.Matrix != null)
                                        {
                                            trn = node.ReadMatrix();
                                        }
                                        else
                                        {
                                            trn = node.ReadTransforms().Matrix;
                                        }

                                        trn = Matrix.Transpose(conversion.ChangeGeometryOrientation(trn));

                                        if (!trn.IsIdentity)
                                        {
                                            if (node.InstanceGeometry != null && node.InstanceGeometry.Length > 0)
                                            {
                                                foreach (InstanceGeometry ig in node.InstanceGeometry)
                                                {
                                                    string meshName = ig.Url.Replace("#", "");

                                                    foreach (var submesh in modelContent.Geometry[meshName].Values)
                                                    {
                                                        for (int v = 0; v < submesh.Vertices.Length; v++)
                                                        {
                                                            submesh.Vertices[v].Transform(trn);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Controllers

                                    if (node.HasController)
                                    {
                                        //TODO: Where to apply this transform?
                                        Matrix trn = Matrix.Identity;
                                        if (node.Matrix != null)
                                        {
                                            trn = node.ReadMatrix();
                                        }
                                        else
                                        {
                                            trn = node.ReadTransforms().Matrix;
                                        }

                                        trn = Matrix.Transpose(conversion.ChangeGeometryOrientation(trn));

                                        if (node.InstanceController != null && node.InstanceController.Length > 0)
                                        {
                                            foreach (InstanceController ic in node.InstanceController)
                                            {
                                                string controllerName = ic.Url.Replace("#", "");

                                                controllers.Add(controllerName);
                                            }
                                        }
                                    }

                                    #endregion
                                }
                            }
                        }

                        if (skeleton != null && controllers.Count > 0)
                        {
                            modelContent.SkinningInfo = new SkinningContent()
                            {
                                Controller = controllers.ToArray(),
                                Skeleton = skeleton,
                            };
                        }
                    }

                    #endregion

                    #region Animations

                    ProcessLibraryAnimations(dae, modelContent, conversion);

                    #endregion

                    res[i] = modelContent;
                }

                return res;
            }
            else
            {
                throw new Exception(string.Format("Model not found: {0}", fileName));
            }
        }

        #region Dictionary loaders

        /// <summary>
        /// Process lightd
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        public static void ProcessLibraryLights(COLLADA dae, ModelContent modelContent)
        {
            if (dae.LibraryLights != null && dae.LibraryLights.Length > 0)
            {
                foreach (Light light in dae.LibraryLights)
                {
                    LightContent info = null;

                    var dirTechnique = Array.Find(light.LightTechniqueCommon, l => l.Directional != null);
                    if (dirTechnique != null)
                    {
                        info = new LightContent()
                        {
                            LightType = LightContentTypeEnum.Directional,
                            Color = dirTechnique.Directional.Color.ToColor4(),
                        };
                    }

                    var pointTechnique = Array.Find(light.LightTechniqueCommon, l => l.Point != null);
                    if (pointTechnique != null)
                    {
                        info = new LightContent()
                        {
                            LightType = LightContentTypeEnum.Point,
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
                            LightType = LightContentTypeEnum.Spot,
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
        public static void ProcessLibraryImages(COLLADA dae, ModelContent modelContent, string contentFolder)
        {
            if (dae.LibraryImages != null && dae.LibraryImages.Length > 0)
            {
                foreach (Image image in dae.LibraryImages)
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
        public static void ProcessLibraryMaterial(COLLADA dae, ModelContent modelContent)
        {
            if (dae.LibraryMaterials != null && dae.LibraryMaterials.Length > 0 &&
                dae.LibraryEffects != null && dae.LibraryEffects.Length > 0)
            {
                foreach (Collada.Material material in dae.LibraryMaterials)
                {
                    MaterialContent info = MaterialContent.Default;

                    //Find effect
                    Effect effect = Array.Find(dae.LibraryEffects, e => e.Id == material.InstanceEffect.Url.Replace("#", ""));
                    if (effect != null)
                    {
                        if (effect.ProfileCG != null)
                        {
                            throw new NotImplementedException();
                        }
                        else if (effect.ProfileGLES != null)
                        {
                            throw new NotImplementedException();
                        }
                        else if (effect.ProfileGLSL != null)
                        {
                            throw new NotImplementedException();
                        }
                        else if (effect.ProfileCOMMON != null)
                        {
                            info = ProcessTechniqueFX(effect.ProfileCOMMON);
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
        /// <param name="conversion">Conversion</param>
        public static void ProcessLibraryGeometries(COLLADA dae, ModelContent modelContent, LoaderConversion conversion)
        {
            if (dae.LibraryGeometries != null && dae.LibraryGeometries.Length > 0)
            {
                foreach (Geometry geometry in dae.LibraryGeometries)
                {
                    SubMeshContent[] info = ProcessGeometry(geometry, conversion);
                    if (info != null && info.Length > 0)
                    {
                        foreach (SubMeshContent subMesh in info)
                        {
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
        /// <param name="conversion">Conversion</param>
        public static void ProcessLibraryControllers(COLLADA dae, ModelContent modelContent, LoaderConversion conversion)
        {
            if (dae.LibraryControllers != null && dae.LibraryControllers.Length > 0)
            {
                foreach (Controller controller in dae.LibraryControllers)
                {
                    ControllerContent info = ProcessController(controller, conversion);
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
        /// <param name="conversion">Conversion</param>
        public static void ProcessLibraryAnimations(COLLADA dae, ModelContent modelContent, LoaderConversion conversion)
        {
            if (dae.LibraryAnimations != null && dae.LibraryAnimations.Length > 0)
            {
                for (int i = 0; i < dae.LibraryAnimations.Length; i++)
                {
                    Animation animation = dae.LibraryAnimations[i];

                    AnimationContent[] info = ProcessAnimation(modelContent, animation, conversion);
                    if (info != null && info.Length > 0)
                    {
                        modelContent.Animations[animation.Id] = info;
                    }
                }
            }
        }

        #endregion

        #region Geometry

        /// <summary>
        /// Process geometry list
        /// </summary>
        /// <param name="geometry">Geometry info</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessGeometry(Geometry geometry, LoaderConversion conversion)
        {
            SubMeshContent[] info = null;

            if (geometry.Mesh != null)
            {
                info = ProcessMesh(geometry.Mesh, conversion);
            }
            else if (geometry.Spline != null)
            {
                info = ProcessSpline(geometry.Spline, conversion);
            }
            else if (geometry.ConvexMesh != null)
            {
                info = ProcessConvexMesh(geometry.ConvexMesh, conversion);
            }

            return info;
        }
        /// <summary>
        /// Process mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessMesh(Collada.Mesh mesh, LoaderConversion conversion)
        {
            SubMeshContent[] res = null;

            //Procesar por topología
            if (mesh.Lines != null && mesh.Lines.Length > 0)
            {
                res = ProcessLines(mesh.Lines, mesh.Sources, conversion);
            }
            else if (mesh.LineStrips != null && mesh.LineStrips.Length > 0)
            {
                res = ProcessLineStrips(mesh.LineStrips, mesh.Sources, conversion);
            }
            else if (mesh.Triangles != null && mesh.Triangles.Length > 0)
            {
                res = ProcessTriangles(mesh.Triangles, mesh.Sources, conversion);
            }
            else if (mesh.TriFans != null && mesh.TriFans.Length > 0)
            {
                res = ProcessTriFans(mesh.TriFans, mesh.Sources, conversion);
            }
            else if (mesh.TriStrips != null && mesh.TriStrips.Length > 0)
            {
                res = ProcessTriStrips(mesh.TriStrips, mesh.Sources, conversion);
            }
            else if (mesh.PolyList != null && mesh.PolyList.Length > 0)
            {
                res = ProcessPolyList(mesh.PolyList, mesh.Sources, conversion);
            }
            else if (mesh.Polygons != null && mesh.Polygons.Length > 0)
            {
                res = ProcessPolygons(mesh.Polygons, mesh.Sources, conversion);
            }

            return res;
        }
        /// <summary>
        /// Process spline
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessSpline(Spline spline, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process convex mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessConvexMesh(ConvexMesh convexMesh, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process lines
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessLines(Lines[] lines, Source[] meshSources, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process line strips
        /// </summary>
        /// <param name="lines">Line strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessLineStrips(LineStrips[] lines, Source[] meshSources, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangles
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriangles(Triangles[] triangles, Source[] meshSources, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangle fans
        /// </summary>
        /// <param name="triFans">Triangle fans</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriFans(TriFans[] triFans, Source[] meshSources, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangle strips
        /// </summary>
        /// <param name="triStrips">Triangle strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriStrips(TriStrips[] triStrips, Source[] meshSources, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process polygon list
        /// </summary>
        /// <param name="polyLists">Polygon list</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessPolyList(PolyList[] polyLists, Source[] meshSources, LoaderConversion conversion)
        {
            List<SubMeshContent> res = new List<SubMeshContent>();

            int sourceCount = meshSources.Length;

            foreach (PolyList polyList in polyLists)
            {
                List<VertexData> verts = new List<VertexData>();

                VertexTypes vertexType = EnumerateSemantics(polyList.Inputs);

                Input vertexInput = polyList[EnumSemantics.Vertex];
                Input normalInput = polyList[EnumSemantics.Normal];
                Input texCoordInput = polyList[EnumSemantics.TexCoord];

                Vector3[] positions = vertexInput != null ? meshSources[vertexInput.Offset].ReadVector3() : null;
                Vector3[] normals = normalInput != null ? meshSources[normalInput.Offset].ReadVector3() : null;
                Vector2[] texCoords = texCoordInput != null ? meshSources[texCoordInput.Offset].ReadVector2() : null;

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

                        if (vertexInput != null)
                        {
                            int vIndex = polyList.P[index + vertexInput.Offset];
                            Vector3 pos = positions[vIndex];
                            pos = conversion.ChangeCoordinateSystem(pos);
                            pos = conversion.ChangeGeometryOrientation(pos);
                            pos = conversion.ApplyCoordinateTransform(pos);

                            vert.VertexIndex = vIndex;
                            vert.Position = pos;
                        }

                        if (normalInput != null)
                        {
                            int nIndex = polyList.P[index + normalInput.Offset];
                            Vector3 nor = normals[nIndex];
                            nor = conversion.ChangeCoordinateSystem(nor);
                            nor = conversion.ChangeGeometryOrientation(nor);
                            nor = conversion.ApplyNormalTransform(nor);

                            vert.Normal = nor;
                        }

                        if (texCoordInput != null)
                        {
                            int tIndex = polyList.P[index + texCoordInput.Offset];
                            Vector2 tex = texCoords[tIndex];

                            //Invert Vertical coordinate
                            tex.Y = -tex.Y;

                            vert.Texture0 = tex;
                        }

                        verts.Add(vert);

                        index += inputCount;
                    }
                }

                //From right handed to left handed
                for (int i = 0; i < verts.Count; i += 3)
                {
                    VertexData tmp = verts[i + 1];
                    verts[i + 1] = verts[i + 2];
                    verts[i + 2] = tmp;
                }

                SubMeshContent meshInfo = new SubMeshContent()
                {
                    Topology = PrimitiveTopology.TriangleList,
                    VertexType = vertexType,
                    Vertices = verts.ToArray(),
                    Material = polyList.Material,
                };

                res.Add(meshInfo);
            }

            return res.ToArray();
        }
        /// <summary>
        /// Process polygons
        /// </summary>
        /// <param name="polygons">Polygons</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessPolygons(Polygons[] polygons, Source[] meshSources, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Enumerate semantics
        /// </summary>
        /// <param name="inputs">Input list</param>
        /// <returns>Return vertex types of inputs</returns>
        private static VertexTypes EnumerateSemantics(Input[] inputs)
        {
            if (Array.Exists(inputs, i => i.Semantic == EnumSemantics.Vertex))
            {
                if (Array.Exists(inputs, i => i.Semantic == EnumSemantics.Normal))
                {
                    if (Array.Exists(inputs, i => i.Semantic == EnumSemantics.TexCoord))
                    {
                        if (Array.Exists(inputs, i => i.Semantic == EnumSemantics.Tangent))
                        {
                            return VertexTypes.PositionNormalTextureTangent;
                        }
                        else
                        {
                            return VertexTypes.PositionNormalTexture;
                        }
                    }
                    else
                    {
                        return VertexTypes.PositionNormalColor;
                    }
                }
                else
                {
                    if (Array.Exists(inputs, i => i.Semantic == EnumSemantics.TexCoord))
                    {
                        return VertexTypes.PositionTexture;
                    }
                    else
                    {
                        return VertexTypes.PositionColor;
                    }
                }
            }
            else
            {
                return VertexTypes.Unknown;
            }
        }

        #endregion

        #region Controllers

        /// <summary>
        /// Process controller
        /// </summary>
        /// <param name="controller">Controller</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessController(Controller controller, LoaderConversion conversion)
        {
            ControllerContent res = null;

            if (controller.Skin != null)
            {
                res = ProcessSkin(controller.Name, controller.Skin, conversion);
            }
            else if (controller.Morph != null)
            {
                res = ProcessMorph(controller.Name, controller.Morph, conversion);
            }

            return res;
        }
        /// <summary>
        /// Process skin
        /// </summary>
        /// <param name="name">Skin name</param>
        /// <param name="skin">Skin information</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessSkin(string name, Skin skin, LoaderConversion conversion)
        {
            ControllerContent res = new ControllerContent();

            res.BindShapeMatrix = Matrix.Transpose(conversion.ChangeGeometryOrientation(skin.BindShapeMatrix.ToMatrix()));
            res.Skin = skin.SourceUri.Replace("#", "");
            res.Armature = name;

            if (skin.VertexWeights != null)
            {
                Dictionary<string, Matrix> ibmList;
                Weight[] wgList;
                ProcessVertexWeights(skin, conversion, out ibmList, out wgList);

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
        /// <param name="conversion">Conversion</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessMorph(string name, Morph morph, LoaderConversion conversion)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process vertext weight information
        /// </summary>
        /// <param name="skin">Skin information</param>
        /// <param name="conversion">Conversion</param>
        /// <param name="inverseBindMatrixList">Inverse bind matrix list result</param>
        /// <param name="weightList">Weight list result</param>
        private static void ProcessVertexWeights(Skin skin, LoaderConversion conversion, out Dictionary<string, Matrix> inverseBindMatrixList, out Weight[] weightList)
        {
            Dictionary<string, Matrix> ibmList = new Dictionary<string, Matrix>();
            List<Weight> wgList = new List<Weight>();

            int jointsOffset = -1;
            int bindsOffset = -1;
            int weightsOffset = -1;

            string[] joints = null;
            Matrix[] mats = null;
            float[] weights = null;

            Input jointsInput = skin.VertexWeights[EnumSemantics.Joint];
            if (jointsInput != null)
            {
                jointsOffset = jointsInput.Offset;
                bindsOffset = jointsInput.Offset;

                //Joint names
                Input jInput = skin.Joints[EnumSemantics.Joint];
                if (jInput != null)
                {
                    joints = skin[jInput.Source].ReadString();
                }

                //Inverse bind matrix for each joint
                Input mInput = skin.Joints[EnumSemantics.InverseBindMatrix];
                if (mInput != null)
                {
                    mats = skin[mInput.Source].ReadMatrix();
                }
            }

            //Weights
            Input weightsInput = skin.VertexWeights[EnumSemantics.Weight];
            if (weightsInput != null)
            {
                weightsOffset = weightsInput.Offset;

                weights = skin[weightsInput.Source].ReadFloat();
            }

            int index = 0;
            int sources = skin.VertexWeights.Inputs.Length;

            for (int i = 0; i < skin.VertexWeights.Count; i++)
            {
                int n = skin.VertexWeights.VCount[i];

                for (int v = 0; v < n; v++)
                {
                    Weight wg = new Weight()
                    {
                        VertexIndex = i,
                    };

                    if (jointsOffset >= 0)
                    {
                        wg.Joint = joints[skin.VertexWeights.V[index + jointsOffset]];
                    }

                    if (weightsOffset >= 0) wg.WeightValue = weights[skin.VertexWeights.V[index + weightsOffset]];

                    if (wg.WeightValue != 0.0f)
                    {
                        wgList.Add(wg);
                    }

                    index += sources;
                }
            }

            if (weightsOffset >= 0)
            {
                for (int i = 0; i < joints.Length; i++)
                {
                    ibmList.Add(joints[i], Matrix.Transpose(conversion.ChangeGeometryOrientation(mats[i])));
                }
            }

            inverseBindMatrixList = ibmList;
            weightList = wgList.ToArray();
        }

        #endregion

        #region Animation

        /// <summary>
        /// Process animation
        /// </summary>
        /// <param name="animation">Animation information</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Retuns animation content list</returns>
        private static AnimationContent[] ProcessAnimation(ModelContent modelContent, Animation animation, LoaderConversion conversion)
        {
            List<AnimationContent> res = new List<AnimationContent>();

            foreach (Channel channel in animation.Channels)
            {
                string jointName = channel.Target.Split("/".ToCharArray())[0];

                if (modelContent.SkinningInfo != null && modelContent.SkinningInfo.Skeleton != null)
                {
                    //Process only joints in the skeleton
                    Joint j = modelContent.SkinningInfo.Skeleton[jointName];
                    if (j == null) continue;
                }

                foreach (Sampler sampler in animation.Samplers)
                {
                    int inputOffset = -1;
                    int outputOffset = -1;
                    int interpolationOffset = -1;

                    float[] inputs = null;
                    Matrix[] outputs = null;
                    string[] interpolations = null;

                    //Keyframe times
                    Input inputsInput = sampler[EnumSemantics.Input];
                    if (inputsInput != null)
                    {
                        inputOffset = inputsInput.Offset;

                        inputs = animation[inputsInput.Source].ReadFloat();
                    }

                    //Keyframe transform matrix
                    Input outputsInput = sampler[EnumSemantics.Output];
                    if (outputsInput != null)
                    {
                        outputOffset = outputsInput.Offset;

                        outputs = animation[outputsInput.Source].ReadMatrix();
                    }

                    //Keyframe interpolation types
                    Input interpolationsInput = sampler[EnumSemantics.Interpolation];
                    if (interpolationsInput != null)
                    {
                        interpolationOffset = interpolationsInput.Offset;

                        interpolations = animation[interpolationsInput.Source].ReadString();
                    }

                    List<Keyframe> keyframes = new List<Keyframe>();

                    for (int i = 0; i < inputs.Length; i++)
                    {
                        Keyframe keyframe = new Keyframe()
                        {
                            Time = inputs[i],
                            Transform = Matrix.Transpose(conversion.ChangeGeometryOrientation(outputs[i])),
                            Interpolation = interpolations[i],
                        };

                        keyframes.Add(keyframe);
                    }

                    AnimationContent info = new AnimationContent()
                    {
                        Joint = jointName,
                        Keyframes = keyframes.ToArray(),
                    };

                    res.Add(info);
                }
            }

            return res.ToArray();
        }

        #endregion

        #region Armatures

        /// <summary>
        /// Process skeleton
        /// </summary>
        /// <param name="trn">Global controller transform</param>
        /// <param name="parent">Parent joint</param>
        /// <param name="node">Armature node</param>
        /// <param name="conversion">Conversion</param>
        /// <returns>Return skeleton joint hierarchy</returns>
        private static Joint ProcessJoints(Matrix trn, Joint parent, Node node, LoaderConversion conversion)
        {
            Matrix localTransform = Matrix.Transpose(conversion.ChangeGeometryOrientation(node.ReadMatrix()));
            Matrix globalTransform = parent != null ? parent.GlobalTransform * localTransform : trn * localTransform;

            Joint jt = new Joint(node.SId, parent, localTransform, globalTransform);

            if (node.Nodes != null && node.Nodes.Length > 0)
            {
                List<Joint> childs = new List<Joint>();

                foreach (Node child in node.Nodes)
                {
                    childs.Add(ProcessJoints(Matrix.Identity, jt, child, conversion));
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
        private static MaterialContent ProcessTechniqueFX(ProfileCOMMON profile)
        {
            TechniqueCOMMON technique = profile.Technique;

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
                BlinnPhong algorithm = technique.Blinn != null ? technique.Blinn : technique.Phong;

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
                reflective = algorithm.Reflective;
                reflectivity = algorithm.Reflectivity;
                transparent = algorithm.Transparent;
                transparency = algorithm.Transparency;
                indexOfRefraction = algorithm.IndexOfRefraction;
            }

            string emissionTexture = null;
            string ambientTexture = null;
            string diffuseTexture = null;
            string reflectiveTexture = null;
            string specularTexture = null;

            Color4 emissionColor = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            Color4 ambientColor = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            Color4 diffuseColor = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            Color4 reflectiveColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            Color4 specularColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            Color4 transparentColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);

            float indexOfRefractionValue = 1.0f;
            float reflectivityValue = 0.0f;
            float shininessValue = 50.0f;
            float transparencyValue = 0.0f;

            if (emission != null)
            {
                emissionTexture = FindTexture(profile, emission.Texture);
                emissionColor = emission.Texture != null ? emissionColor : emission.Color.ToColor4();
            }

            if (ambient != null)
            {
                ambientTexture = FindTexture(profile, ambient.Texture);
                ambientColor = ambient.Texture != null ? ambientColor : ambient.Color.ToColor4();
            }

            if (diffuse != null)
            {
                diffuseTexture = FindTexture(profile, diffuse.Texture);
                diffuseColor = diffuse.Texture != null ? diffuseColor : diffuse.Color.ToColor4();
            }

            if (reflective != null)
            {
                reflectiveTexture = FindTexture(profile, reflective.Texture);
                reflectiveColor = reflective.Texture != null ? reflectiveColor : reflective.Color.ToColor4();
            }

            if (specular != null)
            {
                specularTexture = FindTexture(profile, specular.Texture);
                specularColor = specular.Texture != null ? specularColor : specular.Color.ToColor4();
            }

            if (indexOfRefraction != null)
            {
                indexOfRefractionValue = indexOfRefraction.Float.Value;
            }

            if (reflectivity != null)
            {
                reflectivityValue = reflectivity.Float.Value;
            }

            if (shininess != null)
            {
                shininessValue = shininess.Float.Value;
            }

            if (transparency != null)
            {
                transparencyValue = transparency.Float.Value;
            }

            if (transparent != null)
            {
                transparentColor = transparent.Opaque == EnumOpaque.AlphaOne ? new Color4(0.0f, 0.0f, 0.0f, 1.0f) : new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            }

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
        /// Finds texture
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <param name="texture">Texture information</param>
        /// <returns>Returns texture name</returns>
        private static string FindTexture(ProfileCOMMON profile, BasicTexture texture)
        {
            if (texture != null)
            {
                Sampler2D sampler = profile[texture.Texture].Sampler2D;
                if (sampler != null)
                {
                    Surface surface = profile[sampler.Source].Surface;
                    if (surface != null)
                    {
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
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Finds bump map texture
        /// </summary>
        /// <param name="profile">Profile</param>
        /// <param name="technique">Technique information</param>
        /// <returns>Returns texture name</returns>
        private static string FindBumpMap(ProfileCOMMON profile, TechniqueCOMMON technique)
        {
            if (technique.Extras != null && technique.Extras.Length > 0)
            {
                for (int i = 0; i < technique.Extras.Length; i++)
                {
                    Technique[] techniques = technique.Extras[i].Techniques;
                    if (techniques != null && techniques.Length > 0)
                    {
                        for (int t = 0; t < techniques.Length; t++)
                        {
                            if (techniques[t].BumpMaps != null && techniques[t].BumpMaps.Length > 0)
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
    }
}
