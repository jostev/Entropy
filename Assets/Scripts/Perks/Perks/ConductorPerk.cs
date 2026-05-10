using UnityEngine;

namespace Entropy.Perks
{
    public class ConductorPerk : AdvancedPerk
    {
        [SerializeField] private float _chainRange = 6f;
        [SerializeField] private int _maxChains = 3;
        [SerializeField] private float _chainDamageMultiplier = 0.6f;
        [SerializeField] private LayerMask _enemyMask;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnBulletHit += OnBulletHit;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnBulletHit -= OnBulletHit;
        }

        private void OnBulletHit(Bullet bullet, Collider target, Vector3 point, Vector3 normal)
        {
            if (!target.CompareTag("Enemy")) return;

            ChainToNearby(point, bullet.damage * _chainDamageMultiplier, _maxChains);
        }

        private void ChainToNearby(Vector3 origin, float damage, int remainingChains)
        {
            if (remainingChains <= 0) return;

            Collider[] nearby = Physics.OverlapSphere(origin, _chainRange, _enemyMask);
            foreach (var col in nearby)
            {
                if (col.CompareTag("Enemy") && Vector3.Distance(origin, col.transform.position) > 0.1f)
                {
                    var health = col.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(damage);
                        ChainToNearby(col.transform.position, damage * _chainDamageMultiplier, remainingChains - 1);
                        return;
                    }
                }
            }
        }
    }
}
