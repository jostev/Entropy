using UnityEngine;

namespace Entropy.Perks.UI
{
    [CreateAssetMenu(fileName = "NewPerkDisplay", menuName = "Entropy/Perk Display Data")]
    public class PerkDisplayData : ScriptableObject
    {
        [Tooltip("Must match the IPerk.ID of the perk prefab.")]
        public string PerkID;

        [Tooltip("Optional override for the display name. Falls back to IPerk.Title.")]
        public string DisplayName;

        [Tooltip("Optional override for the description. Falls back to IPerk.Description.")]
        public string DisplayDescription;

        [Tooltip("Icon shown in all menu panels.")]
        public Sprite Icon;

        [Tooltip("Tint color for borders, backgrounds, and hex fills.")]
        public Color AccentColor = Color.white;

        [Tooltip("Classification for deck-building rules and panel placement.")]
        public PerkCategory Category = PerkCategory.Passive;

        public string GetDisplayName(IPerk perk)
        {
            return string.IsNullOrEmpty(DisplayName) ? perk?.Title : DisplayName;
        }

        public string GetDisplayDescription(IPerk perk)
        {
            return string.IsNullOrEmpty(DisplayDescription) ? perk?.Description : DisplayDescription;
        }
    }
}
