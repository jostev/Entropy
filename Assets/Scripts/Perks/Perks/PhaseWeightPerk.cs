using UnityEngine;

namespace Entropy.Perks
{
    public class PhaseWeightPerk : AdvancedPerk
    {
        [SerializeField] private KeyCode _toggleKey = KeyCode.F;
        [SerializeField] private float _phaseDrag = 0.05f;
        [SerializeField] private int _ghostLayer = 8;

        private Rigidbody _playerRb;
        private Collider _playerCollider;
        private Transform _playerTransform;
        private int _originalLayer;
        private float _originalDrag;
        private bool _isPhased;
        private bool _wasTrigger;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
            {
                _playerRb = mb.GetComponent<Rigidbody>();
                _playerCollider = mb.GetComponent<Collider>();
                _playerTransform = mb.transform;
                _originalLayer = mb.gameObject.layer;

                if (_playerRb != null)
                    _originalDrag = _playerRb.linearDamping;
            }
        }

        public override void OnRemove(IModdableStats target)
        {
            if (_isPhased)
                SetPhase(false);

            _playerRb = null;
            _playerCollider = null;
            _playerTransform = null;
        }

        void Update()
        {
            if (_playerTransform == null) return;

            if (Input.GetKeyDown(_toggleKey))
            {
                SetPhase(!_isPhased);
            }
        }

        private void SetPhase(bool active)
        {
            _isPhased = active;

            if (_playerRb != null)
            {
                _playerRb.linearDamping = active ? _phaseDrag : _originalDrag;
                _playerRb.useGravity = !active;
            }

            if (_playerCollider != null)
            {
                if (active)
                {
                    _wasTrigger = _playerCollider.isTrigger;
                    _playerCollider.isTrigger = true;
                }
                else
                {
                    _playerCollider.isTrigger = _wasTrigger;
                }
            }

            if (_playerTransform != null)
            {
                _playerTransform.gameObject.layer = active ? _ghostLayer : _originalLayer;
            }

            if (active)
            {
                Physics.IgnoreLayerCollision(_originalLayer, _ghostLayer, true);
            }
            else
            {
                Physics.IgnoreLayerCollision(_originalLayer, _ghostLayer, false);
            }
        }
    }
}
