using UnityEngine;

namespace Entropy.Perks
{
    public class PatrolState : EnemyState
    {
        private Vector3 targetPosition;
        private bool isWaiting;
        private float waitTimer;

        public PatrolState(EnemyController enemy) : base(enemy) { }

        public override void Enter()
        {
            PickNewWanderPoint();
            isWaiting = false;
        }

        public override void Tick()
        {
            if (enemy.CanSeePlayer)
            {
                enemy.ChangeState(enemy.AlertState);
                return;
            }

            if (isWaiting)
            {
                waitTimer -= Time.fixedDeltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    PickNewWanderPoint();
                }
                return;
            }

            Vector3 toTarget = targetPosition - enemy.transform.position;
            float dist = toTarget.magnitude;

            if (dist < 0.5f)
            {
                isWaiting = true;
                waitTimer = Random.Range(enemy.MinWanderWait, enemy.MaxWanderWait);
                return;
            }

            Vector3 desiredDir = Vector3.ProjectOnPlane(toTarget, enemy.GravityDirection).normalized;
            if (!enemy.IsGroundAhead(desiredDir))
            {
                isWaiting = true;
                waitTimer = Random.Range(enemy.MinWanderWait, enemy.MaxWanderWait);
                PickNewWanderPoint();
                return;
            }

            enemy.MoveTowardPoint(targetPosition);
            enemy.FacePoint(targetPosition);
        }

        private void PickNewWanderPoint()
        {
            Vector3 gravityDir = enemy.GravityDirection;
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir = Vector3.ProjectOnPlane(randomDir, gravityDir).normalized;

            if (randomDir.sqrMagnitude < 0.001f)
                randomDir = Vector3.ProjectOnPlane(Vector3.forward, gravityDir).normalized;

            float distance = Random.Range(1f, enemy.WanderRadius);
            targetPosition = enemy.InitialPosition + randomDir * distance;
        }
    }
}
