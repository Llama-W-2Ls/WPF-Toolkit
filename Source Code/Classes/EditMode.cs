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
        #region private constants/variables
        private static readonly List<GeometryModel3D> AllWireframes = new List<GeometryModel3D>();
        private static List<GeometryModel3D> points = new List<GeometryModel3D>();
        private static List<GeometryModel3D> lines = new List<GeometryModel3D>();
        private static Model3DGroup ChosenGroup = new Model3DGroup();

        private static readonly GeometryModel3D[] Axises = new GeometryModel3D[3];
        private static Point mousePos = new Point();
        private static TranslateTransform3D AxisPosition = new TranslateTransform3D();
        private static string CurrentlyEditingAxis = "null";
        private static readonly MainWindow main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
        private static GeometryModel3D CurrentVertEdgeBeingEdited = new GeometryModel3D();
        private static MeshGeometry3D CurrentMeshBeingEdited = new MeshGeometry3D();
        private static TranslateTransform3D PreviousAxisPosition = new TranslateTransform3D();
        #endregion

        public static void DisplayEdges(MeshGeometry3D mesh, Model3DGroup group)
        {
            Point3D[] points = mesh.Positions.ToArray();
            int[] triangles = mesh.TriangleIndices.ToArray();
            List<Point3D[]> edges = new List<Point3D[]>();

            CurrentMeshBeingEdited = mesh;

            if (points.Length == 8)
            {
                for (int i = 0; i < triangles.Length - 1; i += 6)
                {
                    Point3D[] edge = new Point3D[2];
                    edge[0] = points[triangles[i]]; edge[1] = points[triangles[i + 1]];

                    Point3D[] edge2 = new Point3D[2];
                    edge2[0] = points[triangles[i + 4]]; edge2[1] = points[triangles[i + 5]];

                    if (i == 0 || i == triangles.Length - 6)
                    {
                        Point3D[] edge3 = new Point3D[2];
                        edge3[0] = points[triangles[i + 3]]; edge3[1] = points[triangles[i + 4]];
                        edges.Add(edge3);
                    }

                    if ((edge[0] != edge[1]))
                    {
                        edges.Add(edge);
                        edges.Add(edge2);
                    }
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length - 1; i++)
                {
                    Point3D[] edge = new Point3D[2];
                    edge[0] = points[triangles[i]]; edge[1] = points[triangles[i + 1]];

                    if (edge[0] != edge[1])
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

            UpdateSomeEdges(CurrentMeshBeingEdited);
            lines = DrawEdges(DistinctList.ToList());

            foreach (GeometryModel3D line in lines)
            {
                group.Children.Add(line);
                AllWireframes.Add(line);
            }            
        }

        public static void DisplayVertices(MeshGeometry3D mesh, Model3DGroup group)
        {
            Point3D[] vertices = mesh.Positions.ToArray();
            HashSet<Point3D> DistinctList = new HashSet<Point3D>(vertices);
            points = DrawVertices(DistinctList.ToArray(), 0.025);
            ChosenGroup = group;

            foreach (GeometryModel3D point in points)
            {
                group.Children.Add(point);
                AllWireframes.Add(point);
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

        public static void HighlightVertsAndEdges_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            PreviousAxisPosition = AxisPosition;

            foreach (GeometryModel3D wireframe in AllWireframes)
            {
                wireframe.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Black));
            }

            mousePos = e.GetPosition(sender as Viewport3D);
            HitTestResult result = VisualTreeHelper.HitTest(sender as Viewport3D, mousePos);

            if (result is RayMeshGeometry3DHitTestResult rayMeshResult)
            {
                GeometryModel3D hitGeometry = rayMeshResult.ModelHit as GeometryModel3D;
                if (AllWireframes.Contains(hitGeometry))
                {
                    AxisPosition = new TranslateTransform3D();
                    hitGeometry.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));

                    Rect3D bounds = hitGeometry.Bounds;
                    Point3D CenterOfGeometry = new Point3D()
                    {
                        X = bounds.X + bounds.SizeX / 2,
                        Y = bounds.Y + bounds.SizeY / 2,
                        Z = bounds.Z + bounds.SizeZ / 2,
                    };
                    DisplayAxisAt(ChosenGroup, CenterOfGeometry);
                    CurrentlyEditingAxis = "";
                    CurrentVertEdgeBeingEdited = hitGeometry;
                }
                else if (Axises.Contains(hitGeometry))
                {
                    int index = Array.IndexOf(Axises, hitGeometry);
                    switch (index)
                    {
                        case 0:
                            CurrentlyEditingAxis = "X";
                            break;
                        case 1:
                            CurrentlyEditingAxis = "Y";
                            break;
                        case 2:
                            CurrentlyEditingAxis = "Z";
                            break;
                    }
                    hitGeometry.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow));
                }
            }
        }

        private static void DisplayAxisAt(Model3DGroup group, Point3D PosOfAxis)
        {
            GeometryModel3D Xaxis = new GeometryModel3D()
            {
                Geometry = Shapes.Cylinder(0.03, 4, PosOfAxis, new Vector3D(1, 0, 0)),
                Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red)),
            }; 
            GeometryModel3D Yaxis = new GeometryModel3D()
            {
                Geometry = Shapes.Cylinder(0.03, 4, PosOfAxis, new Vector3D(0, 1, 0)),
                Material = new DiffuseMaterial(new SolidColorBrush(Colors.Green)),
            };
            GeometryModel3D Zaxis = new GeometryModel3D()
            {
                Geometry = Shapes.Cylinder(0.03, 4, PosOfAxis, new Vector3D(0, 0, 1)),
                Material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue)),
            };

            foreach (var item in Axises) { group.Children.Remove(item); }

            Axises[0] = Xaxis; Axises[1] = Yaxis; Axises[2] = Zaxis;
            foreach (var item in Axises) { group.Children.Add(item); }           
        }

        public static void AxisProperties_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point newMousePos = e.GetPosition(sender as Border);

                switch (CurrentlyEditingAxis)
                {
                    case "X":
                        AxisPosition.OffsetX = ((-newMousePos.X + mousePos.X) / main.Width * -4);
                        break;
                    case "Y":
                        AxisPosition.OffsetY = ((newMousePos.Y - mousePos.Y) / main.Height * -4);
                        break;
                    case "Z":
                        AxisPosition.OffsetZ = ((-newMousePos.X + mousePos.X) / main.Width * 4);
                        break;
                    default:
                        break;
                }

                if (CurrentlyEditingAxis != "null")
                {
                    foreach (var item in Axises)
                    {
                        item.Transform = AxisPosition;
                    }
                }

                CurrentVertEdgeBeingEdited.Transform = AxisPosition;

                Rect3D bounds = CurrentVertEdgeBeingEdited.Bounds;
                Point3D CenterOfGeometry = new Point3D()
                {
                    X = bounds.X + bounds.SizeX / 2,
                    Y = bounds.Y + bounds.SizeY / 2,
                    Z = bounds.Z + bounds.SizeZ / 2,
                };

                int index = points.IndexOf(CurrentVertEdgeBeingEdited);
                CurrentMeshBeingEdited.Positions[index] = CenterOfGeometry;

                foreach (var item in lines)
                {
                    ChosenGroup.Children.Remove(item);
                }
                UpdateSomeEdges(CurrentMeshBeingEdited);
            }
        }

        private static void UpdateSomeEdges(MeshGeometry3D mesh)
        {
            Point3D[] points = mesh.Positions.ToArray();
            int[] triangles = mesh.TriangleIndices.ToArray();
            List<Point3D[]> edges = new List<Point3D[]>();

            if (points.Length == 8)
            {
                for (int i = 0; i < triangles.Length - 1; i += 6)
                {
                    Point3D[] edge = new Point3D[2];
                    edge[0] = points[triangles[i]]; edge[1] = points[triangles[i + 1]];

                    Point3D[] edge2 = new Point3D[2];
                    edge2[0] = points[triangles[i + 4]]; edge2[1] = points[triangles[i + 5]];

                    if (i == 0 || i == triangles.Length - 6)
                    {
                        Point3D[] edge3 = new Point3D[2];
                        edge3[0] = points[triangles[i + 3]]; edge3[1] = points[triangles[i + 4]];
                        edges.Add(edge3);
                    }

                    if ((edge[0] != edge[1]))
                    {
                        edges.Add(edge);
                        edges.Add(edge2);
                    }
                }
            }
            else
            {
                for (int i = 0; i < triangles.Length - 1; i++)
                {
                    Point3D[] edge = new Point3D[2];
                    edge[0] = points[triangles[i]]; edge[1] = points[triangles[i + 1]];

                    if (edge[0] != edge[1])
                    {
                        edges.Add(edge);
                    }
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

            Rect3D bounds = CurrentVertEdgeBeingEdited.Bounds;
            Point3D CenterOfGeometry = new Point3D()
            {
                X = bounds.X + bounds.SizeX / 2,
                Y = bounds.Y + bounds.SizeY / 2,
                Z = bounds.Z + bounds.SizeZ / 2,
            };

            lines = UpdateEdges(DistinctList.ToList(), CenterOfGeometry);

            foreach (GeometryModel3D line in lines)
            {
                ChosenGroup.Children.Add(line);
                AllWireframes.Add(line);
            }
        }

        private static List<GeometryModel3D> UpdateEdges(List<Point3D[]> edges, Point3D EditedVertex)
        {
            List<GeometryModel3D> Lines = new List<GeometryModel3D>();
            List<Point3D[]> UpdatedEdges = new List<Point3D[]>();

            foreach (Point3D[] edge in edges)
            {
                if (edge.Contains(EditedVertex))
                {
                    UpdatedEdges.Add(edge);
                }
            }

            foreach (Point3D[] edge in UpdatedEdges)
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
    }
}