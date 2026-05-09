using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Perks.UI
{
    public class ActivePerkSlot : MonoBehaviour
    {
        [SerializeField] private Image _hexOutline;
        [SerializeField] private Image _hexFill;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private GameObject _emptyOverlay;

        [Header("Hex Visuals")]
        [SerializeField] private Image _hexBackground;
        [SerializeField] private Image _hexFrame;
        [SerializeField] private Image _hexGlow;
        [SerializeField] private Image _hexShadow;

        private static readonly Color EmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        private static readonly Color EmptyOutline = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        public void SetEmpty()
        {
            if (_hexFill != null) _hexFill.color = EmptyColor;
            if (_hexOutline != null) _hexOutline.color = EmptyOutline;
            if (_iconImage != null) _iconImage.gameObject.SetActive(false);
            if (_nameText != null) _nameText.text = "";
            if (_descriptionText != null) _descriptionText.text = "Empty Slot";
            if (_emptyOverlay != null) _emptyOverlay.SetActive(true);
        }

        public void SetPerk(Entropy.Perks.IPerk perk, PerkDisplayData data, HexPerkMenuTheme theme)
        {
            if (_hexFill != null) _hexFill.color = data?.AccentColor ?? Color.white;
            if (_hexOutline != null) _hexOutline.color = Color.white;

            if (_iconImage != null)
            {
                _iconImage.gameObject.SetActive(true);
                if (data?.Icon != null) _iconImage.sprite = data.Icon;
            }

            if (_nameText != null)
                _nameText.text = data?.GetDisplayName(perk) ?? perk?.Title ?? "Unknown";

            if (_descriptionText != null)
                _descriptionText.text = data?.GetDisplayDescription(perk) ?? perk?.Description ?? "";

            if (_emptyOverlay != null) _emptyOverlay.SetActive(false);

            ApplyTheme(theme, perk);
        }

        private void ApplyTheme(HexPerkMenuTheme theme, Entropy.Perks.IPerk perk)
        {
            if (theme == null) return;

            if (_hexBackground != null)
            {
                _hexBackground.sprite = theme.slotBackground;
                _hexBackground.color = theme.GetRarityColor(perk is PerkBase pb ? pb.Rarity : PerkRarity.Common);
            }
            if (_hexFrame != null) _hexFrame.sprite = theme.slotFrame;
            if (_hexGlow != null) _hexGlow.sprite = theme.slotGlow;
            if (_hexShadow != null) _hexShadow.sprite = theme.slotShadow;
        }
    }
}
