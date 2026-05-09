using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public class TemporalAnchorPerk : AdvancedPerk
    {
        [SerializeField] private KeyCode _activateKey = KeyCode.Y;
        [SerializeField] private float _duration = 3f;
        [SerializeField] private float _cooldown = 20f;
        [SerializeField] private int _ghostLayer = 8;

        private bool _isActive;
        private bool _onCooldown;
        private Rigidbody _playerRb;
        private Collider _playerCollider;
        private Transform _playerTransform;
        private int _originalLayer;
        private bool _wasKinematic;

        public bool IsAnchored => _isActive;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnActionTriggered += OnActionTriggered;

            if (target is MonoBehaviour mb)
            {
                _playerRb = mb.GetComponent<Rigidbody>();
                _playerCollider = mb.GetComponent<Collider>();
                _playerTransform = mb.transform;
                _originalLayer = mb.gameObject.layer;
            }
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnActionTriggered -= OnActionTriggered;
            StopAllCoroutines();

            if (_isActive)
                RestoreState();
        }

        void Update()
        {
            if (Input.GetKeyDown(_activateKey))
            {
                TryActivate();
            }
        }

        private void OnActionTriggered(ActionEvent evt)
        {
            TryActivate();
        }

        private void TryActivate()
        {
            if (_isActive) return;
            if (_onCooldown) return;

            StartCoroutine(AnchorRoutine());
        }

        private IEnumerator AnchorRoutine()
        {
            _isActive = true;
            ApplyAnchorState();

            yield return new WaitForSecondsRealtime(_duration);

            RestoreState();
            _isActive = false;

            _onCooldown = true;
            yield return new WaitForSecondsRealtime(_cooldown);
            _onCooldown = false;
        }

        private void ApplyAnchorState()
        {
            if (_playerRb != null)
            {
                _wasKinematic = _playerRb.isKinematic;
                _playerRb.isKinematic = true;
            }

            if (_playerTransform != null)
            {
                _playerTransform.gameObject.layer = _ghostLayer;
            }

            if (_playerCollider != null)
            {
                Physics.IgnoreLayerCollision(_originalLayer, _ghostLayer, true);
            }
        }

        private void RestoreState()
        {
            if (_playerRb != null)
            {
                _playerRb.isKinematic = _wasKinematic;
            }

            if (_playerTransform != null)
            {
                _playerTransform.gameObject.layer = _originalLayer;
            }

            if (_playerCollider != null)
            {
                Physics.IgnoreLayerCollision(_originalLayer, _ghostLayer, false);
            }
        }
    }
}
