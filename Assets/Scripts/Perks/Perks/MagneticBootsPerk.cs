using UnityEngine;

namespace Entropy.Perks
{
    public class MagneticBootsPerk : AdvancedPerk
    {
        [SerializeField] private float _groundedDrag = 10f;

        private Rigidbody _playerRb;
        private Collider _playerCollider;
        private float _originalDrag;
        private bool _wasGrounded;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
            {
                _playerRb = mb.GetComponent<Rigidbody>();
                _playerCollider = mb.GetComponent<Collider>();
                if (_playerRb != null)
                    _originalDrag = _playerRb.linearDamping;
            }
        }

        public override void OnRemove(IModdableStats target)
        {
            if (_playerRb != null)
                _playerRb.linearDamping = _originalDrag;
        }

        void FixedUpdate()
        {
            if (_playerRb == null || _playerCollider == null) return;

            bool grounded = Physics.Raycast(
                _playerRb.position + Vector3.up * 0.05f,
                Vector3.down,
                0.15f,
                ~0
            );

            if (grounded && !_wasGrounded)
            {
                _playerRb.linearDamping = _groundedDrag;
                _playerRb.constraints |= RigidbodyConstraints.FreezeRotation;
            }
            else if (!grounded && _wasGrounded)
            {
                _playerRb.linearDamping = _originalDrag;
            }

            _wasGrounded = grounded;
        }
    }
}
