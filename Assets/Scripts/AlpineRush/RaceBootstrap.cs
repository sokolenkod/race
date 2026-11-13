using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace AlpineRush
{
    public class RaceBootstrap : MonoBehaviour
    {
        struct Entry
        {
            public string Name;
            public string Model;
            public Color Color;
            public bool Player;
            public float Speed;
            public float Aggression;
            public float Skill;
            public float Line;
        }

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Physics.queriesHitBackfaces = true;

            var cam = StyleKit.CreateCamera("RaceCamera", true);
            cam.tag = "MainCamera";
            cam.gameObject.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(0f, 8f, -18f);
            cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
            var chase = cam.gameObject.AddComponent<ChaseCamera>();

            var hud = gameObject.AddComponent<RaceHud>();
            hud.ShowLoading();
            yield return null;

            TrackWorld world = TrackBuilder.Build();
            yield return null;

            StyleKit.ApplyAtmosphere(true);
            var racers = SpawnField(world.Track);
            var manager = gameObject.AddComponent<RaceManager>();
            hud.Build(manager);
            chase.Target = racers[0].transform;
            chase.Vehicle = racers[0].Vehicle;
            cam.transform.position = racers[0].transform.position - racers[0].transform.forward * 8f + Vector3.up * 3.5f;
            cam.transform.LookAt(racers[0].transform.position + Vector3.up);
            SetupMinimap(racers[0], hud.MinimapImage);
            gameObject.AddComponent<MusicBed>();
            Physics.SyncTransforms();
            manager.Begin(racers, world.Track, world.Lights, hud);
        }

        static List<Racer> SpawnField(TrackData track)
        {
            var entries = new[]
            {
                new Entry { Name = "You", Model = "race", Color = new Color(1f, 0.82f, 0.2f), Player = true, Speed = 1f, Line = -1.6f },
                new Entry { Name = "Novak", Model = "sedan-sports", Color = new Color(0.2f, 0.85f, 1f), Speed = 0.98f, Aggression = 0.62f, Skill = 0.84f, Line = 1.3f },
                new Entry { Name = "Horak", Model = "hatchback-sports", Color = new Color(1f, 0.45f, 0.15f), Speed = 0.94f, Aggression = 0.34f, Skill = 0.9f, Line = -0.4f },
                new Entry { Name = "Berg", Model = "suv", Color = new Color(0.85f, 0.3f, 0.95f), Speed = 1.02f, Aggression = 0.88f, Skill = 0.58f, Line = 2.1f }
            };

            var racers = new List<Racer>();
            for (int i = 0; i < entries.Length; i++)
            {
                float distance = 16f + i * 9f;
                float side = entries[i].Line;
                var sample = track.SampleAtDistance(distance);
                var go = new GameObject(entries[i].Name);
                var racer = go.AddComponent<Racer>();
                var vehicle = go.AddComponent<ArcadeVehicle>();
                racer.RacerName = entries[i].Name;
                racer.IsPlayer = entries[i].Player;
                racer.Color = entries[i].Color;
                racer.Vehicle = vehicle;
                racer.TrackDistance = distance;
                racer.NextCheckpoint = 1;
                vehicle.IsPlayer = entries[i].Player;
                vehicle.MaxSpeed = 36f;
                vehicle.NitroSpeed = entries[i].Player ? 48f : 40f;

                var model = PropLibrary.SpawnCarVisual(entries[i].Model, 4.35f);
                model.name = "Model";
                model.transform.SetParent(go.transform, true);

                var visual = go.AddComponent<CarVisual>();
                visual.Bind(vehicle, entries[i].Color);
                var audio = go.AddComponent<VehicleAudio>();
                audio.Setup(vehicle, entries[i].Player);
                if (!entries[i].Player)
                {
                    var ai = go.AddComponent<AiDriver>();
                    ai.SpeedScale = entries[i].Speed;
                    ai.Aggression = entries[i].Aggression;
                    ai.Skill = entries[i].Skill;
                    ai.LineOffset = entries[i].Line * 0.35f;
                }

                float yaw = Mathf.Atan2(sample.Tangent.x, sample.Tangent.z) * Mathf.Rad2Deg;
                vehicle.Place(sample.Position + sample.Right * side + sample.Normal * 0.2f, yaw, 0f);
                racers.Add(racer);
            }

            return racers;
        }

        static void SetupMinimap(Racer player, RawImage image)
        {
            var rt = new RenderTexture(256, 256, 16) { name = "MinimapRT", antiAliasing = 1 };
            var cam = StyleKit.CreateCamera("MinimapCamera", false);
            cam.orthographic = true;
            cam.orthographicSize = 85f;
            cam.nearClipPlane = 1f;
            cam.farClipPlane = 400f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.16f, 0.14f, 1f);
            cam.targetTexture = rt;
            cam.cullingMask = ~0;
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data != null)
            {
                data.renderPostProcessing = false;
                data.renderShadows = false;
            }
            var follow = cam.gameObject.AddComponent<MinimapCamera>();
            follow.Target = player.transform;
            if (image != null) image.texture = rt;
        }
    }
}
