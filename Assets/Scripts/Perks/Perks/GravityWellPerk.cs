using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public class GravityWellPerk : AdvancedPerk
    {
        [SerializeField] private float _pullRadius = 6f;
        [SerializeField] private float _pullForce = 25f;
        [SerializeField] private float _duration = 2.5f;
        [SerializeField] private float _explosionForce = 12f;
        [SerializeField] private float _explosionRadius = 8f;
        [SerializeField] private LayerMask _affectedLayers = ~0;

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
            StartCoroutine(RunGravityWell(point));
        }

        private IEnumerator RunGravityWell(Vector3 center)
        {
            float timer = 0f;
            while (timer < _duration)
            {
                timer += Time.fixedDeltaTime;
                PullNearby(center);
                yield return new WaitForFixedUpdate();
            }

            ExplodeOutward(center);
        }

        private void PullNearby(Vector3 center)
        {
            Collider[] nearby = Physics.OverlapSphere(center, _pullRadius, _affectedLayers);
            foreach (var col in nearby)
            {
                if (col.CompareTag("Player")) continue;

                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                Vector3 dir = (center - col.transform.position).normalized;
                float distance = Vector3.Distance(center, col.transform.position);
                float falloff = 1f - Mathf.Clamp01(distance / _pullRadius);

                rb.AddForce(dir * _pullForce * falloff, ForceMode.Acceleration);
            }
        }

        private void ExplodeOutward(Vector3 center)
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

                rb.AddForce(dir * _explosionForce * falloff, ForceMode.Impulse);
            }
        }
    }
}
