using UnityEngine;

namespace Entropy.Perks
{
    public class AlertState : EnemyState
    {
        private float timer;
        private const float ALERT_DURATION = 1f;

        public AlertState(EnemyController enemy) : base(enemy) { }

        public override void Enter()
        {
            timer = 0f;
        }

        public override void Tick()
        {
            enemy.FacePoint(enemy.LastKnownPlayerPosition);

            if (enemy.CanSeePlayer)
            {
                timer += Time.fixedDeltaTime;
                if (timer >= ALERT_DURATION)
                {
                    enemy.ChangeState(enemy.ChaseState);
                }
            }
            else
            {
                enemy.ChangeState(enemy.SearchState);
            }
        }
    }
}
