using System.Collections.Generic;
using UnityEngine;

namespace AlpineRush
{
    public static class MeshUtil
    {
        public static Mesh Cone(int sides)
        {
            var mesh = new Mesh { name = "Cone" };
            var verts = new Vector3[sides + 2];
            var tris = new int[sides * 6];
            verts[0] = new Vector3(0f, 1f, 0f);
            verts[1] = Vector3.zero;
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2f / sides;
                verts[i + 2] = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            }
            int t = 0;
            for (int i = 0; i < sides; i++)
            {
                int a = i + 2;
                int b = (i + 1) % sides + 2;
                tris[t++] = 0; tris[t++] = b; tris[t++] = a;
                tris[t++] = 1; tris[t++] = a; tris[t++] = b;
            }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh Rock(int seed)
        {
            var rng = new System.Random(seed);
            var mesh = new Mesh { name = "Rock" };
            var verts = new List<Vector3>();
            var tris = new List<int>();
            int lat = 5;
            int lon = 7;
            verts.Add(Vector3.up);
            for (int y = 1; y < lat; y++)
            {
                float v = y / (float)lat;
                float py = Mathf.Cos(v * Mathf.PI);
                float r = Mathf.Sin(v * Mathf.PI);
                for (int x = 0; x < lon; x++)
                {
                    float u = x / (float)lon * Mathf.PI * 2f;
                    float n = 0.75f + (float)rng.NextDouble() * 0.5f;
                    verts.Add(new Vector3(Mathf.Cos(u) * r, py, Mathf.Sin(u) * r) * n);
                }
            }
            verts.Add(Vector3.down * (0.7f + (float)rng.NextDouble() * 0.3f));

            for (int x = 0; x < lon; x++)
            {
                tris.Add(0);
                tris.Add(1 + (x + 1) % lon);
                tris.Add(1 + x);
            }
            for (int y = 0; y < lat - 2; y++)
            {
                for (int x = 0; x < lon; x++)
                {
                    int a = 1 + y * lon + x;
                    int b = 1 + y * lon + (x + 1) % lon;
                    int c = a + lon;
                    int d = b + lon;
                    tris.Add(a); tris.Add(b); tris.Add(d);
                    tris.Add(a); tris.Add(d); tris.Add(c);
                }
            }
            int bottom = verts.Count - 1;
            int row = 1 + (lat - 2) * lon;
            for (int x = 0; x < lon; x++)
            {
                tris.Add(bottom);
                tris.Add(row + x);
                tris.Add(row + (x + 1) % lon);
            }
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static void AddBox(List<Vector3> verts, List<int> tris, Vector3 center, Quaternion rotation, Vector3 size)
        {
            Vector3 h = size * 0.5f;
            Vector3[] c =
            {
                new Vector3(-h.x, -h.y, -h.z), new Vector3(h.x, -h.y, -h.z),
                new Vector3(h.x, -h.y, h.z), new Vector3(-h.x, -h.y, h.z),
                new Vector3(-h.x, h.y, -h.z), new Vector3(h.x, h.y, -h.z),
                new Vector3(h.x, h.y, h.z), new Vector3(-h.x, h.y, h.z)
            };
            int o = verts.Count;
            for (int i = 0; i < 8; i++) verts.Add(center + rotation * c[i]);
            int[] faces =
            {
                0, 1, 2, 0, 2, 3,
                4, 7, 6, 4, 6, 5,
                0, 4, 5, 0, 5, 1,
                1, 5, 6, 1, 6, 2,
                2, 6, 7, 2, 7, 3,
                3, 7, 4, 3, 4, 0
            };
            for (int i = 0; i < faces.Length; i++) tris.Add(o + faces[i]);
        }

        public static Mesh Build(List<Vector3> verts, List<int> tris, string name)
        {
            var mesh = new Mesh { name = name };
            if (verts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Bounds WorldBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }
    }
}
