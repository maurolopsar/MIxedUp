using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MixedUp
{
    /// <summary>The card of the main menu where the map of a solo game is chosen with two arrows, with a picture of the map.</summary>
    public class MapSelector : MonoBehaviour
    {
        public Button previous, next;
        public TMP_Text nameLabel;
        public TMP_Text descriptionLabel;
        [Tooltip("The picture of the selected map.")]
        public RawImage thumbnail;

        void Awake()
        {
            previous.onClick.AddListener(() => Step(-1));
            next.onClick.AddListener(() => Step(1));
        }

        void OnEnable()
        {
            Localization.LanguageChanged += Refresh;
            Refresh();
        }

        void OnDisable() => Localization.LanguageChanged -= Refresh;

        void Step(int direction)
        {
            LevelCatalog.Selected = LevelCatalog.Step(LevelCatalog.Selected, direction);
            Refresh();
        }

        public void Refresh()
        {
            var level = LevelCatalog.Selected;
            if (nameLabel != null) nameLabel.text = level.DisplayName;
            if (descriptionLabel != null) descriptionLabel.text = level.Description;
            if (thumbnail != null)
            {
                var picture = level.Thumbnail;
                thumbnail.texture = picture;
                thumbnail.enabled = picture != null;
            }
        }
    }
}
