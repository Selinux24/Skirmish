using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public sealed class Vector4Tests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod]
        public void ConstantsStateAndIndexer_ValidValuesAndPredicates()
        {
            Assert.AreEqual(16, Vector4.SizeInBytes);
            Assert.AreEqual(Vector4.Zero, new Vector4(0f,0f,0f,0f));
            Assert.AreEqual(Vector4.One, new Vector4(1f,1f,1f,1f));
            Assert.AreEqual(Vector4.UnitX, new Vector4(1f,0f,0f,0f));
            Assert.AreEqual(Vector4.UnitY, new Vector4(0f,1f,0f,0f));
            Assert.AreEqual(Vector4.UnitZ, new Vector4(0f,0f,1f,0f));
            Assert.AreEqual(Vector4.UnitW, new Vector4(0f,0f,0f,1f));

            var vector = new Vector4(0.5f,0.5f,0.5f,0.5f);
            Assert.IsTrue(vector.IsNormalized); Assert.IsFalse(vector.IsZero); Assert.IsFalse(vector.IsNaN); Assert.IsFalse(vector.IsInfinity);
            Assert.IsTrue(Vector4.Zero.IsZero);
            Assert.IsTrue(new Vector4(float.PositiveInfinity,0f,0f,0f).IsInfinity);
            Assert.IsTrue(new Vector4(float.NaN,0f,0f,0f).IsNaN);
            vector[0]=1f; vector[1]=2f; vector[2]=3f; vector[3]=4f;
            Assert.AreEqual(1f,vector.X); Assert.AreEqual(2f,vector.Y); Assert.AreEqual(3f,vector.Z); Assert.AreEqual(4f,vector.W);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>vector[-1]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>vector[4]);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>vector[-1]=1f);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>vector[4]=1f);
        }

        [TestMethod]
        public void Swizzles_AllPublicCombinations_ExpectedVectors()
        {
            var v = new Vector4(1f,2f,3f,4f);
            Assert.AreEqual(new Vector2(1f,2f),v.XY); Assert.AreEqual(new Vector2(2f,1f),v.YX);
            Assert.AreEqual(new Vector2(1f,3f),v.XZ); Assert.AreEqual(new Vector2(3f,1f),v.ZX);
            Assert.AreEqual(new Vector2(2f,3f),v.YZ); Assert.AreEqual(new Vector2(3f,2f),v.ZY);
            Assert.AreEqual(new Vector3(1f,2f,3f),v.XYZ);
            Assert.AreEqual(new Vector3(1f,1f,1f),v.XXX);
            Assert.AreEqual(new Vector3(2f,2f,2f),v.YYY);
            Assert.AreEqual(new Vector3(3f,3f,3f),v.ZZZ);
        }

        [TestMethod]
        public void Constructors_ValidAndInvalidInputs()
        {
            Assert.AreEqual(new Vector4(2f,2f,2f,2f),new Vector4(2f));
            Assert.AreEqual(new Vector4(1f,2f,3f,4f),new Vector4(1f,2f,3f,4f));
            Assert.AreEqual(new Vector4(1f,2f,3f,4f),new Vector4(new Vector3(1f,2f,3f),4f));
            Assert.AreEqual(new Vector4(1f,2f,3f,4f),new Vector4(new Vector2(1f,2f),3f,4f));
            Assert.AreEqual(new Vector4(5f,6f,7f,8f),new Vector4([5f,6f,7f,8f]));
            Assert.ThrowsExactly<ArgumentNullException>(()=>new Vector4(null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>new Vector4([1f,2f,3f]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>new Vector4([1f,2f,3f,4f,5f]));
        }

        [TestMethod]
        public void LengthNormalizeAndToArray_StateAndValues()
        {
            var v=new Vector4(1f,2f,2f,0f);
            Assert.AreEqual(9f,v.LengthSquared()); Assert.AreEqual(3f,v.Length(),0.0001f);
            Assert.AreSequenceEqual([1f,2f,2f,0f],v.ToArray());
            v.Normalize();
            Assert.AreEqual(new Vector4(1f/3f,2f/3f,2f/3f,0f),v);
            Assert.IsTrue(v.IsNormalized);
            var zero=Vector4.Zero; zero.Normalize(); Assert.AreEqual(Vector4.Zero,zero);
            var arr=v.ToArray(); arr[0]=99f; Assert.AreEqual(1f/3f,v.X,0.0001f);
        }

        [TestMethod]
        public void ArithmeticFamilies_ValueAndRefOut_ExpectedParity()
        {
            var left=new Vector4(8f,6f,4f,2f); var right=new Vector4(-2f,3f,1f,-1f);
            Assert.AreEqual(new Vector4(6f,9f,5f,1f),Vector4.Add(left,right));
            Assert.AreEqual(new Vector4(9f,7f,5f,3f),Vector4.Add(left,1f));
            Assert.AreEqual(new Vector4(10f,3f,3f,3f),Vector4.Subtract(left,right));
            Assert.AreEqual(new Vector4(7f,5f,3f,1f),Vector4.Subtract(left,1f));
            Assert.AreEqual(new Vector4(0f,-5f,-3f,-1f),Vector4.Subtract(-2f,right));
            Assert.AreEqual(new Vector4(-16f,18f,4f,-2f),Vector4.Multiply(left,right));
            Assert.AreEqual(new Vector4(16f,12f,8f,4f),Vector4.Multiply(left,2f));
            Assert.AreEqual(new Vector4(-4f,2f,4f,-2f),Vector4.Divide(left,right));
            Assert.AreEqual(new Vector4(4f,3f,2f,1f),Vector4.Divide(left,2f));
            Assert.AreEqual(new Vector4(-8f,-6f,-4f,-2f),Vector4.Negate(left));

            Vector4.Add(ref left,ref right,out var add); Assert.AreEqual(Vector4.Add(left,right),add);
            Vector4.Add(ref left,1f,out var addScalar); Assert.AreEqual(Vector4.Add(left,1f),addScalar);
            Vector4.Subtract(ref left,ref right,out var subtract); Assert.AreEqual(Vector4.Subtract(left,right),subtract);
            Vector4.Subtract(ref left,1f,out var subtractScalar); Assert.AreEqual(Vector4.Subtract(left,1f),subtractScalar);
            Vector4.Subtract(-2f,ref right,out var subtractLeft); Assert.AreEqual(Vector4.Subtract(-2f,right),subtractLeft);
            Vector4.Multiply(ref left,2f,out var multiplyScalar); Assert.AreEqual(Vector4.Multiply(left,2f),multiplyScalar);
            Vector4.Multiply(ref left,ref right,out var multiply); Assert.AreEqual(Vector4.Multiply(left,right),multiply);
            Vector4.Divide(ref left,2f,out var divideScalar); Assert.AreEqual(Vector4.Divide(left,2f),divideScalar);
            Vector4.Divide(12f,ref right,out var divideLeft); Assert.AreEqual(Vector4.Divide(12f,right),divideLeft);
            Vector4.Divide(ref left,ref right,out var divide); Assert.AreEqual(Vector4.Divide(left,right),divide);
            Vector4.Negate(ref left,out var negate); Assert.AreEqual(Vector4.Negate(left),negate);
        }

        [TestMethod]
        public void GeometryInterpolationAndRefOverloads_ExpectedValues()
        {
            var zero=Vector4.Zero; var unitX=Vector4.UnitX; var unitY=Vector4.UnitY;
            Assert.AreEqual(unitX,Vector4.Barycentric(zero,unitX,unitY,1f,0f));
            Assert.AreEqual(unitY,Vector4.Barycentric(zero,unitX,unitY,0f,1f));
            Assert.AreEqual(new Vector4(1f,0f,0f,0f),Vector4.Clamp(new Vector4(5f,-2f,0f,0f),zero,Vector4.One));
            Assert.AreEqual(1f,Vector4.Distance(zero,unitX));
            Assert.AreEqual(1f,Vector4.DistanceSquared(zero,unitX));
            Assert.AreEqual(0f,Vector4.Dot(unitX,unitY));

            Assert.AreEqual(zero,Vector4.Lerp(zero,unitX,0f));
            Assert.AreEqual(new Vector4(0.5f,0f,0f,0f),Vector4.Lerp(zero,unitX,0.5f));
            Assert.AreEqual(unitX,Vector4.Lerp(zero,unitX,1f));
            Assert.AreEqual(new Vector4(0f,0.5f,1f,0f),Vector4.Lerp(zero,Vector4.One,new Vector4(0f,0.5f,1f,0f)));
            Assert.AreEqual(zero,Vector4.SmoothStep(zero,unitX,0f));
            Assert.AreEqual(unitX,Vector4.SmoothStep(zero,unitX,1f));
            Assert.AreEqual(new Vector4(1f,1f,2f,1f),Vector4.Max(new Vector4(0f,1f,2f,0f),Vector4.One));
            Assert.AreEqual(Vector4.Zero,Vector4.Min(new Vector4(0f,1f,2f,0f),Vector4.Zero));

            Vector4.Normalize(ref unitX,out var normalized); Assert.AreEqual(Vector4.UnitX,normalized);
            Vector4.Distance(ref zero,ref unitX,out float distance); Assert.AreEqual(1f,distance);
            Vector4.DistanceSquared(ref zero,ref unitX,out float squared); Assert.AreEqual(1f,squared);
            Vector4.Dot(ref unitX,ref unitY,out float dot); Assert.AreEqual(0f,dot);
            Vector4.Lerp(ref zero,ref unitX,0.5f,out var lerp); Assert.AreEqual(new Vector4(0.5f,0f,0f,0f),lerp);
            Vector4.SmoothStep(ref zero,ref unitX,0.5f,out var smooth); Assert.AreEqual(new Vector4(0.5f,0f,0f,0f),smooth);

            var p1=zero; var p2=unitX; var p3=unitY; var p4=Vector4.One;
            Assert.AreEqual(p1,Vector4.Hermite(p1,Vector4.Zero,p1,Vector4.Zero,0f));
            Assert.AreEqual(p2,Vector4.CatmullRom(p1,p2,p3,p4,0f));
            Vector4.CatmullRom(ref p1,ref p2,ref p3,ref p4,0.5f,out var catmull);
            Assert.AreEqual(Vector4.CatmullRom(p1,p2,p3,p4,0.5f),catmull);
            Vector4.Hermite(ref p1,ref p2,ref p3,ref p4,0.5f,out var hermite);
            Assert.AreEqual(Vector4.Hermite(p1,p2,p3,p4,0.5f),hermite);
            Vector4.Barycentric(ref p1,ref p2,ref p3,0.5f,0.25f,out var bary);
            Assert.AreEqual(Vector4.Barycentric(p1,p2,p3,0.5f,0.25f),bary);
        }

        [TestMethod]
        public void OrthogonalizeAndOrthonormalize_ValidAndEdgeCases()
        {
            Vector4[] source=[Vector4.UnitX,new Vector4(1f,1f,0f,0f),new Vector4(1f,1f,1f,0f),new Vector4(1f,1f,1f,1f)];
            var orthogonal=new Vector4[4]; Vector4.Orthogonalize(orthogonal,source);
            Assert.AreEqual(Vector4.UnitX,orthogonal[0]);
            Assert.AreEqual(0f,Vector4.Dot(orthogonal[0],orthogonal[1]),0.0001f);
            var orthonormal=new Vector4[4]; Vector4.Orthonormalize(orthonormal,source);
            Assert.AreEqual(1f,orthonormal[0].Length(),0.0001f);
            Assert.AreEqual(0f,Vector4.Dot(orthonormal[0],orthonormal[1]),0.0001f);
            Assert.ThrowsExactly<ArgumentNullException>(()=>Vector4.Orthogonalize(new Vector4[1],null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>Vector4.Orthonormalize([],Vector4.UnitX));
        }

        [TestMethod]
        public void QuaternionTransforms_SingleRefAndArray_ExpectedParity()
        {
            var v=Vector4.UnitX; var identity=Quaternion.Identity;
            Assert.AreEqual(v,Vector4.Transform(v,identity));
            Vector4.Transform(ref v,ref identity,out var transformed); Assert.AreEqual(v,transformed);
            Vector4[] source=[Vector4.UnitX,Vector4.UnitY]; var destination=new Vector4[2];
            Vector4.Transform(source,ref identity,destination); Assert.AreSequenceEqual(source,destination);
            Assert.ThrowsExactly<ArgumentNullException>(()=>Vector4.Transform(null,ref identity,destination));
            Assert.ThrowsExactly<ArgumentNullException>(()=>Vector4.Transform(source,ref identity,null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>Vector4.Transform(source,ref identity,new Vector4[1]));
        }

        [TestMethod]
        public void Matrix4x4Transforms_SingleRefAndArray_ExpectedParity()
        {
            var v=new Vector4(1f,2f,3f,1f); var matrix=Matrix4x4.Translation(10f,20f,30f);
            Assert.AreEqual(new Vector4(11f,22f,33f,1f),Vector4.Transform(v,matrix));
            Vector4.Transform(ref v,ref matrix,out var transformed); Assert.AreEqual(new Vector4(11f,22f,33f,1f),transformed);
            Vector4[] source=[Vector4.UnitX,Vector4.UnitY]; var destination=new Vector4[2];
            Vector4.Transform(source,ref matrix,destination);
            Assert.AreEqual(new Vector4(1f,0f,0f,0f),destination[0]);
            Assert.AreEqual(new Vector4(0f,1f,0f,0f),destination[1]);
            Assert.ThrowsExactly<ArgumentNullException>(()=>Vector4.Transform(null,ref matrix,destination));
            Assert.ThrowsExactly<ArgumentNullException>(()=>Vector4.Transform(source,ref matrix,null));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(()=>Vector4.Transform(source,ref matrix,new Vector4[1]));
        }

        [TestMethod]
        public void Operators_UnaryAndBinary_AllFamilies_ExpectedParity()
        {
            var left=new Vector4(1f,2f,3f,4f); var right=new Vector4(4f,5f,6f,7f);
            Assert.AreEqual(new Vector4(5f,7f,9f,11f),left+right);
            Assert.AreEqual(new Vector4(-3f,-3f,-3f,-3f),left-right);
            Assert.AreEqual(new Vector4(4f,10f,18f,28f),left*right);
            Assert.AreEqual(new Vector4(0.25f,0.4f,0.5f,4f/7f),left/right);
            Assert.AreEqual(+left,left); Assert.AreEqual(-left,new Vector4(-1f,-2f,-3f,-4f));
            Assert.AreEqual(new Vector4(2f,4f,6f,8f),left*2f); Assert.AreEqual(new Vector4(2f,4f,6f,8f),2f*left);
            Assert.AreEqual(new Vector4(0.5f,1f,1.5f,2f),left/2f); Assert.AreEqual(new Vector4(0.5f,0.4f,1f/3f,2f/7f),2f/right);
            Assert.AreEqual(new Vector4(2f,3f,4f,5f),left+1f); Assert.AreEqual(new Vector4(2f,3f,4f,5f),1f+left);
            Assert.AreEqual(new Vector4(0f,1f,2f,3f),left-1f); Assert.AreEqual(new Vector4(0f,-1f,-2f,-3f),1f-left);
            Assert.IsTrue(left==new Vector4(1f,2f,3f,4f)); Assert.IsFalse(left!=new Vector4(1f,2f,3f,4f));
            Assert.IsFalse(left==right);
        }

        [TestMethod]
        public void Conversions_ExplicitVectorAndNumericsTypes_ExpectedValues()
        {
            var v=new Vector4(1f,2f,3f,4f);
            Assert.AreEqual(new Vector2(1f,2f),(Vector2)v);
            Assert.AreEqual(new Vector3(1f,2f,3f),(Vector3)v);
            var numerics=new System.Numerics.Vector4(5f,6f,7f,8f);
            var converted=(Vector4)numerics;
            Assert.AreEqual(numerics,(System.Numerics.Vector4)converted);
            Assert.AreEqual(new Vector4(5f,6f,7f,8f),converted);
        }

        [TestMethod]
        public void ValueSemanticsAndFormatting_AllOverloads_ExpectedBehavior()
        {
            var v=new Vector4(1.25f,-2.5f,3.75f,4f); var equal=new Vector4(1.25f,-2.5f,3.75f,4f);
            var near=new Vector4(1.2504f,-2.5f,3.75f,4f); var different=new Vector4(1.25f,-2.5f,3.75f,5f);
            Assert.AreEqual(equal.GetHashCode(),v.GetHashCode());
            Assert.IsTrue(v.Equals(ref equal)); Assert.IsTrue(v.Equals(equal)); Assert.IsTrue(v.Equals((object)equal));
            Assert.IsFalse(v.Equals((object)"vector")); Assert.IsFalse(v.Equals(different));
            Assert.IsTrue(Vector4.NearEqual(v,near,0.001f)); Assert.IsFalse(Vector4.NearEqual(v,different,0.001f));
            Assert.AreEqual("X:1.25 Y:-2.50 Z:3.75 W:4.00",v.ToString("F2",CultureInfo.InvariantCulture));
            Assert.AreEqual(v.ToString(),v.ToString(CultureInfo.CurrentCulture));
            Assert.AreEqual(v.ToString("F2"),v.ToString("F2",CultureInfo.CurrentCulture));
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture,"X:{0:0.000}; Y:{1:0.000}; Z:{2:0.000}; W:{3:0.000}",1.25f,-2.5f,3.75f,4f),v.GetDescription());
        }
    }
}