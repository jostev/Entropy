using UnityEngine;

namespace Entropy.Pickups
{
    public class AmmoPickup : MonoBehaviour
    {
        [SerializeField] private int ammoAmount = 6;
        [SerializeField] private float magnetizeRange = 3f;
        [SerializeField] private float magnetizeSpeed = 8f;
        [SerializeField] private float lifetime = 20f;

        private Transform _player;

        void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _player = playerObj.transform;
            Destroy(gameObject, lifetime);
        }

        void Update()
        {
            if (_player == null) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (dist < magnetizeRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _player.position,
                    magnetizeSpeed * Time.deltaTime
                );
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            var pistol = other.GetComponentInChildren<PistolShoot>();
            if (pistol != null)
            {
                pistol.AddAmmo(ammoAmount);
                Destroy(gameObject);
            }
        }
    }
}
