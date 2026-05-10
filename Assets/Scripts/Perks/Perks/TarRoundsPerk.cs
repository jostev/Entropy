using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public class TarRoundsPerk : AdvancedPerk
    {
        [SerializeField] private float _dragMultiplier = 3f;
        [SerializeField] private float _duration = 5f;

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
            var rb = target.GetComponentInParent<Rigidbody>();
            if (rb == null) return;
            if (target.CompareTag("Player")) return;

            StartCoroutine(ApplyTar(rb));
        }

        private IEnumerator ApplyTar(Rigidbody rb)
        {
            float originalDrag = rb.linearDamping;
            rb.linearDamping = originalDrag * _dragMultiplier;

            yield return new WaitForSeconds(_duration);

            if (rb != null)
                rb.linearDamping = originalDrag;
        }
    }
}
