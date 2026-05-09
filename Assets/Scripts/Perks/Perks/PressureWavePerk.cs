using UnityEngine;

namespace Entropy.Perks
{
    public class PressureWavePerk : AdvancedPerk
    {
        [SerializeField] private float _shockwaveRadius = 4f;
        [SerializeField] private float _forceMultiplier = 2f;
        [SerializeField] private float _upwardForce = 3f;
        [SerializeField] private LayerMask _enemyMask = ~0;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnPlayerLanded += OnLanded;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnPlayerLanded -= OnLanded;
        }

        private void OnLanded(float impactVelocity)
        {
            float force = impactVelocity * impactVelocity * _forceMultiplier;

            Collider[] nearby = Physics.OverlapSphere(transform.position, _shockwaveRadius, _enemyMask);
            foreach (var col in nearby)
            {
                if (!col.CompareTag("Enemy")) continue;

                var rb = col.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                Vector3 dir = (col.transform.position - transform.position).normalized;
                dir.y = _upwardForce;
                rb.AddForce(dir * force, ForceMode.Impulse);
            }
        }
    }
}
