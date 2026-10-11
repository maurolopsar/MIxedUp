using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedUp.Tests
{
    /// <summary>
    /// Takes the pictures the map selectors show (Resources/MapThumbs/id.png) from a high viewpoint over each map.
    /// Explicit: needs a GPU. Run it again whenever a map changes, then commit the PNGs.
    /// </summary>
    public abstract class MapThumbnailBase : SceneTestBase
    {
        const int Width = 640, Height = 360;

        protected abstract string MapId { get; }
        protected abstract Vector3 CameraAt { get; }
        protected abstract Vector3 LookAt { get; }

        protected IEnumerator Take()
        {
            var cam = Camera.main;
            cam.GetComponent<ThirdPersonCamera>().enabled = false;
            cam.fieldOfView = 40f;
            cam.transform.position = CameraAt;
            cam.transform.rotation = Quaternion.LookRotation(LookAt - CameraAt);
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

            string dir = System.Environment.GetEnvironmentVariable("MIXEDUP_THUMBS");
            if (string.IsNullOrEmpty(dir)) dir = "Assets/_Project/Resources/MapThumbs";
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, MapId + ".png"), tex.EncodeToPNG());

            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.Destroy(tex);
            rt.Release();
            Object.Destroy(rt);
        }
    }

    public class MeadowThumbnail : MapThumbnailBase
    {
        protected override string MapId => "meadow";
        protected override Vector3 CameraAt => new Vector3(0f, 112f, -34f);
        protected override Vector3 LookAt => new Vector3(0f, 0f, 10f);

        [UnityTest, Explicit("Needs a GPU; writes a PNG into the project")]
        public IEnumerator Meadow() => Take();
    }

    public class SummitThumbnail : MapThumbnailBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Summit.unity";
        protected override string MapId => "summit";
        protected override Vector3 CameraAt => new Vector3(0f, 112f, -34f);
        protected override Vector3 LookAt => new Vector3(0f, 0f, 10f);

        [UnityTest, Explicit("Needs a GPU; writes a PNG into the project")]
        public IEnumerator Summit() => Take();
    }

    public class HarbourThumbnail : MapThumbnailBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Harbour.unity";
        protected override string MapId => "harbour";
        protected override Vector3 CameraAt => new Vector3(0f, 112f, -34f);
        protected override Vector3 LookAt => new Vector3(0f, 0f, 10f);

        [UnityTest, Explicit("Needs a GPU; writes a PNG into the project")]
        public IEnumerator Harbour() => Take();
    }
}
