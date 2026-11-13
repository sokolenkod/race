using UnityEngine;

namespace AlpineRush
{
    /// <summary>
    /// Original 16-second alpine pad (Dm - Bb - F - C) generated on the audio thread.
    /// Vehicle engine, skid and impact clips are separate Kenney recordings.
    /// </summary>
    public class MusicBed : MonoBehaviour
    {
        public float Gain = 0.16f;

        AudioSource source;
        int rate = 44100;
        int step;
        double phaseA;
        double phaseB;
        double phaseC;
        double phaseMelody;
        float low;

        static readonly int[] ChordRoots = { 50, 46, 53, 48 };
        static readonly int[] Melody = {
            0, 3, 7, 3, 10, 7, 3, 0,
            0, 3, 5, 7, 3, 0, -2, 0
        };

        void Awake()
        {
            rate = AudioSettings.outputSampleRate;
            if (rate < 22050) rate = 44100;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 1f;
            source.dopplerLevel = 0f;
            var silence = AudioClip.Create("music-driver", rate, 1, rate, false);
            silence.SetData(new float[rate], 0);
            source.clip = silence;
            source.Play();
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            int frames = data.Length / channels;
            double stepSeconds = 1.0 / rate;
            for (int i = 0; i < frames; i++)
            {
                int bar = (step / (rate * 4)) % 4;
                int note = (step / (rate / 2)) % Melody.Length;
                float root = Midi(ChordRoots[bar]);
                float third = Midi(ChordRoots[bar] + (bar == 0 || bar == 1 ? 3 : 4));
                float fifth = Midi(ChordRoots[bar] + 7);
                float melody = Midi(ChordRoots[bar] + 12 + Melody[note]);

                phaseA += root * stepSeconds;
                phaseB += fifth * stepSeconds;
                phaseC += third * stepSeconds;
                phaseMelody += melody * stepSeconds;

                float env = 0.55f + 0.45f * (float)System.Math.Sin(step * 2.0 * System.Math.PI / (rate * 4.0));
                float pad =
                    (float)System.Math.Sin(phaseA * 2.0 * System.Math.PI) * 0.45f +
                    (float)System.Math.Sin(phaseB * 2.0 * System.Math.PI) * 0.28f +
                    (float)System.Math.Sin(phaseC * 2.0 * System.Math.PI) * 0.18f;
                float mel = (float)System.Math.Sin(phaseMelody * 2.0 * System.Math.PI) * 0.22f * env;
                float sample = (pad + mel) * Gain;
                low += (sample - low) * 0.08f;
                sample = low;

                for (int c = 0; c < channels; c++) data[i * channels + c] += sample;
                step++;
            }
        }

        static float Midi(int midi)
        {
            return 440f * (float)System.Math.Pow(2.0, (midi - 69) / 12.0);
        }
    }
}
