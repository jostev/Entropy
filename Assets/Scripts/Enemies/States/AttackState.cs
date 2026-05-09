using UnityEngine;

namespace Entropy.Perks
{
    public class AttackState : EnemyState
    {
        public AttackState(EnemyController enemy) : base(enemy) { }

        public override void Tick()
        {
            if (!enemy.CanSeePlayer)
            {
                enemy.ChangeState(enemy.SearchState);
                return;
            }

            float distance = Vector3.Distance(enemy.transform.position, enemy.PlayerPosition);

            if (distance > enemy.AttackRange)
            {
                enemy.ChangeState(enemy.ChaseState);
                return;
            }

            enemy.FacePoint(enemy.PlayerPosition);
            enemy.PerformAttack();
        }
    }
}
