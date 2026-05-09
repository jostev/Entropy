using UnityEngine;

namespace Entropy.Perks
{
    public class ChaseState : EnemyState
    {
        public ChaseState(EnemyController enemy) : base(enemy) { }

        public override void Tick()
        {
            if (enemy.CanSeePlayer)
            {
                enemy.LastKnownPlayerPosition = enemy.PlayerPosition;
            }

            float distance = Vector3.Distance(enemy.transform.position, enemy.PlayerPosition);

            if (distance <= enemy.AttackRange)
            {
                enemy.ChangeState(enemy.AttackState);
                return;
            }

            if (!enemy.CanSeePlayer)
            {
                enemy.ChangeState(enemy.SearchState);
                return;
            }

            enemy.MoveTowardPoint(enemy.PlayerPosition);
            enemy.FacePoint(enemy.PlayerPosition);
        }
    }
}
