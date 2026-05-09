using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Entropy.Perks;
using Entropy.Perks.UI;

public class PlayerController : MonoBehaviour
{
    public float drag_grounded;
    public float drag_inair;

    public DetectObs detectVaultObject; //checks for vault object
    public DetectObs detectVaultObstruction; //checks if theres somthing in front of the object e.g walls that will not allow the player to vault
    public DetectObs detectClimbObject; //checks for climb object
    public DetectObs detectClimbObstruction; //checks if theres somthing in front of the object e.g walls that will not allow the player to climb


    public DetectObs DetectWallL; //detects for a wall on the left
    public DetectObs DetectWallR; //detects for a wall on the right

    public Animator cameraAnimator;

    public float WallRunUpForce;
    public float WallRunUpForce_DecreaseRate;

    private float upforce;

    public float WallJumpUpVelocity;
    public float WallJumpForwardVelocity;
    public float drag_wallrun;
    public bool WallRunning;
    public bool WallrunningLeft;
    public bool WallrunningRight;
    private bool canwallrun; // ensure that player can only wallrun once before needing to hit the ground again, can be modified for double wallruns
    
    public bool IsParkour;
    private float t_parkour;
    private float chosenParkourMoveTime;

    private bool CanVault;
    public float VaultTime; //how long the vault takes
    public Transform VaultEndPoint;

    private bool CanClimb;
    public float ClimbTime; //how long the vault takes
    public Transform ClimbEndPoint;

    [Header("Landing Detection")]
    public float minLandingVelocity = 5f;
    private bool _wasGrounded;

    [Header("Slide")]
    public KeyCode slideKey = KeyCode.LeftControl;
    public bool IsSliding;

    public float slideDuration = 0.75f;
    public float slideSpeed = 16f;
    public float slideDamping = 0.5f;
    public float slideCooldown = 0.35f;
    public float minSpeedToSlide = 4f;

    [Header("Slide Height")]
    public float slideHeightMultiplier = 0.5f;
    public float cameraSlideDownAmount = 0.45f;
    public float cameraSlideSpeed = 12f;
    public Transform camOffset;

    private CapsuleCollider capsule;
    private float originalCapsuleHeight;
    private Vector3 originalCapsuleCenter;
    private Vector3 originalCamOffsetLocalPosition;

    private float slideTimer;
    private float nextSlideTime;
    private Vector3 slideDirection;

    [Header("Momentum")]
    public float maxMomentumSpeed = 24f;
    public float slideBoost = 7f;
    public float minimumSlideSpeed = 14f;
    public float parkourExitMomentumMultiplier = 0.8f;
    public float parkourExitForwardBoost = 3f;

    private Vector3 savedParkourVelocity;
    private PlayerStats _playerStats;

    public PlayerCameraJuice cameraJuice;

    private RigidbodyFirstPersonController rbfps;
    private Rigidbody rb;
    private Vector3 RecordedMoveToPosition; //the position of the vault end point in world space to move the player to
    private Vector3 RecordedStartPosition; // position of player right before vault
    // Start is called before the first frame update
    void Start()
    {
        rbfps = GetComponent<RigidbodyFirstPersonController>();
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        if (camOffset == null)
        {
            camOffset = transform.Find("CamOffset");
        }

        if (capsule != null)
        {
            originalCapsuleHeight = capsule.height;
            originalCapsuleCenter = capsule.center;
        }

        if (camOffset != null)
        {
            originalCamOffsetLocalPosition = camOffset.localPosition;
        }

        if (cameraJuice == null)
        {
            cameraJuice = GetComponentInChildren<PlayerCameraJuice>();
        }

        _playerStats = GetComponent<PlayerStats>();
        if (_playerStats != null)
        {
            _playerStats.OnStatsChanged += ApplyMovementStats;
            ApplyMovementStats();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PerkMenuManager.Instance != null && PerkMenuManager.Instance.IsOpen) return;

        if (rbfps.Grounded)
        {
            rb.linearDamping = drag_grounded;
            canwallrun = true;

            if (!_wasGrounded && rb.linearVelocity.y < -minLandingVelocity)
            {
                GameEvents.PlayerLanded(Mathf.Abs(rb.linearVelocity.y));
            }
            _wasGrounded = true;
        }
        else
        {
            rb.linearDamping = drag_inair;
            _wasGrounded = false;
        }
        if(WallRunning)
        {
            rb.linearDamping = drag_wallrun;

        }

        HandleSlideInput();
        

        if (IsSliding)
        {
            UpdateSlide();
            return;
        }
        //vault
        if (detectVaultObject.Obstruction && !detectVaultObstruction.Obstruction && !CanVault && !IsParkour && !WallRunning
            && (Input.GetKey(KeyCode.Space) || !rbfps.Grounded) && Input.GetAxisRaw("Vertical") > 0f)
        // if detects a vault object and there is no wall in front then player can pressing space or in air and pressing forward
        {
            CanVault = true;
        }

        if (CanVault)
        {
            CanVault = false; // so this is only called once
            rb.isKinematic = true; //ensure physics do not interrupt the vault
            if (rbfps != null) rbfps.MovementLocked = true;
            RecordedMoveToPosition = VaultEndPoint.position;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = VaultTime;

            cameraAnimator.CrossFade("Vault",0.1f);
        }

        //climb
        if (detectClimbObject.Obstruction && !detectClimbObstruction.Obstruction && !CanClimb && !IsParkour && !WallRunning
            && (Input.GetKey(KeyCode.Space) || !rbfps.Grounded) && Input.GetAxisRaw("Vertical") > 0f)
        {
            CanClimb = true;
        }

        if (CanClimb)
        {
            CanClimb = false; // so this is only called once
            rb.isKinematic = true; //ensure physics do not interrupt the vault
            if (rbfps != null) rbfps.MovementLocked = true;
            RecordedMoveToPosition = ClimbEndPoint.position;
            RecordedStartPosition = transform.position;
            IsParkour = true;
            chosenParkourMoveTime = ClimbTime;

            cameraAnimator.CrossFade("Climb",0.1f);
        }


        //Parkour movement
        if (IsParkour && t_parkour < 1f)
        {
            t_parkour += Time.deltaTime / chosenParkourMoveTime;
            transform.position = Vector3.Lerp(RecordedStartPosition, RecordedMoveToPosition, t_parkour);

            if (t_parkour >= 1f)
            {
                IsParkour = false;
                t_parkour = 0f;
                rb.isKinematic = false;
                if (rbfps != null) rbfps.MovementLocked = false;
            }
        }


        //Wallrun
        if (DetectWallL.Obstruction && !rbfps.Grounded && !IsParkour && canwallrun) // if detect wall on the left and is not on the ground and not doing parkour(climb/vault)
        {
            WallrunningLeft = true;
            canwallrun = false;
            upforce = WallRunUpForce; //refer to line 186
        }

        if (DetectWallR.Obstruction && !rbfps.Grounded && !IsParkour && canwallrun) // if detect wall on thr right and is not on the ground
        {
            WallrunningRight = true;
            canwallrun = false;
            upforce = WallRunUpForce;
        }
        if (WallrunningLeft && !DetectWallL.Obstruction || Input.GetAxisRaw("Vertical") <= 0f || rbfps.relativevelocity.magnitude < 1f) // if there is no wall on the lef tor pressing forward or forward speed < 1 (refer to fpscontroller script)
        {
            WallrunningLeft = false;
            WallrunningRight = false;
        }
        if (WallrunningRight && !DetectWallR.Obstruction || Input.GetAxisRaw("Vertical") <= 0f || rbfps.relativevelocity.magnitude < 1f) // same as above
        {
            WallrunningLeft = false;
            WallrunningRight = false;
        }

        if (WallrunningLeft || WallrunningRight) 
        {
            WallRunning = true;
            rbfps.Wallrunning = true; // this stops the playermovement (refer to fpscontroller script)
        }
        else
        {
            WallRunning = false;
            rbfps.Wallrunning = false;
        }

        if (WallrunningLeft)
        {     
            cameraAnimator.SetBool("WallLeft", true); //Wallrun camera tilt
        }
        else
        {
            cameraAnimator.SetBool("WallLeft", false);
        }
        if (WallrunningRight)
        {           
            cameraAnimator.SetBool("WallRight", true);
        }
        else
        {
            cameraAnimator.SetBool("WallRight", false);
        }

        if (WallRunning)
        {
            
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, upforce ,rb.linearVelocity.z); //set the y velocity while wallrunning
            upforce -= WallRunUpForce_DecreaseRate * Time.deltaTime; //so the player will have a curve like wallrun, upforce from line 136

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.linearVelocity = transform.forward * WallJumpForwardVelocity + transform.up * WallJumpUpVelocity; //walljump
                WallrunningLeft = false;
                WallrunningRight = false;
            }
            if(rbfps.Grounded)
            {
                WallrunningLeft = false;
                WallrunningRight = false;
            }
        }


    }

    private void HandleSlideInput()
    {
        if (Input.GetKeyDown(slideKey) && CanStartSlide())
        {
            StartSlide();
        }
    }

    private bool CanStartSlide()
    {
        if (IsSliding) return false;
        if (Time.time < nextSlideTime) return false;
        if (!rbfps.Grounded) return false;
        if (IsParkour) return false;
        if (WallRunning) return false;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Do not allow backward sliding
        if (vertical < -0.1f)
        {
            return false;
        }

        // Allow forward, left, right, forward-left, forward-right
        if (Mathf.Abs(horizontal) < 0.1f && vertical < 0.1f)
        {
            return false;
        }

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVelocity.magnitude < minSpeedToSlide)
        {
            return false;
        }

        return true;
    }

    private void StartSlide()
    {
        IsSliding = true;
        slideTimer = slideDuration;
        nextSlideTime = Time.time + slideDuration + slideCooldown;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Prevent backward direction
        vertical = Mathf.Max(vertical, 0f);

        // Build slide direction from input
        slideDirection = (transform.forward * vertical) + (transform.right * horizontal);
        slideDirection.y = 0f;

        if (slideDirection.sqrMagnitude < 0.01f)
        {
            slideDirection = transform.forward;
        }

        slideDirection.Normalize();

        rb.linearDamping = slideDamping;

        if (rbfps != null)
        {
            rbfps.MovementLocked = true;
        }

        if (capsule != null)
        {
            float newHeight = originalCapsuleHeight * slideHeightMultiplier;
            newHeight = Mathf.Max(newHeight, capsule.radius * 2f);

            float heightDifference = originalCapsuleHeight - newHeight;

            capsule.height = newHeight;
            capsule.center = originalCapsuleCenter + Vector3.down * (heightDifference / 2f);
        }

        Vector3 currentFlatVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        // Add speed in the slide direction instead of replacing current speed
        Vector3 newFlatVelocity = currentFlatVelocity + slideDirection * slideBoost;

        // Make sure slide has a minimum speed
        if (newFlatVelocity.magnitude < minimumSlideSpeed)
        {
            newFlatVelocity = slideDirection * minimumSlideSpeed;
        }

        // Prevent insane speed
        if (newFlatVelocity.magnitude > maxMomentumSpeed)
        {
            newFlatVelocity = newFlatVelocity.normalized * maxMomentumSpeed;
        }

        rb.linearVelocity = new Vector3(
            newFlatVelocity.x,
            rb.linearVelocity.y,
            newFlatVelocity.z
        );

        if (cameraJuice != null)
        {
            cameraJuice.SetSliding(true);
        }
    }

    private void UpdateSlide()
    {
        slideTimer -= Time.deltaTime;
        rb.linearDamping = slideDamping;

        if (slideTimer <= 0f || !rbfps.Grounded)
        {
            EndSlide();
        }
    }

    private void EndSlide()
    {
        IsSliding = false;

        if (rbfps != null)
        {
            rbfps.MovementLocked = false;
        }

        if (capsule != null)
        {
            capsule.height = originalCapsuleHeight;
            capsule.center = originalCapsuleCenter;
        }

        if (cameraJuice != null)
        {
            cameraJuice.SetSliding(false);
        }
    }

    private void ApplyMovementStats()
    {
        if (_playerStats == null || rbfps == null) return;

        rbfps.movementSettings.ForwardSpeed = _playerStats.GetStat(StatType.ForwardSpeed);
        rbfps.movementSettings.StrafeSpeed = _playerStats.GetStat(StatType.StrafeSpeed);
        rbfps.movementSettings.BackwardSpeed = _playerStats.GetStat(StatType.BackwardSpeed);
        rbfps.movementSettings.SpeedInAir = _playerStats.GetStat(StatType.SpeedInAir);

        if (rb != null)
            rb.mass = _playerStats.GetStat(StatType.PlayerMass);
    }

    void OnDestroy()
    {
        if (_playerStats != null)
            _playerStats.OnStatsChanged -= ApplyMovementStats;
    }
}
