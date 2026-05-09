using UnityEngine;

namespace Entropy.Environment
{
    public class ForceZone : GravityZone
    {
        [Header("Force")]
        [SerializeField] private Vector3 forceVector = new Vector3(0f, -9.81f, 0f);

        public override bool AffectsRotation => false;

        public override Vector3 GetGravityAt(Vector3 position)
        {
            return forceVector;
        }

        protected override void DrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, forceVector.normalized * 2f);
        }
    }
}
