using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRange = 20f;
        [SerializeField] private float fieldOfView = 120f;
        [SerializeField] private float proximityDetectionRange = 3f;
        [SerializeField] private float detectionInterval = 0.2f;
        [SerializeField] private LayerMask obstructionMask = ~0;

        [Header("Movement")]
        [SerializeField] private float groundCheckDistance = 1.2f;
        [SerializeField] private float obstacleCheckDistance = 1f;
        [SerializeField] private float steerForce = 6f;
        [SerializeField] private float maxSlopeAngle = 60f;

        [Header("Gravity")]
        [SerializeField] private Vector3 gravityVector = new Vector3(0f, -9.81f, 0f);

        [Header("Patrol")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float minWanderWait = 1f;
        [SerializeField] private float maxWanderWait = 3f;

        [Header("Combat")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float rotationSpeed = 8f;

        [Header("Perks")]
        [SerializeField] private PerkRarity perkLevel = PerkRarity.Common;
        [SerializeField] private EnemyPerkProfile perkProfile;

        internal float CurrentMoveSpeed = 5f;
        internal float CurrentAttackDamage = 10f;
        internal float CurrentAttackCooldown = 1f;

        public float AttackRange => attackRange;
        public float WanderRadius => wanderRadius;
        public float MinWanderWait => minWanderWait;
        public float MaxWanderWait => maxWanderWait;
        public Vector3 GravityDirection => -_currentGravity.normalized;
        public Vector3 InitialPosition { get; private set; }
        public Quaternion InitialRotation { get; private set; }
        public bool CanSeePlayer { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; set; }
        public Vector3 PlayerPosition => _player != null ? _player.position : Vector3.zero;

        public EnemyState PatrolState { get; private set; }
        public EnemyState AlertState { get; private set; }
        public EnemyState ChaseState { get; private set; }
        public EnemyState AttackState { get; private set; }
        public EnemyState SearchState { get; private set; }

        private EnemyState _currentState;
        private Rigidbody _rb;
        private Health _health;
        private EnemyStats _stats;
        private Transform _player;
        private float _attackTimer;
        private bool _isGrounded;
        private Vector3 _groundNormal;

        private Vector3 _currentGravity;
        private Vector3 _targetGravity;
        private Vector3 _gravityTransitionStart;
        private float _gravityTransitionRemaining;
        private float _gravityTransitionTotal;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _stats = GetComponent<EnemyStats>();

            _rb.useGravity = false;
            _rb.freezeRotation = true;

            _groundNormal = -gravityVector.normalized;
            _currentGravity = gravityVector;
            _targetGravity = gravityVector;
            _gravityTransitionRemaining = 0f;
            InitialPosition = transform.position;
            InitialRotation = transform.rotation;
        }

        void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;

            AssignPerk();

            PatrolState = new PatrolState(this);
            AlertState = new AlertState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            SearchState = new SearchState(this);

            ChangeState(PatrolState);
            StartCoroutine(SearchForTarget());
        }

        void FixedUpdate()
        {
            UpdateGravityTransition();
            ApplyCustomGravity();
            CheckGrounded();

            if (_health == null || !_health.enabled)
                return;

            _currentState?.Tick();
        }

        public void ChangeState(EnemyState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        private void ApplyCustomGravity()
        {
            _rb.AddForce(_currentGravity, ForceMode.Acceleration);
        }

        private void UpdateGravityTransition()
        {
            if (_gravityTransitionRemaining > 0f)
            {
                _gravityTransitionRemaining -= Time.fixedDeltaTime;
                float t = Mathf.Clamp01(1f - (_gravityTransitionRemaining / _gravityTransitionTotal));
                _currentGravity = Vector3.Lerp(_gravityTransitionStart, _targetGravity, t);
                _groundNormal = -_currentGravity.normalized;

                if (_gravityTransitionRemaining <= 0f)
                {
                    _currentGravity = _targetGravity;
                    _groundNormal = -_currentGravity.normalized;
                }
            }
        }

        private void CheckGrounded()
        {
            Vector3 down = -transform.up;
            _isGrounded = Physics.Raycast(transform.position, down, out RaycastHit hit, groundCheckDistance, ~0);

            if (_isGrounded)
            {
                _groundNormal = hit.normal;
                float slopeAngle = Vector3.Angle(-_currentGravity.normalized, hit.normal);
                if (slopeAngle > maxSlopeAngle)
                {
                    _isGrounded = false;
                    _groundNormal = -_currentGravity.normalized;
                }
            }
            else
            {
                _groundNormal = -_currentGravity.normalized;
            }
        }

        private IEnumerator SearchForTarget()
        {
            var wait = new WaitForSeconds(detectionInterval);

            while (_health != null && _health.enabled)
            {
                yield return wait;

                CanSeePlayer = false;

                if (_player == null) continue;

                Vector3 toPlayer = _player.position - transform.position;
                float distance = toPlayer.magnitude;

                if (distance > detectionRange) continue;

                bool useProximity = distance <= proximityDetectionRange;

                if (!useProximity)
                {
                    Vector3 flatDir = Vector3.ProjectOnPlane(toPlayer, GravityDirection).normalized;
                    Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, GravityDirection).normalized;

                    if (flatForward.sqrMagnitude < 0.001f) continue;

                    float angle = Vector3.Angle(flatForward, flatDir);
                    if (angle > fieldOfView * 0.5f) continue;
                }

                Vector3 eyePos = transform.position + transform.up * 0.5f;
                Vector3 rayDir = toPlayer.normalized;
                float rayDist = Mathf.Max(distance, 0.1f);

                if (Physics.Raycast(eyePos, rayDir, out RaycastHit hit, rayDist, obstructionMask))
                {
                    if (hit.transform != _player) continue;
                }

                CanSeePlayer = true;
                LastKnownPlayerPosition = _player.position;
            }
        }

        public void MoveTowardPoint(Vector3 targetPoint)
        {
            Vector3 toTarget = targetPoint - transform.position;
            Vector3 desiredDir = Vector3.ProjectOnPlane(toTarget, _groundNormal).normalized;
            if (desiredDir.sqrMagnitude < 0.001f) return;

            Vector3 obstacleAvoidance = CalculateObstacleAvoidance();
            Vector3 moveDir = (desiredDir + obstacleAvoidance).normalized;

            Vector3 targetVelocity = moveDir * CurrentMoveSpeed;
            Vector3 velocityDiff = targetVelocity - _rb.linearVelocity;
            velocityDiff = Vector3.ProjectOnPlane(velocityDiff, transform.up);

            _rb.AddForce(velocityDiff * steerForce, ForceMode.Acceleration);
        }

        public void FacePoint(Vector3 point)
        {
            Vector3 toPoint = point - transform.position;
            Vector3 flatDir = Vector3.ProjectOnPlane(toPoint, GravityDirection).normalized;
            if (flatDir.sqrMagnitude < 0.001f) return;

            Quaternion targetRot = Quaternion.LookRotation(flatDir, GravityDirection);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        private Vector3 CalculateObstacleAvoidance()
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 left = -right;

            bool blockedForward = Physics.Raycast(transform.position, forward, obstacleCheckDistance, obstructionMask);
            bool blockedRight = Physics.Raycast(transform.position, right, obstacleCheckDistance * 0.7f, obstructionMask);
            bool blockedLeft = Physics.Raycast(transform.position, left, obstacleCheckDistance * 0.7f, obstructionMask);

            Vector3 avoidance = Vector3.zero;

            if (blockedForward)
            {
                if (!blockedLeft)
                    avoidance += left * 2f;
                else if (!blockedRight)
                    avoidance += right * 2f;
                else
                    avoidance += -forward * 1f;
            }

            return avoidance;
        }

        public void PerformAttack()
        {
            _attackTimer -= Time.fixedDeltaTime;
            if (_attackTimer > 0f) return;

            _attackTimer = CurrentAttackCooldown;

            Health playerHealth = _player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(CurrentAttackDamage);
            }
        }

        private void AssignPerk()
        {
            if (perkProfile == null || _stats == null) return;

            PerkBase perkPrefab = perkProfile.GetRandomPerk(perkLevel);
            if (perkPrefab == null) return;

            PerkBase instance = Instantiate(perkPrefab, transform);
            instance.OnEquip(_stats);
        }

        public void SetGravity(Vector3 newGravity, float transitionDuration = 0f)
        {
            _targetGravity = newGravity;
            _gravityTransitionStart = _currentGravity;
            _gravityTransitionTotal = Mathf.Max(transitionDuration, 0.001f);
            _gravityTransitionRemaining = _gravityTransitionTotal;
        }

        public Vector3 GetCurrentGravity()
        {
            return _currentGravity;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, proximityDetectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Vector3 gravityDir = Application.isPlaying ? _currentGravity : gravityVector;
            Vector3 down = Application.isPlaying ? -transform.up : -gravityDir.normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, down * groundCheckDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, -gravityDir.normalized * 2f);

            if (Application.isPlaying && _currentState != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position + transform.up * 0.5f, LastKnownPlayerPosition);
            }
        }
    }
}
