using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Perks
{
    public class VoidAnchorPerk : AdvancedPerk
    {
        [SerializeField] private float _minImpactVelocity = 8f;
        [SerializeField] private float _pinRadius = 8f;
        [SerializeField] private float _pinDuration = 4f;
        [SerializeField] private float _pinForce = 50f;
        [SerializeField] private LayerMask _enemyMask = ~0;

        private bool _isActive;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnPlayerLanded += OnPlayerLanded;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnPlayerLanded -= OnPlayerLanded;
        }

        private void OnPlayerLanded(float impactVelocity)
        {
            if (impactVelocity < _minImpactVelocity) return;
            if (_isActive) return;

            StartCoroutine(PinEnemies());
        }

        private IEnumerator PinEnemies()
        {
            _isActive = true;
            float timer = 0f;

            List<Rigidbody> affected = new List<Rigidbody>();
            Collider[] nearby = Physics.OverlapSphere(transform.position, _pinRadius, _enemyMask);

            foreach (var col in nearby)
            {
                if (!col.CompareTag("Enemy")) continue;

                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                affected.Add(rb);
            }

            while (timer < _pinDuration)
            {
                timer += Time.fixedDeltaTime;

                foreach (var rb in affected)
                {
                    if (rb == null) continue;
                    rb.AddForce(Vector3.down * _pinForce, ForceMode.Acceleration);
                }

                yield return new WaitForFixedUpdate();
            }

            _isActive = false;
        }
    }
}
