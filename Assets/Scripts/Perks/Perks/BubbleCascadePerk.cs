using UnityEngine;

namespace Entropy.Perks
{
    public class BubbleCascadePerk : AdvancedPerk
    {
        [SerializeField] private float _sphereRadius = 0.8f;
        [SerializeField] private float _explosionForce = 15f;
        [SerializeField] private float _explosionRadius = 3f;
        [SerializeField] private float _chainDelay = 0.1f;
        [SerializeField] private float _bubbleLifetime = 6f;
        [SerializeField] private LayerMask _enemyMask = ~0;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnEnemyKilled += OnEnemyKilled;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(EnemyController enemy, Vector3 position, Vector3 velocity)
        {
            GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bubble.name = "Bubble";
            bubble.transform.position = position;
            bubble.transform.localScale = Vector3.one * _sphereRadius * 2f;

            Destroy(bubble.GetComponent<Collider>());

            Rigidbody rb = bubble.AddComponent<Rigidbody>();
            rb.mass = 2f;
            rb.linearVelocity = velocity;

            Bubble bubbleComp = bubble.AddComponent<Bubble>();
            bubbleComp.Initialize(_explosionForce, _explosionRadius, _chainDelay, _enemyMask);

            Destroy(bubble, _bubbleLifetime);
        }

        public class Bubble : MonoBehaviour
        {
            private float _explosionForce;
            private float _explosionRadius;
            private float _chainDelay;
            private LayerMask _enemyMask;
            private bool _detonated;

            public void Initialize(float force, float radius, float delay, LayerMask mask)
            {
                _explosionForce = force;
                _explosionRadius = radius;
                _chainDelay = delay;
                _enemyMask = mask;
            }

            void OnCollisionEnter(Collision collision)
            {
                if (_detonated) return;
                if (!collision.collider.CompareTag("Enemy")) return;

                _detonated = true;
                Detonate();
            }

            private void Detonate()
            {
                Collider[] nearby = Physics.OverlapSphere(transform.position, _explosionRadius, _enemyMask);
                foreach (var col in nearby)
                {
                    if (!col.CompareTag("Enemy")) continue;

                    Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                    if (rb == null) continue;

                    Vector3 dir = (col.transform.position - transform.position).normalized;
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    float falloff = 1f - Mathf.Clamp01(distance / _explosionRadius);

                    rb.AddForce(dir * _explosionForce * falloff, ForceMode.Impulse);

                    Bubble otherBubble = col.GetComponent<Bubble>();
                    if (otherBubble != null && !otherBubble._detonated)
                    {
                        StartCoroutine(ChainDetonate(otherBubble));
                    }
                }

                Destroy(gameObject, 0.1f);
            }

            private System.Collections.IEnumerator ChainDetonate(Bubble target)
            {
                yield return new WaitForSeconds(_chainDelay);
                if (target != null && !target._detonated)
                    target.Detonate();
            }
        }
    }
}
