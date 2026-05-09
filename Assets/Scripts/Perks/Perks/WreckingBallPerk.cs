using UnityEngine;

namespace Entropy.Perks
{
    public class WreckingBallPerk : AdvancedPerk
    {
        [SerializeField] private float _maxGrappleDistance = 25f;
        [SerializeField] private float _springStrength = 80f;
        [SerializeField] private float _springDamper = 10f;
        [SerializeField] private float _maxSwingSpeed = 30f;
        [SerializeField] private LayerMask _grappleLayers = ~0;
        [SerializeField] private KeyCode _grappleKey = KeyCode.Mouse1;

        private Rigidbody _playerRb;
        private Transform _playerTransform;
        private SpringJoint _springJoint;
        private LineRenderer _lineRenderer;
        private Vector3 _grapplePoint;
        private bool _isGrappling;

        public override void OnEquip(IModdableStats target)
        {
            if (target is MonoBehaviour mb)
            {
                _playerRb = mb.GetComponent<Rigidbody>();
                _playerTransform = mb.transform;
            }
        }

        public override void OnRemove(IModdableStats target)
        {
            StopGrapple();
            _playerRb = null;
            _playerTransform = null;
        }

        void Update()
        {
            if (_playerTransform == null) return;

            if (Input.GetKeyDown(_grappleKey))
                StartGrapple();
            else if (Input.GetKeyUp(_grappleKey))
                StopGrapple();
        }

        void LateUpdate()
        {
            if (_lineRenderer != null && _isGrappling)
            {
                _lineRenderer.SetPosition(0, _playerTransform.position);
                _lineRenderer.SetPosition(1, _grapplePoint);
            }
        }

        void FixedUpdate()
        {
            if (_isGrappling && _playerRb != null)
            {
                Vector3 toAnchor = _grapplePoint - _playerRb.position;
                float distance = toAnchor.magnitude;
                float springForce = Mathf.Max(0f, distance - _springJoint.minDistance) * _springStrength;
                Vector3 springDir = toAnchor.normalized;

                Vector3 tangentVelocity = _playerRb.linearVelocity - Vector3.Project(_playerRb.linearVelocity, springDir);
                Vector3 dampingForce = -tangentVelocity * _springDamper;

                _playerRb.AddForce(springDir * springForce + dampingForce, ForceMode.Acceleration);

                if (_playerRb.linearVelocity.magnitude > _maxSwingSpeed)
                {
                    _playerRb.linearVelocity = _playerRb.linearVelocity.normalized * _maxSwingSpeed;
                }
            }
        }

        private void StartGrapple()
        {
            if (_playerTransform == null) return;

            Ray ray = new Ray(_playerTransform.position + Vector3.up * 0.5f, _playerTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxGrappleDistance, _grappleLayers))
            {
                _grapplePoint = hit.point;
                _isGrappling = true;

                if (_springJoint == null)
                    _springJoint = _playerTransform.gameObject.AddComponent<SpringJoint>();

                _springJoint.autoConfigureConnectedAnchor = false;
                _springJoint.connectedAnchor = _grapplePoint;
                _springJoint.spring = _springStrength;
                _springJoint.damper = _springDamper;
                _springJoint.minDistance = 0f;
                _springJoint.maxDistance = Vector3.Distance(_playerTransform.position, _grapplePoint) * 1.2f;

                if (_lineRenderer == null)
                {
                    GameObject lrObj = new GameObject("GrappleLine");
                    lrObj.transform.SetParent(_playerTransform);
                    _lineRenderer = lrObj.AddComponent<LineRenderer>();
                    _lineRenderer.startWidth = 0.05f;
                    _lineRenderer.endWidth = 0.05f;
                    _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    _lineRenderer.startColor = Color.cyan;
                    _lineRenderer.endColor = Color.cyan;
                    _lineRenderer.positionCount = 2;
                }
                _lineRenderer.enabled = true;
            }
        }

        private void StopGrapple()
        {
            _isGrappling = false;
            if (_springJoint != null)
                Destroy(_springJoint);
            if (_lineRenderer != null)
                _lineRenderer.enabled = false;
        }
    }
}
