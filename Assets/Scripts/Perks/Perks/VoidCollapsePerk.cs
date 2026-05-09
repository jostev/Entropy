using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Perks
{
    public class VoidCollapsePerk : AdvancedPerk
    {
        [SerializeField] private float _collapseRadius = 12f;
        [SerializeField] private float _collapseDuration = 2f;
        [SerializeField] private float _maxPullForce = 60f;
        [SerializeField] private float _explosionRadius = 12f;
        [SerializeField] private float _explosionForceMultiplier = 8f;
        [SerializeField] private LayerMask _affectedLayers = ~0;

        private bool _isCollapsing;

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
            if (_isCollapsing) return;
            StartCoroutine(RunVoidCollapse(position));
        }

        private IEnumerator RunVoidCollapse(Vector3 center)
        {
            _isCollapsing = true;
            float timer = 0f;

            Dictionary<Rigidbody, float> storedMasses = new Dictionary<Rigidbody, float>();
            Dictionary<Rigidbody, Vector3> storedVelocities = new Dictionary<Rigidbody, Vector3>();

            while (timer < _collapseDuration)
            {
                timer += Time.fixedDeltaTime;
                float t = timer / _collapseDuration;
                float currentForce = Mathf.Lerp(0f, _maxPullForce, t * t);

                Collider[] nearby = Physics.OverlapSphere(center, _collapseRadius, _affectedLayers);
                foreach (var col in nearby)
                {
                    if (col.CompareTag("Player")) continue;

                    Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                    if (rb == null) continue;

                    if (!storedMasses.ContainsKey(rb))
                    {
                        storedMasses[rb] = rb.mass;
                        storedVelocities[rb] = rb.linearVelocity;
                    }

                    Vector3 dir = (center - col.transform.position).normalized;
                    float distance = Vector3.Distance(center, col.transform.position);
                    float falloff = 1f - Mathf.Clamp01(distance / _collapseRadius);

                    rb.AddForce(dir * currentForce * falloff, ForceMode.Acceleration);
                }

                yield return new WaitForFixedUpdate();
            }

            ExplodeOutward(center, storedMasses, storedVelocities);
            _isCollapsing = false;
        }

        private void ExplodeOutward(Vector3 center, Dictionary<Rigidbody, float> storedMasses, Dictionary<Rigidbody, Vector3> storedVelocities)
        {
            Collider[] nearby = Physics.OverlapSphere(center, _explosionRadius, _affectedLayers);
            foreach (var col in nearby)
            {
                if (col.CompareTag("Player")) continue;

                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                Vector3 dir = (col.transform.position - center).normalized;
                float distance = Vector3.Distance(center, col.transform.position);
                float falloff = 1f - Mathf.Clamp01(distance / _explosionRadius);

                float mass = storedMasses.ContainsKey(rb) ? storedMasses[rb] : rb.mass;
                Vector3 incomingVel = storedVelocities.ContainsKey(rb) ? storedVelocities[rb] : Vector3.zero;
                float compression = incomingVel.magnitude + mass;

                rb.AddForce(dir * _explosionForceMultiplier * compression * falloff, ForceMode.Impulse);
            }
        }
    }
}
