using UnityEngine;

namespace Entropy.Perks
{
    public class SearchState : EnemyState
    {
        public SearchState(EnemyController enemy) : base(enemy) { }

        public override void Tick()
        {
            if (enemy.CanSeePlayer)
            {
                enemy.ChangeState(enemy.AlertState);
                return;
            }

            Vector3 toTarget = enemy.LastKnownPlayerPosition - enemy.transform.position;
            float dist = toTarget.magnitude;

            if (dist < 0.5f)
            {
                enemy.ChangeState(enemy.PatrolState);
                return;
            }

            Vector3 desiredDir = Vector3.ProjectOnPlane(toTarget, enemy.GravityDirection).normalized;
            if (enemy.IsGroundAhead(desiredDir))
            {
                enemy.MoveTowardPoint(enemy.LastKnownPlayerPosition);
            }
            enemy.FacePoint(enemy.LastKnownPlayerPosition);
        }
    }
}
