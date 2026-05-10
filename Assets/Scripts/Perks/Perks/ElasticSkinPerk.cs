using UnityEngine;

namespace Entropy.Perks
{
    public class ElasticSkinPerk : AdvancedPerk
    {
        [SerializeField] private float _damageReductionMultiplier = 0.5f;

        public float DamageReductionMultiplier => _damageReductionMultiplier;

        public override void OnEquip(IModdableStats target) { }
        public override void OnRemove(IModdableStats target) { }
    }
}
