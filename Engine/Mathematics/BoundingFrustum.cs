// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Defines a frustum which can be used in frustum culling, zoom to Extents (zoom to fit) operations, 
    /// (matrix, frustum, camera) interchange, and many kind of intersection testing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BoundingFrustum : IEquatable<BoundingFrustum>
    {
        private Matrix4x4 pMatrix;
        private Plane pNear;
        private Plane pFar;
        private Plane pLeft;
        private Plane pRight;
        private Plane pTop;
        private Plane pBottom;

        /// <summary>
        /// Gets or sets the Matrix that describes this bounding frustum.
        /// </summary>
        public Matrix4x4 Matrix
        {
            readonly get
            {
                return pMatrix;
            }
            set
            {
                pMatrix = value;
                GetPlanesFromMatrix(ref pMatrix, out pNear, out pFar, out pLeft, out pRight, out pTop, out pBottom);
            }
        }
        /// <summary>
        /// Gets the near plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Near => pNear;
        /// <summary>
        /// Gets the far plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Far => pFar;
        /// <summary>
        /// Gets the left plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Left => pLeft;
        /// <summary>
        /// Gets the right plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Right => pRight;
        /// <summary>
        /// Gets the top plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Top => pTop;
        /// <summary>
        /// Gets the bottom plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Bottom => pBottom;
        /// <summary>
        /// Indicate whether the current BoundingFrustrum is Orthographic.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the current BoundingFrustrum is Orthographic; otherwise, <c>false</c>.
        /// </value>
        public readonly bool IsOrthographic => (pLeft.Normal == -pRight.Normal) && (pTop.Normal == -pBottom.Normal);

        /// <summary>
        /// Creates a new instance of BoundingFrustum.
        /// </summary>
        /// <param name="matrix">Combined matrix that usually takes view × projection matrix.</param>
        public BoundingFrustum(Matrix4x4 matrix)
        {
            pMatrix = matrix;
            GetPlanesFromMatrix(ref pMatrix, out pNear, out pFar, out pLeft, out pRight, out pTop, out pBottom);
        }

        /// <summary>
        /// Returns one of the 6 planes related to this frustum.
        /// </summary>
        /// <param name="index">Plane index where 0 fro Left, 1 for Right, 2 for Top, 3 for Bottom, 4 for Near, 5 for Far</param>
        /// <returns></returns>
        public readonly Plane GetPlane(int index)
        {
            return index switch
            {
                0 => pLeft,
                1 => pRight,
                2 => pTop,
                3 => pBottom,
                4 => pNear,
                5 => pFar,
                _ => new Plane(),
            };
        }

        private static void GetPlanesFromMatrix(ref Matrix4x4 matrix, out Plane near, out Plane far, out Plane left, out Plane right, out Plane top, out Plane bottom)
        {
            //http://www.chadvernon.com/blog/resources/directx9/frustum-culling/

            // Left plane
            left.Normal.X = matrix.M14 + matrix.M11;
            left.Normal.Y = matrix.M24 + matrix.M21;
            left.Normal.Z = matrix.M34 + matrix.M31;
            left.D = matrix.M44 + matrix.M41;
            left.Normalize();

            // Right plane
            right.Normal.X = matrix.M14 - matrix.M11;
            right.Normal.Y = matrix.M24 - matrix.M21;
            right.Normal.Z = matrix.M34 - matrix.M31;
            right.D = matrix.M44 - matrix.M41;
            right.Normalize();

            // Top plane
            top.Normal.X = matrix.M14 - matrix.M12;
            top.Normal.Y = matrix.M24 - matrix.M22;
            top.Normal.Z = matrix.M34 - matrix.M32;
            top.D = matrix.M44 - matrix.M42;
            top.Normalize();

            // Bottom plane
            bottom.Normal.X = matrix.M14 + matrix.M12;
            bottom.Normal.Y = matrix.M24 + matrix.M22;
            bottom.Normal.Z = matrix.M34 + matrix.M32;
            bottom.D = matrix.M44 + matrix.M42;
            bottom.Normalize();

            // Near plane
            near.Normal.X = matrix.M13;
            near.Normal.Y = matrix.M23;
            near.Normal.Z = matrix.M33;
            near.D = matrix.M43;
            near.Normalize();

            // Far plane
            far.Normal.X = matrix.M14 - matrix.M13;
            far.Normal.Y = matrix.M24 - matrix.M23;
            far.Normal.Z = matrix.M34 - matrix.M33;
            far.D = matrix.M44 - matrix.M43;
            far.Normalize();
        }

        private static Vector3 Get3PlanesInterPoint(ref Plane p1, ref Plane p2, ref Plane p3)
        {
            //P = -d1 * N2xN3 / N1.N2xN3 - d2 * N3xN1 / N2.N3xN1 - d3 * N1xN2 / N3.N1xN2 
            var p1p2 = Vector3.Cross(p1.Normal, p2.Normal);
            var p2p3 = Vector3.Cross(p2.Normal, p3.Normal);
            var p3p1 = Vector3.Cross(p3.Normal, p1.Normal);
            return -p1.D * p2p3 / Vector3.Dot(p1.Normal, p2p3) - p2.D * p3p1 / Vector3.Dot(p2.Normal, p3p1) - p3.D * p1p2 / Vector3.Dot(p3.Normal, p1p2);
        }

        /// <summary>
        /// Creates a new frustum relaying on perspective camera parameters
        /// </summary>
        /// <param name="cameraPos">The camera pos.</param>
        /// <param name="lookDir">The look dir.</param>
        /// <param name="upDir">Up dir.</param>
        /// <param name="fov">The fov.</param>
        /// <param name="znear">The znear.</param>
        /// <param name="zfar">The zfar.</param>
        /// <param name="aspect">The aspect.</param>
        /// <returns>The bounding frustum calculated from perspective camera</returns>
        public static BoundingFrustum FromCamera(Vector3 cameraPos, Vector3 lookDir, Vector3 upDir, float fov, float znear, float zfar, float aspect)
        {
            //http://knol.google.com/k/view-frustum

            lookDir = Vector3.Normalize(lookDir);
            upDir = Vector3.Normalize(upDir);

            Vector3 nearCenter = cameraPos + lookDir * znear;
            Vector3 farCenter = cameraPos + lookDir * zfar;
            float nearHalfHeight = (float)(znear * Math.Tan(fov / 2f));
            float farHalfHeight = (float)(zfar * Math.Tan(fov / 2f));
            float nearHalfWidth = nearHalfHeight * aspect;
            float farHalfWidth = farHalfHeight * aspect;

            Vector3 rightDir = Vector3.Normalize(Vector3.Cross(upDir, lookDir));
            Vector3 Near1 = nearCenter - nearHalfHeight * upDir + nearHalfWidth * rightDir;
            Vector3 Near2 = nearCenter + nearHalfHeight * upDir + nearHalfWidth * rightDir;
            Vector3 Near3 = nearCenter + nearHalfHeight * upDir - nearHalfWidth * rightDir;
            Vector3 Near4 = nearCenter - nearHalfHeight * upDir - nearHalfWidth * rightDir;
            Vector3 Far1 = farCenter - farHalfHeight * upDir + farHalfWidth * rightDir;
            Vector3 Far2 = farCenter + farHalfHeight * upDir + farHalfWidth * rightDir;
            Vector3 Far3 = farCenter + farHalfHeight * upDir - farHalfWidth * rightDir;
            Vector3 Far4 = farCenter - farHalfHeight * upDir - farHalfWidth * rightDir;

            var result = new BoundingFrustum
            {
                pNear = new Plane(Near1, Near2, Near3),
                pFar = new Plane(Far3, Far2, Far1),
                pLeft = new Plane(Near4, Near3, Far3),
                pRight = new Plane(Far1, Far2, Near2),
                pTop = new Plane(Near2, Far2, Far3),
                pBottom = new Plane(Far4, Far1, Near1)
            };

            result.pNear.Normalize();
            result.pFar.Normalize();
            result.pLeft.Normalize();
            result.pRight.Normalize();
            result.pTop.Normalize();
            result.pBottom.Normalize();

            result.pMatrix =
                Matrix4x4.LookAtLH(cameraPos, cameraPos + lookDir * 10, upDir) *
                Matrix4x4.PerspectiveFovLH(fov, aspect, znear, zfar);

            return result;
        }
        /// <summary>
        /// Creates a new frustum relaying on perspective camera parameters
        /// </summary>
        /// <param name="cameraParams">The camera params.</param>
        /// <returns>The bounding frustum from camera params</returns>
        public static BoundingFrustum FromCamera(FrustumCameraParams cameraParams)
        {
            return FromCamera(
                cameraParams.Position,
                cameraParams.LookAtDir,
                cameraParams.UpDir,
                cameraParams.FOV,
                cameraParams.ZNear,
                cameraParams.ZFar,
                cameraParams.AspectRatio);
        }

        /// <summary>
        /// Returns the 8 corners of the frustum, element0 is Near1 (near right down corner)
        /// , element1 is Near2 (near right top corner)
        /// , element2 is Near3 (near Left top corner)
        /// , element3 is Near4 (near Left down corner)
        /// , element4 is Far1 (far right down corner)
        /// , element5 is Far2 (far right top corner)
        /// , element6 is Far3 (far left top corner)
        /// , element7 is Far4 (far left down corner)
        /// </summary>
        /// <returns>The 8 corners of the frustum</returns>
        public Vector3[] GetCorners()
        {
            var corners = new Vector3[8];
            GetCorners(corners);
            return corners;
        }
        /// <summary>
        /// Returns the 8 corners of the frustum, element0 is Near1 (near right down corner)
        /// , element1 is Near2 (near right top corner)
        /// , element2 is Near3 (near Left top corner)
        /// , element3 is Near4 (near Left down corner)
        /// , element4 is Far1 (far right down corner)
        /// , element5 is Far2 (far right top corner)
        /// , element6 is Far3 (far left top corner)
        /// , element7 is Far4 (far left down corner)
        /// </summary>
        /// <returns>The 8 corners of the frustum</returns>
        public void GetCorners(Vector3[] corners)
        {
            corners[0] = Get3PlanesInterPoint(ref pNear, ref pBottom, ref pRight);    //Near1
            corners[1] = Get3PlanesInterPoint(ref pNear, ref pTop, ref pRight);       //Near2
            corners[2] = Get3PlanesInterPoint(ref pNear, ref pTop, ref pLeft);        //Near3
            corners[3] = Get3PlanesInterPoint(ref pNear, ref pBottom, ref pLeft);     //Near3
            corners[4] = Get3PlanesInterPoint(ref pFar, ref pBottom, ref pRight);    //Far1
            corners[5] = Get3PlanesInterPoint(ref pFar, ref pTop, ref pRight);       //Far2
            corners[6] = Get3PlanesInterPoint(ref pFar, ref pTop, ref pLeft);        //Far3
            corners[7] = Get3PlanesInterPoint(ref pFar, ref pBottom, ref pLeft);     //Far3
        }

        /// <summary>
        /// Extracts perspective camera parameters from the frustum, doesn't work with orthographic frustums.
        /// </summary>
        /// <returns>Perspective camera parameters from the frustum</returns>
        public FrustumCameraParams GetCameraParams()
        {
            var position = Get3PlanesInterPoint(ref pRight, ref pTop, ref pLeft);
            var corners = GetCorners();

            return new()
            {
                Position = position,
                LookAtDir = pNear.Normal,
                UpDir = Vector3.Normalize(Vector3.Cross(pRight.Normal, pNear.Normal)),
                FOV = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal))) * 2),
                AspectRatio = (corners[6] - corners[5]).Length() / (corners[4] - corners[5]).Length(),
                ZNear = (position + (pNear.Normal * pNear.D)).Length(),
                ZFar = (position + (pFar.Normal * pFar.D)).Length()
            };
        }

        /// <summary>
        /// Get the width of the frustum at specified depth.
        /// </summary>
        /// <param name="depth">the depth at which to calculate frustum width.</param>
        /// <returns>With of the frustum at the specified depth</returns>
        public readonly float GetWidthAtDepth(float depth)
        {
            float hAngle = Helper.PiOver2 - MathF.Acos(Vector3.Dot(pNear.Normal, pLeft.Normal));
            return MathF.Tan(hAngle) * depth * 2f;
        }
        /// <summary>
        /// Get the height of the frustum at specified depth.
        /// </summary>
        /// <param name="depth">the depth at which to calculate frustum height.</param>
        /// <returns>Height of the frustum at the specified depth</returns>
        public readonly float GetHeightAtDepth(float depth)
        {
            float vAngle = Helper.PiOver2 - MathF.Acos(Vector3.Dot(pNear.Normal, pTop.Normal));
            return MathF.Tan(vAngle) * depth * 2f;
        }

        /// <summary>
        /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// if the returned value is positive, the camera will move toward the lookat direction (ZoomIn).
        /// if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <returns>The zoom to fit distance</returns>
        public readonly float GetZoomToExtentsShiftDistance(ref BoundingBox boundingBox)
        {
            return GetZoomToExtentsShiftDistance(boundingBox.GetCorners());
        }
        /// <summary>
        /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// if the returned value is positive, the camera will move toward the lookat direction (ZoomIn).
        /// if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The zoom to fit distance</returns>
        public readonly float GetZoomToExtentsShiftDistance(Vector3[] points)
        {
            float vAngle = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal))));
            float vSin = (float)Math.Sin(vAngle);
            float hAngle = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pLeft.Normal))));
            float hSin = (float)Math.Sin(hAngle);
            float horizontalToVerticalMapping = vSin / hSin;

            var ioFrustrum = GetInsideOutClone();

            float maxPointDist = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                float pointDist = Collision.DistancePlanePoint(ref ioFrustrum.pTop, ref points[i]);
                pointDist = Math.Max(pointDist, Collision.DistancePlanePoint(ref ioFrustrum.pBottom, ref points[i]));
                pointDist = Math.Max(pointDist, Collision.DistancePlanePoint(ref ioFrustrum.pLeft, ref points[i]) * horizontalToVerticalMapping);
                pointDist = Math.Max(pointDist, Collision.DistancePlanePoint(ref ioFrustrum.pRight, ref points[i]) * horizontalToVerticalMapping);

                maxPointDist = Math.Max(maxPointDist, pointDist);
            }

            return -maxPointDist / vSin;
        }
        private readonly BoundingFrustum GetInsideOutClone()
        {
            var frustum = this;
            frustum.pNear.Normal = -frustum.pNear.Normal;
            frustum.pFar.Normal = -frustum.pFar.Normal;
            frustum.pLeft.Normal = -frustum.pLeft.Normal;
            frustum.pRight.Normal = -frustum.pRight.Normal;
            frustum.pTop.Normal = -frustum.pTop.Normal;
            frustum.pBottom.Normal = -frustum.pBottom.Normal;
            return frustum;
        }

        /// <summary>
        /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <returns>The zoom to fit vector</returns>
        public readonly Vector3 GetZoomToExtentsShiftVector(ref BoundingBox boundingBox)
        {
            return GetZoomToExtentsShiftDistance(boundingBox.GetCorners()) * pNear.Normal;
        }
        /// <summary>
        /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The zoom to fit vector</returns>
        public readonly Vector3 GetZoomToExtentsShiftVector(Vector3[] points)
        {
            return GetZoomToExtentsShiftDistance(points) * pNear.Normal;
        }

        /// <summary>
        /// Checks whether a point lay inside, intersects or lay outside the frustum.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Type of the containment</returns>
        public readonly Vortice.Mathematics.ContainmentType Contains(ref Vector3 point)
        {
            var result = Vortice.Mathematics.PlaneIntersectionType.Front;
            for (int i = 0; i < 6; i++)
            {
                var planeResult = GetPlane(i).Intersects(ref point);
                switch (planeResult)
                {
                    case Vortice.Mathematics.PlaneIntersectionType.Back:
                        return Vortice.Mathematics.ContainmentType.Disjoint;
                    case Vortice.Mathematics.PlaneIntersectionType.Intersecting:
                        result = Vortice.Mathematics.PlaneIntersectionType.Intersecting;
                        break;
                }
            }

            return result switch
            {
                Vortice.Mathematics.PlaneIntersectionType.Intersecting => Vortice.Mathematics.ContainmentType.Intersects,
                _ => Vortice.Mathematics.ContainmentType.Contains,
            };
        }
        /// <summary>
        /// Checks whether a point lay inside, intersects or lay outside the frustum.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Type of the containment</returns>
        public readonly Vortice.Mathematics.ContainmentType Contains(Vector3 point)
        {
            return Contains(ref point);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns>Type of the containment</returns>
        public readonly Vortice.Mathematics.ContainmentType Contains(ref BoundingBox box)
        {
            var result = Vortice.Mathematics.ContainmentType.Contains;

            for (int i = 0; i < 6; i++)
            {
                var plane = GetPlane(i);

                GetBoxToPlanePVertexNVertex(ref box, ref plane.Normal, out Vector3 p, out Vector3 n);

                if (Collision.PlaneIntersectsPoint(ref plane, ref p) == Vortice.Mathematics.PlaneIntersectionType.Back)
                {
                    return Vortice.Mathematics.ContainmentType.Disjoint;
                }

                if (Collision.PlaneIntersectsPoint(ref plane, ref n) == Vortice.Mathematics.PlaneIntersectionType.Back)
                {
                    result = Vortice.Mathematics.ContainmentType.Intersects;
                }
            }

            return result;
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns>Type of the containment</returns>
        public readonly Vortice.Mathematics.ContainmentType Contains(BoundingBox box)
        {
            return Contains(ref box);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="result">Type of the containment.</param>
        public readonly void Contains(ref BoundingBox box, out Vortice.Mathematics.ContainmentType result)
        {
            result = Contains(ref box);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public readonly Vortice.Mathematics.ContainmentType Contains(ref BoundingSphere sphere)
        {
            var result = Vortice.Mathematics.PlaneIntersectionType.Front;
            for (int i = 0; i < 6; i++)
            {
                var planeResult = GetPlane(i).Intersects(ref sphere);
                switch (planeResult)
                {
                    case Vortice.Mathematics.PlaneIntersectionType.Back:
                        return Vortice.Mathematics.ContainmentType.Disjoint;
                    case Vortice.Mathematics.PlaneIntersectionType.Intersecting:
                        result = Vortice.Mathematics.PlaneIntersectionType.Intersecting;
                        break;
                }
            }

            return result switch
            {
                Vortice.Mathematics.PlaneIntersectionType.Intersecting => Vortice.Mathematics.ContainmentType.Intersects,
                _ => Vortice.Mathematics.ContainmentType.Contains,
            };
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public readonly Vortice.Mathematics.ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <param name="result">Type of the containment.</param>
        public readonly void Contains(ref BoundingSphere sphere, out Vortice.Mathematics.ContainmentType result)
        {
            result = Contains(ref sphere);
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingSphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public readonly bool Intersects(ref BoundingSphere sphere)
        {
            return Contains(ref sphere) != Vortice.Mathematics.ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingSphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <param name="result">Set to <c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</param>
        public readonly void Intersects(ref BoundingSphere sphere, out bool result)
        {
            result = Contains(ref sphere) != Vortice.Mathematics.ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingBox.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</returns>
        public readonly bool Intersects(ref BoundingBox box)
        {
            return Contains(ref box) != Vortice.Mathematics.ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingBox.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="result"><c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</param>
        public readonly void Intersects(ref BoundingBox box, out bool result)
        {
            result = Contains(ref box) != Vortice.Mathematics.ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <returns>Plane intersection type.</returns>
        public Vortice.Mathematics.PlaneIntersectionType Intersects(ref Plane plane)
        {
            return PlaneIntersectsPoints(ref plane, GetCorners());
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="result">Plane intersection type.</param>
        public void Intersects(ref Plane plane, out Vortice.Mathematics.PlaneIntersectionType result)
        {
            result = PlaneIntersectsPoints(ref plane, GetCorners());
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public readonly bool Intersects(ref Ray ray)
        {
            return Intersects(ref ray, out float? _, out float? _);
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The Ray to check for intersection with.</param>
        /// <param name="inDistance">The distance at which the ray enters the frustum if there is an intersection and the ray starts outside the frustum.</param>
        /// <param name="outDistance">The distance at which the ray exits the frustum if there is an intersection.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public readonly bool Intersects(ref Ray ray, out float? inDistance, out float? outDistance)
        {
            if (Contains(ray.Position) != Vortice.Mathematics.ContainmentType.Disjoint)
            {
                float nearstPlaneDistance = float.MaxValue;

                for (int i = 0; i < 6; i++)
                {
                    var plane = GetPlane(i);

                    if (Collision.RayIntersectsPlane(ref ray, ref plane, out float distance) && distance < nearstPlaneDistance)
                    {
                        nearstPlaneDistance = distance;
                    }
                }

                inDistance = nearstPlaneDistance;
                outDistance = null;
                return true;
            }
            else
            {
                //We will find the two points at which the ray enters and exists the frustum
                //These two points make a line which center inside the frustum if the ray intersects it
                //Or outside the frustum if the ray intersects frustum planes outside it.
                float minDist = float.MaxValue;
                float maxDist = float.MinValue;
                for (int i = 0; i < 6; i++)
                {
                    var plane = GetPlane(i);

                    if (Collision.RayIntersectsPlane(ref ray, ref plane, out float distance))
                    {
                        minDist = Math.Min(minDist, distance);
                        maxDist = Math.Max(maxDist, distance);
                    }
                }

                Vector3 minPoint = ray.Position + ray.Direction * minDist;
                Vector3 maxPoint = ray.Position + ray.Direction * maxDist;
                Vector3 center = (minPoint + maxPoint) / 2f;
                if (Contains(ref center) != Vortice.Mathematics.ContainmentType.Disjoint)
                {
                    inDistance = minDist;
                    outDistance = maxDist;
                    return true;
                }
                else
                {
                    inDistance = null;
                    outDistance = null;
                    return false;
                }
            }
        }

        private static void GetBoxToPlanePVertexNVertex(ref BoundingBox box, ref Vector3 planeNormal, out Vector3 p, out Vector3 n)
        {
            p = box.Minimum;
            if (planeNormal.X >= 0) p.X = box.Maximum.X;
            if (planeNormal.Y >= 0) p.Y = box.Maximum.Y;
            if (planeNormal.Z >= 0) p.Z = box.Maximum.Z;

            n = box.Maximum;
            if (planeNormal.X >= 0) n.X = box.Minimum.X;
            if (planeNormal.Y >= 0) n.Y = box.Minimum.Y;
            if (planeNormal.Z >= 0) n.Z = box.Minimum.Z;
        }

        private static Vortice.Mathematics.PlaneIntersectionType PlaneIntersectsPoints(ref Plane plane, Vector3[] points)
        {
            var result = Collision.PlaneIntersectsPoint(ref plane, ref points[0]);

            for (int i = 1; i < points.Length; i++)
            {
                if (Collision.PlaneIntersectsPoint(ref plane, ref points[i]) != result)
                {
                    return Vortice.Mathematics.PlaneIntersectionType.Intersecting;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return pMatrix.GetHashCode();
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref BoundingFrustum other)
        {
            return pMatrix == other.pMatrix;
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BoundingFrustum other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public readonly override bool Equals(object obj)
        {
            if (obj is not BoundingFrustum) return false;

            var strongValue = (BoundingFrustum)obj;
            return Equals(ref strongValue);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingFrustum left, BoundingFrustum right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingFrustum left, BoundingFrustum right)
        {
            return !left.Equals(ref right);
        }
    }

    public struct FrustumCameraParams
    {
        /// <summary>
        /// Position of the camera.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Looking at direction of the camera.
        /// </summary>
        public Vector3 LookAtDir;
        /// <summary>
        /// Up direction.
        /// </summary>
        public Vector3 UpDir;
        /// <summary>
        /// Field of view.
        /// </summary>
        public float FOV;
        /// <summary>
        /// Z near distance.
        /// </summary>
        public float ZNear;
        /// <summary>
        /// Z far distance.
        /// </summary>
        public float ZFar;
        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public float AspectRatio;
    }
}
