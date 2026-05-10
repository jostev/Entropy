using UnityEngine;

namespace Entropy.Perks
{
    public class OrbitalDefensePerk : AdvancedPerk
    {
        [SerializeField] private float _orbitRadius = 1.5f;
        [SerializeField] private float _baseOrbitSpeed = 180f;
        [SerializeField] private float _orbitHeight = 1.2f;
        [SerializeField] private float _orbitalMass = 8f;
        [SerializeField] private float _deflectionRadius = 0.4f;

        private Transform _playerTransform;
        private Rigidbody _playerRb;
        private GameObject _orbiter;
        private Rigidbody _orbiterRb;
        private float _orbitAngle;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
            {
                _playerTransform = mb.transform;
                _playerRb = mb.GetComponent<Rigidbody>();
            }

            CreateOrbiter();
        }

        public override void OnRemove(IModdableStats target)
        {
            if (_orbiter != null)
                Destroy(_orbiter);

            _playerTransform = null;
            _playerRb = null;
        }

        void FixedUpdate()
        {
            if (_playerTransform == null || _orbiter == null) return;

            float speedMultiplier = _playerRb != null ? Mathf.Clamp01(_playerRb.linearVelocity.magnitude / 10f) : 1f;
            float currentSpeed = _baseOrbitSpeed * (1f + speedMultiplier);

            _orbitAngle += currentSpeed * Time.fixedDeltaTime;

            float rad = _orbitAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad) * _orbitRadius, _orbitHeight, Mathf.Sin(rad) * _orbitRadius);
            Vector3 targetPos = _playerTransform.position + offset;

            _orbiterRb.MovePosition(targetPos);
            _orbiterRb.MoveRotation(Quaternion.Euler(0f, _orbitAngle, 0f));
        }

        private void CreateOrbiter()
        {
            _orbiter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _orbiter.name = "OrbitalDefense";
            _orbiter.transform.localScale = Vector3.one * _deflectionRadius * 2f;

            Destroy(_orbiter.GetComponent<Collider>());
            SphereCollider col = _orbiter.AddComponent<SphereCollider>();
            col.radius = _deflectionRadius;
            col.isTrigger = false;

            _orbiterRb = _orbiter.AddComponent<Rigidbody>();
            _orbiterRb.mass = _orbitalMass;
            _orbiterRb.useGravity = false;
            _orbiterRb.isKinematic = true;
            _orbiterRb.interpolation = RigidbodyInterpolation.Interpolate;

            Renderer rend = _orbiter.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = new Color(0.3f, 0.3f, 1f, 0.7f);
        }
    }
}
