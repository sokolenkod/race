using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AlpineRush
{
    public class MenuBootstrap : MonoBehaviour
    {
        void Start()
        {
            PropLibrary.EnsureLoaded();
            StyleKit.ApplyAtmosphere(false);
            BuildDiorama();
            BuildUi();
            var music = gameObject.AddComponent<MusicBed>();
            music.Gain = 0.11f;
        }

        void BuildDiorama()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "MenuGround";
            ground.transform.localScale = new Vector3(18f, 1f, 18f);
            ground.GetComponent<Renderer>().sharedMaterial = StyleKit.Lit(new Color(0.28f, 0.36f, 0.22f), 0.05f, 0f);
            var rockMat = StyleKit.Lit(new Color(0.42f, 0.4f, 0.38f), 0.1f, 0f);
            var snowMat = StyleKit.Lit(new Color(0.9f, 0.93f, 0.95f), 0.15f, 0f);
            for (int i = 0; i < 7; i++)
            {
                float ang = -40f + i * 28f;
                float rad = 18f + (i % 2) * 6f;
                var peak = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Object.Destroy(peak.GetComponent<Collider>());
                peak.transform.position = new Vector3(Mathf.Sin(ang * Mathf.Deg2Rad) * rad, 0f, Mathf.Cos(ang * Mathf.Deg2Rad) * rad + 6f);
                float height = 7f + (i % 3) * 3.5f;
                peak.transform.localScale = new Vector3(6f + i, height, 6f + i);
                peak.GetComponent<Renderer>().sharedMaterial = i % 3 == 0 ? snowMat : rockMat;
            }

            if (PropLibrary.Trees.Count > 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    float x = -7f + i * 2.4f;
                    var tree = PropLibrary.SpawnProp(PropLibrary.Trees[i % PropLibrary.Trees.Count], 4f + (i % 3));
                    tree.transform.position = new Vector3(x, 0f, 4.5f + (i % 2));
                }
            }

            var podium = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            podium.transform.position = new Vector3(0f, 0.12f, 0f);
            podium.transform.localScale = new Vector3(3.2f, 0.12f, 3.2f);
            podium.GetComponent<Renderer>().sharedMaterial = StyleKit.Lit(new Color(0.15f, 0.16f, 0.18f), 0.4f, 0.2f);

            var pivot = new GameObject("Showcase");
            pivot.transform.position = new Vector3(0f, 0.28f, 0f);
            var car = PropLibrary.SpawnCarVisual("race", 4.3f);
            car.transform.SetParent(pivot.transform, true);
            var seated = MeshUtil.WorldBounds(car);
            car.transform.position += new Vector3(-seated.center.x, 0.36f - seated.min.y, -seated.center.z);
            pivot.AddComponent<ShowcaseSpin>();

            var cam = StyleKit.CreateCamera("MenuCamera", true);
            cam.tag = "MainCamera";
            cam.gameObject.AddComponent<AudioListener>();
            cam.transform.position = new Vector3(3.6f, 1.7f, -6.2f);
            cam.transform.LookAt(new Vector3(0f, 0.9f, 0.4f));
            cam.fieldOfView = 42f;
        }

        void BuildUi()
        {
            UiFactory.EnsureEventSystem();
            var canvas = UiFactory.CreateOverlay("MenuCanvas", 30);
            var card = UiFactory.Panel(canvas.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(40f, 40f), new Vector2(620f, -40f), new Color(0.04f, 0.06f, 0.09f, 0.78f));
            var title = UiFactory.CreateText(card, "ALPINE RUSH", 64, TextAnchor.UpperLeft, new Color(1f, 0.86f, 0.35f));
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(36f, -36f);
            title.rectTransform.sizeDelta = new Vector2(-60f, 80f);
            var sub = UiFactory.CreateText(card, "Mountain Grand Prix", 26, TextAnchor.UpperLeft, new Color(0.85f, 0.9f, 0.95f));
            sub.rectTransform.anchorMin = new Vector2(0f, 1f);
            sub.rectTransform.anchorMax = new Vector2(1f, 1f);
            sub.rectTransform.pivot = new Vector2(0f, 1f);
            sub.rectTransform.anchoredPosition = new Vector2(40f, -118f);
            sub.rectTransform.sizeDelta = new Vector2(-70f, 36f);

            var start = UiFactory.CreateButton(card, "Start Game", new Vector2(420f, 70f), GameFlow.StartRace);
            var startRect = start.GetComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0f, 1f);
            startRect.anchorMax = new Vector2(0f, 1f);
            startRect.pivot = new Vector2(0f, 1f);
            startRect.anchoredPosition = new Vector2(40f, -210f);

            var quit = UiFactory.CreateButton(card, "Quit", new Vector2(420f, 70f), GameFlow.Quit);
            var quitRect = quit.GetComponent<RectTransform>();
            quitRect.anchorMin = new Vector2(0f, 1f);
            quitRect.anchorMax = new Vector2(0f, 1f);
            quitRect.pivot = new Vector2(0f, 1f);
            quitRect.anchoredPosition = new Vector2(40f, -300f);

            var help = UiFactory.CreateText(card,
                "Keyboard: WASD or arrows to drive\nSpace drift    Left Shift nitro\nR respawn    Esc pause\n\nGamepad: sticks and triggers\nA / LB drift    RB nitro\n\n3 laps  ·  3 AI rivals  ·  Col de la Serra",
                22, TextAnchor.UpperLeft, new Color(0.9f, 0.92f, 0.94f));
            help.rectTransform.anchorMin = new Vector2(0f, 0f);
            help.rectTransform.anchorMax = new Vector2(1f, 1f);
            help.rectTransform.offsetMin = new Vector2(40f, 36f);
            help.rectTransform.offsetMax = new Vector2(-30f, -400f);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(start.gameObject);
        }
    }

    public class ShowcaseSpin : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(0f, 22f * Time.deltaTime, 0f, Space.World);
        }
    }
}
