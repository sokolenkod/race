using UnityEngine;
using UnityEngine.UI;

namespace AlpineRush
{
    public class RaceHud : MonoBehaviour
    {
        public RawImage MinimapImage { get; private set; }

        RaceManager race;
        Text positionText;
        Text lapText;
        Text timerText;
        Text bestText;
        Text speedText;
        Text speedUnit;
        Text standingsText;
        Text bannerText;
        Text wrongWayText;
        Text hintText;
        RectTransform nitroFill;
        GameObject resultsRoot;
        Text resultsBody;
        GameObject pauseRoot;
        GameObject loadingRoot;
        float hintLeft = 9f;

        public void ShowLoading()
        {
            UiFactory.EnsureEventSystem();
            var canvas = UiFactory.CreateOverlay("RaceCanvas", 20);
            loadingRoot = UiFactory.Panel(canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-360f, -70f), new Vector2(360f, 70f), new Color(0.04f, 0.06f, 0.09f, 0.82f)).gameObject;
            var text = UiFactory.CreateText(loadingRoot.transform, "Building the mountain pass...", 36, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.7f));
            UiFactory.Stretch(text.rectTransform);
        }

        public void Build(RaceManager manager)
        {
            race = manager;
            if (loadingRoot != null) loadingRoot.SetActive(false);
            var canvas = loadingRoot != null ? loadingRoot.GetComponentInParent<Canvas>() : UiFactory.CreateOverlay("RaceCanvas", 20);
            if (canvas == null)
            {
                UiFactory.EnsureEventSystem();
                canvas = UiFactory.CreateOverlay("RaceCanvas", 20);
            }

            var posPanel = UiFactory.Panel(canvas.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -210f), new Vector2(360f, -24f), new Color(0.04f, 0.06f, 0.09f, 0.62f));
            positionText = UiFactory.CreateText(posPanel, "1st", 72, TextAnchor.UpperLeft, new Color(1f, 0.84f, 0.3f));
            positionText.rectTransform.anchorMin = new Vector2(0f, 0.4f);
            positionText.rectTransform.anchorMax = Vector2.one;
            positionText.rectTransform.offsetMin = new Vector2(18f, 0f);
            positionText.rectTransform.offsetMax = new Vector2(-12f, -8f);
            lapText = UiFactory.CreateText(posPanel, "LAP 1/3", 28, TextAnchor.LowerLeft, Color.white);
            lapText.rectTransform.anchorMin = Vector2.zero;
            lapText.rectTransform.anchorMax = new Vector2(1f, 0.42f);
            lapText.rectTransform.offsetMin = new Vector2(20f, 8f);
            lapText.rectTransform.offsetMax = new Vector2(-12f, 0f);

            timerText = UiFactory.CreateText(canvas.transform, "0:00.00", 42, TextAnchor.UpperCenter, Color.white);
            timerText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            timerText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            timerText.rectTransform.pivot = new Vector2(0.5f, 1f);
            timerText.rectTransform.sizeDelta = new Vector2(360f, 56f);
            timerText.rectTransform.anchoredPosition = new Vector2(0f, -18f);
            bestText = UiFactory.CreateText(canvas.transform, "", 22, TextAnchor.UpperCenter, new Color(0.85f, 0.85f, 0.85f));
            bestText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            bestText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            bestText.rectTransform.pivot = new Vector2(0.5f, 1f);
            bestText.rectTransform.sizeDelta = new Vector2(360f, 32f);
            bestText.rectTransform.anchoredPosition = new Vector2(0f, -72f);

            var standPanel = UiFactory.Panel(canvas.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-390f, -230f), new Vector2(-24f, -24f), new Color(0.04f, 0.06f, 0.09f, 0.55f));
            standingsText = UiFactory.CreateText(standPanel, "", 24, TextAnchor.UpperLeft, Color.white);
            UiFactory.Stretch(standingsText.rectTransform);
            standingsText.rectTransform.offsetMin = new Vector2(16f, 10f);
            standingsText.rectTransform.offsetMax = new Vector2(-12f, -10f);

            var speedPanel = UiFactory.Panel(canvas.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 28f), new Vector2(340f, 190f), new Color(0.04f, 0.06f, 0.09f, 0.62f));
            speedText = UiFactory.CreateText(speedPanel, "0", 68, TextAnchor.UpperLeft, Color.white);
            speedText.rectTransform.anchorMin = new Vector2(0f, 0.35f);
            speedText.rectTransform.anchorMax = Vector2.one;
            speedText.rectTransform.offsetMin = new Vector2(18f, 0f);
            speedText.rectTransform.offsetMax = new Vector2(-80f, -6f);
            speedUnit = UiFactory.CreateText(speedPanel, "KM/H", 22, TextAnchor.UpperRight, new Color(1f, 0.84f, 0.3f));
            speedUnit.rectTransform.anchorMin = new Vector2(0.55f, 0.55f);
            speedUnit.rectTransform.anchorMax = Vector2.one;
            speedUnit.rectTransform.offsetMin = Vector2.zero;
            speedUnit.rectTransform.offsetMax = new Vector2(-16f, -18f);

            var nitroBg = UiFactory.Panel(speedPanel, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 16f), new Vector2(-16f, 42f), new Color(0.08f, 0.1f, 0.12f, 1f));
            var nitroLabel = UiFactory.CreateText(speedPanel, "NITRO", 16, TextAnchor.UpperLeft, new Color(0.6f, 0.9f, 1f));
            nitroLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
            nitroLabel.rectTransform.anchorMax = new Vector2(0f, 0f);
            nitroLabel.rectTransform.pivot = new Vector2(0f, 0f);
            nitroLabel.rectTransform.anchoredPosition = new Vector2(16f, 46f);
            nitroLabel.rectTransform.sizeDelta = new Vector2(120f, 22f);
            var fill = UiFactory.Panel(nitroBg, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f), new Color(0.25f, 0.8f, 1f, 1f));
            nitroFill = fill;

            hintText = UiFactory.CreateText(canvas.transform, "WASD / arrows drive    Space drift    Shift nitro    R respawn    Esc pause", 20, TextAnchor.LowerCenter, new Color(1f, 1f, 1f, 0.8f));
            hintText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            hintText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            hintText.rectTransform.pivot = new Vector2(0.5f, 0f);
            hintText.rectTransform.sizeDelta = new Vector2(1100f, 36f);
            hintText.rectTransform.anchoredPosition = new Vector2(0f, 18f);

            bannerText = UiFactory.CreateText(canvas.transform, "", 140, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.45f));
            bannerText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            bannerText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            bannerText.rectTransform.sizeDelta = new Vector2(800f, 200f);
            bannerText.rectTransform.anchoredPosition = new Vector2(0f, 40f);

            wrongWayText = UiFactory.CreateText(canvas.transform, "WRONG WAY", 48, TextAnchor.MiddleCenter, new Color(1f, 0.3f, 0.25f));
            wrongWayText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            wrongWayText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            wrongWayText.rectTransform.sizeDelta = new Vector2(600f, 70f);
            wrongWayText.rectTransform.anchoredPosition = new Vector2(0f, -80f);
            wrongWayText.gameObject.SetActive(false);

            var mapFrame = UiFactory.Panel(canvas.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-280f, 28f), new Vector2(-28f, 280f), new Color(0.05f, 0.07f, 0.1f, 0.9f));
            var mapGo = new GameObject("Minimap");
            mapGo.transform.SetParent(mapFrame, false);
            MinimapImage = mapGo.AddComponent<RawImage>();
            UiFactory.Stretch(MinimapImage.rectTransform);
            MinimapImage.rectTransform.offsetMin = new Vector2(6f, 6f);
            MinimapImage.rectTransform.offsetMax = new Vector2(-6f, -6f);

            BuildResults(canvas.transform);
            BuildPause(canvas.transform);
        }

        void BuildResults(Transform canvas)
        {
            resultsRoot = UiFactory.Panel(canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.05f, 0.82f)).gameObject;
            var card = UiFactory.Panel(resultsRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-430f, -340f), new Vector2(430f, 340f), new Color(0.07f, 0.09f, 0.12f, 0.96f));
            var title = UiFactory.CreateText(card, "RACE FINISHED", 54, TextAnchor.UpperCenter, new Color(1f, 0.86f, 0.35f));
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(0f, 70f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -18f);
            var sub = UiFactory.CreateText(card, "GAME OVER", 24, TextAnchor.UpperCenter, new Color(0.8f, 0.8f, 0.8f));
            sub.rectTransform.anchorMin = new Vector2(0f, 1f);
            sub.rectTransform.anchorMax = new Vector2(1f, 1f);
            sub.rectTransform.pivot = new Vector2(0.5f, 1f);
            sub.rectTransform.sizeDelta = new Vector2(0f, 30f);
            sub.rectTransform.anchoredPosition = new Vector2(0f, -86f);
            resultsBody = UiFactory.CreateText(card, "", 26, TextAnchor.UpperLeft, Color.white);
            resultsBody.rectTransform.anchorMin = Vector2.zero;
            resultsBody.rectTransform.anchorMax = Vector2.one;
            resultsBody.rectTransform.offsetMin = new Vector2(40f, 130f);
            resultsBody.rectTransform.offsetMax = new Vector2(-40f, -130f);

            var restart = UiFactory.CreateButton(card, "Restart", new Vector2(280f, 64f), GameFlow.RestartRace);
            restart.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
            restart.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
            restart.GetComponent<RectTransform>().pivot = new Vector2(1f, 0f);
            restart.GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 36f);
            var menu = UiFactory.CreateButton(card, "Main Menu", new Vector2(280f, 64f), GameFlow.MainMenu);
            menu.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
            menu.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
            menu.GetComponent<RectTransform>().pivot = new Vector2(0f, 0f);
            menu.GetComponent<RectTransform>().anchoredPosition = new Vector2(12f, 36f);
            resultsRoot.SetActive(false);
        }

        void BuildPause(Transform canvas)
        {
            pauseRoot = UiFactory.Panel(canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.05f, 0.7f)).gameObject;
            var card = UiFactory.Panel(pauseRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, -220f), new Vector2(260f, 220f), new Color(0.07f, 0.09f, 0.12f, 0.96f));
            var title = UiFactory.CreateText(card, "PAUSED", 48, TextAnchor.UpperCenter, Color.white);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(0f, 70f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            var resume = UiFactory.CreateButton(card, "Resume", new Vector2(360f, 58f), () => race.SetPaused(false));
            resume.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            resume.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            resume.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 30f);
            var restart = UiFactory.CreateButton(card, "Restart", new Vector2(360f, 58f), GameFlow.RestartRace);
            restart.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            restart.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            restart.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);
            var menu = UiFactory.CreateButton(card, "Main Menu", new Vector2(360f, 58f), GameFlow.MainMenu);
            menu.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            menu.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            menu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -110f);
            pauseRoot.SetActive(false);
        }

        public void SetPaused(bool paused)
        {
            if (pauseRoot != null) pauseRoot.SetActive(paused);
        }

        public void ShowResults()
        {
            if (resultsRoot == null || race == null || race.Player == null) return;
            int place = race.PositionOf(race.Player);
            var player = race.Player;
            string body = Ordinal(place) + " PLACE\n\n";
            body += "Time   " + FormatTime(player.FinishTime) + "\n";
            if (player.BestLap > 0f) body += "Best lap   " + FormatTime(player.BestLap) + "\n";
            body += "\n";
            for (int i = 0; i < race.Standings.Count; i++)
            {
                var racer = race.Standings[i];
                string time = racer.Finished ? FormatTime(racer.FinishTime) : "running";
                body += (i + 1) + ".  " + racer.RacerName + "    " + time + "\n";
            }
            resultsBody.text = body;
            resultsRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Tick()
        {
            if (race == null || race.Player == null || positionText == null) return;
            var player = race.Player;
            int place = Mathf.Max(1, race.PositionOf(player));
            positionText.text = Ordinal(place);
            positionText.color = place == 1 ? new Color(1f, 0.84f, 0.3f) : Color.white;
            int shownLap = Mathf.Clamp(player.CompletedLaps + 1, 1, race.TotalLaps);
            lapText.text = "LAP " + shownLap + "/" + race.TotalLaps;
            timerText.text = FormatTime(race.Phase == RacePhase.Racing || race.Phase == RacePhase.Finished ? race.RaceTime : 0f);
            bestText.text = player.BestLap > 0f ? "BEST " + FormatTime(player.BestLap) : "COL DE LA SERRA";
            var vehicle = player.Vehicle;
            int kmh = vehicle != null ? Mathf.RoundToInt(vehicle.SpeedKmh) : 0;
            speedText.text = kmh.ToString();
            speedText.color = vehicle != null && vehicle.NitroActive ? new Color(0.45f, 0.9f, 1f) : Color.white;
            if (nitroFill != null && vehicle != null)
                nitroFill.anchorMax = new Vector2(Mathf.Clamp01(vehicle.NitroAmount), 1f);

            if (hintText != null && race.Phase == RacePhase.Racing)
            {
                hintLeft -= Time.deltaTime;
                if (hintLeft < 1.2f)
                {
                    var c = hintText.color;
                    c.a = Mathf.Clamp01(hintLeft / 1.2f);
                    hintText.color = c;
                }
            }

            bool showBanner = !string.IsNullOrEmpty(race.Banner) && Time.time < race.BannerUntil;
            bannerText.gameObject.SetActive(showBanner);
            if (showBanner) bannerText.text = race.Banner;
            wrongWayText.gameObject.SetActive(race.WrongWay && race.Phase == RacePhase.Racing && Mathf.FloorToInt(Time.time * 4f) % 2 == 0);

            var lines = "";
            for (int i = 0; i < race.Standings.Count; i++)
            {
                var racer = race.Standings[i];
                lines += (i + 1) + "  " + racer.RacerName + "\n";
            }
            standingsText.text = lines;
        }

        public static string Ordinal(int n)
        {
            int rem100 = n % 100;
            if (rem100 >= 11 && rem100 <= 13) return n + "th";
            switch (n % 10)
            {
                case 1: return n + "st";
                case 2: return n + "nd";
                case 3: return n + "rd";
                default: return n + "th";
            }
        }

        public static string FormatTime(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            int minutes = Mathf.FloorToInt(seconds / 60f);
            float secs = seconds - minutes * 60f;
            return minutes + ":" + secs.ToString("00.00");
        }
    }
}
