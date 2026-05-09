using UnityEngine;

namespace Entropy.Environment
{
    public class SphereGravityZone : GravityZone
    {
        public enum DirectionMode
        {
            TowardCenter,
            AwayFromCenter
        }

        public enum MagnitudeMode
        {
            Constant,
            DistanceFalloff
        }

        [Header("Sphere Gravity")]
        [SerializeField] private float gravityMagnitude = 9.81f;
        [SerializeField] private DirectionMode directionMode = DirectionMode.TowardCenter;
        [SerializeField] private MagnitudeMode magnitudeMode = MagnitudeMode.Constant;
        [SerializeField] private float minDistance = 0.1f;

        public override Vector3 GetGravityAt(Vector3 position)
        {
            Vector3 toCenter = transform.position - position;
            float distance = Mathf.Max(toCenter.magnitude, minDistance);
            Vector3 direction = toCenter.normalized;

            if (directionMode == DirectionMode.AwayFromCenter)
            {
                direction = -direction;
            }

            float magnitude = gravityMagnitude;

            if (magnitudeMode == MagnitudeMode.DistanceFalloff)
            {
                magnitude = gravityMagnitude / (distance * distance);
            }

            return direction * magnitude;
        }

        protected override void DrawGizmos()
        {
            Gizmos.color = Color.cyan;

            Vector3 direction = directionMode == DirectionMode.TowardCenter
                ? -Vector3.up
                : Vector3.up;

            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
