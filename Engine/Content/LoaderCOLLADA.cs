using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using SharpDX.Direct3D;

namespace Engine.Content
{
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
        /// <param name="upAxis">Up axis</param>
        /// <returns>Returns de content loaded</returns>
        public static ModelContent Load(string contentFolder, string fileName, EnumAxis upAxis = EnumAxis.YUp)
        {
            return Load(contentFolder, fileName, Matrix.Identity, upAxis);
        }
        /// <summary>
        /// Load a collada model
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="fileName">Collada model</param>
        /// <param name="transform">Global geometry transform</param>
        /// <param name="upAxis">Up axis</param>
        /// <returns>Returns de content loaded</returns>
        public static ModelContent Load(string contentFolder, string fileName, Matrix transform, EnumAxis upAxis = EnumAxis.YUp)
        {
            string[] modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList != null && modelList.Length == 1)
            {
                COLLADA dae = COLLADA.Load(modelList[0]);

                EnumAxisConversion conversion = GetAxisConversion(dae.Asset.UpAxis, upAxis);

                ModelContent modelContent = new ModelContent();

                #region Scene Objects

                ProcessLibraryImages(dae, modelContent, contentFolder);
                ProcessLibraryMaterial(dae, modelContent);

                ProcessLibraryGeometries(dae, modelContent, conversion, transform);
                ProcessLibraryControllers(dae, modelContent, conversion, transform);
                ProcessLibraryAnimations(dae, modelContent, conversion, transform);

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
                                #region Armatures (Skeletons)

                                if (node.IsArmature)
                                {
                                    if (skeleton != null)
                                    {
                                        throw new Exception("Only one armature definition per file!");
                                    }

                                    //TODO: Where to apply this transform?
                                    //Transforms trn = node.ReadTransforms();

                                    if (node.Nodes != null && node.Nodes.Length > 0)
                                    {
                                        Joint root = ProcessJoints(null, node.Nodes[0], conversion, transform);

                                        skeleton = new Skeleton(root);
                                    }
                                }

                                #endregion

                                #region Geometry nodes

                                if (node.HasGeometry)
                                {
                                    //Transforms trn = node.ReadTransforms();

                                    MeshContent info = new MeshContent()
                                    {
                                        //TODO: Where to apply this transform?
                                        //Transform = trn.Matrix.ChangeAxis(conversion),
                                    };

                                    if (node.InstanceGeometry != null && node.InstanceGeometry.Length > 0)
                                    {
                                        List<string> meshList = new List<string>();

                                        foreach (InstanceGeometry ig in node.InstanceGeometry)
                                        {
                                            meshList.Add(ig.Url.Replace("#", ""));
                                        }

                                        info.SubMeshes = meshList.ToArray();
                                    }
                                }

                                #endregion

                                #region Controllers

                                if (node.HasController)
                                {
                                    //TODO: Where to apply this transform?
                                    //Transforms trn = node.ReadTransforms();

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

                return modelContent;
            }
            else
            {
                throw new Exception(string.Format("Model not found: {0}", fileName));
            }
        }
        /// <summary>
        /// Axis conversion selection
        /// </summary>
        /// <param name="currentUpAxis">Current up axis</param>
        /// <param name="newUpAxis">Desired up axis</param>
        /// <returns>Returns the conversion to perform</returns>
        public static EnumAxisConversion GetAxisConversion(EnumAxis currentUpAxis, EnumAxis newUpAxis)
        {
            if (currentUpAxis == EnumAxis.XUp && newUpAxis == EnumAxis.YUp)
            {
                return EnumAxisConversion.XtoY;
            }
            else if (currentUpAxis == EnumAxis.XUp && newUpAxis == EnumAxis.ZUp)
            {
                return EnumAxisConversion.XtoZ;
            }
            else if (currentUpAxis == EnumAxis.YUp && newUpAxis == EnumAxis.XUp)
            {
                return EnumAxisConversion.YtoX;
            }
            else if (currentUpAxis == EnumAxis.YUp && newUpAxis == EnumAxis.ZUp)
            {
                return EnumAxisConversion.YtoZ;
            }
            else if (currentUpAxis == EnumAxis.ZUp && newUpAxis == EnumAxis.XUp)
            {
                return EnumAxisConversion.ZtoX;
            }
            else if (currentUpAxis == EnumAxis.ZUp && newUpAxis == EnumAxis.YUp)
            {
                return EnumAxisConversion.ZtoY;
            }
            else
            {
                return EnumAxisConversion.None;
            }
        }

        #region Dictionary loaders

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
                    ImageContent info = new ImageContent();

                    if (image.Data != null)
                    {
                        info.Stream = new MemoryStream((byte[])image.Data);
                    }
                    else if (!string.IsNullOrEmpty(image.InitFrom))
                    {
                        info.Paths = ContentManager.FindContent(contentFolder, Uri.UnescapeDataString(image.InitFrom));
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
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Transformation</param>
        public static void ProcessLibraryGeometries(COLLADA dae, ModelContent modelContent, EnumAxisConversion conversion, Matrix transform)
        {
            if (dae.LibraryGeometries != null && dae.LibraryGeometries.Length > 0)
            {
                foreach (Geometry geometry in dae.LibraryGeometries)
                {
                    SubMeshContent[] info = ProcessGeometry(geometry, conversion, transform);
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
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Transformation</param>
        public static void ProcessLibraryControllers(COLLADA dae, ModelContent modelContent, EnumAxisConversion conversion, Matrix transform)
        {
            if (dae.LibraryControllers != null && dae.LibraryControllers.Length > 0)
            {
                foreach (Controller controller in dae.LibraryControllers)
                {
                    ControllerContent info = ProcessController(controller, conversion, transform);
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
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Transformation</param>
        public static void ProcessLibraryAnimations(COLLADA dae, ModelContent modelContent, EnumAxisConversion conversion, Matrix transform)
        {
            if (dae.LibraryAnimations != null && dae.LibraryAnimations.Length > 0)
            {
                for (int i = 0; i < dae.LibraryAnimations.Length; i++)
                {
                    Animation animation = dae.LibraryAnimations[i];

                    AnimationContent[] info = ProcessAnimation(animation, conversion, i == 0 ? transform : Matrix.Identity);
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
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessGeometry(Geometry geometry, EnumAxisConversion conversion, Matrix transform)
        {
            SubMeshContent[] info = null;

            if (geometry.Mesh != null)
            {
                info = ProcessMesh(geometry.Mesh, conversion, transform);
            }
            else if (geometry.Spline != null)
            {
                info = ProcessSpline(geometry.Spline, conversion, transform);
            }
            else if (geometry.ConvexMesh != null)
            {
                info = ProcessConvexMesh(geometry.ConvexMesh, conversion, transform);
            }

            return info;
        }
        /// <summary>
        /// Process mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessMesh(Collada.Mesh mesh, EnumAxisConversion conversion, Matrix transform)
        {
            SubMeshContent[] res = null;

            //Procesar por topología
            if (mesh.Lines != null && mesh.Lines.Length > 0)
            {
                res = ProcessLines(mesh.Lines, mesh.Sources, conversion, transform);
            }
            else if (mesh.LineStrips != null && mesh.LineStrips.Length > 0)
            {
                res = ProcessLineStrips(mesh.LineStrips, mesh.Sources, conversion, transform);
            }
            else if (mesh.Triangles != null && mesh.Triangles.Length > 0)
            {
                res = ProcessTriangles(mesh.Triangles, mesh.Sources, conversion, transform);
            }
            else if (mesh.TriFans != null && mesh.TriFans.Length > 0)
            {
                res = ProcessTriFans(mesh.TriFans, mesh.Sources, conversion, transform);
            }
            else if (mesh.TriStrips != null && mesh.TriStrips.Length > 0)
            {
                res = ProcessTriStrips(mesh.TriStrips, mesh.Sources, conversion, transform);
            }
            else if (mesh.PolyList != null && mesh.PolyList.Length > 0)
            {
                res = ProcessPolyList(mesh.PolyList, mesh.Sources, conversion, transform);
            }
            else if (mesh.Polygons != null && mesh.Polygons.Length > 0)
            {
                res = ProcessPolygons(mesh.Polygons, mesh.Sources, conversion, transform);
            }

            return res;
        }
        /// <summary>
        /// Process spline
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessSpline(Spline spline, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process convex mesh
        /// </summary>
        /// <param name="mesh">Mesh</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessConvexMesh(ConvexMesh convexMesh, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process lines
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessLines(Lines[] lines, Source[] meshSources, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process line strips
        /// </summary>
        /// <param name="lines">Line strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessLineStrips(LineStrips[] lines, Source[] meshSources, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangles
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriangles(Triangles[] triangles, Source[] meshSources, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangle fans
        /// </summary>
        /// <param name="triFans">Triangle fans</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriFans(TriFans[] triFans, Source[] meshSources, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process triangle strips
        /// </summary>
        /// <param name="triStrips">Triangle strips</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessTriStrips(TriStrips[] triStrips, Source[] meshSources, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process polygon list
        /// </summary>
        /// <param name="polyLists">Polygon list</param>
        /// <param name="meshSources">Mesh sources</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessPolyList(PolyList[] polyLists, Source[] meshSources, EnumAxisConversion conversion, Matrix transform)
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
                            Vector3 pos = positions[vIndex].ChangeTransformAxis(conversion);

                            vert.VertexIndex = vIndex;
                            vert.Position = transform.IsIdentity ? pos : Vector3.TransformCoordinate(pos, transform);
                        }

                        if (normalInput != null)
                        {
                            int nIndex = polyList.P[index + normalInput.Offset];
                            Vector3 nor = normals[nIndex].ChangeTransformAxis(conversion);

                            vert.Normal = transform.IsIdentity ? nor : Vector3.TransformNormal(nor, transform);
                        }

                        if (texCoordInput != null)
                        {
                            int tIndex = polyList.P[index + texCoordInput.Offset];
                            Vector2 tex = texCoords[tIndex];

                            vert.Texture = tex;
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
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns sub mesh content</returns>
        private static SubMeshContent[] ProcessPolygons(Polygons[] polygons, Source[] meshSources, EnumAxisConversion conversion, Matrix transform)
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
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessController(Controller controller, EnumAxisConversion conversion, Matrix transform)
        {
            ControllerContent res = null;

            if (controller.Skin != null)
            {
                res = ProcessSkin(controller.Name, controller.Skin, conversion, transform);
            }
            else if (controller.Morph != null)
            {
                res = ProcessMorph(controller.Name, controller.Morph, conversion, transform);
            }

            return res;
        }
        /// <summary>
        /// Process skin
        /// </summary>
        /// <param name="name">Skin name</param>
        /// <param name="skin">Skin information</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessSkin(string name, Skin skin, EnumAxisConversion conversion, Matrix transform)
        {
            ControllerContent res = new ControllerContent();

            res.BindShapeMatrix = Matrix.Transpose(skin.BindShapeMatrix.ToMatrix().ChangeAxis(conversion));

            res.Skin = skin.SourceUri.Replace("#", "");
            res.Armature = name;

            if (skin.VertexWeights != null)
            {
                Dictionary<string, Matrix> ibmList;
                Weight[] wgList;
                ProcessVertexWeights(skin, conversion, transform, out ibmList, out wgList);

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
        /// <param name="conversion">Axis conversion</param>
        /// <param name="transform">Initial transformation</param>
        /// <returns>Returns controller content</returns>
        private static ControllerContent ProcessMorph(string name, Morph morph, EnumAxisConversion conversion, Matrix transform)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Process vertext weight information
        /// </summary>
        /// <param name="skin">Skin information</param>
        /// <param name="conversion">Axis conversion</param>
        /// <param name="inverseBindMatrixList">Inverse bind matrix list result</param>
        /// <param name="weightList">Weight list result</param>
        private static void ProcessVertexWeights(Skin skin, EnumAxisConversion conversion, Matrix transform, out Dictionary<string, Matrix> inverseBindMatrixList, out Weight[] weightList)
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
                    ibmList.Add(joints[i], Matrix.Transpose(mats[i].ChangeAxis(conversion)));
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
        /// <param name="conversion">Axis conversion</param>
        /// <returns>Retuns animation content list</returns>
        private static AnimationContent[] ProcessAnimation(Animation animation, EnumAxisConversion conversion, Matrix transform)
        {
            List<AnimationContent> res = new List<AnimationContent>();

            foreach (Channel channel in animation.Channels)
            {
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
                            Transform = Matrix.Transpose(outputs[i].ChangeAxis(conversion)),
                            Interpolation = interpolations[i],
                        };

                        keyframes.Add(keyframe);
                    }

                    AnimationContent info = new AnimationContent()
                    {
                        Target = channel.Target,
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
        /// <param name="parent">Parent joint</param>
        /// <param name="node">Armature node</param>
        /// <param name="conversion">Axis conversion</param>
        /// <returns>Return skeleton joint hierarchy</returns>
        private static Joint ProcessJoints(Joint parent, Node node, EnumAxisConversion conversion, Matrix transform)
        {
            Matrix parentMatrix = (parent != null ? parent.Local : Matrix.Identity);
            Matrix nodeMatrix = Matrix.Transpose(node.ReadMatrix().ChangeAxis(conversion));

            Joint jt = new Joint()
            {
                Name = node.SId,
                Parent = parent,
                World = nodeMatrix,
                Local = nodeMatrix * parentMatrix,
            };

            if (node.Nodes != null && node.Nodes.Length > 0)
            {
                List<Joint> childs = new List<Joint>();

                foreach (Node child in node.Nodes)
                {
                    childs.Add(ProcessJoints(jt, child, conversion, Matrix.Identity));
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
