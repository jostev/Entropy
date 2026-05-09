using Entropy.Environment;
using UnityEngine;

namespace Entropy.Perks
{
    public class CustomGravity : MonoBehaviour
    {
        private Rigidbody _rb;
        private PlayerStats _stats;
        private GravityBody _gravityBody;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<PlayerStats>();
            _gravityBody = GetComponent<GravityBody>();

            if (_rb != null)
                _rb.useGravity = false;
        }

        void FixedUpdate()
        {
            if (_gravityBody == null || _stats == null) return;

            float scale = _stats.GetStat(StatType.GravityScale);
            _gravityBody.Scale = scale;
        }
    }
}
