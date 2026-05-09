using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Entropy.Perks
{
    public class DensityStarPerk : AdvancedPerk
    {
        [SerializeField] private float _activeDuration = 5f;
        [SerializeField] private float _pullRadius = 15f;
        [SerializeField] private float _pullForce = 200f;
        [SerializeField] private float _starMass = 500f;
        [SerializeField] private LayerMask _affectedLayers = ~0;

        private bool _isActive;
        private float _remainingDuration;
        private Rigidbody _playerRb;
        private RigidbodyFirstPersonController _fpsController;
        private Health _health;
        private float _originalMass;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnActionTriggered += OnActionTriggered;

            if (target is MonoBehaviour mb)
            {
                _playerRb = mb.GetComponent<Rigidbody>();
                _fpsController = mb.GetComponent<RigidbodyFirstPersonController>();
                _health = mb.GetComponent<Health>();
            }

            if (_playerRb != null)
                _originalMass = _playerRb.mass;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnActionTriggered -= OnActionTriggered;
            if (_isActive)
                Deactivate();
        }

        public override void OnActionTriggered(ActionEvent evt)
        {
            if (evt.EventName != "DensityStarToggle") return;

            if (_isActive)
                Deactivate();
            else
                StartCoroutine(Activate());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                if (_isActive)
                    Deactivate();
                else
                    StartCoroutine(Activate());
            }
        }

        private IEnumerator Activate()
        {
            _isActive = true;
            _remainingDuration = _activeDuration;

            if (_playerRb != null)
            {
                _playerRb.mass = _starMass;
                _playerRb.isKinematic = true;
            }

            if (_fpsController != null)
                _fpsController.MovementLocked = true;

            if (_health != null)
                _health.IsInvulnerable = true;

            while (_remainingDuration > 0f)
            {
                _remainingDuration -= Time.fixedDeltaTime;
                PullNearby();
                yield return new WaitForFixedUpdate();
            }

            Deactivate();
        }

        private void PullNearby()
        {
            if (_playerRb == null) return;

            Vector3 center = _playerRb.position;
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

        private void Deactivate()
        {
            if (!_isActive) return;
            _isActive = false;
            StopAllCoroutines();

            if (_playerRb != null)
            {
                _playerRb.mass = _originalMass;
                _playerRb.isKinematic = false;
            }

            if (_fpsController != null)
                _fpsController.MovementLocked = false;

            if (_health != null)
                _health.IsInvulnerable = false;
        }
    }
}
