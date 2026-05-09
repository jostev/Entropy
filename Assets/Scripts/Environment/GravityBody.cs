using UnityEngine;

namespace Entropy.Environment
{
    [RequireComponent(typeof(Rigidbody))]
    public class GravityBody : MonoBehaviour
    {
        [SerializeField] private Vector3 defaultGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] private float scale = 1f;
        [SerializeField] private bool autoAlignRotation = false;
        [SerializeField] private float rotationSpeed = 8f;

        public Vector3 CurrentGravity { get; private set; }
        public float Scale
        {
            get => scale;
            set => scale = value;
        }

        private Vector3 _baseGravity;
        private Vector3 _targetGravity;
        private Vector3 _transitionStart;
        private float _transitionRemaining;
        private float _transitionTotal;

        void Awake()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.useGravity = false;

            _baseGravity = defaultGravity;
            _targetGravity = defaultGravity;
            _transitionStart = defaultGravity;
            _transitionRemaining = 0f;
            _transitionTotal = 0f;
            CurrentGravity = defaultGravity;
        }

        void FixedUpdate()
        {
            UpdateTransition();
            ApplyGravityForce();

            if (autoAlignRotation)
            {
                AlignRotation();
            }
        }

        public void SetGravity(Vector3 newGravity, float transitionDuration = 0f)
        {
            _targetGravity = newGravity;
            _transitionStart = _baseGravity;
            _transitionTotal = Mathf.Max(transitionDuration, 0.001f);
            _transitionRemaining = _transitionTotal;
        }

        public Vector3 GetCurrentGravity()
        {
            return CurrentGravity;
        }

        public Vector3 GetAntiGravityDirection()
        {
            return -CurrentGravity.normalized;
        }

        private void UpdateTransition()
        {
            if (_transitionRemaining > 0f)
            {
                _transitionRemaining -= Time.fixedDeltaTime;
                float t = Mathf.Clamp01(1f - (_transitionRemaining / _transitionTotal));
                _baseGravity = Vector3.Lerp(_transitionStart, _targetGravity, t);

                if (_transitionRemaining <= 0f)
                {
                    _baseGravity = _targetGravity;
                }
            }

            CurrentGravity = _baseGravity * scale;
        }

        private void ApplyGravityForce()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(CurrentGravity, ForceMode.Acceleration);
            }
        }

        private void AlignRotation()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) return;

            Vector3 desiredUp = -CurrentGravity.normalized;
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, desiredUp) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, -CurrentGravity.normalized * 2f);
        }
    }
}
