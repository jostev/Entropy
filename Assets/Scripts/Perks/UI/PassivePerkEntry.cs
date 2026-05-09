using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Perks.UI
{
    public class PassivePerkEntry : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _bonusText;
        [SerializeField] private Image _accentBar;

        [Header("Hex Visuals")]
        [SerializeField] private Image _hexBackground;
        [SerializeField] private Image _hexFrame;
        [SerializeField] private Image _hexGlow;

        public void Bind(IPerk perk, PerkDisplayData data, int stackCount, HexPerkMenuTheme theme)
        {
            if (_nameText != null)
            {
                string displayName = data?.GetDisplayName(perk) ?? perk?.Title ?? "Unknown";
                _nameText.text = displayName;
            }

            if (_levelText != null)
                _levelText.text = $"Lv.{stackCount}";

            if (_iconImage != null && data != null && data.Icon != null)
                _iconImage.sprite = data.Icon;

            if (_accentBar != null && data != null)
                _accentBar.color = data.AccentColor;

            if (_bonusText != null)
            {
                _bonusText.text = GetBonusString(perk, stackCount);
                _bonusText.color = Color.green;
            }

            ApplyTheme(theme, perk);
        }

        private void ApplyTheme(HexPerkMenuTheme theme, IPerk perk)
        {
            if (theme == null) return;

            if (_hexBackground != null)
            {
                _hexBackground.sprite = theme.rowBackground;
                _hexBackground.color = theme.GetRarityColor(perk is PerkBase pb ? pb.Rarity : PerkRarity.Common);
            }
            if (_hexFrame != null) _hexFrame.sprite = theme.rowFrame;
            if (_hexGlow != null) _hexGlow.sprite = theme.rowGlow;
        }

        private string GetBonusString(IPerk perk, int stackCount)
        {
            if (perk is not StatPerk statPerk) return "";

            float value = statPerk.OperationValue;
            string sign = value >= 0 ? "+" : "";
            string pct = statPerk.ModType == ModifierType.Multiply ? "%" : "";
            float displayedValue = statPerk.ModType == ModifierType.Multiply
                ? (value - 1f) * 100f
                : value;

            if (stackCount > 1)
            {
                float totalValue = statPerk.ModType == ModifierType.Multiply
                    ? (Mathf.Pow(value, stackCount) - 1f) * 100f
                    : value * stackCount;
                return $"{sign}{totalValue:0.#}{pct} ({stackCount}x)";
            }

            return $"{sign}{displayedValue:0.#}{pct}";
        }
    }
}
