using UnityEngine;

namespace Entropy.Perks
{
    public class RailgunBodyPerk : AdvancedPerk
    {
        [SerializeField] private float _damageMultiplier = 1.0f;
        [SerializeField] private float _minSpeed = 4f;
        [SerializeField] private float _hitRadius = 0.6f;
        [SerializeField] private float _damageCooldown = 0.2f;
        [SerializeField] private LayerMask _enemyMask = ~0;

        private Rigidbody _playerRb;
        private float _lastDamageTime = -999f;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
                _playerRb = mb.GetComponent<Rigidbody>();
        }

        public override void OnRemove(IModdableStats target)
        {
            _playerRb = null;
        }

        void FixedUpdate()
        {
            if (_playerRb == null) return;

            float speed = _playerRb.linearVelocity.magnitude;
            if (speed < _minSpeed) return;
            if (Time.time < _lastDamageTime + _damageCooldown) return;

            Collider[] nearby = Physics.OverlapSphere(transform.position, _hitRadius, _enemyMask);
            foreach (var col in nearby)
            {
                if (!col.CompareTag("Enemy")) continue;

                Health health = col.GetComponent<Health>();
                if (health == null) health = col.GetComponentInParent<Health>();
                if (health == null) continue;

                health.TakeDamage(speed * _damageMultiplier);
                _lastDamageTime = Time.time;
                return;
            }
        }
    }
}
