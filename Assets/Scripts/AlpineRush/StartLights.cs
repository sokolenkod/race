using UnityEngine;

namespace AlpineRush
{
    public class StartLights : MonoBehaviour
    {
        public Renderer[] Lamps;
        static readonly Color Red = new Color(1f, 0.12f, 0.08f);
        static readonly Color Green = new Color(0.25f, 1f, 0.35f);
        static readonly Color Off = new Color(0.15f, 0.05f, 0.05f);

        public void ShowCount(int lit, bool go)
        {
            if (Lamps == null) return;
            for (int i = 0; i < Lamps.Length; i++)
            {
                if (Lamps[i] == null) continue;
                Color color = go ? Green : (i < lit ? Red : Off);
                float glow = go || i < lit ? 3.2f : 0.2f;
                var mat = Lamps[i].material;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                mat.color = color;
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color * glow);
            }
        }
    }
}
