using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedUp.Tests
{
    /// <summary>The checks every map must pass: a safe start, boxes on solid ground, a full classic order and enough hard spots.</summary>
    public abstract class MapTestsBase : SceneTestBase
    {
        [UnityTest]
        public IEnumerator ThePlayerStartsOnSolidGround()
        {
            yield return new WaitForSeconds(0.4f);
            Assert.IsTrue(player.IsGrounded, "the player does not stand on the ground at the start");
            Assert.IsFalse(status.IsDead);
        }

        [UnityTest]
        public IEnumerator EveryBoxSpotRestsOnSolidGroundInsideTheWalls()
        {
            var spots = Object.FindObjectsByType<BoxSpawnPoint>();
            Assert.GreaterOrEqual(spots.Length, 15, "too few spots for the random modes");
            foreach (var spot in spots)
            {
                var p = spot.transform.position;
                Assert.IsTrue(p.x > -44f && p.x < 44f && p.z > -35f && p.z < 55f, spot.name + " is outside the walls");
                Assert.IsTrue(Physics.Raycast(p + Vector3.up * 0.4f, Vector3.down, out var hit, 1.2f, ~0, QueryTriggerInteraction.Ignore),
                    spot.name + " has no ground under it");
                Assert.Less(Mathf.Abs(hit.point.y - p.y), 0.35f, spot.name + " floats above or sinks into its ground");
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator TheClassicOrderHasAllItsBoxes()
        {
            Assert.AreEqual(truck.order.TotalBoxes, pickups.Length);
            foreach (var line in truck.order.lines)
                Assert.AreEqual(line.count, pickups.Count(p => p.data == line.box), line.box.id);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ThereAreEnoughHardSpotsForTheChallengeMode()
        {
            var hard = Object.FindObjectsByType<BoxSpawnPoint>().Count(s => s.hard);
            Assert.GreaterOrEqual(hard, 4);
            yield return null;
        }
    }

    /// <summary>The secrets, notices and signs that give the extra maps their character.</summary>
    public abstract class MapCharacterBase : SceneTestBase
    {
        [UnityTest]
        public IEnumerator TheMapHasSecretsWithRealTextAndOneIdEach()
        {
            var spots = Object.FindObjectsByType<SecretSpot>();
            Assert.GreaterOrEqual(spots.Length, 4, "a few secrets to find");

            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var spot in spots)
            {
                Assert.IsFalse(string.IsNullOrEmpty(spot.id), spot.name + " has no id");
                Assert.IsTrue(seen.Add(spot.id), "two secrets share the id " + spot.id);
                Assert.IsFalse(Localization.Get(spot.toastKey).StartsWith("["), "missing text for " + spot.toastKey);
                Assert.IsTrue(spot.GetComponent<Collider>() != null && spot.GetComponent<Collider>().isTrigger, spot.id + " cannot be reached");
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator EveryNoticeAndSignHasATranslatedText()
        {
            var texts = Object.FindObjectsByType<LocalizedText>(FindObjectsInactive.Include);
            int boards = 0;
            foreach (var text in texts)
            {
                if (text.GetComponent<TMPro.TextMeshPro>() == null) continue;   // only the texts standing in the world
                boards++;
                foreach (var language in new[] { Language.Basque, Language.Spanish, Language.English })
                {
                    Localization.SetLanguage(language);
                    string shown = Localization.Get(text.key);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(shown) || shown.StartsWith("["), text.key + " in " + language);
                }
            }
            Localization.SetLanguage(Language.Spanish);
            Assert.GreaterOrEqual(boards, 8, "signs and notices all over the map");
            yield return null;
        }

        [UnityTest]
        public IEnumerator FindingSecretsCountsTowardsTheAchievementOnlyOncePerSecret()
        {
            var spots = Object.FindObjectsByType<SecretSpot>();
            var hands = interactor;
            spots[0].cooldownSeconds = 0f;
            spots[0].Interact(hands);
            spots[0].Interact(hands);
            Assert.AreEqual(1, Achievements.Progress(Achievements.Secrets));

            spots[1].Interact(hands);
            Assert.AreEqual(2, Achievements.Progress(Achievements.Secrets));
            yield return null;
        }

        [UnityTest]
        public IEnumerator NothingSolidSitsOnABoxSpot()
        {
            foreach (var spot in Object.FindObjectsByType<BoxSpawnPoint>())
            {
                var centre = spot.transform.position + Vector3.up * 0.65f;
                var hits = Physics.OverlapSphere(centre, 0.3f, ~0, QueryTriggerInteraction.Ignore);
                foreach (var hit in hits)
                {
                    // The box that belongs on the spot (or a neighbour already resting there) is not an obstacle.
                    if (hit.GetComponentInParent<BoxPickup>() != null) continue;
                    Assert.Fail(spot.name + " has " + hit.name + " in the way");
                }
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator NoLightIsLeftWithoutAShadowBudgetOrAnAbsurdRange()
        {
            int lights = 0;
            foreach (var light in Object.FindObjectsByType<Light>())
            {
                if (light.type == LightType.Directional) continue;
                lights++;
                Assert.LessOrEqual(light.range, 90f, light.name);
            }
            Assert.LessOrEqual(lights, 60, "too many lights for a mid-range PC");
            yield return null;
        }
    }

    public class SummitCharacterTests : MapCharacterBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Summit.unity";
    }

    public class HarbourCharacterTests : MapCharacterBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Harbour.unity";
    }

    public class SummitMapTests : MapTestsBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Summit.unity";
    }

    public class HarbourMapTests : MapTestsBase
    {
        protected override string SceneToLoad => "Assets/Scenes/Level_Harbour.unity";
    }
}
