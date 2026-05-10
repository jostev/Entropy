using UnityEngine;

namespace Entropy.Perks
{
    public class InertiaThiefPerk : AdvancedPerk
    {
        [SerializeField] private float _velocityInheritPercent = 0.2f;

        private Rigidbody _playerRb;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
                _playerRb = mb.GetComponent<Rigidbody>();

            GameEvents.OnEnemyKilled += OnEnemyKilled;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(EnemyController enemy, Vector3 position, Vector3 velocity)
        {
            if (_playerRb == null) return;

            Vector3 boost = velocity * _velocityInheritPercent;
            _playerRb.AddForce(boost, ForceMode.VelocityChange);
        }
    }
}
