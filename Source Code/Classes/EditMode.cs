using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Windows.Input;
using System.Windows.Controls;

namespace BlenderBTech
{
    public static class EditMode
    {
        public static void DisplayEdges(MeshGeometry3D mesh, Model3DGroup group)
        {
            Point3D[] points = mesh.Positions.ToArray();
            int[] triangles = mesh.TriangleIndices.ToArray();
            List<Point3D[]> edges = new List<Point3D[]>();

            for (int i = 0; i < triangles.Length - 1; i++)
            {
                Point3D[] edge = new Point3D[2];
                edge[0] = points[triangles[i]]; edge[1] = points[triangles[i + 1]];

                /*bool IsDiagonalEdge = ((edge[0].X != edge[1].X) && (edge[0].Y == edge[1].Y) && edge[0].Z == edge[1].Z)
                || ((edge[0].X == edge[1].X) && (edge[0].Y != edge[1].Y) && edge[0].Z == edge[1].Z)
                || ((edge[0].X == edge[1].X) && (edge[0].Y == edge[1].Y) && edge[0].Z != edge[1].Z);*/

                if ((edge[0] != edge[1]))
                {
                    edges.Add(edge);
                }
            }

            for (int i = 0; i < edges.Count; i++)
            {
                Point3D[] edge = edges[i];

                for (int j = 0; j < edges.Count; j++)
                {
                    if (edge[0] == edges[j][1] && edge[1] == edges[j][0])
                    {
                        edges.Remove(edge);
                    }
                }
            }

            HashSet<Point3D[]> DistinctList = new HashSet<Point3D[]>(edges);

            List<GeometryModel3D> lines = DrawEdges(DistinctList.ToList());

            foreach (GeometryModel3D line in lines)
            {
                group.Children.Add(line);
            }
        }

        public static void DisplayVertices(MeshGeometry3D mesh, Model3DGroup group)
        {
            Point3D[] vertices = mesh.Positions.ToArray();
            List<GeometryModel3D> points = DrawVertices(vertices, 0.025);

            foreach (GeometryModel3D point in points)
            {
                group.Children.Add(point);
            }
        }

        private static List<GeometryModel3D> DrawEdges(List<Point3D[]> edges)
        {
            List<GeometryModel3D> Lines = new List<GeometryModel3D>();

            foreach (Point3D[] edge in edges)
            {
                Vector3D Axis = new Vector3D()
                {
                    X = edge[0].X - edge[1].X,
                    Y = edge[0].Y - edge[1].Y,
                    Z = edge[0].Z - edge[1].Z,
                };
                GeometryModel3D Line = new GeometryModel3D
                {
                    Geometry = Shapes.Cylinder(0.01, 4, edge[1], Axis),
                    Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black)),
                };

                Lines.Add(Line);
            }

            return Lines;
        }

        private static List<GeometryModel3D> DrawVertices(Point3D[] verts, double size)
        {
            List<GeometryModel3D> Points = new List<GeometryModel3D>();

            foreach (Point3D vertex in verts)
            {
                MeshGeometry3D Cube = new MeshGeometry3D()
                {
                    Normals = new Vector3DCollection 
                    {                
                        new Vector3D(0, 0, 1),
                        new Vector3D(0, 1, 1),
                        new Vector3D(0, 1, 0),
                        new Vector3D(1, 0, 0),
                        new Vector3D(1, 0, 1),
                        new Vector3D(1, 1, 0), 
                    },
                    Positions = new Point3DCollection
                    {
                        new Point3D(-size + vertex.X, -size + vertex.Y, size + vertex.Z), // BL FRONT 0
                        new Point3D(size + vertex.X, -size + vertex.Y, size + vertex.Z), // BR FRONT 1
                        new Point3D(size + vertex.X, size + vertex.Y, size + vertex.Z), // TR FRONT 2
                        new Point3D(-size + vertex.X, size + vertex.Y, size + vertex.Z), // TL FRONT 3
                        new Point3D(-size + vertex.X, -size + vertex.Y, -size + vertex.Z), // BL BACK 4
                        new Point3D(size + vertex.X, -size + vertex.Y, -size + vertex.Z), // BR BACK 5
                        new Point3D(size + vertex.X, size + vertex.Y, -size + vertex.Z), // TR BACK 6
                        new Point3D(-size + vertex.X, size + vertex.Y, -size + vertex.Z) // TL BACK 7
                    },
                    TextureCoordinates = new PointCollection
                    {
                        new Point(0, 0),
                        new Point(1, 0),
                        new Point(1, 1),
                        new Point(0, 1),
                    },
                    TriangleIndices = new Int32Collection
                    {
                        0, 1, 2, 2, 3, 0,
                        6, 5, 4, 4, 7, 6,
                        4, 0, 3, 3, 7, 4,
                        2, 1, 5, 5, 6, 2,
                        7, 3, 2, 2, 6, 7,
                        1, 0, 4, 4, 5, 1
                    },
                };
                GeometryModel3D Point = new GeometryModel3D() 
                { Geometry = Cube, Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black)) };

                Points.Add(Point);
            }

            return Points;
        }

        public static void HighlightVertices(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(sender as Viewport3D);

            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            HitTestResult result = VisualTreeHelper.HitTest((Viewport3D)sender, mousePos);
            RayMeshGeometry3DHitTestResult rayMeshResult = result as RayMeshGeometry3DHitTestResult;
            GeometryModel3D model = new GeometryModel3D();

            if (rayMeshResult != null)
            {
                MeshGeometry3D mesh = new MeshGeometry3D
                {
                    Positions = rayMeshResult.MeshHit.Positions,
                    TriangleIndices = rayMeshResult.MeshHit.TriangleIndices,
                    TextureCoordinates = rayMeshResult.MeshHit.TextureCoordinates,
                    Normals = rayMeshResult.MeshHit.Normals
                };

                model.Geometry = mesh;
                model.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
            }


        }
    }
}