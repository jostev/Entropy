using UnityEngine;

namespace Entropy.Player
{
    public class FallDeathDetector : MonoBehaviour
    {
        [SerializeField] private float _deathY = -50f;
        [SerializeField] private float _checkInterval = 0.5f;

        private Health _health;
        private float _timer;

        void Start()
        {
            _health = GetComponent<Health>();
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _checkInterval) return;
            _timer = 0f;

            if (transform.position.y < _deathY)
            {
                if (_health != null && !_health.IsInvulnerable)
                {
                    _health.TakeDamage(_health.maxHealth * 2f);
                }
            }
        }
    }
}
