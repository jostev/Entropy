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
                weapon.SetTarget(enemy.PlayerTransform);

                if (!weapon.IsTargetInRange())
                {
                    enemy.ChangeState(enemy.ChaseState);
                    return;
                }

                weapon.Fire();

                if (distance < tooCloseRange)
                {
                    RetreatFromPlayer();
                }
                else if (!weapon.IsTargetInOptimalRange())
                {
                    Vector3 toPlayer = enemy.PlayerPosition - enemy.transform.position;
                    Vector3 desiredDir = Vector3.ProjectOnPlane(toPlayer, enemy.GravityDirection).normalized;
                    if (enemy.IsGroundAhead(desiredDir))
                    {
                        enemy.MoveTowardPoint(enemy.PlayerPosition);
                    }
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

            if (!enemy.IsGroundAhead(retreatDir))
            {
                return;
            }

            Vector3 retreatPoint = enemy.transform.position + retreatDir * 3f;
            float originalSpeed = enemy.CurrentMoveSpeed;
            enemy.CurrentMoveSpeed *= retreatSpeedMultiplier;
            enemy.MoveTowardPoint(retreatPoint);
            enemy.CurrentMoveSpeed = originalSpeed;
        }
    }
}
