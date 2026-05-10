using System.Collections;
using Entropy.Enemies;
using Entropy.Perks;
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

            var allEnemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            foreach (var enemy in allEnemies)
            {
                if (enemy != null && enemy.GetComponent<EnemySpawnData>() == null)
                    enemy.gameObject.AddComponent<EnemySpawnData>();
            }
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

            var spawnPos = FindSafeSpawnPosition(spawnData.SpawnPosition);

            enemyGO.transform.position = spawnPos;
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

        private Vector3 FindSafeSpawnPosition(Vector3 origin)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return origin;

            float playerRadius = 1.5f;
            var pc = player.GetComponent<CapsuleCollider>();
            if (pc != null) playerRadius = Mathf.Max(pc.radius, playerRadius);

            float minDist = playerRadius + 2f;
            Vector3 toPlayer = origin - player.transform.position;
            float currentDist = toPlayer.magnitude;

            if (currentDist >= minDist)
                return origin;

            Vector3 pushDir = toPlayer.magnitude > 0.01f
                ? toPlayer.normalized
                : Vector3.forward;

            Vector3 safePos = origin + pushDir * (minDist - currentDist);
            safePos += Vector3.up * 0.5f;

            if (Physics.Raycast(safePos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, ~0))
                safePos.y = hit.point.y + 0.1f;

            return safePos;
        }

        public bool ShouldRespawn(Health health)
        {
            if (health == null) return false;
            var spawnData = health.GetComponent<EnemySpawnData>();
            return spawnData != null && spawnData.HasRespawnsRemaining;
        }
    }
}
