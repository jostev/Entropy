using UnityEngine;

namespace Entropy.Environment
{
    public class PlatformGravityZone : GravityZone
    {
        [Header("Platform Gravity")]
        [SerializeField] private float gravityMagnitude = 9.81f;
        [Tooltip("Invert direction - gravity pulls away from surface instead of toward it")]
        [SerializeField] private bool invertDirection = false;

        public override Vector3 GetGravityAt(Vector3 position)
        {
            Vector3 direction = invertDirection ? transform.up : -transform.up;
            return direction * gravityMagnitude;
        }

        protected override void DrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 direction = invertDirection ? transform.up : -transform.up;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }
    }
}
