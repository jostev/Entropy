using UnityEngine;

namespace Entropy.Perks
{
    public static class GameEvents
    {
        public static event System.Action<ActionEvent> OnActionTriggered;
        public static void Trigger(ActionEvent evt) => OnActionTriggered?.Invoke(evt);

        public static event System.Action<Bullet, Collider, Vector3, Vector3> OnBulletHit;
        public static void BulletHit(Bullet bullet, Collider target, Vector3 point, Vector3 normal)
            => OnBulletHit?.Invoke(bullet, target, point, normal);

        public static event System.Action<EnemyController, Vector3, Vector3> OnEnemyKilled;
        public static void EnemyKilled(EnemyController enemy, Vector3 position, Vector3 velocity)
            => OnEnemyKilled?.Invoke(enemy, position, velocity);

        public static event System.Action<float> OnPlayerLanded;
        public static void PlayerLanded(float impactVelocity)
            => OnPlayerLanded?.Invoke(impactVelocity);

        public static event System.Action<float, GameObject> OnPlayerDamaged;
        public static void PlayerDamaged(float amount, GameObject source)
            => OnPlayerDamaged?.Invoke(amount, source);
    }
}
