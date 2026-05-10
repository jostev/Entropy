using UnityEngine;

namespace Entropy.Perks
{
    public class UnstoppableRoundPerk : AdvancedPerk
    {
        [SerializeField] private float _superMass = 999999f;
        [SerializeField] private int _roundInterval = 1;

        private int _shotCounter;

        public override void OnEquip(IModdableStats target)
        {
            PistolShoot.OnBulletFired += OnBulletFired;
        }

        public override void OnRemove(IModdableStats target)
        {
            PistolShoot.OnBulletFired -= OnBulletFired;
        }

        private void OnBulletFired(Bullet bullet)
        {
            _shotCounter++;
            if (_shotCounter < _roundInterval) return;

            _shotCounter = 0;

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.mass = _superMass;

            Collider col = bullet.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            UnstoppableBullet ub = bullet.gameObject.AddComponent<UnstoppableBullet>();
            ub.Initialize();
        }

        public class UnstoppableBullet : MonoBehaviour
        {
            private Vector3 _lastPosition;

            public void Initialize()
            {
                _lastPosition = transform.position;
            }

            void FixedUpdate()
            {
                Vector3 movement = transform.position - _lastPosition;
                if (movement.sqrMagnitude > 0.0001f)
                {
                    if (Physics.Raycast(_lastPosition, movement.normalized, out RaycastHit hit, movement.magnitude))
                    {
                        Health health = hit.collider.GetComponent<Health>();
                        if (health == null) health = hit.collider.GetComponentInParent<Health>();
                        if (health != null)
                        {
                            health.TakeDamage(9999f);
                        }

                        Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();
                        if (rb != null)
                        {
                            Vector3 dir = (rb.position - hit.point).normalized;
                            rb.AddForce(dir * 50f, ForceMode.Impulse);
                        }
                    }
                }
                _lastPosition = transform.position;
            }
        }
    }
}
