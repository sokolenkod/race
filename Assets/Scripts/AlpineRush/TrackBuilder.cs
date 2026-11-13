using System.Collections.Generic;
using UnityEngine;

namespace AlpineRush
{
    public sealed class TrackWorld
    {
        public TrackData Track;
        public StartLights Lights;
    }

    public static class TrackBuilder
    {
        public static TrackWorld Build()
        {
            PropLibrary.EnsureLoaded();
            var track = TrackData.Create();
            var world = new TrackWorld { Track = track };
            var root = new GameObject("MountainPass");

            var asphalt = StyleKit.Noise(128, new Color(0.10f, 0.10f, 0.11f), new Color(0.22f, 0.22f, 0.23f), 0.08f, 3, true);
            var dirtTex = StyleKit.Noise(64, new Color(0.38f, 0.28f, 0.16f), new Color(0.52f, 0.40f, 0.24f), 0.12f, 9);
            var grass = StyleKit.Noise(64, new Color(0.22f, 0.38f, 0.16f), new Color(0.40f, 0.55f, 0.22f), 0.1f, 4);
            var rock = StyleKit.Noise(64, new Color(0.38f, 0.36f, 0.34f), new Color(0.58f, 0.55f, 0.52f), 0.14f, 5);
            var snow = StyleKit.Noise(64, new Color(0.82f, 0.86f, 0.9f), new Color(0.95f, 0.97f, 1f), 0.08f, 6);

            BuildRoad(root.transform, track, asphalt, dirtTex);
            BuildLines(root.transform, track);
            BuildGuardrails(root.transform, track);
            BuildTerrain(root.transform, track, grass, dirtTex, rock, snow);
            ScatterProps(root.transform, track);
            world.Lights = BuildGantry(root.transform, track);
            BuildCheckpoints(root.transform, track);
            BuildNitroPads(root.transform, track);
            BuildChevrons(root.transform, track);
            return world;
        }

        static void BuildRoad(Transform parent, TrackData track, Texture asphalt, Texture dirt)
        {
            int n = track.Samples.Length;
            var verts = new List<Vector3>(n * 4);
            var uvs = new List<Vector2>(n * 4);
            var center = new List<int>();
            var shoulder = new List<int>();
            float half = track.HalfWidth;
            float outer = half + track.Shoulder;

            for (int i = 0; i < n; i++)
            {
                var s = track.Samples[i];
                Vector3 up = s.Normal;
                verts.Add(s.Position - s.Right * outer - up * 0.14f);
                verts.Add(s.Position - s.Right * half);
                verts.Add(s.Position + s.Right * half);
                verts.Add(s.Position + s.Right * outer - up * 0.14f);
                float v = s.Distance * 0.06f;
                uvs.Add(new Vector2(0f, v));
                uvs.Add(new Vector2(0.2f, v));
                uvs.Add(new Vector2(0.8f, v));
                uvs.Add(new Vector2(1f, v));
            }

            for (int i = 0; i < n; i++)
            {
                int a = i * 4;
                int b = ((i + 1) % n) * 4;
                center.Add(a + 1); center.Add(b + 2); center.Add(a + 2);
                center.Add(a + 1); center.Add(b + 1); center.Add(b + 2);
                shoulder.Add(a); shoulder.Add(b + 1); shoulder.Add(a + 1);
                shoulder.Add(a); shoulder.Add(b); shoulder.Add(b + 1);
                shoulder.Add(a + 2); shoulder.Add(b + 3); shoulder.Add(a + 3);
                shoulder.Add(a + 2); shoulder.Add(b + 2); shoulder.Add(b + 3);
            }

            var mesh = new Mesh { name = "Road" };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(center, 0);
            mesh.SetTriangles(shoulder, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            FlipIfDown(mesh);

            var go = new GameObject("Road");
            go.transform.SetParent(parent, false);
            go.layer = AlpineLayers.Road;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = new[]
            {
                StyleKit.Lit(Color.white, 0.16f, 0f, asphalt),
                StyleKit.Lit(Color.white, 0.08f, 0f, dirt)
            };
            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;

            BuildSkirts(parent, track);
        }

        static void BuildSkirts(Transform parent, TrackData track)
        {
            int n = track.Samples.Length;
            var verts = new List<Vector3>();
            var tris = new List<int>();
            float outer = track.HalfWidth + track.Shoulder;
            for (int side = -1; side <= 1; side += 2)
            {
                int start = verts.Count;
                for (int i = 0; i < n; i++)
                {
                    var s = track.Samples[i];
                    Vector3 edge = s.Position + s.Right * (outer * side) - s.Normal * 0.14f;
                    verts.Add(edge);
                    verts.Add(edge - s.Normal * 1.6f);
                }
                for (int i = 0; i < n; i++)
                {
                    int a = start + i * 2;
                    int b = start + ((i + 1) % n) * 2;
                    if (side < 0)
                    {
                        tris.Add(a); tris.Add(a + 1); tris.Add(b);
                        tris.Add(b); tris.Add(a + 1); tris.Add(b + 1);
                    }
                    else
                    {
                        tris.Add(a); tris.Add(b); tris.Add(a + 1);
                        tris.Add(b); tris.Add(b + 1); tris.Add(a + 1);
                    }
                }
            }
            var mesh = MeshUtil.Build(verts, tris, "Skirts");
            var go = new GameObject("RoadSkirt");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var skirtMat = StyleKit.Lit(new Color(0.32f, 0.28f, 0.24f), 0.05f, 0f);
            skirtMat.SetFloat("_Cull", 0f);
            go.AddComponent<MeshRenderer>().sharedMaterial = skirtMat;
        }

        static void BuildLines(Transform parent, TrackData track)
        {
            var white = new List<Vector3>();
            var whiteTris = new List<int>();
            var yellow = new List<Vector3>();
            var yellowTris = new List<int>();
            int n = track.Samples.Length;
            float half = track.HalfWidth - 0.35f;
            for (int i = 0; i < n; i++)
            {
                var a = track.Samples[i];
                var b = track.Samples[(i + 1) % n];
                AddRibbon(white, whiteTris, a, b, -half, 0.16f);
                AddRibbon(white, whiteTris, a, b, half, 0.16f);
                if (Mathf.Repeat(a.Distance, 8f) < 3.4f)
                    AddRibbon(yellow, yellowTris, a, b, 0f, 0.18f);
            }
            MakeLine(parent, white, whiteTris, "EdgeLines", new Color(0.92f, 0.92f, 0.9f));
            MakeLine(parent, yellow, yellowTris, "CenterLine", new Color(0.95f, 0.78f, 0.15f));
        }

        static void AddRibbon(List<Vector3> verts, List<int> tris, TrackSample a, TrackSample b, float lateral, float width)
        {
            int o = verts.Count;
            Vector3 liftA = a.Normal * 0.045f;
            Vector3 liftB = b.Normal * 0.045f;
            verts.Add(a.Position + a.Right * (lateral - width) + liftA);
            verts.Add(a.Position + a.Right * (lateral + width) + liftA);
            verts.Add(b.Position + b.Right * (lateral + width) + liftB);
            verts.Add(b.Position + b.Right * (lateral - width) + liftB);
            tris.Add(o); tris.Add(o + 1); tris.Add(o + 2);
            tris.Add(o); tris.Add(o + 2); tris.Add(o + 3);
        }

        static void MakeLine(Transform parent, List<Vector3> verts, List<int> tris, string name, Color color)
        {
            if (verts.Count == 0) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = MeshUtil.Build(verts, tris, name);
            go.AddComponent<MeshRenderer>().sharedMaterial = StyleKit.Unlit(color);
        }

        static void BuildGuardrails(Transform parent, TrackData track)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var holder = new GameObject("Guardrails");
            holder.transform.SetParent(parent, false);
            var metal = StyleKit.Lit(new Color(0.62f, 0.64f, 0.66f), 0.55f, 0.65f);
            var red = StyleKit.Emissive(new Color(0.8f, 0.05f, 0.04f), 1.5f);
            int n = track.Samples.Length;
            float side = track.HalfWidth + track.Shoulder + 0.15f;
            float step = 8f;
            float nextPost = 0f;
            for (int i = 0; i < n; i++)
            {
                var a = track.Samples[i];
                var b = track.Samples[(i + 1) % n];
                for (int s = -1; s <= 1; s += 2)
                {
                    Quaternion rot = Quaternion.LookRotation(a.Tangent, a.Normal);
                    Vector3 origin = a.Position + a.Right * (side * s);
                    Vector3 along = b.Position - a.Position;
                    float len = Mathf.Max(0.5f, along.magnitude + 0.15f);
                    MeshUtil.AddBox(verts, tris, origin + a.Normal * 0.72f, rot, new Vector3(0.1f, 0.08f, len));
                    MeshUtil.AddBox(verts, tris, origin + a.Normal * 0.38f, rot, new Vector3(0.08f, 0.07f, len));
                }

                if (a.Distance + 0.01f >= nextPost || i == 0)
                {
                    nextPost = a.Distance + step;
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Quaternion rot = Quaternion.LookRotation(a.Tangent, a.Normal);
                        Vector3 origin = a.Position + a.Right * (side * s);
                        MeshUtil.AddBox(verts, tris, origin + a.Normal * 0.42f, rot, new Vector3(0.12f, 0.84f, 0.12f));

                        var box = new GameObject("RailHit");
                        box.transform.SetParent(holder.transform, false);
                        box.transform.SetPositionAndRotation(origin + a.Normal * 0.45f, rot);
                        var col = box.AddComponent<BoxCollider>();
                        col.size = new Vector3(0.28f, 0.9f, step + 0.4f);

                        if (Mathf.Repeat(a.Distance, 16f) < step)
                        {
                            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            Object.Destroy(marker.GetComponent<Collider>());
                            marker.transform.SetParent(holder.transform, false);
                            marker.transform.SetPositionAndRotation(origin + a.Normal * 0.78f + a.Right * (0.12f * s), rot);
                            marker.transform.localScale = new Vector3(0.16f, 0.12f, 0.18f);
                            marker.GetComponent<Renderer>().sharedMaterial = red;
                        }
                    }
                }
            }

            var meshGo = new GameObject("RailMesh");
            meshGo.transform.SetParent(holder.transform, false);
            meshGo.AddComponent<MeshFilter>().sharedMesh = MeshUtil.Build(verts, tris, "Rails");
            meshGo.AddComponent<MeshRenderer>().sharedMaterial = metal;
        }

        static void BuildTerrain(Transform parent, TrackData track, Texture grass, Texture dirt, Texture rock, Texture snow)
        {
            float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
            for (int i = 0; i < track.Samples.Length; i++)
            {
                var p = track.Samples[i].Position;
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
            }
            float pad = 250f;
            minX -= pad; maxX += pad; minZ -= pad; maxZ += pad;
            float sizeX = maxX - minX;
            float sizeZ = maxZ - minZ;
            float sizeY = 150f;
            int res = 257;

            Vector3 centroid = Vector3.zero;
            for (int i = 0; i < track.Samples.Length; i++) centroid += track.Samples[i].Position;
            centroid /= track.Samples.Length;

            var peaks = new List<Vector4>();
            var rng = new System.Random(20261113);
            for (int i = 0; i < 10; i++)
            {
                float ang = (float)rng.NextDouble() * Mathf.PI * 2f;
                float rad = 280f + (float)rng.NextDouble() * 140f;
                peaks.Add(new Vector4(centroid.x + Mathf.Cos(ang) * rad, 0f, centroid.z + Mathf.Sin(ang) * rad, 28f + (float)rng.NextDouble() * 36f));
            }
            for (int i = 0; i < 3; i++)
            {
                peaks.Add(new Vector4(
                    centroid.x + (float)(rng.NextDouble() - 0.5) * 80f,
                    0f,
                    centroid.z + (float)(rng.NextDouble() - 0.5) * 60f,
                    18f + (float)rng.NextDouble() * 16f));
            }

            Vector2 lake = new Vector2(centroid.x - 210f, centroid.z - 40f);
            int lakeIndex = track.NearestIndex(lake.x, lake.y);
            var lakeSample = track.Samples[lakeIndex];
            Vector2 lakeDelta = lake - new Vector2(lakeSample.Position.x, lakeSample.Position.z);
            float lakeSide = lakeSample.Tangent.x * lakeDelta.y - lakeSample.Tangent.z * lakeDelta.x;
            bool lakeOutside = lakeSide < 0f;
            float lakeLevel = 9.5f;

            var heights = new float[res, res];
            for (int z = 0; z < res; z++)
            {
                float wz = minZ + sizeZ * z / (res - 1f);
                for (int x = 0; x < res; x++)
                {
                    float wx = minX + sizeX * x / (res - 1f);
                    int idx = track.NearestIndex(wx, wz);
                    var s = track.Samples[idx];
                    float dx = wx - s.Position.x;
                    float dz = wz - s.Position.z;
                    float dist = Mathf.Sqrt(dx * dx + dz * dz);
                    float side = s.Tangent.x * dz - s.Tangent.z * dx;
                    bool inside = side > 0f;
                    float roadY = s.Position.y;
                    float half = track.HalfWidth + track.Shoulder;

                    float h;
                    if (dist < half + 1.1f)
                    {
                        h = roadY - 0.9f;
                    }
                    else if (dist < half + 18f)
                    {
                        float t = Mathf.InverseLerp(half + 1.1f, half + 18f, dist);
                        t = t * t * (3f - 2f * t);
                        float far = inside
                            ? roadY + 3f + t * 12f
                            : Mathf.Lerp(roadY - 2f, Valley(wx, wz, peaks), t);
                        h = Mathf.Lerp(roadY - 0.45f, far, t);
                    }
                    else if (inside)
                    {
                        h = roadY + 8f + Mathf.Clamp((dist - half) * 0.38f, 0f, 58f);
                        h += (Mathf.PerlinNoise(wx * 0.01f, wz * 0.01f) - 0.35f) * 10f;
                        h += Summit(wx, wz, peaks) * 0.45f;
                    }
                    else
                    {
                        h = Valley(wx, wz, peaks);
                    }

                    if (lakeOutside && (wx - lake.x) * (wx - lake.x) + (wz - lake.y) * (wz - lake.y) < 34f * 34f && dist > half + 20f)
                        h = lakeLevel - 1.2f;

                    if (dist > half + 3f)
                        h += (Mathf.PerlinNoise(wx * 0.045f + 8f, wz * 0.045f) - 0.5f) * 1.6f;

                    heights[z, x] = Mathf.Clamp(h / sizeY, 0.01f, 0.98f);
                }
            }

            var data = new TerrainData();
            data.heightmapResolution = res;
            data.size = new Vector3(sizeX, sizeY, sizeZ);
            data.SetHeights(0, 0, heights);
            data.alphamapResolution = 256;

            var layers = new TerrainLayer[4];
            layers[0] = Layer(grass, new Vector2(14f, 14f));
            layers[1] = Layer(dirt, new Vector2(10f, 10f));
            layers[2] = Layer(rock, new Vector2(9f, 9f));
            layers[3] = Layer(snow, new Vector2(16f, 16f));
            data.terrainLayers = layers;

            int alphaRes = data.alphamapResolution;
            var alpha = new float[alphaRes, alphaRes, 4];
            for (int z = 0; z < alphaRes; z++)
            {
                for (int x = 0; x < alphaRes; x++)
                {
                    float nx = x / (alphaRes - 1f);
                    float nz = z / (alphaRes - 1f);
                    float wx = minX + sizeX * nx;
                    float wz = minZ + sizeZ * nz;
                    float h = data.GetInterpolatedHeight(nx, nz);
                    float slope = data.GetSteepness(nx, nz);
                    int idx = track.NearestIndex(wx, wz);
                    float dist = Vector2.Distance(new Vector2(wx, wz), new Vector2(track.Samples[idx].Position.x, track.Samples[idx].Position.z));
                    float snowW = Mathf.Clamp01((h - 40f) / 8f);
                    float rockW = Mathf.Clamp01((slope - 24f) / 20f);
                    float dirtW = Mathf.Clamp01(1f - (dist - (track.HalfWidth + 2f)) / 14f);
                    float grassW = 1f;
                    float sum = grassW + dirtW + rockW + snowW;
                    alpha[z, x, 0] = grassW / sum;
                    alpha[z, x, 1] = dirtW / sum;
                    alpha[z, x, 2] = rockW / sum;
                    alpha[z, x, 3] = snowW / sum;
                }
            }
            data.SetAlphamaps(0, 0, alpha);

            var terrainGo = Terrain.CreateTerrainGameObject(data);
            terrainGo.name = "Mountains";
            terrainGo.transform.SetParent(parent, false);
            terrainGo.transform.position = new Vector3(minX, 0f, minZ);
            var terrain = terrainGo.GetComponent<Terrain>();
            terrain.heightmapPixelError = 7f;
            terrain.basemapDistance = 2500f;
            terrain.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

            if (lakeOutside)
            {
                var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.Destroy(water.GetComponent<Collider>());
                water.name = "AlpineLake";
                water.transform.SetParent(parent, false);
                water.transform.position = new Vector3(lake.x, lakeLevel, lake.y);
                water.transform.localScale = new Vector3(62f, 0.15f, 62f);
                var mat = StyleKit.Lit(new Color(0.15f, 0.38f, 0.52f, 0.85f), 0.82f, 0.05f);
                water.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        static float Valley(float x, float z, List<Vector4> peaks)
        {
            float h = 7f + Mathf.PerlinNoise(x * 0.004f, z * 0.004f) * 12f;
            h += Mathf.PerlinNoise(x * 0.015f + 4f, z * 0.015f) * 5f;
            h += Summit(x, z, peaks);
            return h;
        }

        static float Summit(float x, float z, List<Vector4> peaks)
        {
            float h = 0f;
            for (int i = 0; i < peaks.Count; i++)
            {
                float dx = x - peaks[i].x;
                float dz = z - peaks[i].z;
                float sigma = 55f + (i % 3) * 12f;
                h += peaks[i].w * Mathf.Exp(-(dx * dx + dz * dz) / (2f * sigma * sigma));
            }
            return h;
        }

        static TerrainLayer Layer(Texture tex, Vector2 tile)
        {
            return new TerrainLayer
            {
                diffuseTexture = tex,
                tileSize = tile,
                smoothness = 0.08f
            };
        }

        static void ScatterProps(Transform parent, TrackData track)
        {
            var rng = new System.Random(481516);
            var holder = new GameObject("Props");
            holder.transform.SetParent(parent, false);
            var terrain = Terrain.activeTerrain;
            if (terrain == null) return;
            var data = terrain.terrainData;
            Vector3 origin = terrain.transform.position;
            int trees = 0, rocks = 0, plants = 0;
            int attempts = 0;
            while ((trees < 150 || rocks < 80 || plants < 40) && attempts < 2500)
            {
                attempts++;
                float x = origin.x + (float)rng.NextDouble() * data.size.x;
                float z = origin.z + (float)rng.NextDouble() * data.size.z;
                int idx = track.NearestIndex(x, z);
                var s = track.Samples[idx];
                float dist = Vector2.Distance(new Vector2(x, z), new Vector2(s.Position.x, s.Position.z));
                if (dist < track.HalfWidth + track.Shoulder + 2.5f) continue;
                float nx = (x - origin.x) / data.size.x;
                float nz = (z - origin.z) / data.size.z;
                float y = terrain.SampleHeight(new Vector3(x, 0f, z)) + origin.y;
                float slope = data.GetSteepness(nx, nz);
                float grove = Mathf.PerlinNoise(x * 0.02f, z * 0.02f);

                bool wantTree = trees < 150 && slope < 32f && y < 46f && grove > 0.38f && dist > 12f;
                bool wantRock = rocks < 80 && (slope > 18f || grove < 0.45f || y > 40f) && dist > 9f;
                bool wantPlant = plants < 40 && slope < 24f && y < 42f && dist > 10f && dist < 40f;

                if (wantTree && (trees <= rocks || !wantRock))
                {
                    PlaceNature(holder.transform, PropLibrary.Trees, x, y, z, 7f + (float)rng.NextDouble() * 8f, (float)rng.NextDouble() * 360f, dist < 16f);
                    trees++;
                }
                else if (wantRock)
                {
                    float height = dist > 30f && slope > 28f ? 4f + (float)rng.NextDouble() * 8f : 1.1f + (float)rng.NextDouble() * 2.6f;
                    PlaceNature(holder.transform, PropLibrary.Rocks, x, y, z, height, (float)rng.NextDouble() * 360f, dist < 14f);
                    rocks++;
                }
                else if (wantPlant)
                {
                    PlaceNature(holder.transform, PropLibrary.Plants, x, y, z, 0.6f + (float)rng.NextDouble() * 1.1f, (float)rng.NextDouble() * 360f, false);
                    plants++;
                }
            }
        }

        static void PlaceNature(Transform parent, IReadOnlyList<GameObject> prefabs, float x, float y, float z, float height, float yaw, bool solid)
        {
            GameObject go;
            if (prefabs != null && prefabs.Count > 0)
            {
                var prefab = prefabs[RandomIndex(prefabs.Count, x, z)];
                go = PropLibrary.SpawnProp(prefab, height);
            }
            else
            {
                go = new GameObject("ProcProp");
                var filter = go.AddComponent<MeshFilter>();
                filter.sharedMesh = height > 3f ? PropLibrary.FallbackCone : PropLibrary.FallbackRock;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = height > 3f
                    ? StyleKit.Lit(new Color(0.16f, 0.38f, 0.18f), 0.08f, 0f)
                    : StyleKit.Lit(new Color(0.45f, 0.43f, 0.4f), 0.12f, 0f);
                go.transform.localScale = Vector3.one * height;
            }

            go.transform.SetParent(parent, true);
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.position = Vector3.zero;
            var bounds = MeshUtil.WorldBounds(go);
            go.transform.position = new Vector3(x - bounds.center.x, y - bounds.min.y, z - bounds.center.z);
            go.isStatic = !solid;
            if (solid)
            {
                var box = go.AddComponent<BoxCollider>();
                var b = MeshUtil.WorldBounds(go);
                box.center = go.transform.InverseTransformPoint(b.center);
                Vector3 scale = go.transform.lossyScale;
                box.size = new Vector3(
                    b.size.x / Mathf.Max(0.001f, scale.x),
                    b.size.y / Mathf.Max(0.001f, scale.y),
                    b.size.z / Mathf.Max(0.001f, scale.z));
            }
        }

        static int RandomIndex(int count, float x, float z)
        {
            int h = Mathf.Abs(x.GetHashCode() ^ (z.GetHashCode() * 17));
            return h % count;
        }

        static StartLights BuildGantry(Transform parent, TrackData track)
        {
            var s = track.SampleAtDistance(2f);
            var root = new GameObject("StartGantry");
            root.transform.SetParent(parent, false);
            var metal = StyleKit.Lit(new Color(0.18f, 0.2f, 0.22f), 0.4f, 0.4f);
            float span = track.HalfWidth + 1.2f;
            var leftPillar = CreateBeam(root.transform, s.Position + s.Right * -span, s.Normal, new Vector3(0.45f, 5.2f, 0.45f), metal);
            var rightPillar = CreateBeam(root.transform, s.Position + s.Right * span, s.Normal, new Vector3(0.45f, 5.2f, 0.45f), metal);
            leftPillar.AddComponent<BoxCollider>();
            rightPillar.AddComponent<BoxCollider>();
            var beamPos = s.Position + s.Normal * 5.2f;
            var beam = CreateBeam(root.transform, beamPos, s.Normal, new Vector3(span * 2f + 1f, 0.35f, 0.5f), metal);
            beam.transform.rotation = Quaternion.LookRotation(s.Tangent, s.Normal);

            var lights = root.AddComponent<StartLights>();
            lights.Lamps = new Renderer[3];
            for (int i = 0; i < 3; i++)
            {
                var lamp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(lamp.GetComponent<Collider>());
                lamp.transform.SetParent(root.transform, false);
                lamp.transform.position = beamPos - s.Normal * 0.45f + s.Right * ((i - 1) * 1.1f);
                lamp.transform.localScale = Vector3.one * 0.42f;
                var mat = StyleKit.Emissive(new Color(0.15f, 0.05f, 0.05f), 0.2f);
                lamp.GetComponent<Renderer>().sharedMaterial = mat;
                lights.Lamps[i] = lamp.GetComponent<Renderer>();
            }
            lights.ShowCount(0, false);

            var banner = new GameObject("Banner");
            var canvas = banner.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            banner.transform.SetParent(root.transform, false);
            banner.transform.position = beamPos + s.Normal * 0.7f;
            banner.transform.rotation = Quaternion.LookRotation(-s.Tangent, Vector3.up);
            var rect = banner.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(520f, 90f);
            banner.transform.localScale = Vector3.one * 0.012f;
            var text = UiFactory.CreateText(banner.transform, "ALPINE RUSH", 64, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.35f));
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var checker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(checker.GetComponent<Collider>());
            checker.name = "FinishLine";
            checker.transform.SetParent(root.transform, false);
            var start = track.SampleAtDistance(0.2f);
            checker.transform.position = start.Position + start.Normal * 0.06f;
            checker.transform.rotation = Quaternion.LookRotation(start.Normal, start.Tangent);
            checker.transform.localScale = new Vector3(track.HalfWidth * 2f, 2.2f, 1f);
            checker.GetComponent<Renderer>().sharedMaterial = StyleKit.Unlit(Color.white);
            var tex = new Texture2D(8, 2, TextureFormat.RGBA32, false);
            for (int y = 0; y < 2; y++)
            for (int x = 0; x < 8; x++)
                tex.SetPixel(x, y, ((x + y) & 1) == 0 ? Color.white : new Color(0.08f, 0.08f, 0.08f));
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            var cmat = checker.GetComponent<Renderer>().material;
            if (cmat.HasProperty("_BaseMap")) cmat.SetTexture("_BaseMap", tex);
            cmat.mainTexture = tex;
            return lights;
        }

        static GameObject CreateBeam(Transform parent, Vector3 position, Vector3 up, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.position = position + up * (scale.y * 0.5f);
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static void BuildCheckpoints(Transform parent, TrackData track)
        {
            var orange = StyleKit.Emissive(new Color(1f, 0.45f, 0.05f), 0.8f);
            for (int i = 0; i < track.CheckpointDistances.Length; i++)
            {
                var s = track.SampleAtDistance(track.CheckpointDistances[i]);
                var go = new GameObject("Checkpoint_" + i);
                go.transform.SetParent(parent, false);
                go.transform.SetPositionAndRotation(s.Position + s.Normal * 1.5f, Quaternion.LookRotation(s.Tangent, s.Normal));
                var box = go.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(track.HalfWidth * 2f + 8f, 5f, 8f);
                var cp = go.AddComponent<Checkpoint>();
                cp.Index = i;

                PlaceCone(parent, s.Position + s.Right * (track.HalfWidth + 0.7f), s, orange);
                PlaceCone(parent, s.Position - s.Right * (track.HalfWidth + 0.7f), s, orange);
            }
        }

        static void PlaceCone(Transform parent, Vector3 position, TrackSample s, Material mat)
        {
            GameObject go = PropLibrary.Cone != null ? Object.Instantiate(PropLibrary.Cone) : new GameObject("Cone");
            if (PropLibrary.Cone == null)
            {
                go.AddComponent<MeshFilter>().sharedMesh = PropLibrary.FallbackCone;
                go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                go.transform.localScale = new Vector3(0.45f, 1.15f, 0.45f);
            }
            else PropLibrary.UpgradeMaterials(go, false);

            go.transform.SetParent(parent, false);
            go.transform.rotation = Quaternion.identity;
            go.transform.position = Vector3.zero;
            var bounds = MeshUtil.WorldBounds(go);
            go.transform.localScale *= 1.15f / Mathf.Max(0.05f, bounds.size.y);
            bounds = MeshUtil.WorldBounds(go);
            Vector3 seat = position + s.Normal * 0.02f;
            go.transform.position = new Vector3(seat.x - bounds.center.x, seat.y - bounds.min.y, seat.z - bounds.center.z);
        }

        static void BuildNitroPads(Transform parent, TrackData track)
        {
            float[] spots = { track.Length * 0.28f, track.Length * 0.63f };
            var mat = StyleKit.Emissive(new Color(0.2f, 0.75f, 1f), 2.2f);
            for (int i = 0; i < spots.Length; i++)
            {
                var s = track.SampleAtDistance(spots[i]);
                var go = new GameObject("NitroPad");
                go.transform.SetParent(parent, false);
                go.transform.position = s.Position + s.Normal * 0.4f;
                go.transform.rotation = Quaternion.LookRotation(s.Tangent, s.Normal);
                var trigger = go.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(3.2f, 1.2f, 6f);
                go.AddComponent<NitroPad>();
                var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.Destroy(pad.GetComponent<Collider>());
                pad.transform.SetParent(go.transform, false);
                pad.transform.localPosition = new Vector3(0f, -0.25f, 0f);
                pad.transform.localScale = new Vector3(2.4f, 0.08f, 5f);
                pad.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        static void BuildChevrons(Transform parent, TrackData track)
        {
            var mat = StyleKit.Emissive(new Color(1f, 0.55f, 0.05f), 1.4f);
            int n = track.Samples.Length;
            for (int i = 2; i < n; i += 6)
            {
                float curv = Mathf.Abs(track.Samples[i].Curvature);
                if (curv < 0.02f) continue;
                if (curv < Mathf.Abs(track.Samples[i - 2].Curvature)) continue;
                var s = track.Samples[i];
                float side = -Mathf.Sign(s.Curvature);
                if (Mathf.Abs(side) < 0.1f) side = 1f;
                Vector3 pos = s.Position + s.Right * side * (track.HalfWidth + 0.4f) + s.Normal * 1.15f;
                for (int c = 0; c < 3; c++)
                {
                    var chevron = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Object.Destroy(chevron.GetComponent<Collider>());
                    chevron.transform.SetParent(parent, false);
                    chevron.transform.position = pos - s.Tangent * (6f + c * 2.2f) + s.Right * side * (c * 0.15f);
                    chevron.transform.rotation = Quaternion.LookRotation(-s.Tangent, s.Normal) * Quaternion.Euler(0f, 28f * side, 0f);
                    chevron.transform.localScale = new Vector3(1.1f, 0.18f, 0.28f);
                    chevron.GetComponent<Renderer>().sharedMaterial = mat;
                }
            }
        }

        static void FlipIfDown(Mesh mesh)
        {
            var normals = mesh.normals;
            float sum = 0f;
            int step = Mathf.Max(1, normals.Length / 40);
            for (int i = 0; i < normals.Length; i += step) sum += normals[i].y;
            if (sum >= 0f) return;
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                var tris = mesh.GetTriangles(sub);
                for (int i = 0; i < tris.Length; i += 3)
                {
                    int tmp = tris[i];
                    tris[i] = tris[i + 1];
                    tris[i + 1] = tmp;
                }
                mesh.SetTriangles(tris, sub);
            }
            mesh.RecalculateNormals();
        }
    }
}
