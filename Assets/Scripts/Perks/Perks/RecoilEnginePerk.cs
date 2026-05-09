using UnityEngine;

namespace Entropy.Perks
{
    public class RecoilEnginePerk : AdvancedPerk
    {
        [SerializeField] private float _recoilForce = 12f;

        private Rigidbody _playerRb;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
                _playerRb = mb.GetComponent<Rigidbody>();

            PistolShoot.OnBulletFired += OnBulletFired;
        }

        public override void OnRemove(IModdableStats target)
        {
            PistolShoot.OnBulletFired -= OnBulletFired;
            _playerRb = null;
        }

        private void OnBulletFired(Bullet bullet)
        {
            if (_playerRb == null) return;

            Vector3 recoilDir = -bullet.transform.forward;
            _playerRb.AddForce(recoilDir * _recoilForce, ForceMode.Impulse);
        }
    }
}
