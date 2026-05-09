using UnityEngine;

namespace Entropy.Perks
{
    public class SonicThresholdPerk : AdvancedPerk
    {
        [SerializeField] private float _speedThreshold = 16f;
        [SerializeField] private float _coneLength = 4f;
        [SerializeField] private float _coneAngle = 60f;
        [SerializeField] private float _staggerForce = 10f;
        [SerializeField] private float _cooldown = 2f;
        [SerializeField] private LayerMask _enemyMask = ~0;

        private Rigidbody _playerRb;
        private Transform _playerTransform;
        private float _lastTriggerTime = -999f;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
            {
                _playerRb = mb.GetComponent<Rigidbody>();
                _playerTransform = mb.transform;
            }
        }

        public override void OnRemove(IModdableStats target)
        {
            _playerRb = null;
            _playerTransform = null;
        }

        void FixedUpdate()
        {
            if (_playerRb == null || _playerTransform == null) return;
            if (Time.time < _lastTriggerTime + _cooldown) return;

            float flatSpeed = new Vector3(_playerRb.linearVelocity.x, 0f, _playerRb.linearVelocity.z).magnitude;
            if (flatSpeed < _speedThreshold) return;

            _lastTriggerTime = Time.time;

            Vector3 origin = _playerTransform.position;
            Vector3 forward = _playerTransform.forward;

            Collider[] nearby = Physics.OverlapSphere(origin, _coneLength, _enemyMask);
            foreach (var col in nearby)
            {
                if (!col.CompareTag("Enemy")) continue;

                Vector3 toEnemy = col.transform.position - origin;
                if (toEnemy.magnitude > _coneLength) continue;

                float angle = Vector3.Angle(forward, toEnemy);
                if (angle > _coneAngle * 0.5f) continue;

                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                Vector3 dir = toEnemy.normalized;
                dir.y = 0.3f;
                rb.AddForce(dir * _staggerForce, ForceMode.Impulse);
            }
        }
    }
}
