using UnityEngine;

namespace Entropy.Perks
{
    public class FrictionBurnPerk : AdvancedPerk
    {
        [SerializeField] private float _damageMultiplier = 0.5f;
        [SerializeField] private float _minSpeed = 3f;
        [SerializeField] private LayerMask _enemyMask = ~0;

        private PlayerController _playerController;
        private Rigidbody _playerRb;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
            {
                _playerController = mb.GetComponent<PlayerController>();
                _playerRb = mb.GetComponent<Rigidbody>();
            }
        }

        public override void OnRemove(IModdableStats target)
        {
            _playerController = null;
            _playerRb = null;
        }

        void FixedUpdate()
        {
            if (_playerController == null || !_playerController.IsSliding) return;
            if (_playerRb == null) return;

            float speed = _playerRb.linearVelocity.magnitude;
            if (speed < _minSpeed) return;

            float radius = 0.6f;
            Collider[] nearby = Physics.OverlapSphere(transform.position, radius, _enemyMask);
            foreach (var col in nearby)
            {
                if (!col.CompareTag("Enemy")) continue;

                var health = col.GetComponent<Health>();
                if (health == null) health = col.GetComponentInParent<Health>();
                if (health != null)
                {
                    health.TakeDamage(speed * _damageMultiplier * Time.fixedDeltaTime);
                }
            }
        }
    }
}
