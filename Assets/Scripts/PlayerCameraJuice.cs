using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerCameraJuice : MonoBehaviour
{
    [Header("References")]
    public RigidbodyFirstPersonController rbfps;
    public PlayerController playerController;
    public Rigidbody rb;

    [Header("General Smoothing")]
    public float positionSmoothSpeed = 12f;
    public float rotationSmoothSpeed = 10f;

    [Header("Movement Bob")]
    public float bobAmount = 0.035f;
    public float bobSpeed = 10f;
    public float sideBobAmount = 0.025f;

    [Header("Jump")]
    public float jumpDipAmount = -0.08f;
    public float jumpPitchAmount = -3f;

    [Header("Landing")]
    public float landingDipAmount = -0.16f;
    public float landingPitchAmount = 4f;
    public float maxLandingStrength = 1.6f;

    [Header("Slide")]
    public float slideDownAmount = 0.45f;
    public float slideForwardAmount = 0.12f;
    public float slideRollAmount = 5f;

    [Header("Air Movement")]
    public float airTiltAmount = 2f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Vector3 targetPositionOffset;
    private Vector3 currentPositionOffset;

    private Vector3 targetRotationOffset;
    private Vector3 currentRotationOffset;

    private Vector3 impulsePositionOffset;
    private Vector3 impulseRotationOffset;

    private bool wasGrounded;
    private bool isSliding;
    private float lastAirYVelocity;
    private float bobTimer;

    void Start()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        if (rbfps == null)
        {
            rbfps = GetComponentInParent<RigidbodyFirstPersonController>();
        }

        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController>();
        }

        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
        }

        if (rbfps != null)
        {
            wasGrounded = rbfps.Grounded;
        }
    }

    void Update()
    {
        if (rbfps == null || rb == null)
        {
            return;
        }

        DetectJumpAndLanding();
        BuildCameraTarget();
        SmoothCamera();
        DecayImpulses();
    }

    private void DetectJumpAndLanding()
    {
        bool grounded = rbfps.Grounded;

        if (!grounded)
        {
            lastAirYVelocity = rb.linearVelocity.y;
        }

        // Jump started
        if (wasGrounded && !grounded && Input.GetKey(KeyCode.Space))
        {
            PlayJump();
        }

        // Landed
        if (!wasGrounded && grounded)
        {
            float landingStrength = Mathf.Clamp01(Mathf.Abs(lastAirYVelocity) / 15f);
            PlayLanding(landingStrength);
        }

        wasGrounded = grounded;
    }

    private void BuildCameraTarget()
    {
        targetPositionOffset = Vector3.zero;
        targetRotationOffset = Vector3.zero;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = flatVelocity.magnitude;

        bool grounded = rbfps.Grounded;
        bool isParkour = playerController != null && playerController.IsParkour;
        bool isWallRunning = playerController != null && playerController.WallRunning;

        // Walking/running bob
        if (grounded && !isSliding && !isParkour && speed > 1f)
        {
            bobTimer += Time.deltaTime * bobSpeed * Mathf.Clamp(speed / 8f, 0.5f, 1.8f);

            float verticalBob = Mathf.Sin(bobTimer) * bobAmount;
            float sideBob = Mathf.Cos(bobTimer * 0.5f) * sideBobAmount;

            targetPositionOffset += new Vector3(sideBob, verticalBob, 0f);
        }
        else
        {
            bobTimer = 0f;
        }

        // Slide camera drop
        if (isSliding)
        {
            targetPositionOffset += Vector3.down * slideDownAmount;
            targetPositionOffset += Vector3.forward * slideForwardAmount;

            targetRotationOffset.z += -horizontal * slideRollAmount;
            targetRotationOffset.x += 2f;
        }

        // Air tilt
        if (!grounded && !isWallRunning)
        {
            targetRotationOffset.z += -horizontal * airTiltAmount;
        }

        // Small movement strafe tilt
        if (grounded && !isSliding)
        {
            targetRotationOffset.z += -horizontal * 1.5f;
        }

        targetPositionOffset += impulsePositionOffset;
        targetRotationOffset += impulseRotationOffset;
    }

    private void SmoothCamera()
    {
        currentPositionOffset = Vector3.Lerp(
            currentPositionOffset,
            targetPositionOffset,
            positionSmoothSpeed * Time.deltaTime
        );

        currentRotationOffset = Vector3.Lerp(
            currentRotationOffset,
            targetRotationOffset,
            rotationSmoothSpeed * Time.deltaTime
        );

        transform.localPosition = originalLocalPosition + currentPositionOffset;
        transform.localRotation = originalLocalRotation * Quaternion.Euler(currentRotationOffset);
    }

    private void DecayImpulses()
    {
        impulsePositionOffset = Vector3.Lerp(
            impulsePositionOffset,
            Vector3.zero,
            9f * Time.deltaTime
        );

        impulseRotationOffset = Vector3.Lerp(
            impulseRotationOffset,
            Vector3.zero,
            9f * Time.deltaTime
        );
    }

    public void PlayJump()
    {
        impulsePositionOffset += new Vector3(0f, jumpDipAmount, 0f);
        impulseRotationOffset += new Vector3(jumpPitchAmount, 0f, 0f);
    }

    public void PlayLanding(float strength)
    {
        strength = Mathf.Clamp(strength, 0.3f, maxLandingStrength);

        impulsePositionOffset += new Vector3(0f, landingDipAmount * strength, 0f);
        impulseRotationOffset += new Vector3(landingPitchAmount * strength, 0f, 0f);
    }

    public void SetSliding(bool sliding)
    {
        isSliding = sliding;
    }
}
