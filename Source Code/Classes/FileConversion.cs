using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using System;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;

namespace BlenderBTech
{
    public static class FileConversion
    {
        public static void MeshToSTL(MeshGeometry3D mesh, string directory)
        {
            Point3D[] points = mesh.Positions.ToArray();
            int[] tris = mesh.TriangleIndices.ToArray();

            File.WriteAllText(directory, "");

            File.AppendAllText(directory, "solid ASCII_STL_of_MyMesh\n");

            for (int i = 0; i < tris.Length; i += 3)
            {
                File.AppendAllText(directory, " facet normal "
                    + "0" + " "
                    + "0" + " "
                    + "1" + "\n"
                    + "  outer loop\n"
                    + "   vertex "
                    + points[tris[i]].X + " "
                    + points[tris[i]].Y + " "
                    + points[tris[i]].Z + "\n"
                    + "   vertex "
                    + points[tris[i + 1]].X + " "
                    + points[tris[i + 1]].Y + " "
                    + points[tris[i + 1]].Z + "\n"
                    + "   vertex "
                    + points[tris[i + 2]].X + " "
                    + points[tris[i + 2]].Y + " "
                    + points[tris[i + 2]].Z + "\n"
                    + "  endloop\n"
                    + " endfacet\n");
            }

            File.AppendAllText(directory, "endsolid ASCII_STL_of_MyMesh");
        }

        public static void MeshToSaif(MeshGeometry3D mesh, string directory)
        {
            Point3D[] points = mesh.Positions.ToArray();
            int[] tris = mesh.TriangleIndices.ToArray();
            Vector3D[] normals = mesh.Normals.ToArray();
            Point[] textCoords = mesh.TextureCoordinates.ToArray();

            StringBuilder builder = new StringBuilder();

            File.WriteAllText(directory, "");

            File.AppendAllText(directory, "Vertices: ");
            foreach (var item in points)
            {
                string[] point = new string[3];
                point[0] = item.X.ToString();
                point[1] = item.Y.ToString();
                point[2] = item.Z.ToString();

                StringBuilder builder2 = new StringBuilder();
                builder2.Append(point[0] + "," + point[1] + "," + point[2]);
                builder.Append(builder2.ToString() + " ");
            }
            File.AppendAllText(directory, builder.ToString());
            builder.Clear();

            File.AppendAllText(directory, "\nTriangle_Indices: ");
            foreach (var item in tris)
            {
                builder.Append(item + " ");
            }
            File.AppendAllText(directory, builder.ToString());
            builder.Clear();

            File.AppendAllText(directory, "\nNormals: ");
            foreach (var item in normals)
            {
                builder.Append(item + " ");
            }
            File.AppendAllText(directory, builder.ToString());
            builder.Clear();

            File.AppendAllText(directory, "\nTexture_Coordinates: ");
            foreach (var item in textCoords)
            {
                builder.Append(item + " ");
            }
            File.AppendAllText(directory, builder.ToString());
            builder.Clear();
        }

        public static MeshGeometry3D SaifToMesh(string directory)
        {
            Point3DCollection verts = new Point3DCollection();
            Int32Collection tris = new Int32Collection();
            Vector3DCollection normals = new Vector3DCollection();
            PointCollection textureCoordinates = new PointCollection();

            StreamReader reader = new StreamReader(directory);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string IdentifiedChar = line.Substring(0, 3);
                string[] CurrentLine;

                switch (IdentifiedChar)
                {
                    case "Ver":
                        try
                        {
                            string vertices = line;
                            CurrentLine = vertices.Split(' ');

                            for (int i = 1; i < CurrentLine.Length - 1; i++)
                            {
                                string[] vertStr = CurrentLine[i].Split(',');

                                Point3D vert = new Point3D()
                                {
                                    X = double.Parse(vertStr[0].ToString()),
                                    Y = double.Parse(vertStr[1].ToString()),
                                    Z = double.Parse(vertStr[2].ToString()),
                                };

                                verts.Add(vert);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                            Environment.Exit(0);
                        }
                        break;
                    case "Tri":
                        try
                        {
                            string triangles = line;
                            CurrentLine = triangles.Split(' ');

                            for (int i = 1; i < CurrentLine.Length - 1; i++)
                            {
                                tris.Add(int.Parse(CurrentLine[i]));
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                            Environment.Exit(0);
                        }
                        break;
                    case "Nor":
                        try
                        {
                            string normalz = line;
                            CurrentLine = normalz.Split(' ');

                            for (int i = 1; i < CurrentLine.Length - 1; i++)
                            {
                                string[] normStr = CurrentLine[i].Split(',');

                                Vector3D norm = new Vector3D()
                                {
                                    X = double.Parse(normStr[0].ToString()),
                                    Y = double.Parse(normStr[1].ToString()),
                                    Z = double.Parse(normStr[2].ToString()),
                                };

                                normals.Add(norm);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                            Environment.Exit(0);
                        }
                        break;
                    case "Tex":
                        try
                        {
                            string textCoords = line;
                            CurrentLine = textCoords.Split(' ');

                            for (int i = 1; i < CurrentLine.Length - 1; i++)
                            {
                                string[] coordStr = CurrentLine[i].Split(',');

                                Point coord = new Point()
                                {
                                    X = double.Parse(coordStr[0]),
                                    Y = double.Parse(coordStr[1])
                                };

                                textureCoordinates.Add(coord);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                            Environment.Exit(0);
                        }
                        break;
                    default:
                        break;
                }
            }
            reader.Close();

            MeshGeometry3D loadedMesh = new MeshGeometry3D()
            {
                Positions = verts,
                TriangleIndices = tris,
                Normals = normals,
                TextureCoordinates = textureCoordinates
            };

            return loadedMesh;
        }

        public static void MeshesToSaif(MeshGeometry3D[] meshes, string directory)
        {
            File.WriteAllText(directory, "");

            foreach (MeshGeometry3D mesh in meshes)
            {
                Point3D[] points = mesh.Positions.ToArray();
                int[] tris = mesh.TriangleIndices.ToArray();
                Vector3D[] normals = mesh.Normals.ToArray();
                Point[] textCoords = mesh.TextureCoordinates.ToArray();

                StringBuilder builder = new StringBuilder();

                File.AppendAllText(directory, "Vertices: ");
                foreach (var item in points)
                {
                    string[] point = new string[3];
                    point[0] = item.X.ToString();
                    point[1] = item.Y.ToString();
                    point[2] = item.Z.ToString();

                    StringBuilder builder2 = new StringBuilder();
                    builder2.Append(point[0] + "," + point[1] + "," + point[2]);
                    builder.Append(builder2.ToString() + " ");
                }
                File.AppendAllText(directory, builder.ToString());
                builder.Clear();

                File.AppendAllText(directory, "\nTriangle_Indices: ");
                foreach (var item in tris)
                {
                    builder.Append(item + " ");
                }
                File.AppendAllText(directory, builder.ToString());
                builder.Clear();

                File.AppendAllText(directory, "\nNormals: ");
                foreach (var item in normals)
                {
                    builder.Append(item + " ");
                }
                File.AppendAllText(directory, builder.ToString());
                builder.Clear();

                File.AppendAllText(directory, "\nTexture_Coordinates: ");
                foreach (var item in textCoords)
                {
                    builder.Append(item + " ");
                }
                File.AppendAllText(directory, builder.ToString() + "\nEND-MESH\n");
                builder.Clear();
            }
        }

        public static MeshGeometry3D[] SaifToMeshes(string directory, string extradirectory)
        {
            List<MeshGeometry3D> meshes = new List<MeshGeometry3D>();

            StreamReader reader = new StreamReader(directory);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Substring(0, 3) != "END")
                {
                    File.AppendAllText(extradirectory, line + "\n");
                }
                else
                {
                    MeshGeometry3D mesh = SaifToMesh(extradirectory);
                    meshes.Add(mesh);
                    File.WriteAllText(extradirectory, "");
                }
            }
            reader.Close();

            File.Delete(extradirectory);

            return meshes.ToArray();
        }
    }
}