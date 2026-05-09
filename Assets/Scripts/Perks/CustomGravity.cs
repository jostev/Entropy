using UnityEngine;

namespace Entropy.Perks
{
    /// <summary>
    /// Replaces default Rigidbody gravity with a stat-driven gravity scale.
    /// Required for Moon Legs and future gravity perks.
    /// </summary>
    public class CustomGravity : MonoBehaviour
    {
        private Rigidbody _rb;
        private PlayerStats _stats;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<PlayerStats>();

            if (_rb != null)
                _rb.useGravity = false;
        }

        void FixedUpdate()
        {
            if (_rb == null || _stats == null) return;

            float scale = _stats.GetStat(StatType.GravityScale);
            _rb.AddForce(Physics.gravity * scale, ForceMode.Acceleration);
        }
    }
}
