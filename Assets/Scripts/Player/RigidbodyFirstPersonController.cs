using System;
using Entropy.Environment;
using Entropy.Perks.UI;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (Rigidbody))]
    [RequireComponent(typeof (CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float SpeedInAir = 8.0f;   // Speed when onair
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
					//strafe
					CurrentTargetSpeed = StrafeSpeed;
				}
				if (input.y < 0)
				{
					//backwards
					CurrentTargetSpeed = BackwardSpeed;
				}
				if (input.y > 0)
				{
					//forwards
					//handled last as if strafing and moving forward at the same time forwards speed should take precedence
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


        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private GravityBody m_GravityBody;
        private float m_YRotation;
        private bool  m_IsGrounded;


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
            m_GravityBody = GetComponent<GravityBody>();
            mouseLook.Init(transform, cam.transform, m_GravityBody);
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

        public void CamGoBack(float speed)
        {
            mouseLook.CamGoBack(transform, cam.transform, speed);
        }

        public void CamGoBackAll()
        {
            mouseLook.CamGoBackAll(transform, cam.transform);
        }
        private void FixedUpdate()
        {
            if (PerkMenuManager.Instance != null && PerkMenuManager.Instance.IsOpen) return;

            GroundCheck();
            Vector2 input = GetInput();

            float h = input.x;
            float v = input.y;
            Vector3 inputVector = new Vector3(h, 0, v);
            inputVector = Vector3.ClampMagnitude(inputVector, 1);

            //grounded
            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && m_IsGrounded && !Wallrunning && !MovementLocked)
            {
                if (Input.GetAxisRaw("Vertical") > 0.3f)
                {
                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * movementSettings.ForwardSpeed * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Vertical") < -0.3f)
                {
                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * -movementSettings.BackwardSpeed * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Horizontal") > 0.5f)
                {
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * movementSettings.StrafeSpeed * Mathf.Abs(inputVector.x), 0, 0);
                }
                if (Input.GetAxisRaw("Horizontal") < -0.5f)
                {
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * -movementSettings.StrafeSpeed * Mathf.Abs(inputVector.x), 0, 0);
                }

            }
            //inair
            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && !m_IsGrounded  && !Wallrunning && !MovementLocked)
            {
                if (Input.GetAxisRaw("Vertical") > 0.3f)
                {
                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * movementSettings.SpeedInAir * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Vertical") < -0.3f)
                {
                    m_RigidBody.AddRelativeForce(0, 0, Time.deltaTime * 1000f * -movementSettings.SpeedInAir * Mathf.Abs(inputVector.z));
                }
                if (Input.GetAxisRaw("Horizontal") > 0.5f)
                {
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * movementSettings.SpeedInAir * Mathf.Abs(inputVector.x), 0, 0);
                }
                if (Input.GetAxisRaw("Horizontal") < -0.5f)
                {
                    m_RigidBody.AddRelativeForce(Time.deltaTime * 1000f * -movementSettings.SpeedInAir * Mathf.Abs(inputVector.x), 0, 0);
                }

            }
        }

        public float jumpMomentumBoost = 1.05f;
        public float maxJumpMomentumSpeed = 24f;

        public void NormalJump()
        {
            Vector3 antiGravityDir = m_GravityBody != null
                ? m_GravityBody.GetAntiGravityDirection()
                : Vector3.up;

            Vector3 verticalVelocity = Vector3.Project(m_RigidBody.linearVelocity, antiGravityDir);
            Vector3 flatVelocity = m_RigidBody.linearVelocity - verticalVelocity;

            flatVelocity *= jumpMomentumBoost;

            if (flatVelocity.magnitude > maxJumpMomentumSpeed)
            {
                flatVelocity = flatVelocity.normalized * maxJumpMomentumSpeed;
            }

            m_RigidBody.linearVelocity = flatVelocity;

            m_RigidBody.AddForce(
                antiGravityDir * movementSettings.JumpForce,
                ForceMode.Impulse
            );
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
