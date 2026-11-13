using UnityEngine;

namespace AlpineRush
{
    [DefaultExecutionOrder(40)]
    public class AiDriver : MonoBehaviour
    {
        public float SpeedScale = 0.96f;
        public float Aggression = 0.5f;
        public float Skill = 0.75f;
        public float LineOffset;

        ArcadeVehicle vehicle;
        Racer racer;
        float noiseSeed;

        void Awake()
        {
            vehicle = GetComponent<ArcadeVehicle>();
            racer = GetComponent<Racer>();
            noiseSeed = Random.Range(0f, 10f);
        }

        void FixedUpdate()
        {
            var race = RaceManager.Instance;
            if (vehicle == null || racer == null || race == null || race.Track == null) return;
            if (race.Phase != RacePhase.Racing || racer.Finished)
            {
                vehicle.Throttle = 0f;
                vehicle.Steer = 0f;
                vehicle.Drift = false;
                vehicle.Nitro = false;
                return;
            }

            var track = race.Track;
            float distance = racer.TrackDistance;
            float speed01 = vehicle.Speed01;
            float look = Mathf.Lerp(9f, 20f, speed01) * Mathf.Lerp(1.15f, 0.85f, Aggression);
            var ahead = track.SampleAtDistance(distance + look);
            var further = track.SampleAtDistance(distance + Mathf.Lerp(22f, 14f, Aggression));
            var here = track.SampleAtDistance(distance);

            Vector3 target = ahead.Position + ahead.Right * LineOffset;
            Vector3 local = transform.InverseTransformPoint(target);
            float steer = Mathf.Clamp(local.x / Mathf.Max(5f, look * 0.42f), -1f, 1f);
            float latError = Vector3.Dot(transform.position - here.Position, here.Right) - LineOffset;
            steer += Mathf.Clamp(-latError / 5f, -0.7f, 0.7f);
            float wobble = (Mathf.PerlinNoise(Time.time * 0.35f, noiseSeed) - 0.5f) * (0.22f * (1f - Skill));
            steer = Mathf.Clamp(steer + wobble, -1f, 1f);

            float curvature = Mathf.Abs(further.Curvature);
            float radius = curvature > 0.0008f ? 1f / curvature : 180f;
            float comfort = Mathf.Lerp(11f, 17f, Aggression);
            float cornerSpeed = Mathf.Sqrt(Mathf.Max(radius, 8f) * comfort);
            float targetSpeed = Mathf.Min(vehicle.MaxSpeed * SpeedScale, cornerSpeed);
            float slope = track.SampleAtDistance(distance + 18f).Position.y - here.Position.y;
            if (slope > 2f) targetSpeed *= 0.9f;
            if (Mathf.Abs(latError) > 4f) targetSpeed *= 0.75f;
            if (straight && vehicle.NitroAmount > 0.45f && Aggression > 0.55f)
                targetSpeed = Mathf.Max(targetSpeed, vehicle.NitroSpeed * 0.92f);

            var player = race.Player;
            if (player != null && !player.Finished)
            {
                float gap = (player.ProgressDistance() - racer.ProgressDistance()) / track.Length;
                if (gap > 0.25f) targetSpeed *= 1.06f;
                else if (gap < -0.4f) targetSpeed *= 0.97f;
            }

            float speed = vehicle.ForwardSpeed;
            if (speed > targetSpeed + 1.2f) vehicle.Throttle = -1f;
            else if (speed > targetSpeed) vehicle.Throttle = 0.1f;
            else vehicle.Throttle = 1f;

            vehicle.Steer = steer;
            vehicle.Drift = curvature > 0.03f && speed > 14f && Mathf.Abs(steer) > 0.35f && Aggression > 0.4f;
            bool straight = curvature < 0.012f && Mathf.Abs(latError) < 2.5f;
            vehicle.Nitro = straight && vehicle.NitroAmount > 0.45f && Aggression > 0.55f && speed > 18f;
        }
    }
}
