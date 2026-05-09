using System;
using UnityEngine;
using Entropy.Perks.UI;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Max speed when walking forward
            public float BackwardSpeed = 4.0f;  // Max speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Max speed when walking sideways
            public float SpeedInAir = 8.0f;     // Max speed while airborne
            public float JumpForce = 30f;

            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
#endif

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero) return;

                if (input.x > 0 || input.x < 0)
                {
                    CurrentTargetSpeed = StrafeSpeed;
                }

                if (input.y < 0)
                {
                    CurrentTargetSpeed = BackwardSpeed;
                }

                if (input.y > 0)
                {
                    // Handled last so forward speed takes priority over strafe speed.
                    CurrentTargetSpeed = ForwardSpeed;
                }
            }
        }

        public bool canrotate;
        public Camera cam;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public Vector3 relativevelocity;

        public DetectObs detectGround;

        public bool Wallrunning;
        public bool MovementLocked;

        [Header("Acceleration")]
        public float groundAcceleration = 35f;
        public float airAcceleration = 10f;
        public float groundDeceleration = 25f;
        public float airDeceleration = 2f;

        [Header("Jump Momentum")]
        public float jumpMomentumBoost = 1.05f;
        public float maxJumpMomentumSpeed = 24f;

        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private bool m_IsGrounded;

        public Vector3 Velocity
        {
            get { return m_RigidBody.linearVelocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        private void Awake()
        {
            canrotate = true;
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();

            mouseLook.Init(transform, cam.transform);
        }

        private void Update()
        {
            if (PerkMenuManager.Instance != null && PerkMenuManager.Instance.IsOpen) return;

            relativevelocity = transform.InverseTransformDirection(m_RigidBody.linearVelocity);

            if (m_IsGrounded && !MovementLocked)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    NormalJump();
                }
            }
        }

        private void LateUpdate()
        {
            if (canrotate)
            {
                RotateView();
            }
            else
            {
                mouseLook.LookOveride(transform, cam.transform);
            }
        }

        private void FixedUpdate()
        {
            if (PerkMenuManager.Instance != null && PerkMenuManager.Instance.IsOpen) return;

            GroundCheck();

            if (Wallrunning || MovementLocked)
            {
                return;
            }

            Vector2 input = GetInput();

            Vector3 inputVector = new Vector3(input.x, 0f, input.y);
            inputVector = Vector3.ClampMagnitude(inputVector, 1f);

            MoveWithAcceleration(inputVector);
        }

        private void MoveWithAcceleration(Vector3 inputVector)
        {
            Vector3 currentVelocity = m_RigidBody.linearVelocity;

            Vector3 flatVelocity = new Vector3(
                currentVelocity.x,
                0f,
                currentVelocity.z
            );

            float targetSpeed;

            if (!m_IsGrounded)
            {
                targetSpeed = movementSettings.SpeedInAir;
            }
            else if (inputVector.z > 0f)
            {
                targetSpeed = movementSettings.ForwardSpeed;
            }
            else if (inputVector.z < 0f)
            {
                targetSpeed = movementSettings.BackwardSpeed;
            }
            else
            {
                targetSpeed = movementSettings.StrafeSpeed;
            }

            Vector3 desiredDirection =
                transform.forward * inputVector.z +
                transform.right * inputVector.x;

            desiredDirection.y = 0f;

            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                desiredDirection.Normalize();
            }

            Vector3 targetVelocity = desiredDirection * targetSpeed;

            bool hasInput = desiredDirection.sqrMagnitude > 0.01f;

            float acceleration = hasInput
                ? m_IsGrounded ? groundAcceleration : airAcceleration
                : m_IsGrounded ? groundDeceleration : airDeceleration;

            Vector3 newFlatVelocity = Vector3.MoveTowards(
                flatVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );

            m_RigidBody.linearVelocity = new Vector3(
                newFlatVelocity.x,
                currentVelocity.y,
                newFlatVelocity.z
            );
        }

        public void NormalJump()
        {
            Vector3 flatVelocity = new Vector3(
                m_RigidBody.linearVelocity.x,
                0f,
                m_RigidBody.linearVelocity.z
            );

            flatVelocity *= jumpMomentumBoost;

            if (flatVelocity.magnitude > maxJumpMomentumSpeed)
            {
                flatVelocity = flatVelocity.normalized * maxJumpMomentumSpeed;
            }

            m_RigidBody.linearVelocity = new Vector3(
                flatVelocity.x,
                0f,
                flatVelocity.z
            );

            m_RigidBody.AddForce(
                new Vector3(0f, movementSettings.JumpForce, 0f),
                ForceMode.Impulse
            );
        }

        public void CamGoBack(float speed)
        {
            mouseLook.CamGoBack(transform, cam.transform, speed);
        }

        public void CamGoBackAll()
        {
            mouseLook.CamGoBackAll(transform, cam.transform);
        }

        private Vector2 GetInput()
        {
            Vector2 input = new Vector2
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = Input.GetAxisRaw("Vertical")
            };

            movementSettings.UpdateDesiredTargetSpeed(input);

            return input;
        }

        private void RotateView()
        {
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            mouseLook.LookRotation(transform, cam.transform);
        }

        private void GroundCheck()
        {
            if (detectGround.Obstruction)
            {
                m_IsGrounded = true;
            }
            else
            {
                m_IsGrounded = false;
            }
        }
    }
}
