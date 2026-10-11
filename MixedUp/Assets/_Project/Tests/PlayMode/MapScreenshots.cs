using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedUp.Tests
{
    /// <summary>Renders the two extra maps from several viewpoints to PNG files. Explicit: needs a GPU. Output folder: env MIXEDUP_SHOTS.</summary>
    public abstract class MapGalleryBase : SceneTestBase
    {
        const int Width = 1600, Height = 900;

        static string OutDir
        {
            get
            {
                string dir = System.Environment.GetEnvironmentVariable("MIXEDUP_SHOTS");
                if (string.IsNullOrEmpty(dir)) dir = "Temp/Shots";
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        protected IEnumerator Shot(string name, Vector3 position, Vector3 lookAt)
        {
            var cam = Camera.main;
            cam.GetComponent<ThirdPersonCamera>().enabled = false;
            cam.transform.position = position;
            cam.transform.rotation = Quaternion.LookRotation(lookAt - position);
            ui.hudRoot.SetActive(false);

            var rt = new RenderTexture(Width, Height, 24);
            cam.targetTexture = rt;
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
            Object.Destroy(tex);
            rt.Release();
            Object.Destroy(rt);
        }

        protected IEnumerator Gallery(string prefix)
        {
            yield return Shot(prefix + "_01_overview_south", new Vector3(0f, 52f, -70f), new Vector3(0f, 0f, 12f));
            yield return Shot(prefix + "_02_overview_north", new Vector3(0f, 62f, 6f), new Vector3(0f, 0f, 40f));
            yield return Shot(prefix + "_03_overview_west", new Vector3(-70f, 40f, 10f), new Vector3(0f, 0f, 10f));
            yield return Shot(prefix + "_04_overview_east", new Vector3(70f, 40f, 10f), new Vector3(0f, 0f, 10f));
            yield return Shot(prefix + "_05_start", new Vector3(14f, 5f, -38f), new Vector3(0f, 1f, -26f));
            yield return Shot(prefix + "_06_middle", new Vector3(0f, 14f, -4f), new Vector3(0f, 0f, 14f));
            yield return Shot(prefix + "_07_far", new Vector3(0f, 12f, 30f), new Vector3(0f, 1f, 50f));
            yield return Shot(prefix + "_08_low_south", new Vector3(-20f, 3f, -34f), new Vector3(10f, 2f, 0f));
        }
    }

    public class SummitGallery : MapGalleryBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Summit.unity";

        [UnityTest, Explicit("Needs a GPU; writes PNGs")]
        public IEnumerator Summit() => Gallery("summit");

        [UnityTest, Explicit("Needs a GPU; writes PNGs")]
        public IEnumerator SummitDetails()
        {
            yield return Shot("summit_d1_cabin", new Vector3(-13f, 4.5f, 37f), new Vector3(-20f, 1.5f, 46f));
            yield return Shot("summit_d1b_cabin_front", new Vector3(-20f, 3.2f, 37f), new Vector3(-20f, 1.6f, 46f));
            yield return Shot("summit_d1c_cabin_side", new Vector3(-27f, 3.5f, 40f), new Vector3(-20f, 1.6f, 46f));
            yield return Shot("summit_d2_camp", new Vector3(-21f, 5f, 5f), new Vector3(-30f, 1f, 15f));
            yield return Shot("summit_d3_yeti", new Vector3(-31f, 4f, 42f), new Vector3(-40.5f, 1.8f, 52.5f));
            yield return Shot("summit_d4_fishing", new Vector3(8f, 4f, -21f), new Vector3(13f, 0f, -13f));
            yield return Shot("summit_d5_signpost", new Vector3(-11f, 3.2f, -3f), new Vector3(-17f, 2f, -12f));
        }
    }

    public class HarbourGallery : MapGalleryBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Harbour.unity";

        [UnityTest, Explicit("Needs a GPU; writes PNGs")]
        public IEnumerator Harbour() => Gallery("harbour");

        [UnityTest, Explicit("Needs a GPU; writes PNGs")]
        public IEnumerator HarbourDetails()
        {
            yield return Shot("harbour_d1_warehouse", new Vector3(29f, 5f, -20f), new Vector3(40f, 1.5f, -30f));
            yield return Shot("harbour_d2_crane_market", new Vector3(18f, 5.5f, -1f), new Vector3(28f, 3f, -11f));
            yield return Shot("harbour_d3_kraken", new Vector3(24f, 4f, 34f), new Vector3(33f, 1f, 45f));
            yield return Shot("harbour_d4_lighthouse", new Vector3(30f, 6f, -8f), new Vector3(38f, 6f, -19f));
            yield return Shot("harbour_d5_hut", new Vector3(-7f, 4f, -5f), new Vector3(-14f, 1.5f, -13f));
            yield return Shot("harbour_d6_gull_pier", new Vector3(-1f, 3.2f, 11.5f), new Vector3(2.5f, 1f, 17.5f));
        }
    }
}
