using UnityEngine;

namespace Entropy.Perks
{
    public class InertiaMirrorPerk : AdvancedPerk
    {
        [SerializeField] private float _reflectionMultiplier = 1.5f;
        [SerializeField] private float _reflectionForceMultiplier = 1.5f;

        private Rigidbody _playerRb;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
                _playerRb = mb.GetComponent<Rigidbody>();

            GameEvents.OnPlayerDamaged += OnPlayerDamaged;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
        }

        private void OnPlayerDamaged(float amount, GameObject source)
        {
            if (source == null) return;

            Health sourceHealth = source.GetComponentInParent<Health>();
            if (sourceHealth != null)
            {
                sourceHealth.TakeDamage(amount * _reflectionMultiplier);
            }

            Rigidbody sourceRb = source.GetComponentInParent<Rigidbody>();
            if (sourceRb != null && _playerRb != null)
            {
                Vector3 playerPos = _playerRb.position;
                Vector3 knockbackDir = (source.transform.position - playerPos).normalized;
                sourceRb.AddForce(knockbackDir * _reflectionForceMultiplier * sourceRb.mass, ForceMode.Impulse);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody otherRb = collision.rigidbody;
            if (otherRb == null) return;

            otherRb.AddForce(-collision.relativeVelocity * _reflectionForceMultiplier * otherRb.mass, ForceMode.Impulse);
        }
    }
}
