using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace BlenderBTech
{
    public static class Shapes
    {
        public static MeshGeometry3D Cube()
        {
            Vector3DCollection Normals = new Vector3DCollection
            {
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
                new Vector3D(0, 0, 1),
            };

            PointCollection TextureCoordinates = new PointCollection
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1),
            };

            Point3DCollection Positions = new Point3DCollection
            {
                new Point3D(-0.5, -0.5, 0.5), // BL FRONT 0
                new Point3D(0.5, -0.5, 0.5), // BR FRONT 1
                new Point3D(0.5, 0.5, 0.5), // TR FRONT 2
                new Point3D(-0.5, 0.5, 0.5), // TL FRONT 3
                new Point3D(-0.5, -0.5, -0.5), // BL BACK 4
                new Point3D(0.5, -0.5, -0.5), // BR BACK 5
                new Point3D(0.5, 0.5, -0.5), // TR BACK 6
                new Point3D(-0.5, 0.5, -0.5) // TL BACK 7
            };

            MeshGeometry3D Faces = new MeshGeometry3D()
            {
                Normals = Normals,
                Positions = Positions,
                TextureCoordinates = TextureCoordinates,
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

            return Faces;
        }

        public static MeshGeometry3D Sphere(double radius, int TopBottomDetail, int SidesDetail)
        {
            MeshGeometry3D sphere_mesh = new MeshGeometry3D();

            double dphi = Math.PI / TopBottomDetail;
            double dtheta = 2 * Math.PI / SidesDetail;

            // Remember the first point.
            int pt0 = sphere_mesh.Positions.Count;

            // Make the points.
            double phi1 = Math.PI / 2;
            for (int p = 0; p <= TopBottomDetail; p++)
            {
                double r1 = radius * Math.Cos(phi1);
                double y1 = radius * Math.Sin(phi1);

                double theta = 0;
                for (int t = 0; t <= SidesDetail; t++)
                {
                    sphere_mesh.Positions.Add(new Point3D(r1 * Math.Cos(theta), y1, -r1 * Math.Sin(theta)));
                    sphere_mesh.TextureCoordinates.Add(new Point((double)t / SidesDetail, (double)p / TopBottomDetail));
                    theta += dtheta;
                }
                phi1 -= dphi;
            }

            // Make the triangles.
            int i1, i2, i3, i4;
            for (int p = 0; p <= TopBottomDetail - 1; p++)
            {
                i1 = p * (SidesDetail + 1);
                i2 = i1 + (SidesDetail + 1);
                for (int t = 0; t <= SidesDetail - 1; t++)
                {
                    i3 = i1 + 1;
                    i4 = i2 + 1;
                    sphere_mesh.TriangleIndices.Add(pt0 + i1);
                    sphere_mesh.TriangleIndices.Add(pt0 + i2);
                    sphere_mesh.TriangleIndices.Add(pt0 + i4);

                    sphere_mesh.TriangleIndices.Add(pt0 + i1);
                    sphere_mesh.TriangleIndices.Add(pt0 + i4);
                    sphere_mesh.TriangleIndices.Add(pt0 + i3);
                    i1 += 1;
                    i2 += 1;
                }
            }

            return sphere_mesh;
        }

        public static MeshGeometry3D Cylinder(double radius, int num_sides, Point3D end_point, Vector3D axis)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            // Get two vectors perpendicular to the axis.
            Vector3D v1;
            if ((axis.Z < -0.01) || (axis.Z > 0.01))
                v1 = new Vector3D(axis.Z, axis.Z, -axis.X - axis.Y);
            else
                v1 = new Vector3D(-axis.Y - axis.Z, axis.X, axis.X);
            Vector3D v2 = Vector3D.CrossProduct(v1, axis);

            // Make the vectors have length radius.
            v1 *= (radius / v1.Length);
            v2 *= (radius / v2.Length);

            // Make the top end cap.
            // Make the end point.
            int pt0 = mesh.Positions.Count; // Index of end_point.
            mesh.Positions.Add(end_point);

            // Make the top points.
            double theta = 0;
            double dtheta = 2 * Math.PI / num_sides;
            for (int i = 0; i < num_sides; i++)
            {
                mesh.Positions.Add(end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2);
                theta += dtheta;
            }

            // Make the top triangles.
            int pt1 = mesh.Positions.Count - 1; // Index of last point.
            int pt2 = pt0 + 1;                  // Index of first point.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(pt0);
                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt2);
                pt1 = pt2++;
            }

            // Make the bottom end cap.
            // Make the end point.
            pt0 = mesh.Positions.Count; // Index of end_point2.
            Point3D end_point2 = end_point + axis;
            mesh.Positions.Add(end_point2);

            // Make the bottom points.
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                mesh.Positions.Add(end_point2 +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2);
                theta += dtheta;
            }

            // Make the bottom triangles.
            theta = 0;
            pt1 = mesh.Positions.Count - 1; // Index of last point.
            pt2 = pt0 + 1;                  // Index of first point.
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(num_sides + 1);    // end_point2
                mesh.TriangleIndices.Add(pt2);
                mesh.TriangleIndices.Add(pt1);
                pt1 = pt2++;
            }

            // Make the sides.
            // Add the points to the mesh.
            int first_side_point = mesh.Positions.Count;
            theta = 0;
            for (int i = 0; i < num_sides; i++)
            {
                Point3D p1 = end_point +
                    Math.Cos(theta) * v1 +
                    Math.Sin(theta) * v2;
                mesh.Positions.Add(p1);
                Point3D p2 = p1 + axis;
                mesh.Positions.Add(p2);
                theta += dtheta;
            }

            // Make the side triangles.
            pt1 = mesh.Positions.Count - 2;
            pt2 = pt1 + 1;
            int pt3 = first_side_point;
            int pt4 = pt3 + 1;
            for (int i = 0; i < num_sides; i++)
            {
                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt2);
                mesh.TriangleIndices.Add(pt4);

                mesh.TriangleIndices.Add(pt1);
                mesh.TriangleIndices.Add(pt4);
                mesh.TriangleIndices.Add(pt3);

                pt1 = pt3;
                pt3 += 2;
                pt2 = pt4;
                pt4 += 2;
            }

            return mesh;
        }
    }
}