using System.Collections.Generic;
using UnityEngine;

namespace AlpineRush
{
    public struct TrackSample
    {
        public Vector3 Position;
        public Vector3 Tangent;
        public Vector3 Right;
        public Vector3 Normal;
        public float Distance;
        public float Curvature;
    }

    public sealed class TrackData
    {
        public TrackSample[] Samples;
        public float Length;
        public float HalfWidth = 6.05f;
        public float Shoulder = 1.9f;
        public float[] CheckpointDistances;
        public const int CheckpointCount = 12;

        readonly Dictionary<long, List<int>> grid = new Dictionary<long, List<int>>();
        const float Cell = 28f;

        public static TrackData Create()
        {
            const float phase = 0.68940505f;
            const int rawCount = 900;
            var raw = new List<Vector3>(rawCount);
            for (int i = 0; i < rawCount; i++)
            {
                float t = phase + Mathf.PI * 2f * i / rawCount;
                raw.Add(EvaluateXZ(t));
            }

            float rawLength = 0f;
            var prefix = new float[rawCount];
            for (int i = 1; i < rawCount; i++)
            {
                rawLength += Vector3.Distance(raw[i - 1], raw[i]);
                prefix[i] = rawLength;
            }
            rawLength += Vector3.Distance(raw[rawCount - 1], raw[0]);

            var samples = new List<TrackSample>();
            float spacing = 2f;
            float cursor = 0f;
            int guard = 0;
            while (cursor < rawLength - spacing * 0.5f && guard++ < 5000)
            {
                Vector3 xz = PointAlong(raw, prefix, rawLength, cursor);
                float u = cursor / rawLength;
                float y = 12f + 38f * (0.5f - 0.5f * Mathf.Cos(Mathf.PI * 2f * u)) + 3.2f * Mathf.Sin(Mathf.PI * 4f * u);
                samples.Add(new TrackSample
                {
                    Position = new Vector3(xz.x, y, xz.z),
                    Distance = cursor
                });
                cursor += spacing;
            }

            var data = new TrackData();
            data.Samples = samples.ToArray();
            data.FinishFrame();
            return data;
        }

        static Vector3 EvaluateXZ(float t)
        {
            float wobble = 1f + 0.145f * Mathf.Sin(3f * t) + 0.05f * Mathf.Sin(5f * t + 0.4f);
            float x = Mathf.Cos(t) * 225f * wobble;
            float z = Mathf.Sin(t) * 152f * wobble;
            float serp = 18f * Mathf.Sin(4f * t);
            x += -Mathf.Sin(t) * serp;
            z += Mathf.Cos(t) * serp;
            return new Vector3(x, 0f, z);
        }

        static Vector3 PointAlong(List<Vector3> raw, float[] prefix, float length, float distance)
        {
            distance = Mathf.Repeat(distance, length);
            int hi = 1;
            while (hi < prefix.Length && prefix[hi] < distance) hi++;
            if (hi >= raw.Count)
            {
                float edge = length - prefix[prefix.Length - 1];
                float t = edge <= 0.0001f ? 0f : (distance - prefix[prefix.Length - 1]) / edge;
                return Vector3.Lerp(raw[raw.Count - 1], raw[0], Mathf.Clamp01(t));
            }
            int lo = hi - 1;
            float span = prefix[hi] - prefix[lo];
            float u = span <= 0.0001f ? 0f : (distance - prefix[lo]) / span;
            return Vector3.Lerp(raw[lo], raw[hi], Mathf.Clamp01(u));
        }

        void FinishFrame()
        {
            int n = Samples.Length;
            float length = 0f;
            for (int i = 0; i < n; i++)
            {
                Samples[i].Distance = length;
                length += Vector3.Distance(Samples[i].Position, Samples[(i + 1) % n].Position);
            }
            Length = length;

            for (int i = 0; i < n; i++)
            {
                Vector3 prev = Samples[(i - 1 + n) % n].Position;
                Vector3 next = Samples[(i + 1) % n].Position;
                Vector3 tangent = (next - prev);
                if (tangent.sqrMagnitude < 0.0001f) tangent = Samples[(i + 1) % n].Position - Samples[i].Position;
                tangent.Normalize();

                Vector3 flatA = Samples[i].Position - prev;
                Vector3 flatB = next - Samples[i].Position;
                flatA.y = 0f;
                flatB.y = 0f;
                float ang = Mathf.Atan2(
                    flatA.x * flatB.z - flatA.z * flatB.x,
                    Vector3.Dot(flatA, flatB));
                float ds = (flatA.magnitude + flatB.magnitude) * 0.5f;
                float curvature = ds > 0.001f ? ang / ds : 0f;
                float bank = Mathf.Clamp(-curvature * 55f, -9f, 9f);

                Quaternion frame = Quaternion.LookRotation(tangent, Vector3.up);
                frame = Quaternion.AngleAxis(bank, tangent) * frame;
                Samples[i].Tangent = frame * Vector3.forward;
                Samples[i].Right = frame * Vector3.right;
                Samples[i].Normal = frame * Vector3.up;
                Samples[i].Curvature = curvature;
            }

            CheckpointDistances = new float[CheckpointCount];
            for (int i = 0; i < CheckpointCount; i++)
                CheckpointDistances[i] = Length * i / CheckpointCount;

            for (int i = 0; i < n; i++)
            {
                long key = Key(Samples[i].Position.x, Samples[i].Position.z);
                if (!grid.TryGetValue(key, out var list))
                {
                    list = new List<int>();
                    grid[key] = list;
                }
                list.Add(i);
            }
        }

        public TrackSample SampleAtDistance(float distance)
        {
            distance = Mathf.Repeat(distance, Length);
            int lo = 0;
            int hi = Samples.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (Samples[mid].Distance < distance) lo = mid + 1;
                else hi = mid;
            }
            int i1 = lo % Samples.Length;
            int i0 = (i1 - 1 + Samples.Length) % Samples.Length;
            float d0 = Samples[i0].Distance;
            float d1 = Samples[i1].Distance;
            if (d1 < d0) d1 += Length;
            float span = Mathf.Max(0.0001f, d1 - d0);
            float u = Mathf.Clamp01((distance < d0 ? distance + Length : distance) - d0) / span;
            TrackSample a = Samples[i0];
            TrackSample b = Samples[i1];
            a.Position = Vector3.Lerp(a.Position, b.Position, u);
            a.Tangent = Vector3.Slerp(a.Tangent, b.Tangent, u).normalized;
            a.Right = Vector3.Slerp(a.Right, b.Right, u).normalized;
            a.Normal = Vector3.Slerp(a.Normal, b.Normal, u).normalized;
            a.Distance = distance;
            a.Curvature = Mathf.Lerp(a.Curvature, b.Curvature, u);
            return a;
        }

        public float Project(Vector3 world, float hint)
        {
            hint = Mathf.Repeat(hint, Length);
            int center = IndexAtDistance(hint);
            int best = center;
            float bestD = float.MaxValue;
            int span = 36;
            for (int k = -span; k <= span; k++)
            {
                int i = (center + k + Samples.Length * 4) % Samples.Length;
                float d = (Samples[i].Position - world).sqrMagnitude;
                if (d < bestD)
                {
                    bestD = d;
                    best = i;
                }
            }

            if (bestD > 40f * 40f)
            {
                for (int i = 0; i < Samples.Length; i++)
                {
                    float d = (Samples[i].Position - world).sqrMagnitude;
                    if (d < bestD)
                    {
                        bestD = d;
                        best = i;
                    }
                }
            }

            return Samples[best].Distance;
        }

        public int NearestIndex(float x, float z)
        {
            int cx = Mathf.FloorToInt(x / Cell);
            int cz = Mathf.FloorToInt(z / Cell);
            int best = 0;
            float bestD = float.MaxValue;
            for (int r = 0; r < 24; r++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz)) != r) continue;
                        if (!grid.TryGetValue(KeyOf(cx + dx, cz + dz), out var list)) continue;
                        for (int n = 0; n < list.Count; n++)
                        {
                            int i = list[n];
                            float ddx = Samples[i].Position.x - x;
                            float ddz = Samples[i].Position.z - z;
                            float d = ddx * ddx + ddz * ddz;
                            if (d < bestD)
                            {
                                bestD = d;
                                best = i;
                            }
                        }
                    }
                }

                if (bestD < float.MaxValue && r * Cell >= Mathf.Sqrt(bestD)) return best;
            }

            return best;
        }

        public int IndexAtDistance(float distance)
        {
            distance = Mathf.Repeat(distance, Length);
            int lo = 0;
            int hi = Samples.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (Samples[mid].Distance < distance) lo = mid + 1;
                else hi = mid;
            }
            int i1 = lo % Samples.Length;
            int i0 = (i1 - 1 + Samples.Length) % Samples.Length;
            float wrap0 = Mathf.Min(Mathf.Abs(Samples[i0].Distance - distance), Mathf.Abs(Length - Mathf.Abs(Samples[i0].Distance - distance)));
            float wrap1 = Mathf.Min(Mathf.Abs(Samples[i1].Distance - distance), Mathf.Abs(Length - Mathf.Abs(Samples[i1].Distance - distance)));
            return wrap0 <= wrap1 ? i0 : i1;
        }

        static long Key(float x, float z)
        {
            return KeyOf(Mathf.FloorToInt(x / Cell), Mathf.FloorToInt(z / Cell));
        }

        static long KeyOf(int cx, int cz)
        {
            return ((long)cx << 32) ^ (uint)cz;
        }
    }
}
