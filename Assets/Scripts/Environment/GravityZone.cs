using System.Collections.Generic;
using Entropy.Perks;
using UnityEngine;

namespace Entropy.Environment
{
    [RequireComponent(typeof(Collider))]
    public abstract class GravityZone : MonoBehaviour
    {
        [Header("Transition")]
        [SerializeField] protected float transitionDuration = 0.2f;

        private readonly Dictionary<EnemyController, Vector3> _enemyStack = new();
        private readonly Dictionary<EnemyController, int> _zoneCount = new();

        public float TransitionDuration => transitionDuration;

        void OnTriggerEnter(Collider other)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy == null) return;

            if (!_zoneCount.ContainsKey(enemy))
            {
                _zoneCount[enemy] = 0;
                _enemyStack[enemy] = enemy.GetCurrentGravity();
            }

            _zoneCount[enemy]++;
            ApplyGravity(enemy);
        }

        void OnTriggerExit(Collider other)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy == null || !_zoneCount.ContainsKey(enemy)) return;

            _zoneCount[enemy]--;

            if (_zoneCount[enemy] <= 0)
            {
                Vector3 previousGravity = _enemyStack[enemy];
                enemy.SetGravity(previousGravity, transitionDuration);
                _zoneCount.Remove(enemy);
                _enemyStack.Remove(enemy);
            }
            else
            {
                ApplyGravity(enemy);
            }
        }

        private void ApplyGravity(EnemyController enemy)
        {
            Vector3 gravity = GetGravityAt(enemy.transform.position);
            enemy.SetGravity(gravity, transitionDuration);
        }

        public abstract Vector3 GetGravityAt(Vector3 position);

        void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }

        protected virtual void DrawGizmos() { }
    }
}
