using UnityEngine;

namespace AlpineRush
{
    public class Racer : MonoBehaviour
    {
        public string RacerName = "Racer";
        public bool IsPlayer;
        public Color Color = Color.white;
        public int CompletedLaps;
        public int CheckpointIndex;
        public int NextCheckpoint = 1;
        public bool Finished;
        public float FinishTime;
        public float BestLap;
        public float LastLap;
        public float LapStartTime;
        public float TrackDistance;
        public ArcadeVehicle Vehicle;

        public float ProgressDistance()
        {
            var race = RaceManager.Instance;
            if (race == null || race.Track == null) return 0f;
            var track = race.Track;
            if (Finished) return track.Length * race.TotalLaps + 50f;
            float cp = track.CheckpointDistances[CheckpointIndex];
            float next = NextCheckpoint == 0 ? track.Length : track.CheckpointDistances[NextCheckpoint];
            float sector = next - cp;
            if (sector < 1f) sector = Mathf.Max(1f, track.Length - cp);
            float travelled = TrackDistance - cp;
            if (travelled < -1f) travelled += track.Length;
            travelled = Mathf.Clamp(travelled, 0f, sector);
            return CompletedLaps * track.Length + cp + travelled;
        }

        public float SortScore()
        {
            if (Finished) return 1000000f - FinishTime;
            return ProgressDistance();
        }

        public void PassCheckpoint(int index)
        {
            var race = RaceManager.Instance;
            if (race == null || Finished) return;
            if (race.Phase != RacePhase.Racing) return;
            if (index != NextCheckpoint) return;

            if (race.Track != null)
                TrackDistance = race.Track.CheckpointDistances[index];
            CheckpointIndex = index;
            NextCheckpoint = (index + 1) % RaceManager.CheckpointCount;
            if (index == 0)
            {
                float now = race.RaceTime;
                float lap = now - LapStartTime;
                LastLap = lap;
                if (CompletedLaps == 0 || lap < BestLap || BestLap <= 0f) BestLap = lap;
                LapStartTime = now;
                CompletedLaps++;
                if (IsPlayer) Sfx.Play(race.LapChime, 0.55f);
                if (CompletedLaps >= race.TotalLaps)
                {
                    Finished = true;
                    FinishTime = now;
                    race.NotifyFinished(this);
                }
            }
            else if (IsPlayer)
            {
                Sfx.Play(race.CheckpointBlip, 0.28f);
            }
        }
    }
}
