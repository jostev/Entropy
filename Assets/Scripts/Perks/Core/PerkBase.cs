using UnityEngine;

namespace Entropy.Perks
{
    /// <summary>
    /// Base Monobehaviour for all perks. Instantiate as prefabs.
    /// </summary>
    public abstract class PerkBase : MonoBehaviour, IPerk
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _title;
        [SerializeField, TextArea] private string _description;

        [Header("Meta")]
        [SerializeField] private PerkRarity _rarity = PerkRarity.Common;

        public string ID => _id;
        public string Title => _title;
        public string Description => _description;
        public PerkRarity Rarity => _rarity;

        public abstract void OnEquip(IModdableStats target);
        public abstract void OnRemove(IModdableStats target);
    }
}
