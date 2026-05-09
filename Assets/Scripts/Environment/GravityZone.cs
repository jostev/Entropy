using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Environment
{
    [RequireComponent(typeof(Collider))]
    public abstract class GravityZone : MonoBehaviour
    {
        [Header("Transition")]
        [SerializeField] protected float transitionDuration = 0.2f;

        private readonly Dictionary<GravityBody, Vector3> _bodyStack = new();
        private readonly Dictionary<GravityBody, int> _zoneCount = new();

        public float TransitionDuration => transitionDuration;

        void OnTriggerEnter(Collider other)
        {
            GravityBody body = other.GetComponent<GravityBody>();
            if (body == null) return;

            if (!_zoneCount.ContainsKey(body))
            {
                _zoneCount[body] = 0;
                _bodyStack[body] = body.GetCurrentGravity();
            }

            _zoneCount[body]++;
            ApplyGravity(body);
        }

        void OnTriggerExit(Collider other)
        {
            GravityBody body = other.GetComponent<GravityBody>();
            if (body == null || !_zoneCount.ContainsKey(body)) return;

            _zoneCount[body]--;

            if (_zoneCount[body] <= 0)
            {
                Vector3 previousGravity = _bodyStack[body];
                body.SetGravity(previousGravity, transitionDuration);
                _zoneCount.Remove(body);
                _bodyStack.Remove(body);
            }
            else
            {
                ApplyGravity(body);
            }
        }

        private void ApplyGravity(GravityBody body)
        {
            Vector3 gravity = GetGravityAt(body.transform.position);
            body.SetGravity(gravity, transitionDuration);
        }

        public abstract Vector3 GetGravityAt(Vector3 position);

        void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }

        protected virtual void DrawGizmos() { }
    }
}
