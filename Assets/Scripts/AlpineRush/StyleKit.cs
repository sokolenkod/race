using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace AlpineRush
{
    public static class StyleKit
    {
        static Shader lit;
        static Shader unlit;
        static Shader particle;

        public static Shader LitShader => lit != null ? lit : lit = Shader.Find("Universal Render Pipeline/Lit");
        public static Shader UnlitShader => unlit != null ? unlit : unlit = Shader.Find("Universal Render Pipeline/Unlit");
        public static Shader ParticleShader => particle != null ? particle : particle = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        public static Material Lit(Color color, float smoothness, float metallic, Texture tex = null)
        {
            var shader = LitShader != null ? LitShader : Shader.Find("Standard");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            }
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            mat.enableInstancing = true;
            return mat;
        }

        public static Material Unlit(Color color)
        {
            var shader = UnlitShader != null ? UnlitShader : Shader.Find("Unlit/Color");
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            mat.enableInstancing = true;
            return mat;
        }

        public static Material Emissive(Color color, float intensity)
        {
            var mat = Lit(color, 0.4f, 0f);
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * intensity);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return mat;
        }

        public static Material Particle(Color color)
        {
            var shader = ParticleShader != null ? ParticleShader : UnlitShader;
            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            return mat;
        }

        public static Texture2D Noise(int size, Color a, Color b, float frequency, int seed, bool streaks = false)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var rng = new System.Random(seed);
            float ox = (float)rng.NextDouble() * 40f;
            float oy = (float)rng.NextDouble() * 40f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(ox + x * frequency, oy + y * frequency);
                    n += 0.35f * Mathf.PerlinNoise(ox + x * frequency * 3.1f, oy + y * frequency * 3.1f);
                    n /= 1.35f;
                    if (streaks) n = Mathf.Clamp01(n * 0.85f + 0.08f * Mathf.Sin(x * 0.7f + n * 6f));
                    tex.SetPixel(x, y, Color.Lerp(a, b, n));
                }
            }
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.anisoLevel = 4;
            tex.Apply();
            return tex;
        }

        public static void ApplyAtmosphere(bool race)
        {
            var sky = Shader.Find("Skybox/Procedural");
            if (sky != null)
            {
                var mat = new Material(sky);
                mat.SetFloat("_SunSize", 0.035f);
                mat.SetFloat("_SunSizeConvergence", 5f);
                mat.SetFloat("_AtmosphereThickness", race ? 1.05f : 1.15f);
                mat.SetColor("_SkyTint", new Color(0.45f, 0.58f, 0.78f));
                mat.SetColor("_GroundColor", new Color(0.32f, 0.30f, 0.28f));
                mat.SetFloat("_Exposure", 1.15f);
                RenderSettings.skybox = mat;
            }

            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.62f, 0.70f, 0.78f);
            RenderSettings.fogStartDistance = race ? 70f : 28f;
            RenderSettings.fogEndDistance = race ? 520f : 180f;
            RenderSettings.reflectionIntensity = 0.6f;

            QualitySettings.shadowDistance = race ? 240f : 80f;
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowCascades = 2;

            var sunGo = new GameObject("Sun");
            var light = sunGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.93f, 0.82f);
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.82f;
            light.shadowBias = 0.04f;
            light.shadowNormalBias = 0.35f;
            sunGo.transform.rotation = Quaternion.Euler(42f, -38f, 0f);
            RenderSettings.sun = light;
            sunGo.AddComponent<UniversalAdditionalLightData>();

            var volumeGo = new GameObject("PostProcessing");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            var profile = volume.profile;

            var bloom = profile.Add<Bloom>();
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(race ? 0.4f : 0.55f);
            bloom.scatter.Override(0.62f);

            var color = profile.Add<ColorAdjustments>();
            color.postExposure.Override(0.18f);
            color.contrast.Override(16f);
            color.saturation.Override(14f);
            color.colorFilter.Override(new Color(1f, 0.97f, 0.92f));

            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(0.26f);
            vignette.smoothness.Override(0.42f);

            var tone = profile.Add<Tonemapping>();
            tone.mode.Override(TonemappingMode.ACES);
        }

        public static Camera CreateCamera(string name, bool post)
        {
            var go = new GameObject(name);
            var cam = go.AddComponent<Camera>();
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 900f;
            cam.fieldOfView = 64f;
            cam.allowHDR = true;
            cam.clearFlags = CameraClearFlags.Skybox;
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = post;
            data.renderShadows = true;
            data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            data.antialiasingQuality = AntialiasingQuality.High;
            int blip = 1 << AlpineLayers.MinimapBlip;
            cam.cullingMask = post ? ~blip : ~0;
            return cam;
        }
    }
}
