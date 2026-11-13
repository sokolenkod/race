using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlpineRush
{
    public enum RacePhase
    {
        Boot,
        Countdown,
        Racing,
        Finished
    }

    [DefaultExecutionOrder(-100)]
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance { get; private set; }
        public const int TotalLapsConstant = 3;
        public const int CheckpointCount = 12;

        [System.NonSerialized] public TrackData Track;
        public StartLights Lights;
        public RacePhase Phase { get; private set; } = RacePhase.Boot;
        public float RaceTime { get; private set; }
        public int TotalLaps => TotalLapsConstant;
        public string Banner = "";
        public float BannerUntil;
        public bool WrongWay;
        public bool Paused;
        public Racer Player { get; private set; }
        public readonly List<Racer> Racers = new List<Racer>();
        public readonly List<Racer> Standings = new List<Racer>();
        public AudioClip LapChime;
        public AudioClip CheckpointBlip;

        RaceHud hud;

        void Awake()
        {
            Instance = this;
            LapChime = Sfx.Tone(660f, 0.22f, 0.45f);
            CheckpointBlip = Sfx.Tone(880f, 0.07f, 0.35f);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        public void Begin(IList<Racer> racers, TrackData track, StartLights lights, RaceHud raceHud)
        {
            Track = track;
            Lights = lights;
            hud = raceHud;
            Racers.Clear();
            for (int i = 0; i < racers.Count; i++)
            {
                Racers.Add(racers[i]);
                if (racers[i].IsPlayer) Player = racers[i];
                racers[i].LapStartTime = 0f;
            }
            Phase = RacePhase.Countdown;
            if (Lights != null) Lights.ShowCount(0, false);
            StartCoroutine(Countdown());
        }

        IEnumerator Countdown()
        {
            var tones = new[] { Sfx.Tone(440f, 0.16f, 0.5f), Sfx.Tone(440f, 0.16f, 0.5f), Sfx.Tone(440f, 0.16f, 0.5f), Sfx.Tone(880f, 0.28f, 0.6f) };
            for (int i = 3; i >= 1; i--)
            {
                Banner = i.ToString();
                BannerUntil = Time.time + 0.95f;
                if (Lights != null) Lights.ShowCount(4 - i, false);
                Sfx.Play(tones[3 - i], 0.7f);
                yield return new WaitForSeconds(1f);
            }

            Banner = "GO!";
            BannerUntil = Time.time + 1.15f;
            if (Lights != null) Lights.ShowCount(3, true);
            Sfx.Play(tones[3], 0.8f);
            Phase = RacePhase.Racing;
            RaceTime = 0f;
            for (int i = 0; i < Racers.Count; i++) Racers[i].LapStartTime = 0f;
        }

        void Update()
        {
            DrivingInput.Poll();
            if (DrivingInput.PausePressed && Phase != RacePhase.Finished)
                SetPaused(!Paused);
            if (DrivingInput.RespawnPressed && Phase == RacePhase.Racing && !Paused
                && Player != null && Player.Vehicle != null && !Player.Finished)
                Player.Vehicle.Respawn();

            if (Phase == RacePhase.Racing) RaceTime += Time.deltaTime;
            UpdateWrongWay();
            SortStandings();
            if (hud != null) hud.Tick();
        }

        void FixedUpdate()
        {
            if (Track == null) return;
            for (int i = 0; i < Racers.Count; i++)
            {
                var racer = Racers[i];
                if (racer == null) continue;
                float projected = Track.Project(racer.transform.position, racer.TrackDistance);
                var sample = Track.SampleAtDistance(projected);
                if ((sample.Position - racer.transform.position).sqrMagnitude < 25f * 25f)
                    racer.TrackDistance = projected;
            }
        }

        void UpdateWrongWay()
        {
            WrongWay = false;
            if (Phase != RacePhase.Racing || Player == null || Track == null || Player.Finished) return;
            var sample = Track.SampleAtDistance(Player.TrackDistance);
            float along = Vector3.Dot(Player.Vehicle != null ? Player.transform.forward : Vector3.forward, sample.Tangent);
            float speed = Player.Vehicle != null ? Player.Vehicle.ForwardSpeed : 0f;
            WrongWay = speed < -2.5f || (speed > 4f && along < -0.25f);
        }

        void SortStandings()
        {
            Standings.Clear();
            Standings.AddRange(Racers);
            Standings.Sort((a, b) => b.SortScore().CompareTo(a.SortScore()));
        }

        public int PositionOf(Racer racer)
        {
            for (int i = 0; i < Standings.Count; i++)
                if (Standings[i] == racer) return i + 1;
            return Standings.Count;
        }

        public void NotifyFinished(Racer racer)
        {
            if (racer != null && racer.IsPlayer) FinishRace();
        }

        public void FinishRace()
        {
            if (Phase == RacePhase.Finished) return;
            Phase = RacePhase.Finished;
            Banner = "";
            SetPaused(false);
            SortStandings();
            if (hud != null) hud.ShowResults();
        }

        public void SetPaused(bool paused)
        {
            if (Phase == RacePhase.Finished) paused = false;
            Paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (hud != null) hud.SetPaused(paused);
        }
    }
}
