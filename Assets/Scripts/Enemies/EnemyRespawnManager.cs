using System.Collections;
using Entropy.Enemies;
using UnityEngine;

namespace Entropy.Enemies
{
    public class EnemyRespawnManager : MonoBehaviour
    {
        public static EnemyRespawnManager Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float _respawnDelay = 5f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            GameEvents.OnEnemyKilled += OnEnemyKilled;
        }

        void OnDestroy()
        {
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(EnemyController enemy, Vector3 position, Vector3 velocity)
        {
            if (enemy == null) return;

            var spawnData = enemy.GetComponent<EnemySpawnData>();
            if (spawnData == null || !spawnData.HasRespawnsRemaining)
                return;

            spawnData.IncrementRespawnCount();
            StartCoroutine(RespawnCoroutine(enemy.gameObject, spawnData));
        }

        private IEnumerator RespawnCoroutine(GameObject enemyGO, EnemySpawnData spawnData)
        {
            enemyGO.SetActive(false);

            yield return new WaitForSeconds(_respawnDelay);

            if (enemyGO == null) yield break;

            enemyGO.transform.position = spawnData.SpawnPosition;
            enemyGO.transform.rotation = Quaternion.identity;

            var rb = enemyGO.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            var health = enemyGO.GetComponent<Health>();
            if (health != null)
                health.Revive();

            var ec = enemyGO.GetComponent<EnemyController>();
            if (ec != null)
                ec.enabled = true;

            var buff = enemyGO.GetComponent<EnemyPerkBuff>();
            if (buff == null)
                buff = enemyGO.AddComponent<EnemyPerkBuff>();
            buff.ApplyRandomBuff();

            enemyGO.SetActive(true);
        }

        public bool ShouldRespawn(Health health)
        {
            if (health == null) return false;
            var spawnData = health.GetComponent<EnemySpawnData>();
            return spawnData != null && spawnData.HasRespawnsRemaining;
        }
    }
}
