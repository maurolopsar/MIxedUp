using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace MixedUp.EditorTools
{
    /// <summary>
    /// What makes the two extra maps feel like places: huts, signs that say what things are, props with a reason to be there
    /// and a handful of secrets to find (like the ducks, the log and the snowman of the first map).
    /// </summary>
    public static partial class PrototypeBuilder
    {
        static Material roofRed, roofSlate, clothRed, creamPaint, furWhite, darkWater, buoyOrange, coalBlack, fishBlue, krakenPurple, glassGreen;

        static void EnsureExtraMaterials()
        {
            if (roofRed != null) return;
            roofRed = Mat("RoofRed", new Color(0.6f, 0.25f, 0.2f));
            roofSlate = Mat("RoofSlate", new Color(0.3f, 0.32f, 0.37f));
            clothRed = Mat("ClothRed", new Color(0.78f, 0.22f, 0.2f));
            creamPaint = Mat("CreamPaint", new Color(0.93f, 0.88f, 0.74f));
            furWhite = Mat("FurWhite", new Color(0.8f, 0.87f, 0.95f), 0.1f);
            darkWater = Mat("DarkWater", new Color(0.09f, 0.19f, 0.25f), 0.7f);
            buoyOrange = Mat("BuoyOrange", new Color(0.95f, 0.5f, 0.14f));
            coalBlack = Mat("Coal", new Color(0.17f, 0.16f, 0.17f));
            fishBlue = Mat("FishBlue", new Color(0.5f, 0.68f, 0.8f), 0.4f);
            krakenPurple = Mat("KrakenPurple", new Color(0.46f, 0.22f, 0.5f), 0.35f);
            glassGreen = Mat("GlassGreen", new Color(0.4f, 0.62f, 0.45f), 0.8f);
        }

        // ------------------------------------------------------------- text and signs

        static float YawToward(Vector3 from, Vector3 to)
        {
            var d = to - from;
            return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        }

        static TextMeshPro WorldText(Transform parent, string key, Vector3 localPosition, Quaternion localRotation, Vector2 size, float maxFont, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            var text = go.AddComponent<TextMeshPro>();
            if (UiFactory.Font != null) text.font = UiFactory.Font;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.enableAutoSizing = true;
            text.fontSizeMin = 0.3f;
            text.fontSizeMax = maxFont;
            text.rectTransform.sizeDelta = size;
            text.text = Localization.Get(key);
            var localized = go.AddComponent<LocalizedText>();
            localized.key = key;
            return text;
        }

        /// <summary>
        /// A wooden board on a post with a few lines of text (title, then the joke). It faces `facing`, so it is read when walking
        /// in from that side.
        /// </summary>
        static void Notice(Transform parent, Vector3 position, Vector3 facing, string key, Mats m, float width = 2.3f)
        {
            EnsureExtraMaterials();
            var root = new GameObject("Notice").transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(position, Quaternion.Euler(0f, YawToward(position, facing), 0f));

            Prim(PrimitiveType.Cube, "Post", root, new Vector3(0f, 0.85f, 0f), new Vector3(0.14f, 1.7f, 0.14f), m.woodDark, true, null, true);
            Prim(PrimitiveType.Cube, "Board", root, new Vector3(0f, 1.6f, 0f), new Vector3(width, 1.05f, 0.08f), m.wood, false, null, true);
            Prim(PrimitiveType.Cube, "Paper", root, new Vector3(0f, 1.6f, 0.05f), new Vector3(width - 0.18f, 0.9f, 0.02f), creamPaint, false);
            // TextMeshPro is read from its -Z side, so the text on the +Z face is turned round.
            WorldText(root, key, new Vector3(0f, 1.6f, 0.075f), Quaternion.Euler(0f, 180f, 0f), new Vector2(width - 0.3f, 0.8f), 1.5f, UiFactory.Ink);
        }

        /// <summary>A post with arrow planks pointing at the places named on them.</summary>
        static void DirectionSign(Transform parent, Vector3 position, Mats m, params (string key, Vector3 target)[] arms)
        {
            EnsureExtraMaterials();
            var root = new GameObject("DirectionSign").transform;
            root.SetParent(parent, false);
            root.position = position;

            Prim(PrimitiveType.Cube, "Post", root, new Vector3(0f, 1.5f, 0f), new Vector3(0.2f, 3f, 0.2f), m.woodDark, true, null, true);
            for (int i = 0; i < arms.Length; i++)
            {
                var plank = new GameObject("Arm" + (i + 1)).transform;
                plank.SetParent(root, false);
                plank.localPosition = new Vector3(0f, 2.65f - i * 0.5f, 0f);
                plank.localRotation = Quaternion.Euler(0f, YawToward(position, arms[i].target), 0f);

                Prim(PrimitiveType.Cube, "Board", plank, new Vector3(0f, 0f, 0.95f), new Vector3(0.1f, 0.38f, 1.9f), m.wood, false, null, true);
                Prim(PrimitiveType.Cube, "Tip", plank, new Vector3(0f, 0f, 1.9f), new Vector3(0.1f, 0.27f, 0.27f), m.wood, false, Quaternion.Euler(0f, 45f, 0f), true);
                // The arm points along +Z of the plank: the text sits on both of its flat sides, readable from either.
                WorldText(plank, arms[i].key, new Vector3(0.07f, 0f, 0.95f), Quaternion.Euler(0f, -90f, 0f), new Vector2(1.7f, 0.3f), 0.9f, UiFactory.Ink);
                WorldText(plank, arms[i].key, new Vector3(-0.07f, 0f, 0.95f), Quaternion.Euler(0f, 90f, 0f), new Vector2(1.7f, 0.3f), 0.9f, UiFactory.Ink);
            }
        }

        // ------------------------------------------------------------- secrets and effects

        static SecretSpot AddSecret(Transform parent, string id, Vector3 position, string toastKey, string promptKey = "prompt.secret",
            float radius = 1.6f, Transform reaction = null)
        {
            var go = new GameObject("Secret_" + id);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var trigger = go.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius;
            var spot = go.AddComponent<SecretSpot>();
            spot.id = id;
            spot.promptKey = promptKey;
            spot.toastKey = toastKey;
            spot.reaction = reaction;
            return spot;
        }

        static RubberDuck AddDuck(Transform parent, Vector3 position, float yaw, Material material)
        {
            if (duckMesh == null) duckMesh = SaveInkedMesh(LowPolyProps.Duck());
            var duck = new GameObject("RubberDuck");
            duck.transform.SetParent(parent, false);
            duck.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            var visual = MeshObject("Visual", duck.transform, duckMesh, material);
            visual.isStatic = false;
            var trigger = duck.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.6f;
            trigger.center = Vector3.up * 0.3f;
            var script = duck.AddComponent<RubberDuck>();
            script.visual = visual.transform;
            return script;
        }

        /// <summary>A patch of ground swept by gusts of wind, with streaks of wind drawn while it blows.</summary>
        static void AddGust(Transform parent, Vector3 position, Vector3 direction, Vector3 halfExtents)
        {
            var gust = new GameObject("GustZone");
            gust.transform.SetParent(parent, false);
            gust.transform.position = position;
            var zone = gust.AddComponent<GustZone>();
            zone.direction = direction;
            zone.halfExtents = halfExtents;

            var wind = NewParticles("Wind", gust.transform, fxSoft);
            Vector3 along = direction.normalized;
            wind.transform.localPosition = -along * halfExtents.x;
            wind.transform.localRotation = Quaternion.LookRotation(along);
            var main = wind.main;
            main.loop = true;
            main.startLifetime = 0.9f;
            main.startSpeed = 12f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
            main.startColor = new Color(1f, 1f, 1f, 0.55f);
            main.maxParticles = 120;
            var emission = wind.emission;
            emission.enabled = false;
            var shape = wind.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(halfExtents.z * 1.6f, 2.4f, 0.2f);
            var windRenderer = wind.GetComponent<ParticleSystemRenderer>();
            windRenderer.renderMode = ParticleSystemRenderMode.Stretch;
            windRenderer.lengthScale = 9f;
            zone.wind = wind;
        }

        // ------------------------------------------------------------- buildings

        /// <summary>
        /// A small building with a gabled roof, a door and a lit window on its +Z face, and an optional chimney. `size` is the
        /// width, wall height and depth. The roof is a pair of slabs, with a thin cap of snow on top when `cap` is given.
        /// </summary>
        static Transform Hut(Transform parent, string name, Vector3 position, float yaw, Vector3 size, Material walls, Material roof, Material cap,
            Mats m, ArtAssets art, bool chimney = true)
        {
            EnsureExtraMaterials();
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            float w = size.x, h = size.y, d = size.z;
            Prim(PrimitiveType.Cube, "Walls", root, new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, d), walls, true, null, true);

            // Roof slabs: their outer edges rest on the top of the walls (with an overhang) and meet at the ridge.
            const float angle = 32f;
            float half = d * 0.5f + 0.35f;
            float length = half / Mathf.Cos(angle * Mathf.Deg2Rad);
            float centreY = h + Mathf.Sin(angle * Mathf.Deg2Rad) * length * 0.5f;
            float centreZ = Mathf.Cos(angle * Mathf.Deg2Rad) * length * 0.5f;
            foreach (float side in new[] { 1f, -1f })
            {
                var tilt = Quaternion.Euler(side * angle, 0f, 0f);
                Prim(PrimitiveType.Cube, "Roof", root, new Vector3(0f, centreY, side * centreZ), new Vector3(w + 0.5f, 0.16f, length), roof, false, tilt, true);
                if (cap != null)
                    Prim(PrimitiveType.Cube, "RoofSnow", root, new Vector3(0f, centreY + 0.1f, side * centreZ), new Vector3(w + 0.56f, 0.1f, length * 0.98f), cap, false, tilt);
            }

            // The gable ends are filled with three stacked blocks that stay under the roof, so no sky shows through the triangle.
            foreach (float fraction in new[] { 0.85f, 0.68f, 0.51f, 0.34f, 0.17f })
            {
                float reach = d * 0.5f * fraction;
                float rise = (half - reach) * Mathf.Tan(angle * Mathf.Deg2Rad) * 0.85f;
                Prim(PrimitiveType.Cube, "Gable", root, new Vector3(0f, h + rise * 0.5f, 0f), new Vector3(w * 0.99f, rise, reach * 2f), walls, false);
            }

            Prim(PrimitiveType.Cube, "Door", root, new Vector3(0f, 1f, d * 0.5f + 0.04f), new Vector3(0.95f, 2f, 0.08f), m.woodDark, false, null, true);
            Prim(PrimitiveType.Cube, "Window", root, new Vector3(w * 0.3f, h * 0.62f, d * 0.5f + 0.03f), new Vector3(0.85f, 0.65f, 0.06f), creamPaint, false);
            Prim(PrimitiveType.Cube, "Step", root, new Vector3(0f, 0.08f, d * 0.5f + 0.5f), new Vector3(1.5f, 0.16f, 0.8f), m.stone, true, null, true);
            if (chimney) Prim(PrimitiveType.Cube, "Chimney", root, new Vector3(w * 0.3f, h + 1.1f, -d * 0.15f), new Vector3(0.5f, 1.5f, 0.5f), m.stone, false, null, true);
            AddLampLight(root, new Vector3(-w * 0.28f, h * 0.7f, d * 0.5f + 0.35f), 9f, 1.8f);
            return root;
        }

        static Transform Barrel(Transform parent, Vector3 position, Material material, float scale = 1f)
        {
            var b = Prim(PrimitiveType.Cylinder, "Barrel", parent, position + new Vector3(0f, 0.45f * scale, 0f), new Vector3(0.7f, 0.45f, 0.7f) * scale, material, true, null, false);
            return b.transform;
        }

        /// <summary>The ring of a lifebuoy hung on a post.</summary>
        static void Lifebuoy(Transform parent, Vector3 position, float yaw, Mats m)
        {
            var root = new GameObject("Lifebuoy").transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            Prim(PrimitiveType.Cube, "Post", root, new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 1.6f, 0.12f), m.woodDark, true, null, true);
            Prim(PrimitiveType.Cylinder, "Ring", root, new Vector3(0f, 1.25f, 0.18f), new Vector3(0.7f, 0.05f, 0.7f), buoyOrange, false, Quaternion.Euler(90f, 0f, 0f));
            Prim(PrimitiveType.Cylinder, "Hole", root, new Vector3(0f, 1.25f, 0.2f), new Vector3(0.36f, 0.055f, 0.36f), m.woodDark, false, Quaternion.Euler(90f, 0f, 0f));
        }

        // =============================================================== frozen summit

        static void AddSummitDetails(MapContext c, List<(Vector2 centre, float radius)> reserved)
        {
            EnsureExtraMaterials();
            var env = c.env;
            var m = c.m;
            var art = c.art;
            var facing = c.truckPosition;
            var details = new GameObject("Details").transform;
            details.SetParent(env, false);

            // ---- the trail's start: where you are and where things are
            Notice(details, c.truckPosition + new Vector3(7.5f, 0f, 6.5f), c.truckPosition + new Vector3(12f, 0f, 20f), "notice.summit.start", m);
            DirectionSign(details, new Vector3(-17f, 0f, -12f), m,
                ("sign.lake", new Vector3(6f, 0f, -6f)), ("sign.glacier", new Vector3(-30f, 0f, 1.4f)),
                ("sign.camp", new Vector3(-30f, 0f, 14f)), ("sign.tower", new Vector3(38f, 0f, -7f)), ("sign.cabin", new Vector3(-20f, 0f, 46f)));
            reserved.Add((new Vector2(-17f, -12f), 3f));

            // ---- base camp: logs to sit on, a drying rack and the tent made into a proper tent
            var camp = new Vector3(-30f, 0f, 14f);
            foreach (float angle in new[] { 100f, 200f, 300f })
            {
                float a = angle * Mathf.Deg2Rad;
                var at = camp + new Vector3(Mathf.Cos(a) * 3.1f, 0.2f, Mathf.Sin(a) * 3.1f);
                Prim(PrimitiveType.Cube, "Bench", details, at, new Vector3(2f, 0.4f, 0.5f), m.woodDark, true, Quaternion.Euler(0f, -angle + 90f, 0f), true);
            }
            Prim(PrimitiveType.Cylinder, "CookingPot", details, camp + new Vector3(0.1f, 0.15f, 0.2f), new Vector3(0.5f, 0.18f, 0.5f), coalBlack, false);
            var rackBase = new Vector3(-26.5f, 0f, 20.5f);
            Prim(PrimitiveType.Cube, "RackPostA", details, rackBase + new Vector3(-1.4f, 0.95f, 0f), new Vector3(0.12f, 1.9f, 0.12f), m.woodDark, true, null, true);
            Prim(PrimitiveType.Cube, "RackPostB", details, rackBase + new Vector3(1.4f, 0.95f, 0f), new Vector3(0.12f, 1.9f, 0.12f), m.woodDark, true, null, true);
            Prim(PrimitiveType.Cube, "RackBeam", details, rackBase + new Vector3(0f, 1.85f, 0f), new Vector3(3f, 0.1f, 0.1f), m.woodDark, false, null, true);
            var clothMats = new[] { clothRed, creamPaint, m.ice, clothRed };
            for (int i = 0; i < 4; i++)
                Prim(PrimitiveType.Cube, "Sock" + i, details, rackBase + new Vector3(-1.05f + i * 0.7f, 1.45f, 0.02f), new Vector3(0.42f, 0.75f, 0.03f), clothMats[i], false);

            // The tent gets a roof ridge, a door flap and a story.
            var tentCentre = new Vector3(-33.5f, 0f, 18f);
            var tent = new GameObject("TentRoof").transform;
            tent.SetParent(details, false);
            tent.SetPositionAndRotation(tentCentre, Quaternion.Euler(0f, 20f, 0f));
            foreach (float side in new[] { 1f, -1f })
                Prim(PrimitiveType.Cube, "Flap", tent, new Vector3(0f, 2.15f, side * 0.95f), new Vector3(3.3f, 0.12f, 2.2f), clothRed, false, Quaternion.Euler(side * 35f, 0f, 0f), true);
            Prim(PrimitiveType.Cube, "Door", tent, new Vector3(0f, 0.85f, 1.52f), new Vector3(1f, 1.6f, 0.06f), coalBlack, false);
            AddSecret(details, "tent", tentCentre + new Vector3(0.5f, 1f, 2.4f), "secret.tent", "prompt.secret", 1.4f);
            Notice(details, camp + new Vector3(4.8f, 0f, -3.8f), facing, "notice.summit.camp", m);
            reserved.Add((new Vector2(-26.5f, 20.5f), 3.2f));

            // ---- the mountain hut with a sled outside and a stack of firewood
            var cabinPosition = new Vector3(-20f, 0f, 46f);
            Hut(details, "Cabin", cabinPosition, 180f, new Vector3(5.2f, 2.4f, 3.8f), m.woodDark, roofRed, m.snow, m, art);
            reserved.Add((new Vector2(cabinPosition.x, cabinPosition.z), 5.5f));
            AddSecret(details, "cabin", cabinPosition + new Vector3(0f, 1f, -2.9f), "secret.cabin", "prompt.knock", 1.5f);
            Notice(details, cabinPosition + new Vector3(4.4f, 0f, -3.6f), facing, "notice.summit.cabin", m);
            Lamp(details, art, cabinPosition + new Vector3(-3.4f, 0f, -3.2f));

            var sled = new GameObject("Sled").transform;
            sled.SetParent(details, false);
            sled.SetPositionAndRotation(cabinPosition + new Vector3(-5.6f, 0f, -3.2f), Quaternion.Euler(0f, 25f, 0f));
            foreach (float side in new[] { 1f, -1f })
                Prim(PrimitiveType.Cube, "Skid", sled, new Vector3(side * 0.35f, 0.09f, 0f), new Vector3(0.1f, 0.1f, 2.2f), m.woodDark, false, null, true);
            Prim(PrimitiveType.Cube, "Seat", sled, new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.1f, 1.6f), m.wood, true, null, true);
            Prim(PrimitiveType.Cube, "Rope", sled, new Vector3(0f, 0.16f, 1.4f), new Vector3(0.05f, 0.05f, 0.8f), creamPaint, false);
            AddSecret(details, "sled", sled.position + new Vector3(0f, 0.6f, 0f), "secret.sled", "prompt.secret", 1.3f, sled);

            var wood = cabinPosition + new Vector3(3.6f, 0f, 0.6f);
            for (int row = 0; row < 3; row++)
                for (int i = 0; i < 4 - row; i++)
                    Prim(PrimitiveType.Cylinder, "Log", details, wood + new Vector3(i * 0.42f + row * 0.21f, 0.2f + row * 0.36f, 0f), new Vector3(0.38f, 0.5f, 0.38f), m.woodDark, false, Quaternion.Euler(90f, 0f, 0f));

            // ---- the other places say what they are
            Notice(details, new Vector3(-14.5f, 0f, -8.5f), facing, "notice.summit.lake", m);
            Notice(details, new Vector3(28.5f, 0f, 9.5f), facing, "notice.summit.pylon", m);
            Notice(details, new Vector3(4.5f, 0f, 34.5f), facing, "notice.summit.outcrop", m);
            Notice(details, new Vector3(-34.5f, 0f, -9f), facing, "notice.summit.glacier", m);
            Notice(details, new Vector3(33.2f, 0f, -3.2f), facing, "notice.summit.tower", m);
            Notice(details, new Vector3(-32.4f, 0f, 29.2f), facing, "notice.summit.mine", m);

            // ---- frozen ducks, the ice-fishing hole and the gusts that sweep the east shore
            foreach (var (x, z, yaw) in new[] { (-9.5f, -16.2f, 40f), (21f, -16.5f, 210f), (22.6f, 3.4f, 120f) })
                AddDuck(details, new Vector3(x, 0.02f, z), yaw, art.palette);

            var fishing = new Vector3(13f, 0f, -13.5f);
            Prim(PrimitiveType.Cylinder, "IceHole", details, fishing + new Vector3(0f, 0.034f, 0f), new Vector3(1.5f, 0.012f, 1.5f), darkWater, false);
            Prim(PrimitiveType.Cylinder, "Bucket", details, fishing + new Vector3(1.4f, 0.25f, 0.3f), new Vector3(0.5f, 0.25f, 0.5f), m.woodDark, true);
            Prim(PrimitiveType.Cylinder, "Stool", details, fishing + new Vector3(-1.5f, 0.2f, -0.3f), new Vector3(0.55f, 0.2f, 0.55f), m.wood, true);
            Beam(details, "Rod", fishing + new Vector3(-1.5f, 0.75f, -0.3f), fishing + new Vector3(-0.1f, 0.1f, 0.4f), 0.05f, m.woodDark, false);
            AddSecret(details, "fish", fishing + new Vector3(0f, 0.5f, 0f), "secret.fish", "prompt.fish", 1.6f);
            Notice(details, fishing + new Vector3(2.6f, 0f, -2.2f), facing, "notice.summit.lakeshore", m);
            reserved.Add((new Vector2(fishing.x, fishing.z), 3f));

            AddGust(details, new Vector3(27f, 1.5f, -16f), new Vector3(-1f, 0f, 0.3f), new Vector3(5f, 2.5f, 4f));

            // ---- the yeti: a trail of footprints leads to a very shy pile of fur peeking from behind a rock
            var yetiAt = new Vector3(-40.5f, 0f, 52.5f);
            for (int i = 0; i < 11; i++)
            {
                float t = i / 10f;
                var along = Vector3.Lerp(new Vector3(-24f, 0f, 38f), yetiAt + new Vector3(3.2f, 0f, -3f), t);
                float yaw = YawToward(new Vector3(-24f, 0f, 38f), yetiAt);
                var sideStep = Quaternion.Euler(0f, yaw, 0f) * Vector3.right * (i % 2 == 0 ? 0.45f : -0.45f);
                Prim(PrimitiveType.Sphere, "Print" + i, details, along + sideStep + new Vector3(0f, 0.03f, 0f), new Vector3(0.55f, 0.03f, 1.0f),
                    coalBlack, false, Quaternion.Euler(0f, yaw, 0f));
            }
            var yeti = new GameObject("Yeti").transform;
            yeti.SetParent(details, false);
            yeti.SetPositionAndRotation(yetiAt, Quaternion.Euler(0f, YawToward(yetiAt, new Vector3(0f, 0f, 10f)), 0f));
            Prim(PrimitiveType.Sphere, "Body", yeti, new Vector3(0f, 1.15f, 0f), new Vector3(2.5f, 2.3f, 2.2f), furWhite, true);
            var head = Prim(PrimitiveType.Sphere, "Head", yeti, new Vector3(0f, 2.8f, 0.35f), new Vector3(1.5f, 1.4f, 1.4f), furWhite, false);
            foreach (float side in new[] { 1f, -1f })
            {
                Prim(PrimitiveType.Sphere, "Eye", yeti, new Vector3(side * 0.32f, 2.95f, 1.02f), Vector3.one * 0.3f, creamPaint, false);
                Prim(PrimitiveType.Sphere, "Pupil", yeti, new Vector3(side * 0.32f, 2.95f, 1.15f), Vector3.one * 0.13f, coalBlack, false);
                Prim(PrimitiveType.Sphere, "Arm", yeti, new Vector3(side * 1.25f, 1.25f, 0.5f), new Vector3(0.85f, 1.4f, 0.85f), furWhite, false);
            }
            Prim(PrimitiveType.Sphere, "Nose", yeti, new Vector3(0f, 2.72f, 1.1f), new Vector3(0.28f, 0.22f, 0.22f), m.ice, false);
            AddSecret(details, "yeti", yetiAt + new Vector3(0f, 1.2f, 2.6f), "secret.yeti", "prompt.wave", 1.8f, head.transform);
            PlaceProp(details, art.rocks[1 % art.rocks.Length], yetiAt + new Vector3(2.2f, 0f, 1.6f), 40f, 1.8f);
            reserved.Add((new Vector2(yetiAt.x, yetiAt.z), 5f));
        }

        // =============================================================== dusk harbour

        static void AddHarbourDetails(MapContext c, List<(Vector2 centre, float radius)> reserved)
        {
            EnsureExtraMaterials();
            var env = c.env;
            var m = c.m;
            var art = c.art;
            var facing = new Vector3(0f, 0f, 20f);
            var details = new GameObject("Details").transform;
            details.SetParent(env, false);

            // ---- signs: what is where
            DirectionSign(details, new Vector3(6f, 0f, -11f), m,
                ("sign.lighthouse", new Vector3(38f, 0f, -19f)), ("sign.warehouse", new Vector3(40f, 0f, -30f)),
                ("sign.market", new Vector3(32f, 0f, -12f)), ("sign.master", new Vector3(-14f, 0f, -12f)),
                ("sign.barge", new Vector3(-18f, 0f, 36f)));
            reserved.Add((new Vector2(6f, -11f), 3f));
            Notice(details, new Vector3(-6f, 0f, -10f), new Vector3(-6f, 0f, -30f), "notice.harbour.welcome", m, 2.8f);
            Notice(details, new Vector3(2.4f, 0.25f, 5.5f), new Vector3(0f, 0f, -20f), "notice.harbour.pier", m);
            Notice(details, new Vector3(21.5f, 0.25f, 11.4f), new Vector3(-20f, 0f, -20f), "notice.harbour.east", m);
            Notice(details, new Vector3(-2.2f, 0.25f, 24.5f), new Vector3(0f, 0f, -20f), "notice.harbour.generator", m);
            Notice(details, new Vector3(-14.2f, 0.25f, 14f), new Vector3(0f, 0f, -20f), "notice.harbour.brazier", m);
            Notice(details, new Vector3(-14.3f, 0.25f, 33.8f), new Vector3(0f, 0f, -20f), "notice.harbour.barge", m);

            // ---- the harbour master's hut with its flag
            var hutAt = new Vector3(-14f, 0f, -13f);
            Hut(details, "HarbourMaster", hutAt, 0f, new Vector3(4f, 2.4f, 3.2f), creamPaint, roofRed, null, m, art);
            reserved.Add((new Vector2(hutAt.x, hutAt.z), 4.2f));
            Notice(details, hutAt + new Vector3(3.6f, 0f, 3.2f), new Vector3(0f, 0f, -30f), "notice.harbour.master", m);
            var pole = Prim(PrimitiveType.Cylinder, "Flagpole", details, new Vector3(-10.5f, 3.2f, -13f), new Vector3(0.12f, 3.2f, 0.12f), m.woodDark, true);
            var flag = MeshObject("Flag", details, flags[0], art.palette);
            flag.transform.position = new Vector3(-10.5f, 5.4f, -13f);
            flag.transform.localScale = Vector3.one * 0.9f;

            // ---- the warehouse where the crates come from, and the crane that lifts them
            var warehouseAt = new Vector3(40f, 0f, -30f);
            Hut(details, "Warehouse", warehouseAt, 0f, new Vector3(7f, 3.4f, 5f), m.wood, roofSlate, null, m, art, false);
            reserved.Add((new Vector2(warehouseAt.x, warehouseAt.z), 6f));
            Notice(details, warehouseAt + new Vector3(-5.2f, 0f, 3.8f), new Vector3(0f, 0f, -20f), "notice.harbour.warehouse", m);

            var crane = new GameObject("Crane").transform;
            crane.SetParent(details, false);
            crane.position = new Vector3(26f, 0f, -10f);
            Prim(PrimitiveType.Cube, "Base", crane, new Vector3(0f, 0.35f, 0f), new Vector3(2.2f, 0.7f, 2.2f), m.stone, true, null, true);
            Prim(PrimitiveType.Cube, "Mast", crane, new Vector3(0f, 4.4f, 0f), new Vector3(0.6f, 8f, 0.6f), roofSlate, true, null, true);
            Beam(crane, "Jib", new Vector3(0f, 8.2f, -3.2f), new Vector3(0f, 8.2f, 9.5f), 0.4f, m.woodDark);
            Beam(crane, "Stay", new Vector3(0f, 8.1f, 0f), new Vector3(0f, 8.1f, -3.2f), 0.3f, m.woodDark, false);
            Prim(PrimitiveType.Cube, "Counterweight", crane, new Vector3(0f, 7.6f, -3.4f), new Vector3(1.2f, 1.1f, 1.2f), m.stone, false, null, true);
            var hook = new GameObject("Hook").transform;
            hook.SetParent(crane, false);
            hook.localPosition = new Vector3(0f, 8f, 7.2f);
            hook.gameObject.AddComponent<Bob>().swayDegrees = 4f;
            hook.GetComponent<Bob>().height = 0.04f;
            Prim(PrimitiveType.Cube, "Rope", hook, new Vector3(0f, -2.1f, 0f), new Vector3(0.06f, 4.2f, 0.06f), coalBlack, false);
            Prim(PrimitiveType.Cube, "HangingCrate", hook, new Vector3(0f, -4.6f, 0f), new Vector3(1.2f, 1.2f, 1.2f), m.wood, false, null, true);
            AddSecret(details, "crane", crane.position + new Vector3(1.8f, 1f, 0.5f), "secret.crane", "prompt.secret", 1.4f);
            Notice(details, crane.position + new Vector3(-3.4f, 0f, 1.5f), new Vector3(0f, 0f, -30f), "notice.harbour.crane", m);
            reserved.Add((new Vector2(26f, -10f), 3.2f));

            // ---- the old lighthouse: the borrowed tower gets a beam that sweeps round the bay
            var beam = new GameObject("LighthouseBeam");
            beam.transform.SetParent(details, false);
            beam.transform.position = new Vector3(38f, 8.6f, -19f);
            var lightObject = new GameObject("Beam");
            lightObject.transform.SetParent(beam.transform, false);
            lightObject.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 75f;
            light.spotAngle = 32f;
            light.intensity = 9f;
            light.color = new Color(1f, 0.9f, 0.6f);
            light.shadows = LightShadows.None;
            beam.AddComponent<Spinner>().degreesPerSecond = new Vector3(0f, 35f, 0f);
            Notice(details, new Vector3(33.5f, 0f, -22.5f), new Vector3(0f, 0f, -30f), "notice.harbour.lighthouse", m);

            // ---- the fish market: three stalls under striped awnings
            for (int i = 0; i < 3; i++)
            {
                var stall = new GameObject("Stall" + (i + 1)).transform;
                stall.SetParent(details, false);
                stall.SetPositionAndRotation(new Vector3(28.5f + i * 3.6f, 0f, -13.5f), Quaternion.identity);
                Prim(PrimitiveType.Cube, "Counter", stall, new Vector3(0f, 0.5f, 0f), new Vector3(2.4f, 1f, 0.9f), m.wood, true, null, true);
                Prim(PrimitiveType.Cube, "PostL", stall, new Vector3(-1.1f, 1.1f, -0.7f), new Vector3(0.1f, 2.2f, 0.1f), m.woodDark, false, null, true);
                Prim(PrimitiveType.Cube, "PostR", stall, new Vector3(1.1f, 1.1f, -0.7f), new Vector3(0.1f, 2.2f, 0.1f), m.woodDark, false, null, true);
                Prim(PrimitiveType.Cube, "Awning", stall, new Vector3(0f, 2.25f, -0.1f), new Vector3(2.7f, 0.1f, 1.8f), i % 2 == 0 ? clothRed : creamPaint, false, Quaternion.Euler(14f, 0f, 0f), true);
                for (int fish = 0; fish < 3; fish++)
                    Prim(PrimitiveType.Sphere, "Fish", stall, new Vector3(-0.7f + fish * 0.7f, 1.05f, 0.05f), new Vector3(0.5f, 0.14f, 0.22f), fishBlue, false, Quaternion.Euler(0f, 15f * fish, 0f));
            }
            AddSecret(details, "market", new Vector3(32.1f, 1f, -11.6f), "secret.market", "prompt.secret", 1.6f);
            Notice(details, new Vector3(33.2f, 0f, -9.6f), new Vector3(0f, 0f, 30f), "notice.harbour.market", m);
            reserved.Add((new Vector2(32f, -13.5f), 6f));

            // ---- along the quay edge: bollards, lifebuoys and an anchor
            for (int i = 0; i < 9; i++)
            {
                float x = -40f + i * 10f;
                if (Mathf.Abs(x - 26f) < 4f || Mathf.Abs(x - 6f) < 3.5f) continue;
                Prim(PrimitiveType.Cylinder, "Bollard", details, new Vector3(x, 0.4f, -8.6f), new Vector3(0.45f, 0.4f, 0.45f), coalBlack, true);
                Prim(PrimitiveType.Sphere, "BollardTop", details, new Vector3(x, 0.82f, -8.6f), new Vector3(0.5f, 0.3f, 0.5f), coalBlack, false);
            }
            Lifebuoy(details, new Vector3(-8f, 0f, -9.2f), 0f, m);
            Lifebuoy(details, new Vector3(16f, 0f, -9.2f), 0f, m);
            var anchor = new Vector3(-24f, 0f, -9.5f);
            Prim(PrimitiveType.Cube, "AnchorShank", details, anchor + new Vector3(0f, 0.9f, 0f), new Vector3(0.18f, 1.8f, 0.18f), coalBlack, true);
            Prim(PrimitiveType.Cube, "AnchorStock", details, anchor + new Vector3(0f, 1.55f, 0f), new Vector3(0.9f, 0.14f, 0.14f), coalBlack, false);
            Prim(PrimitiveType.Cube, "AnchorArmL", details, anchor + new Vector3(-0.42f, 0.28f, 0f), new Vector3(0.14f, 0.14f, 0.6f), coalBlack, false, Quaternion.Euler(0f, 0f, 35f));
            Prim(PrimitiveType.Cube, "AnchorArmR", details, anchor + new Vector3(0.42f, 0.28f, 0f), new Vector3(0.14f, 0.14f, 0.6f), coalBlack, false, Quaternion.Euler(0f, 0f, -35f));
            Barrel(details, new Vector3(-20f, 0f, -11f), m.woodDark);
            Barrel(details, new Vector3(-19.2f, 0f, -11.6f), m.woodDark);
            reserved.Add((new Vector2(-22f, -10.5f), 3f));

            // ---- in the bay: ducks, a message in a bottle, a seagull that knows too much and something big below
            foreach (var (x, z, yaw) in new[] { (11f, 38f, 20f), (-27f, 10f, 140f), (19f, 31f, 300f) })
                AddDuck(details, new Vector3(x, -0.3f, z), yaw, art.palette);

            var bottle = new GameObject("Bottle").transform;
            bottle.SetParent(details, false);
            bottle.position = new Vector3(-6.6f, 0.25f, 29.3f);
            Prim(PrimitiveType.Cube, "Crate", bottle, new Vector3(0f, 0.35f, 0f), new Vector3(0.8f, 0.7f, 0.8f), m.woodDark, true, null, true);
            Prim(PrimitiveType.Cylinder, "Glass", bottle, new Vector3(0f, 0.95f, 0f), new Vector3(0.22f, 0.26f, 0.22f), glassGreen, false, Quaternion.Euler(0f, 0f, 70f));
            Prim(PrimitiveType.Cylinder, "Neck", bottle, new Vector3(0.28f, 0.95f, 0f), new Vector3(0.09f, 0.12f, 0.09f), glassGreen, false, Quaternion.Euler(0f, 0f, 70f));
            AddSecret(details, "bottle", bottle.position + new Vector3(0f, 0.9f, 0f), "secret.bottle", "prompt.secret", 1.4f, bottle);

            var gull = new GameObject("Seagull").transform;
            gull.SetParent(details, false);
            gull.SetPositionAndRotation(new Vector3(2.5f, 0.25f, 17.5f), Quaternion.Euler(0f, 230f, 0f));
            Prim(PrimitiveType.Cylinder, "Perch", gull, new Vector3(0f, 0.3f, 0f), new Vector3(0.3f, 0.3f, 0.3f), coalBlack, true);
            Prim(PrimitiveType.Sphere, "Body", gull, new Vector3(0f, 0.9f, 0f), new Vector3(0.5f, 0.45f, 0.75f), furWhite, false);
            Prim(PrimitiveType.Sphere, "Head", gull, new Vector3(0f, 1.2f, 0.32f), Vector3.one * 0.3f, furWhite, false);
            Prim(PrimitiveType.Cube, "Beak", gull, new Vector3(0f, 1.18f, 0.55f), new Vector3(0.08f, 0.07f, 0.2f), buoyOrange, false);
            foreach (float side in new[] { 1f, -1f })
                Prim(PrimitiveType.Cube, "Wing", gull, new Vector3(side * 0.3f, 0.92f, -0.05f), new Vector3(0.08f, 0.3f, 0.6f), roofSlate, false, Quaternion.Euler(0f, 0f, side * 12f));
            AddSecret(details, "gull", gull.position + new Vector3(0f, 1f, 0f), "secret.gull", "prompt.secret", 1.3f, gull);

            var kraken = new GameObject("Kraken").transform;
            kraken.SetParent(details, false);
            kraken.position = new Vector3(33f, -0.3f, 45f);
            for (int arm = 0; arm < 3; arm++)
            {
                var tentacle = new GameObject("Tentacle" + (arm + 1)).transform;
                tentacle.SetParent(kraken, false);
                tentacle.localPosition = new Vector3(arm * 1.8f - 1.8f, 0f, (arm % 2) * 1.2f);
                var bob = tentacle.gameObject.AddComponent<Bob>();
                bob.height = 0.18f;
                bob.swayDegrees = 9f;
                bob.speed = 0.8f + arm * 0.15f;
                // Overlapping spheres that get thinner and curl over: one smooth arm rising out of the water.
                float x = 0f, y = 0f;
                float curl = (arm == 1 ? -1f : 1f) * (38f + arm * 8f);
                for (int seg = 0; seg < 10; seg++)
                {
                    float t = seg / 9f;
                    float radius = Mathf.Lerp(0.55f, 0.14f, t);
                    Prim(PrimitiveType.Sphere, "Segment" + seg, tentacle, new Vector3(x, y, 0f), Vector3.one * radius * 2f, krakenPurple, false);
                    float heading = curl * t * t;
                    x += Mathf.Sin(heading * Mathf.Deg2Rad) * radius * 0.85f;
                    y += Mathf.Cos(heading * Mathf.Deg2Rad) * radius * 0.85f;
                }
            }
            AddSecret(details, "kraken", kraken.position + new Vector3(0f, 0.8f, -2.2f), "secret.kraken", "prompt.wave", 2.2f, kraken);
            reserved.Add((new Vector2(33f, 45f), 5f));
        }
    }
}
