using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Perks
{
    public class ZeroGFieldPerk : AdvancedPerk
    {
        [SerializeField] private float _fieldRadius = 5f;
        [SerializeField] private float _duration = 3f;
        [SerializeField] private LayerMask _affectedLayers = ~0;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnEnemyKilled += OnEnemyKilled;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(EnemyController enemy, Vector3 position, Vector3 velocity)
        {
            StartCoroutine(ApplyZeroG(position));
        }

        private IEnumerator ApplyZeroG(Vector3 center)
        {
            List<Rigidbody> freeBodies = new List<Rigidbody>();
            List<EnemyController> enemies = new List<EnemyController>();
            List<Vector3> originalGravities = new List<Vector3>();

            Collider[] nearby = Physics.OverlapSphere(center, _fieldRadius, _affectedLayers);

            foreach (var col in nearby)
            {
                if (col.CompareTag("Player")) continue;

                EnemyController ec = col.GetComponentInParent<EnemyController>();
                if (ec != null)
                {
                    enemies.Add(ec);
                    originalGravities.Add(ec.GetCurrentGravity());
                    ec.SetGravity(Vector3.zero, 0f);
                    continue;
                }

                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb == null) continue;

                rb.useGravity = false;
                freeBodies.Add(rb);
            }

            yield return new WaitForSeconds(_duration);

            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null)
                    enemies[i].SetGravity(originalGravities[i], 0f);
            }

            foreach (var rb in freeBodies)
            {
                if (rb != null)
                    rb.useGravity = true;
            }
        }
    }
}
