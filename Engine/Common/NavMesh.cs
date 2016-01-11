using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Engine.Common
{
    public class NavMesh
    {
        List<Polygon> poligons = new List<Polygon>();

        public static NavMesh Build(Triangle[] triangles, float size, float angle = MathUtil.PiOverFour)
        {
            NavMesh result = new NavMesh();

            //Eliminar los triángulos por ángulo

            //Identificar los bordes de los triángulos eliminados
            //Son bordes los que coinciden con triángulos no eliminados

            //Generar polígonos con los bordes

            var tris = Array.FindAll(triangles, t => t.Inclination >= angle);
            if (tris != null && tris.Length > 0)
            {
                Array.ForEach(tris, t =>
                {
                    //Buscar polígono para el triángulo
                    var poly = result.poligons.Find(p => p.Inside(new Vector2(t.Point1.X, t.Point1.Z)));
                });
            }

            return result;
        }
    }

    public class Polygon
    {
        private List<Vector2> vertexList = new List<Vector2>();

        public Vector2[] VertexList
        {
            get
            {
                return this.vertexList.ToArray();
            }
            set
            {
                this.vertexList.Clear();

                if (value != null && value.Length > 0)
                {
                    this.vertexList.AddRange(value);
                }
            }
        }

        public void Add(Vector2 vertex1, Vector2 vertex2, Vector2 vertex3)
        {
            //Si un segmento coincide

            //Si dos segmentos coinciden

            //Si tres segmentos coinciden no añadir nada

            //Si un vértice coincide

            //Si dos vértices coinciden

            //Si tres vértices coinciden
        }

        public bool Inside(Vector2 position, bool toleranceOnOutside = true)
        {
            Vector2 point = position;

            const float epsilon = 0.5f;

            bool inside = false;

            // Must have 3 or more edges
            if (this.vertexList.Count < 3) return false;

            Vector2 oldPoint = this.vertexList[this.vertexList.Count - 1];
            float oldSqDist = Vector2.DistanceSquared(oldPoint, point);

            for (int i = 0; i < this.vertexList.Count; i++)
            {
                Vector2 newPoint = this.vertexList[i];
                float newSqDist = Vector2.DistanceSquared(newPoint, point);

                if (oldSqDist + newSqDist + 2.0f * System.Math.Sqrt(oldSqDist * newSqDist) - Vector2.DistanceSquared(newPoint, oldPoint) < epsilon)
                    return toleranceOnOutside;

                Vector2 left;
                Vector2 right;
                if (newPoint.X > oldPoint.X)
                {
                    left = oldPoint;
                    right = newPoint;
                }
                else
                {
                    left = newPoint;
                    right = oldPoint;
                }

                if (left.X < point.X && point.X <= right.X && (point.Y - left.Y) * (right.X - left.X) < (right.Y - left.Y) * (point.X - left.X))
                    inside = !inside;

                oldPoint = newPoint;
                oldSqDist = newSqDist;
            }

            return inside;
        }
    }
}
