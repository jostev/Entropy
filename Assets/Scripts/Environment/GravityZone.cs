using UnityEngine;

namespace Entropy.Environment
{
    [RequireComponent(typeof(Collider))]
    public abstract class GravityZone : MonoBehaviour
    {
        [Header("Transition")]
        [SerializeField] protected float transitionDuration = 0.2f;

        [Header("Priority")]
        [SerializeField] private int priority = 0;

        public int Priority => priority;
        public float TransitionDuration => transitionDuration;

        void OnTriggerEnter(Collider other)
        {
            GravityBody body = other.GetComponent<GravityBody>();
            if (body == null) return;
            body.RegisterZone(this);
        }

        void OnTriggerExit(Collider other)
        {
            GravityBody body = other.GetComponent<GravityBody>();
            if (body == null) return;
            body.UnregisterZone(this);
        }

        public abstract Vector3 GetGravityAt(Vector3 position);

        void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }

        protected virtual void DrawGizmos() { }
    }
}
