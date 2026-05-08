using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// Pistol shooting controller.
/// 
/// Setup:
///   1. Attach this script to the Player GameObject (same object as PlayerController).
///   2. Assign:
///      - bulletPrefab      : Your bullet prefab (must have Bullet.cs + Rigidbody + Collider).
///      - firePoint         : An empty child Transform at the gun's barrel tip, facing forward.
///      - cam               : The same Camera already assigned in RigidbodyFirstPersonController.
///      - cameraAnimator    : Same Animator already assigned in PlayerController (add Recoil trigger to it).
///   3. Optionally assign gunshotAudioSource for a fire sound.
/// </summary>
public class PistolShoot : MonoBehaviour
{
    // ── References ──────────────────────────────────────────────────────────
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;         // Muzzle of the gun
    public Camera cam;                  // Same camera as in RigidbodyFirstPersonController
    public Animator cameraAnimator;     // Same animator as in PlayerController

    [Header("Audio")]
    public AudioSource gunshotAudioSource;  // Optional — assign a clip in AudioSource component

    // ── Pistol Stats ─────────────────────────────────────────────────────────
    [Header("Pistol Stats")]
    public float bulletSpeed = 60f;

    [Tooltip("Rounds per minute")]
    public float fireRate = 180f;           // 180 RPM ≈ semi-auto pistol

    public int maxAmmo = 12;
    public float reloadTime = 1.5f;

    // ── Recoil ───────────────────────────────────────────────────────────────
    [Header("Recoil")]
    [Tooltip("Degrees kicked upward per shot (applied to MouseLook camera target).")]
    public float recoilKickUp = 1.5f;

    [Tooltip("How fast the camera snaps back after recoil (degrees per second).")]
    public float recoilRecoverySpeed = 8f;

    // ── State ────────────────────────────────────────────────────────────────
    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;

    [Header("Internal (auto-found, or assign manually)")]
    [Tooltip("Drag the Player root here if PistolShoot lives on a child GameObject.")]
    public RigidbodyFirstPersonController rbfps;
    public PlayerController playerController;

    // We track accumulated recoil so we can recover it smoothly.
    private float recoilOffset;

    // ── Unity Lifecycle ──────────────────────────────────────────────────────
    void Start()
    {
        // Try same GameObject first, then search parents (gun is often a child of player).
        if (rbfps == null)
            rbfps = GetComponentInParent<RigidbodyFirstPersonController>();
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        if (rbfps == null)
            Debug.LogError("PistolShoot: RigidbodyFirstPersonController not found. " +
                           "Assign it manually in the Inspector.", this);

        currentAmmo = maxAmmo;
    }

    void Update()
    {
        // Block shooting during parkour moves.
        if (playerController != null && playerController.IsParkour) return;

        HandleRecoilRecovery();
        HandleInput();
    }

    // ── Input ────────────────────────────────────────────────────────────────
    private void HandleInput()
    {
        if (isReloading) return;

        // Reload manually with R, or auto-reload when empty.
        if (Input.GetKeyDown(KeyCode.R) || (currentAmmo <= 0 && Input.GetButtonDown("Fire1")))
        {
            StartCoroutine(Reload());
            return;
        }

        // Fire on left mouse button — respects fire rate.
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Fire();
        }
    }

    // ── Firing ───────────────────────────────────────────────────────────────
    private void Fire()
    {
        nextFireTime = Time.time + 60f / fireRate;
        currentAmmo--;

        // ── Direction from centre of screen (crosshair aim) ──────────────────
        // Raycast from the centre of the camera to find the precise aim point,
        // then shoot the bullet toward that point from the muzzle.
        Vector3 aimDirection = GetAimDirection();

        // Spawn bullet.
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(aimDirection));
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Launch(aimDirection, bulletSpeed);

        // Camera recoil — push camera target upward via MouseLook.
        ApplyRecoil();

        // Trigger the Recoil animation state if it exists.
        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("Recoil");

        // Audio.
        if (gunshotAudioSource != null)
            gunshotAudioSource.Play();
    }

    /// <summary>
    /// Casts a ray from the screen centre and returns the direction from the
    /// firePoint toward the hit point (or the ray endpoint if nothing is hit).
    /// This keeps aim accurate even when the muzzle is offset from screen centre.
    /// </summary>
    private Vector3 GetAimDirection()
    {
				// return cam.main.transform.forward;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            // Exclude the player's own colliders from the aim ray.
            if (hit.collider != null && hit.collider.CompareTag("Player"))
                targetPoint = ray.GetPoint(500f);
            else
                targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(500f);
        }

        return (targetPoint - firePoint.position).normalized;
    }

    // ── Recoil ───────────────────────────────────────────────────────────────
    private void ApplyRecoil()
    {
        if (rbfps == null) return;

        // Directly modify the MouseLook camera target rotation so recoil
        // integrates with the same system used for mouse look.
        rbfps.mouseLook.m_CameraTargetRot *= Quaternion.Euler(-recoilKickUp, 0f, 0f);
        recoilOffset += recoilKickUp;
    }

    private void HandleRecoilRecovery()
    {
        if (recoilOffset <= 0f || rbfps == null) return;

        float recovery = recoilRecoverySpeed * Time.deltaTime;
        recovery = Mathf.Min(recovery, recoilOffset);

        rbfps.mouseLook.m_CameraTargetRot *= Quaternion.Euler(recovery, 0f, 0f);
        recoilOffset -= recovery;
    }

    // ── Reload ───────────────────────────────────────────────────────────────
    private IEnumerator Reload()
    {
        if (currentAmmo == maxAmmo) yield break;  // Already full.

        isReloading = true;
        Debug.Log("Reloading...");

        // Trigger reload animation if you have one.
        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reload complete.");
    }

    // ── Public Accessors (for HUD / UI) ─────────────────────────────────────
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool IsReloading => isReloading;
}
