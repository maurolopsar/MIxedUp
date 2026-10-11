using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace MixedUp.Tests
{
    /// <summary>Renders every menu of the game to PNG so the interface can be reviewed. Explicit: needs a GPU. Output: env MIXEDUP_SHOTS.</summary>
    public static class MenuShot
    {
        const int Width = 1920, Height = 1080;

        public static string OutDir
        {
            get
            {
                string dir = System.Environment.GetEnvironmentVariable("MIXEDUP_SHOTS");
                if (string.IsNullOrEmpty(dir)) dir = "Temp/Shots";
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        /// <summary>Renders the main camera with every canvas drawn on top of it.</summary>
        public static IEnumerator Capture(string name)
        {
            var cam = Camera.main;
            var rt = new RenderTexture(Width, Height, 24);
            var canvases = Object.FindObjectsByType<Canvas>();
            var modes = new RenderMode[canvases.Length];
            var cameras = new Camera[canvases.Length];
            for (int i = 0; i < canvases.Length; i++)
            {
                modes[i] = canvases[i].renderMode;
                cameras[i] = canvases[i].worldCamera;
                if (!canvases[i].isRootCanvas || canvases[i].renderMode == RenderMode.WorldSpace) continue;
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = cam;
                canvases[i].planeDistance = 1f;
            }
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return null;

            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(OutDir, name + ".png"), tex.EncodeToPNG());

            RenderTexture.active = null;
            cam.targetTexture = null;
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i] == null || !canvases[i].isRootCanvas) continue;
                canvases[i].renderMode = modes[i];
                canvases[i].worldCamera = cameras[i];
            }
            Object.Destroy(tex);
            rt.Release();
            Object.Destroy(rt);
        }
    }

    public class MainMenuGallery2 : SceneTestBase
    {
        protected override string SceneToLoad => MenuScenePath;
        protected override bool NeedsPlayer => false;

        [UnityTest, Explicit("Needs a GPU; writes PNGs")]
        public IEnumerator Menus()
        {
            OnlineSession.Enabled = false;
            RoomServices.Use(new LocalRoomService { BotDelay = 0.3f });
            var menu = Object.FindAnyObjectByType<MainMenu>();
            Localization.SetLanguage(Language.Spanish);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return MenuShot.Capture("m01_main");

            menu.achievementsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            yield return MenuShot.Capture("m02_achievements");
            menu.achievements.Close();

            menu.settingsButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);
            yield return MenuShot.Capture("m03_settings");
            menu.settings.Close();

            menu.multiplayerButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return MenuShot.Capture("m04_lobby_entry");
            menu.lobby.nameInput.text = "Mauro";
            menu.lobby.createButton.onClick.Invoke();
            yield return new WaitForSecondsRealtime(2.2f);
            yield return MenuShot.Capture("m05_lobby_room");
            menu.lobby.Close();

            OnlineSession.Enabled = true;
            RoomServices.Use(null);
        }
    }

    public class LevelMenuGallery : SceneTestBase
    {
        [UnityTest, Explicit("Needs a GPU; writes PNGs")]
        public IEnumerator Menus()
        {
            Localization.SetLanguage(Language.Spanish);
            yield return new WaitForSecondsRealtime(0.5f);
            Cursor.lockState = CursorLockMode.None;

            ui.pausePanel.SetActive(true);
            ui.hudRoot.SetActive(true);
            yield return new WaitForSecondsRealtime(0.2f);
            yield return MenuShot.Capture("l01_pause");
            ui.pausePanel.SetActive(false);

            ui.settingsPanel.Open();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return MenuShot.Capture("l02_pause_settings");
            ui.settingsPanel.Close();

            ui.manualPanel.Show();
            yield return new WaitForSecondsRealtime(0.3f);
            yield return MenuShot.Capture("l03_manual");
            ui.manualPanel.Hide();

            ui.hudRoot.SetActive(false);
            ui.gameOverPanel.SetActive(true);
            ui.deathCauseLabel.text = Localization.Get("death.heat");
            ui.quipLabel.text = Localization.Get("quip.1");
            yield return new WaitForSecondsRealtime(0.2f);
            yield return MenuShot.Capture("l04_gameover");
            ui.gameOverPanel.SetActive(false);

            ui.resultsPanel.SetActive(true);
            ui.resultsScreen.Show(new DeliveryResult { Outcome = CombinationOutcome.Safe, BaseReward = 350, Reward = 350, Delivered = 6, Total = 6, Seconds = 95f });
            yield return new WaitForSecondsRealtime(0.4f);
            yield return MenuShot.Capture("l05_results");
            ui.resultsPanel.SetActive(false);
        }
    }

    public class OnlineLobbyGallery : SceneTestBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Lobby.unity";
        protected override bool NeedsPlayer => false;

        [UnityTest, Explicit("Needs a GPU; writes PNGs")]
        public IEnumerator Lobby()
        {
            Localization.SetLanguage(Language.Spanish);
            yield return new WaitForSecondsRealtime(0.8f);
            yield return MenuShot.Capture("o01_online_lobby");
        }
    }
}
