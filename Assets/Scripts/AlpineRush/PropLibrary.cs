using System.Collections.Generic;
using UnityEngine;

namespace AlpineRush
{
    public static class PropLibrary
    {
        static bool loaded;
        static readonly Dictionary<string, GameObject> cars = new Dictionary<string, GameObject>();
        static readonly List<GameObject> trees = new List<GameObject>();
        static readonly List<GameObject> rocks = new List<GameObject>();
        static readonly List<GameObject> plants = new List<GameObject>();
        static GameObject cone;
        static Texture2D palette;
        static Mesh fallbackRock;
        static Mesh fallbackCone;

        public static IReadOnlyList<GameObject> Trees => trees;
        public static IReadOnlyList<GameObject> Rocks => rocks;
        public static IReadOnlyList<GameObject> Plants => plants;
        public static GameObject Cone => cone;
        public static bool HasCars => cars.Count > 0;

        public static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;

            var vehicleAssets = Resources.LoadAll<GameObject>("Vehicles");
            for (int i = 0; i < vehicleAssets.Length; i++)
            {
                if (vehicleAssets[i] == null) continue;
                if (vehicleAssets[i].name.ToLowerInvariant().Contains("cone"))
                {
                    cone = vehicleAssets[i];
                    continue;
                }
                cars[vehicleAssets[i].name] = vehicleAssets[i];
            }

            var props = Resources.LoadAll<GameObject>("Props");
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                if (prop == null) continue;
                string n = prop.name.ToLowerInvariant();
                if (n.Contains("cone")) cone = prop;
                else if (n.Contains("tree")) trees.Add(prop);
                else if (n.Contains("rock") || n.Contains("stone")) rocks.Add(prop);
                else plants.Add(prop);
            }

            fallbackRock = MeshUtil.Rock(7);
            fallbackCone = MeshUtil.Cone(8);
        }

        public static GameObject SpawnCarVisual(string preferred, float length)
        {
            EnsureLoaded();
            GameObject prefab = null;
            if (!string.IsNullOrEmpty(preferred)) cars.TryGetValue(preferred, out prefab);
            if (prefab == null)
            {
                foreach (var pair in cars)
                {
                    prefab = pair.Value;
                    break;
                }
            }

            GameObject model = prefab != null ? Object.Instantiate(prefab) : BuildBlockCar();
            model.name = prefab != null ? prefab.name : "BlockCar";
            CleanupImported(model);
            UpgradeMaterials(model, prefab != null);
            Fit(model, length, 0f, true);
            return model;
        }

        public static GameObject SpawnProp(GameObject prefab, float targetHeight)
        {
            var go = Object.Instantiate(prefab);
            CleanupImported(go);
            UpgradeMaterials(go, false);
            Fit(go, targetHeight, 0f, false);
            return go;
        }

        public static Mesh FallbackRock => fallbackRock != null ? fallbackRock : fallbackRock = MeshUtil.Rock(3);
        public static Mesh FallbackCone => fallbackCone != null ? fallbackCone : fallbackCone = MeshUtil.Cone(8);

        static GameObject BuildBlockCar()
        {
            var root = new GameObject("BlockCar");
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(1.7f, 0.55f, 3.6f);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.Destroy(cabin.GetComponent<Collider>());
            cabin.transform.SetParent(root.transform, false);
            cabin.transform.localScale = new Vector3(1.45f, 0.48f, 1.6f);
            cabin.transform.localPosition = new Vector3(0f, 1.15f, -0.15f);
            var mat = StyleKit.Lit(new Color(0.85f, 0.15f, 0.12f), 0.45f, 0.05f);
            body.GetComponent<Renderer>().sharedMaterial = mat;
            cabin.GetComponent<Renderer>().sharedMaterial = StyleKit.Lit(new Color(0.15f, 0.2f, 0.24f), 0.7f, 0f);
            for (int i = 0; i < 4; i++)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.Destroy(wheel.GetComponent<Collider>());
                wheel.name = i < 2 ? "wheel-front-" + i : "wheel-back-" + i;
                wheel.transform.SetParent(root.transform, false);
                wheel.transform.localScale = new Vector3(0.42f, 0.18f, 0.42f);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                float x = i % 2 == 0 ? -0.85f : 0.85f;
                float z = i < 2 ? 1.15f : -1.2f;
                wheel.transform.localPosition = new Vector3(x, 0.32f, z);
                wheel.GetComponent<Renderer>().sharedMaterial = StyleKit.Lit(new Color(0.08f, 0.08f, 0.08f), 0.2f, 0f);
            }
            return root;
        }

        static void Fit(GameObject model, float target, float bottomY, bool fitLength)
        {
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            var before = MeshUtil.WorldBounds(model);
            float current = fitLength ? Mathf.Max(before.size.x, before.size.z) : before.size.y;
            if (current < 0.001f) current = 1f;
            model.transform.localScale = Vector3.one * (target / current);
            var bounds = MeshUtil.WorldBounds(model);
            model.transform.position += new Vector3(-bounds.center.x, bottomY - bounds.min.y, -bounds.center.z);
        }

        static void CleanupImported(GameObject go)
        {
            foreach (var camera in go.GetComponentsInChildren<Camera>(true)) Object.Destroy(camera);
            foreach (var light in go.GetComponentsInChildren<Light>(true)) Object.Destroy(light);
            foreach (var listener in go.GetComponentsInChildren<AudioListener>(true)) Object.Destroy(listener);
            foreach (var collider in go.GetComponentsInChildren<Collider>(true)) Object.Destroy(collider);
            foreach (var body in go.GetComponentsInChildren<Rigidbody>(true)) Object.Destroy(body);
        }

        static Texture2D Palette()
        {
            if (palette == null) palette = Resources.Load<Texture2D>("Vehicles/Textures/colormap");
            return palette;
        }

        public static void UpgradeMaterials(GameObject go, bool vehicle)
        {
            var shader = StyleKit.LitShader;
            if (shader == null) return;
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int r = 0; r < renderers.Length; r++)
            {
                var source = renderers[r].sharedMaterials;
                var copy = new Material[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    var src = source[i];
                    if (src != null && src.shader != null && src.shader.name.Contains("Universal"))
                    {
                        copy[i] = src;
                        if (vehicle && src.HasProperty("_BaseMap") && src.GetTexture("_BaseMap") == null)
                        {
                            var pal = Palette();
                            if (pal != null)
                            {
                                var tinted = new Material(src);
                                tinted.SetTexture("_BaseMap", pal);
                                copy[i] = tinted;
                            }
                        }
                        continue;
                    }

                    var mat = new Material(shader);
                    Color color = Color.white;
                    Texture tex = null;
                    if (src != null)
                    {
                        if (src.HasProperty("_Color")) color = src.GetColor("_Color");
                        if (src.HasProperty("_BaseColor")) color = src.GetColor("_BaseColor");
                        if (src.HasProperty("_MainTex")) tex = src.GetTexture("_MainTex");
                        if (tex == null && src.HasProperty("_BaseMap")) tex = src.GetTexture("_BaseMap");
                    }
                    if (tex == null && vehicle) tex = Palette();
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    mat.color = color;
                    if (tex != null)
                    {
                        mat.SetTexture("_BaseMap", tex);
                        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                    }
                    string n = src != null ? src.name.ToLowerInvariant() : "";
                    float smooth = vehicle ? 0.42f : 0.18f;
                    if (n.Contains("leaf") || n.Contains("bark") || n.Contains("wood") || n.Contains("grass")) smooth = 0.08f;
                    if (n.Contains("glass") || n.Contains("window")) smooth = 0.82f;
                    mat.SetFloat("_Smoothness", smooth);
                    mat.SetFloat("_Metallic", 0f);
                    mat.enableInstancing = true;
                    copy[i] = mat;
                }
                renderers[r].sharedMaterials = copy;
                renderers[r].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderers[r].receiveShadows = true;
            }
        }
    }
}
