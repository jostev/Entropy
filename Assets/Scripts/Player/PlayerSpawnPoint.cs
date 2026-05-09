using UnityEngine;

namespace Entropy.Player
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        public static PlayerSpawnPoint Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public Vector3 SpawnPosition => transform.position;
        public Quaternion SpawnRotation => transform.rotation;
    }
}
