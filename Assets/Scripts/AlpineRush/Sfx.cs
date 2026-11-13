using UnityEngine;

namespace AlpineRush
{
    public static class Sfx
    {
        static AudioSource oneShot;

        public static AudioClip Tone(float frequency, float duration, float volume)
        {
            int rate = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(rate * duration));
            float[] data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Clamp01(t / 0.012f) * Mathf.Clamp01((duration - t) / 0.06f);
                float wave = Mathf.Sin(2f * Mathf.PI * frequency * t);
                float harm = Mathf.Sin(4f * Mathf.PI * frequency * t) * 0.25f;
                data[i] = (wave + harm) * env * volume;
            }

            var clip = AudioClip.Create("tone", count, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static void Play(AudioClip clip, float volume)
        {
            if (clip == null) return;
            if (oneShot == null)
            {
                var go = new GameObject("OneShotAudio");
                Object.DontDestroyOnLoad(go);
                oneShot = go.AddComponent<AudioSource>();
                oneShot.playOnAwake = false;
                oneShot.spatialBlend = 0f;
            }

            oneShot.PlayOneShot(clip, volume);
        }
    }
}
