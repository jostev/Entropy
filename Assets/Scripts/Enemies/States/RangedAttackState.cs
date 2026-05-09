using UnityEngine;
using Entropy.Enemies;

namespace Entropy.Perks
{
    public class RangedAttackState : EnemyState
    {
        private readonly float tooCloseRange;
        private readonly float retreatSpeedMultiplier;

        public RangedAttackState(EnemyController enemy, float tooCloseRange = 5f, float retreatSpeedMultiplier = 0.7f)
            : base(enemy)
        {
            this.tooCloseRange = tooCloseRange;
            this.retreatSpeedMultiplier = retreatSpeedMultiplier;
        }

        public override void Tick()
        {
            if (!enemy.CanSeePlayer)
            {
                enemy.ChangeState(enemy.SearchState);
                return;
            }

            float distance = Vector3.Distance(enemy.transform.position, enemy.PlayerPosition);

            enemy.FacePoint(enemy.PlayerPosition);

            EnemyWeapon weapon = enemy.GetComponent<EnemyWeapon>();

            if (weapon != null)
            {
                if (!weapon.IsTargetInRange())
                {
                    enemy.ChangeState(enemy.ChaseState);
                    return;
                }

                weapon.SetTarget(enemy.PlayerTransform);
                weapon.Fire();

                if (distance < tooCloseRange)
                {
                    RetreatFromPlayer();
                }
                else if (!weapon.IsTargetInOptimalRange())
                {
                    enemy.MoveTowardPoint(enemy.PlayerPosition);
                }
            }
            else
            {
                if (distance > enemy.AttackRange)
                {
                    enemy.ChangeState(enemy.ChaseState);
                    return;
                }

                enemy.PerformAttack();
            }
        }

        private void RetreatFromPlayer()
        {
            Vector3 awayFromPlayer = enemy.transform.position - enemy.PlayerPosition;
            Vector3 retreatDir = Vector3.ProjectOnPlane(awayFromPlayer, enemy.GravityDirection).normalized;

            if (retreatDir.sqrMagnitude < 0.001f)
            {
                retreatDir = enemy.transform.right;
            }

            Vector3 retreatPoint = enemy.transform.position + retreatDir * 3f;
            float originalSpeed = enemy.CurrentMoveSpeed;
            enemy.CurrentMoveSpeed *= retreatSpeedMultiplier;
            enemy.MoveTowardPoint(retreatPoint);
            enemy.CurrentMoveSpeed = originalSpeed;
        }
    }
}
