using UnityEngine;

namespace Entropy.Environment
{
    public class BoxGravityZone : GravityZone
    {
        [Header("Box Gravity")]
        [SerializeField] private Vector3 gravityVector = new Vector3(0f, -9.81f, 0f);

        public override Vector3 GetGravityAt(Vector3 position)
        {
            return gravityVector;
        }

        protected override void DrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, gravityVector.normalized * 2f);
        }
    }
}
