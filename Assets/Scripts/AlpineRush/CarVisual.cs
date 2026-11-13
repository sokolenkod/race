using System.Collections.Generic;
using UnityEngine;

namespace AlpineRush
{
    public class CarVisual : MonoBehaviour
    {
        struct Wheel
        {
            public Transform Transform;
            public Quaternion BaseRotation;
            public Vector3 Axis;
        }

        readonly List<Wheel> wheels = new List<Wheel>();
        TrailRenderer leftTrail;
        TrailRenderer rightTrail;
        ParticleSystem smoke;
        ArcadeVehicle vehicle;
        float spin;

        public void Bind(ArcadeVehicle owner, Color blipColor)
        {
            vehicle = owner;
            var model = transform.Find("Model");
            if (model == null && transform.childCount > 0) model = transform.GetChild(0);
            if (model != null) CollectWheels(model);

            leftTrail = CreateTrail(new Vector3(-0.65f, 0.08f, -1.45f));
            rightTrail = CreateTrail(new Vector3(0.65f, 0.08f, -1.45f));
            smoke = CreateSmoke();

            var blip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(blip.GetComponent<Collider>());
            blip.name = "Blip";
            blip.layer = AlpineLayers.MinimapBlip;
            blip.transform.SetParent(transform, false);
            blip.transform.localPosition = new Vector3(0f, 5.5f, 0f);
            blip.transform.localScale = new Vector3(2.4f, 6f, 2.4f);
            blip.GetComponent<Renderer>().sharedMaterial = StyleKit.Emissive(blipColor, 2.5f);
        }

        void CollectWheels(Transform root)
        {
            var all = root.GetComponentsInChildren<Transform>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == root) continue;
                if (all[i].name.ToLowerInvariant().Contains("wheel") == false) continue;
                Vector3 axis = Vector3.right;
                var meshFilter = all[i].GetComponentInChildren<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Vector3 size = meshFilter.sharedMesh.bounds.size;
                    if (size.y < size.x && size.y < size.z) axis = Vector3.up;
                    else if (size.z < size.x && size.z < size.y) axis = Vector3.forward;
                }
                wheels.Add(new Wheel
                {
                    Transform = all[i],
                    BaseRotation = all[i].localRotation,
                    Axis = axis
                });
            }
        }

        TrailRenderer CreateTrail(Vector3 local)
        {
            var go = new GameObject("Skid");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = local;
            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 1.5f;
            trail.startWidth = 0.26f;
            trail.endWidth = 0.05f;
            trail.minVertexDistance = 0.25f;
            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.material = StyleKit.Particle(new Color(0.12f, 0.12f, 0.12f, 0.75f));
            return trail;
        }

        ParticleSystem CreateSmoke()
        {
            var go = new GameObject("TireSmoke");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.25f, -1.4f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = 0.7f;
            main.startSpeed = 1.1f;
            main.startSize = 0.7f;
            main.startColor = new Color(0.85f, 0.85f, 0.85f, 0.35f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1.5f, 0.1f, 0.4f);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = StyleKit.Particle(new Color(0.8f, 0.8f, 0.8f, 0.5f));
            return ps;
        }

        void Update()
        {
            if (vehicle == null) return;
            spin += vehicle.ForwardSpeed * 160f * Time.deltaTime;
            for (int i = 0; i < wheels.Count; i++)
            {
                var wheel = wheels[i];
                if (wheel.Transform == null) continue;
                wheel.Transform.localRotation = wheel.BaseRotation * Quaternion.AngleAxis(spin, wheel.Axis);
            }

            bool mark = vehicle.IsDrifting || (vehicle.Grounded && !vehicle.OnRoad && vehicle.SpeedKmh > 18f);
            if (leftTrail != null) leftTrail.emitting = mark;
            if (rightTrail != null) rightTrail.emitting = mark;
            if (smoke != null)
            {
                var emission = smoke.emission;
                emission.rateOverTime = mark ? 36f : 0f;
            }
        }
    }
}
