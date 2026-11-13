using UnityEngine;

namespace AlpineRush
{
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(100)]
    public class ArcadeVehicle : MonoBehaviour
    {
        public bool IsPlayer;
        public float MaxSpeed = 36f;
        public float NitroSpeed = 47f;
        public float Acceleration = 16f;
        public float Brake = 34f;
        public float ReverseSpeed = 11f;

        public float Throttle;
        public float Steer;
        public bool Drift;
        public bool Nitro;

        public float ForwardSpeed { get; private set; }
        public float NitroAmount { get; private set; } = 1f;
        public bool NitroActive { get; private set; }
        public bool IsDrifting { get; private set; }
        public bool OnRoad { get; private set; }
        public bool Grounded { get; private set; }
        public float SpeedKmh => Mathf.Abs(ForwardSpeed) * 3.6f;
        public float Speed01 => Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / MaxSpeed);

        Rigidbody body;
        Racer racer;
        float yaw;
        float lateral;
        float verticalSpeed;
        float airborneTime;
        float stuckTime;
        float respawnLock;
        float impactLock;
        const float RideHeight = 0.12f;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            racer = GetComponent<Racer>();

            var box = gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.78f, 0f);
            box.size = new Vector3(1.62f, 0.72f, 3.55f);
        }

        public void Place(Vector3 position, float yawDegrees, float speed)
        {
            yaw = yawDegrees;
            ForwardSpeed = speed;
            lateral = 0f;
            verticalSpeed = 0f;
            airborneTime = 0f;
            stuckTime = 0f;
            respawnLock = 0.6f;
            var rotation = Quaternion.Euler(0f, yaw, 0f);
            body.position = position;
            body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }

        public void RefillNitro()
        {
            NitroAmount = 1f;
        }

        void FixedUpdate()
        {
            if (racer == null) racer = GetComponent<Racer>();
            float dt = Time.fixedDeltaTime;
            respawnLock = Mathf.Max(0f, respawnLock - dt);

            var race = RaceManager.Instance;
            bool racing = race != null && race.Phase == RacePhase.Racing && (racer == null || !racer.Finished);
            if (IsPlayer && racing)
            {
                DrivingInput.Poll();
                Throttle = DrivingInput.Throttle;
                Steer = DrivingInput.Steer;
                Drift = DrivingInput.Drift;
                Nitro = DrivingInput.Nitro;
                if (DrivingInput.RespawnPressed) Respawn();
            }

            if (!racing)
            {
                Throttle = ForwardSpeed > 2f ? -0.7f : 0f;
                Steer = 0f;
                Drift = false;
                Nitro = false;
            }

            Vector3 pos = body.position;
            bool grounded = Probe(pos, out RaycastHit ground);
            Grounded = grounded;
            OnRoad = grounded && ground.collider != null && ground.collider.gameObject.layer == AlpineLayers.Road;

            float speedCap = MaxSpeed;
            float accel = Acceleration;
            NitroActive = false;
            if (Nitro && Throttle > 0.2f && NitroAmount > 0.02f && grounded && OnRoad)
            {
                NitroActive = true;
                NitroAmount = Mathf.Max(0f, NitroAmount - dt * 0.34f);
                speedCap = NitroSpeed;
                accel *= 1.55f;
            }
            else
            {
                NitroAmount = Mathf.Min(1f, NitroAmount + dt * (Throttle > 0.2f ? 0.045f : 0.14f));
            }

            if (!OnRoad && grounded) speedCap = Mathf.Min(speedCap, 15f);

            if (grounded && Throttle > 0.05f)
                ForwardSpeed = Mathf.MoveTowards(ForwardSpeed, speedCap, accel * Throttle * dt);
            else if (Throttle < -0.05f)
            {
                if (ForwardSpeed > 0.4f) ForwardSpeed = Mathf.MoveTowards(ForwardSpeed, 0f, Brake * dt);
                else ForwardSpeed = Mathf.MoveTowards(ForwardSpeed, -ReverseSpeed, Acceleration * 0.65f * dt);
            }
            else ForwardSpeed = Mathf.MoveTowards(ForwardSpeed, 0f, 5.5f * dt);

            float speed01 = Mathf.Clamp01(Mathf.Abs(ForwardSpeed) / MaxSpeed);
            float steerRate = Mathf.Lerp(135f, 36f, speed01);
            if (Drift) steerRate *= 1.45f;
            float steerSign = ForwardSpeed < -1f ? -1f : 1f;
            yaw += Steer * steerRate * steerSign * dt;

            if (grounded && !Drift && Mathf.Abs(Steer) > 0.25f && Mathf.Abs(ForwardSpeed) > 16f)
                ForwardSpeed = Mathf.MoveTowards(ForwardSpeed, Mathf.Sign(ForwardSpeed) * 16f, 12f * Mathf.Abs(Steer) * dt);

            IsDrifting = Drift && grounded && Mathf.Abs(ForwardSpeed) > 8f;
            float grip = IsDrifting ? 3.2f : 16f;
            if (IsDrifting)
            {
                float kick = Steer * ForwardSpeed * 0.55f;
                lateral = Mathf.MoveTowards(lateral, kick, 28f * dt);
                ForwardSpeed *= 1f - 0.22f * dt;
            }
            lateral = Mathf.MoveTowards(lateral, 0f, grip * dt * (6f + Mathf.Abs(ForwardSpeed) * 0.12f));

            Vector3 flatForward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward);
            Vector3 move = (flatForward * ForwardSpeed + flatRight * lateral) * dt;
            Vector3 wallNormal = Vector3.zero;
            float hitSpeed = 0f;
            pos = Slide(pos, Quaternion.Euler(0f, yaw, 0f), ref move, ref wallNormal, ref hitSpeed);
            if (wallNormal.sqrMagnitude > 0.01f)
            {
                Vector3 vel = flatForward * ForwardSpeed + flatRight * lateral;
                vel = Vector3.ProjectOnPlane(vel, wallNormal.normalized);
                ForwardSpeed = Vector3.Dot(vel, flatForward);
                lateral = Vector3.Dot(vel, flatRight);
                if (hitSpeed > 7f && impactLock <= 0f)
                {
                    impactLock = 0.35f;
                    var audio = GetComponent<VehicleAudio>();
                    if (audio != null) audio.PlayImpact(Mathf.Clamp01(hitSpeed / 18f));
                }
            }
            impactLock = Mathf.Max(0f, impactLock - dt);

            grounded = Probe(pos, out ground);
            Grounded = grounded;
            OnRoad = grounded && ground.collider != null && ground.collider.gameObject.layer == AlpineLayers.Road;
            Quaternion rotation;
            if (grounded)
            {
                float gap = Vector3.Dot(pos - ground.point, ground.normal);
                if (gap < RideHeight + 1.15f && gap > -0.4f)
                {
                    pos += ground.normal * (RideHeight - gap);
                    airborneTime = 0f;
                    verticalSpeed = 0f;
                    Vector3 fwd = Vector3.ProjectOnPlane(flatForward, ground.normal).normalized;
                    if (fwd.sqrMagnitude < 0.001f) fwd = flatForward;
                    rotation = Quaternion.LookRotation(fwd, ground.normal);
                }
                else
                {
                    grounded = false;
                    Grounded = false;
                    rotation = Quaternion.Euler(0f, yaw, 0f);
                }
            }
            else rotation = Quaternion.Euler(0f, yaw, 0f);

            if (!grounded)
            {
                airborneTime += dt;
                verticalSpeed += Physics.gravity.y * dt;
                pos += Vector3.up * verticalSpeed * dt;
            }

            body.MovePosition(pos);
            body.MoveRotation(rotation);

            if (racing && respawnLock <= 0f)
            {
                bool falling = airborneTime > 2.4f || pos.y < 2f;
                bool stuck = Mathf.Abs(Throttle) > 0.5f && Mathf.Abs(ForwardSpeed) < 1.4f;
                stuckTime = stuck ? stuckTime + dt : 0f;
                if (falling || stuckTime > 2.3f) Respawn();
            }
        }

        bool Probe(Vector3 pos, out RaycastHit hit)
        {
            hit = default;
            var hits = Physics.RaycastAll(pos + Vector3.up * 3.5f, Vector3.down, 12f, AlpineLayers.GroundMask, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            bool found = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].normal.y < 0.42f) continue;
                if (hits[i].distance < best)
                {
                    best = hits[i].distance;
                    hit = hits[i];
                    found = true;
                }
            }
            return found;
        }

        Vector3 Slide(Vector3 pos, Quaternion rotation, ref Vector3 move, ref Vector3 wallNormal, ref float hitSpeed)
        {
            Vector3 half = new Vector3(0.78f, 0.32f, 1.65f);
            for (int pass = 0; pass < 2; pass++)
            {
                float mag = move.magnitude;
                if (mag < 0.0005f) break;
                Vector3 dir = move / mag;
                Vector3 center = pos + rotation * new Vector3(0f, 0.8f, 0f);
                if (Physics.BoxCast(center, half, dir, out RaycastHit hit, rotation, mag + 0.08f, AlpineLayers.WallMask, QueryTriggerInteraction.Ignore)
                    && hit.normal.y < 0.5f)
                {
                    float travel = Mathf.Max(0f, hit.distance - 0.07f);
                    pos += dir * Mathf.Min(travel, mag);
                    wallNormal += hit.normal;
                    hitSpeed = Mathf.Max(hitSpeed, mag / Time.fixedDeltaTime);
                    move = Vector3.ProjectOnPlane(move, hit.normal);
                    if (hit.distance < 0.04f) pos += hit.normal * 0.06f;
                }
                else
                {
                    pos += move;
                    move = Vector3.zero;
                    break;
                }
            }
            return pos;
        }

        public void Respawn()
        {
            var race = RaceManager.Instance;
            if (race == null || racer == null || race.Track == null) return;
            float distance = race.Track.CheckpointDistances[racer.CheckpointIndex] + 10f;
            var sample = race.Track.SampleAtDistance(distance);
            Vector3 pos = sample.Position + sample.Normal * 0.4f;
            float yawDegrees = Mathf.Atan2(sample.Tangent.x, sample.Tangent.z) * Mathf.Rad2Deg;
            Place(pos, yawDegrees, 12f);
            if (racer != null) racer.TrackDistance = distance;
        }
    }
}
