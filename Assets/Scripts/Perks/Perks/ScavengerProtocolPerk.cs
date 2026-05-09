using UnityEngine;

namespace Entropy.Perks
{
    public class ScavengerProtocolPerk : AdvancedPerk
    {
        [SerializeField] private float _dropChanceBonus = 0.35f;

        public float DropChanceBonus => _dropChanceBonus;

        public override void OnEquip(IModdableStats target) { }
        public override void OnRemove(IModdableStats target) { }
    }
}
