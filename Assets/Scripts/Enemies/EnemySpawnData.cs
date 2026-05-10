using UnityEngine;

namespace Entropy.Enemies
{
    public class EnemySpawnData : MonoBehaviour
    {
        [Header("Spawn")]
        public Vector3 SpawnPosition;

        [Header("Respawn")]
        public bool CanRespawn = true;
        public int MaxRespawns = 3;
        public int RespawnCount { get; private set; }

        [Header("Visual")]
        public bool ShowRespawnCounter = true;

        void Awake()
        {
            if (SpawnPosition == Vector3.zero)
                SpawnPosition = transform.position;
        }

        public void IncrementRespawnCount()
        {
            RespawnCount++;
        }

        public bool HasRespawnsRemaining => CanRespawn && RespawnCount < MaxRespawns;
    }
}
