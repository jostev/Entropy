using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public class EventHorizonPerk : AdvancedPerk
    {
        [SerializeField] private float _horizonRadius = 5f;
        [SerializeField] private float _baseGravityMultiplier = 50f;
        [SerializeField] private LayerMask _affectedLayers = ~0;

        private Coroutine _horizonRoutine;
        private Transform _playerTransform;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
                _playerTransform = mb.transform;

            _horizonRoutine = StartCoroutine(RunEventHorizon());
        }

        public override void OnRemove(IModdableStats target)
        {
            if (_horizonRoutine != null)
            {
                StopCoroutine(_horizonRoutine);
                _horizonRoutine = null;
            }
        }

        private IEnumerator RunEventHorizon()
        {
            while (true)
            {
                ApplyGravityGradient();
                yield return new WaitForFixedUpdate();
            }
        }

        private void ApplyGravityGradient()
        {
            if (_playerTransform == null) return;

            Vector3 center = _playerTransform.position;
            Collider[] nearby = Physics.OverlapSphere(center, _horizonRadius, _affectedLayers);
            foreach (var col in nearby)
            {
                if (col.CompareTag("Player")) continue;

                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                Vector3 dir = (center - col.transform.position).normalized;
                float distance = Vector3.Distance(center, col.transform.position);
                float falloff = 1f - Mathf.Clamp01(distance / _horizonRadius);

                float force = rb.mass * _baseGravityMultiplier * falloff;
                rb.AddForce(dir * force, ForceMode.Force);
            }
        }
    }
}
