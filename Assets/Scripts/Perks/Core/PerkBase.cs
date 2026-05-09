using UnityEngine;

namespace Entropy.Perks
{
    public abstract class PerkBase : MonoBehaviour, IPerk
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _title;
        [SerializeField, TextArea] private string _description;

        [Header("Meta")]
        [SerializeField] private PerkRarity _rarity = PerkRarity.Common;
        [SerializeField] private PerkCategory _category = PerkCategory.Passive;
        [SerializeField] private ExclusivityGroup _exclusivityGroup = ExclusivityGroup.None;

        public string ID => _id;
        public string Title => _title;
        public string Description => _description;
        public PerkRarity Rarity => _rarity;
        public PerkCategory Category => _category;
        public ExclusivityGroup ExclusivityGroup => _exclusivityGroup;

        public abstract void OnEquip(IModdableStats target);
        public abstract void OnRemove(IModdableStats target);
    }
}
