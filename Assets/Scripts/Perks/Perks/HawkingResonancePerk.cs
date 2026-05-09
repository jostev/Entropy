using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public class HawkingResonancePerk : AdvancedPerk
    {
        [SerializeField] private float _radius = 10f;
        [SerializeField] private float _massMultiplier = 2.0f;
        [SerializeField] private float _tickInterval = 1f;
        [SerializeField] private LayerMask _enemyMask = ~0;

        private Rigidbody _playerRb;
        private Coroutine _damageRoutine;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
                _playerRb = mb.GetComponent<Rigidbody>();

            _damageRoutine = StartCoroutine(DamageLoop());
        }

        public override void OnRemove(IModdableStats target)
        {
            if (_damageRoutine != null)
                StopCoroutine(_damageRoutine);
        }

        private IEnumerator DamageLoop()
        {
            var wait = new WaitForSeconds(_tickInterval);

            while (true)
            {
                yield return wait;

                if (_playerRb == null) continue;

                float damagePerSecond = _playerRb.mass * _massMultiplier;
                Vector3 center = _playerRb.position;

                Collider[] nearby = Physics.OverlapSphere(center, _radius, _enemyMask);
                foreach (var col in nearby)
                {
                    if (!col.CompareTag("Enemy")) continue;

                    Health health = col.GetComponentInParent<Health>();
                    if (health == null) continue;

                    health.TakeDamage(damagePerSecond);
                }
            }
        }
    }
}
