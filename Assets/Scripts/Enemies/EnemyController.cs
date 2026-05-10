using System.Collections;
using Entropy.Environment;
using Entropy.Enemies;
using UnityEngine;

namespace Entropy.Perks
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(GravityBody))]
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

        [Header("Edge Detection")]
        [SerializeField] private float edgeCheckDistance = 1f;
        [SerializeField] private float edgeCheckDepth = 1.2f;

        [Header("Gravity")]
        [SerializeField] private Vector3 gravityVector = new Vector3(0f, -9.81f, 0f);
        [Header("Patrol")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float minWanderWait = 1f;
        [SerializeField] private float maxWanderWait = 3f;

        [Header("Combat")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float rotationSpeed = 8f;
        [SerializeField] private bool useRangedAttack = false;
        [SerializeField] private float tooCloseRange = 5f;

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
        public Vector3 GravityDirection => -_gravityBody.CurrentGravity.normalized;
        public Vector3 InitialPosition { get; private set; }
        public Quaternion InitialRotation { get; private set; }
        public bool CanSeePlayer { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; set; }
        public Vector3 PlayerPosition => _player != null ? _player.position : Vector3.zero;
        public Transform PlayerTransform => _player;

        public EnemyState PatrolState { get; private set; }
        public EnemyState AlertState { get; private set; }
        public EnemyState ChaseState { get; private set; }
        public EnemyState AttackState { get; private set; }
        public EnemyState SearchState { get; private set; }

        private EnemyState _currentState;
        private Rigidbody _rb;
        private Health _health;
        private EnemyStats _stats;
        private GravityBody _gravityBody;
        private Transform _player;
        private float _attackTimer;
        private bool _isGrounded;
        private Vector3 _groundNormal;
        private Collider _collider;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _stats = GetComponent<EnemyStats>();
            _gravityBody = GetComponent<GravityBody>();
            _collider = GetComponent<Collider>();

            _rb.useGravity = false;
            _rb.freezeRotation = true;

            if (GetComponent<EnemySpawnData>() == null)
                gameObject.AddComponent<EnemySpawnData>();

            if (EnemyRespawnManager.Instance == null)
            {
                var go = new GameObject("EnemyRespawnManager");
                go.AddComponent<EnemyRespawnManager>();
            }

            if (_gravityBody != null)
                _groundNormal = -_gravityBody.CurrentGravity.normalized;
            else
                _groundNormal = -gravityVector.normalized;

            _currentGravity = _gravityBody != null ? _gravityBody.CurrentGravity : gravityVector;
            _targetGravity = _currentGravity;
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
            AttackState = useRangedAttack
                ? new RangedAttackState(this, tooCloseRange)
                : new AttackState(this);
            SearchState = new SearchState(this);

            ChangeState(PatrolState);
            StartCoroutine(SearchForTarget());
        }

        void FixedUpdate()
        {
            CheckGrounded();

            if (_health == null || !_health.enabled)
                return;

            _currentState?.Tick();

            // Edge safety net: zero horizontal velocity if momentum would carry us off a ledge
            if (_isGrounded)
            {
                Vector3 horizontalVel = Vector3.ProjectOnPlane(_rb.linearVelocity, _groundNormal);
                if (horizontalVel.sqrMagnitude > 0.01f)
                {
                    Vector3 velDir = horizontalVel.normalized;
                    if (!IsGroundAhead(velDir))
                    {
                        _rb.linearVelocity -= horizontalVel;
                    }
                }
            }
        }

        public void ChangeState(EnemyState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        private void CheckGrounded()
        {
            Vector3 down = -transform.up;
            _isGrounded = Physics.Raycast(transform.position, down, out RaycastHit hit, groundCheckDistance, ~0);

            if (_isGrounded)
            {
                _groundNormal = hit.normal;
                float slopeAngle = Vector3.Angle(-_gravityBody.CurrentGravity.normalized, hit.normal);
                if (slopeAngle > maxSlopeAngle)
                {
                    _isGrounded = false;
                    _groundNormal = -_gravityBody.CurrentGravity.normalized;
                }
            }
            else
            {
                _groundNormal = -_gravityBody.CurrentGravity.normalized;
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

        public bool IsGroundAhead(Vector3 direction)
        {
            Vector3 flatDir = Vector3.ProjectOnPlane(direction, _groundNormal).normalized;
            if (flatDir.sqrMagnitude < 0.001f) return true;

            Vector3 down = -transform.up;

            // Predict ahead based on horizontal speed so fast enemies don't overshoot
            float horizontalSpeed = Vector3.ProjectOnPlane(_rb.linearVelocity, _groundNormal).magnitude;
            float lookAhead = Mathf.Max(edgeCheckDistance, horizontalSpeed * Time.fixedDeltaTime * 2f);

            // Use collider size to check width of the enemy, not just center
            float checkWidth = 0.25f;
            if (_collider != null)
            {
                checkWidth = Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.z) * 0.5f;
            }

            Vector3 perp = Vector3.Cross(flatDir, down).normalized;
            if (perp.sqrMagnitude < 0.001f)
                perp = Vector3.ProjectOnPlane(transform.right, _groundNormal).normalized;

            Vector3 origin = transform.position;

            // Center probe — must have ground
            Vector3 centerOrigin = origin + flatDir * lookAhead;
            bool centerGround = Physics.Raycast(centerOrigin, down, edgeCheckDepth, ~0);
            if (!centerGround) return false;

            // Side probes — at least one side must have ground to prevent tipping off narrow ledges
            Vector3 leftOrigin = centerOrigin + perp * checkWidth;
            Vector3 rightOrigin = centerOrigin - perp * checkWidth;
            bool leftGround = Physics.Raycast(leftOrigin, down, edgeCheckDepth, ~0);
            bool rightGround = Physics.Raycast(rightOrigin, down, edgeCheckDepth, ~0);

            return leftGround || rightGround;
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
                playerHealth.lastDamageSource = gameObject;
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

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, proximityDetectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Vector3 gravityDir = _gravityBody != null ? _gravityBody.CurrentGravity : new Vector3(0f, -9.81f, 0f);
            Vector3 down = Application.isPlaying ? -transform.up : -gravityDir.normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, down * groundCheckDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, -gravityDir.normalized * 2f);

            Vector3 gDown = Application.isPlaying ? -transform.up : -gravityDir.normalized;
            Vector3 gFwd = Vector3.ProjectOnPlane(transform.forward, gDown).normalized;
            Vector3 gPerp = Vector3.Cross(gFwd, gDown).normalized;
            if (gPerp.sqrMagnitude < 0.001f)
                gPerp = Vector3.ProjectOnPlane(transform.right, gDown).normalized;

            float w = 0.25f;
            if (_collider != null) w = Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.z) * 0.5f;

            Vector3 c = transform.position + gFwd * edgeCheckDistance;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(c, c + gDown * edgeCheckDepth);
            Gizmos.DrawLine(c + gPerp * w, c + gPerp * w + gDown * edgeCheckDepth);
            Gizmos.DrawLine(c - gPerp * w, c - gPerp * w + gDown * edgeCheckDepth);

            if (Application.isPlaying && _currentState != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(transform.position + transform.up * 0.5f, LastKnownPlayerPosition);
            }
        }
    }
}
