using UnityEngine;

namespace Entropy.Perks
{
    public class MassTransferPerk : AdvancedPerk
    {
        [SerializeField] private float _massMultiplier = 1.0f;

        private float _storedMass = 0f;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            PistolShoot.OnBulletFired += OnBulletFired;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
            PistolShoot.OnBulletFired -= OnBulletFired;
            _storedMass = 0f;
        }

        private void OnEnemyKilled(EnemyController enemy, Vector3 position, Vector3 velocity)
        {
            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb == null) return;

            _storedMass += enemyRb.mass * _massMultiplier;
        }

        private void OnBulletFired(Bullet bullet)
        {
            if (_storedMass <= 0f) return;

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
                bulletRb.mass += _storedMass;

            _storedMass = 0f;
        }
    }
}
