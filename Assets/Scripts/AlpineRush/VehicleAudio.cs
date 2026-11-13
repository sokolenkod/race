using UnityEngine;

namespace AlpineRush
{
    public class VehicleAudio : MonoBehaviour
    {
        AudioSource engine;
        AudioSource skid;
        AudioSource impact;
        AudioClip impactClip;
        ArcadeVehicle vehicle;

        public void Setup(ArcadeVehicle owner, bool player)
        {
            vehicle = owner;
            var engineClip = Resources.Load<AudioClip>("Audio/engine");
            var skidClip = Resources.Load<AudioClip>("Audio/skid");
            impactClip = Resources.Load<AudioClip>("Audio/impact");

            engine = gameObject.AddComponent<AudioSource>();
            engine.loop = true;
            engine.playOnAwake = false;
            engine.spatialBlend = player ? 0.15f : 0.85f;
            engine.dopplerLevel = 0f;
            engine.minDistance = 4f;
            engine.maxDistance = 70f;
            engine.volume = player ? 0.55f : 0.28f;
            if (engineClip != null)
            {
                engine.clip = engineClip;
                engine.Play();
            }

            skid = gameObject.AddComponent<AudioSource>();
            skid.loop = true;
            skid.playOnAwake = false;
            skid.spatialBlend = player ? 0.2f : 1f;
            skid.dopplerLevel = 0f;
            skid.volume = 0f;
            if (skidClip != null)
            {
                skid.clip = skidClip;
                skid.Play();
            }

            impact = gameObject.AddComponent<AudioSource>();
            impact.playOnAwake = false;
            impact.spatialBlend = player ? 0.3f : 1f;
            impact.dopplerLevel = 0f;
        }

        void Update()
        {
            if (vehicle == null || engine == null || engine.clip == null) return;
            float throttle = Mathf.Abs(vehicle.Throttle);
            float pitch = Mathf.Lerp(0.72f, 1.65f, vehicle.Speed01);
            if (vehicle.NitroActive) pitch += 0.18f;
            if (throttle < 0.1f) pitch *= 0.9f;
            engine.pitch = pitch;
            engine.volume = Mathf.Lerp(engine.volume, (vehicle.IsPlayer ? 0.32f : 0.16f) + throttle * 0.35f + vehicle.Speed01 * 0.2f, 6f * Time.deltaTime);

            if (skid != null && skid.clip != null)
            {
                float target = vehicle.IsDrifting || (!vehicle.OnRoad && vehicle.SpeedKmh > 20f) ? 0.55f : 0f;
                skid.volume = Mathf.Lerp(skid.volume, target, 8f * Time.deltaTime);
                skid.pitch = Mathf.Lerp(0.9f, 1.15f, vehicle.Speed01);
            }
        }

        public void PlayImpact(float strength)
        {
            if (impact != null && impactClip != null) impact.PlayOneShot(impactClip, Mathf.Lerp(0.25f, 0.9f, strength));
        }
    }
}
