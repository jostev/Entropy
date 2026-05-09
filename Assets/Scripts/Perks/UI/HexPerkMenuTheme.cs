using UnityEngine;

namespace Entropy.Perks.UI
{
    [CreateAssetMenu(fileName = "Hex Perk Menu Theme", menuName = "Entropy/Hex Perk Menu Theme")]
    public class HexPerkMenuTheme : ScriptableObject
    {
        [Header("Passive Perk Row")]
        public Sprite rowBackground;
        public Sprite rowFrame;
        public Sprite rowGlow;
        public Sprite rowShadow;

        [Header("Active Slot (Honeycomb)")]
        public Sprite slotBackground;
        public Sprite slotFrame;
        public Sprite slotGlow;
        public Sprite slotShadow;
        public Sprite slotIconPlaceholder;

        [Header("Panel")]
        public Sprite panelBackground;
        public Sprite panelFrame;

        [Header("Header")]
        public Sprite headerBackground;
        public Sprite headerDivider;

        [Header("Colors")]
        public Color commonColor = new(0.55f, 0.55f, 0.6f, 1f);
        public Color uncommonColor = new(0.3f, 0.8f, 0.77f, 1f);
        public Color rareColor = new(0.29f, 0.56f, 0.89f, 1f);
        public Color epicColor = new(0.6f, 0.35f, 0.71f, 1f);
        public Color legendaryColor = new(0.9f, 0.49f, 0.13f, 1f);
        public Color mythicColor = new(0.9f, 0.3f, 0.24f, 1f);

        public Color GetRarityColor(PerkRarity rarity)
        {
            return rarity switch
            {
                PerkRarity.Common => commonColor,
                PerkRarity.Uncommon => uncommonColor,
                PerkRarity.Rare => rareColor,
                PerkRarity.Epic => epicColor,
                PerkRarity.Legendary => legendaryColor,
                PerkRarity.Mythic => mythicColor,
                _ => commonColor,
            };
        }
    }
}
