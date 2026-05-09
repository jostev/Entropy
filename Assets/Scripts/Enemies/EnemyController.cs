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
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float rotationSpeed = 8f;
        [SerializeField] private LayerMask obstructionMask = ~0;

        [Header("Movement")]
        [SerializeField] private float groundCheckDistance = 1.2f;
        [SerializeField] private float obstacleCheckDistance = 1f;
        [SerializeField] private float steerForce = 6f;
        [SerializeField] private float maxSlopeAngle = 60f;

        [Header("Gravity")]
        [SerializeField] private Vector3 gravityVector = new Vector3(0f, -9.81f, 0f);

        [Header("Perks")]
        [SerializeField] private PerkRarity perkLevel = PerkRarity.Common;
        [SerializeField] private EnemyPerkProfile perkProfile;

        internal float CurrentMoveSpeed = 5f;
        internal float CurrentAttackDamage = 10f;
        internal float CurrentAttackCooldown = 1f;

        private Rigidbody _rb;
        private Health _health;
        private EnemyStats _stats;
        private Transform _player;
        private float _attackTimer;
        private bool _isGrounded;
        private Vector3 _groundNormal;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _stats = GetComponent<EnemyStats>();

            _rb.useGravity = false;
            _rb.freezeRotation = true;

            _groundNormal = -gravityVector.normalized;
        }

        void Start()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _player = playerObj.transform;

            AssignPerk();
        }

        void FixedUpdate()
        {
            ApplyCustomGravity();

            if (_player == null || _health == null || !_health.enabled)
                return;

            Vector3 toPlayer = _player.position - transform.position;
            float distance = toPlayer.magnitude;

            AlignRotation(toPlayer, distance);
            CheckGrounded();

            if (distance <= detectionRange && HasLineOfSight(toPlayer))
            {
                if (distance > attackRange)
                {
                    MoveTowardTarget(toPlayer);
                }
                else
                {
                    TryAttack();
                }
            }
        }

        private void ApplyCustomGravity()
        {
            _rb.AddForce(gravityVector, ForceMode.Acceleration);
        }

        private void AlignRotation(Vector3 toPlayer, float distance)
        {
            Vector3 desiredUp = -gravityVector.normalized;
            Quaternion targetRot;

            if (distance <= detectionRange)
            {
                Vector3 desiredForward = Vector3.ProjectOnPlane(toPlayer, desiredUp).normalized;
                if (desiredForward.sqrMagnitude > 0.001f)
                {
                    targetRot = Quaternion.LookRotation(desiredForward, desiredUp);
                }
                else
                {
                    targetRot = Quaternion.LookRotation(transform.forward, desiredUp);
                }
            }
            else
            {
                Vector3 currentForward = Vector3.ProjectOnPlane(transform.forward, desiredUp).normalized;
                if (currentForward.sqrMagnitude > 0.001f)
                {
                    targetRot = Quaternion.LookRotation(currentForward, desiredUp);
                }
                else
                {
                    targetRot = Quaternion.LookRotation(transform.right, desiredUp);
                }
            }

            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        private void CheckGrounded()
        {
            Vector3 down = -transform.up;
            _isGrounded = Physics.Raycast(transform.position, down, out RaycastHit hit, groundCheckDistance, ~0);

            if (_isGrounded)
            {
                _groundNormal = hit.normal;
                float slopeAngle = Vector3.Angle(-gravityVector.normalized, hit.normal);
                if (slopeAngle > maxSlopeAngle)
                {
                    _isGrounded = false;
                    _groundNormal = -gravityVector.normalized;
                }
            }
            else
            {
                _groundNormal = -gravityVector.normalized;
            }
        }

        private bool HasLineOfSight(Vector3 toPlayer)
        {
            Vector3 eyePos = transform.position + transform.up * 0.5f;
            if (Physics.Raycast(eyePos, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude, obstructionMask))
            {
                return hit.transform == _player;
            }
            return true;
        }

        private void MoveTowardTarget(Vector3 toPlayer)
        {
            Vector3 desiredDir = Vector3.ProjectOnPlane(toPlayer, _groundNormal).normalized;
            if (desiredDir.sqrMagnitude < 0.001f) return;

            Vector3 obstacleAvoidance = CalculateObstacleAvoidance();
            Vector3 moveDir = (desiredDir + obstacleAvoidance).normalized;

            Vector3 targetVelocity = moveDir * CurrentMoveSpeed;
            Vector3 velocityDiff = targetVelocity - _rb.linearVelocity;

            velocityDiff = Vector3.ProjectOnPlane(velocityDiff, transform.up);

            _rb.AddForce(velocityDiff * steerForce, ForceMode.Acceleration);
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

        private void TryAttack()
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

        public void SetGravity(Vector3 newGravity)
        {
            gravityVector = newGravity;
            _groundNormal = -gravityVector.normalized;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Vector3 down = Application.isPlaying ? -transform.up : -gravityVector.normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, down * groundCheckDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, -gravityVector.normalized * 2f);
        }
    }
}
