using UnityEngine;

namespace AlpineRush
{
    public class Checkpoint : MonoBehaviour
    {
        public int Index;

        void OnTriggerEnter(Collider other)
        {
            var racer = other.GetComponentInParent<Racer>();
            if (racer == null || racer.Vehicle == null) return;
            // Gates only count when the car is travelling with the circuit.
            // Reversing through a gate, or clipping it sideways, does not award a lap.
            float facing = Vector3.Dot(racer.transform.forward, transform.forward);
            if (racer.Vehicle.ForwardSpeed > 0.8f && facing > 0.2f)
                racer.PassCheckpoint(Index);
        }
    }

    public class NitroPad : MonoBehaviour
    {
        float readyAt;

        void OnTriggerEnter(Collider other)
        {
            if (Time.time < readyAt) return;
            var vehicle = other.GetComponentInParent<ArcadeVehicle>();
            if (vehicle == null) return;
            vehicle.RefillNitro();
            readyAt = Time.time + 12f;
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.enabled = false;
            CancelInvoke(nameof(Relight));
            Invoke(nameof(Relight), 12f);
        }

        void Relight()
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.enabled = true;
        }
    }
}
