using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using Entropy.Perks;

public class PistolShoot : MonoBehaviour
{
    // ── References ──────────────────────────────────────────────────────────
    [Header("References")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Camera cam;
    public Animator cameraAnimator;
    public ParticleSystem muzzleFlash;
    public GunRecoil gunRecoil;

    [Header("Audio")]
    public AudioSource gunshotAudioSource;

    // ── Weapon Stats ────────────────────────────────────────────────────────
    [Header("Weapon Stats")]
    public float bulletSpeed = 60f;

    [Tooltip("Rounds per minute")]
    public float fireRate = 180f;

    public int maxAmmo = 12;
    public float reloadTime = 1.5f;

    [Header("Reload Spin Animation")]
    public Transform gunSpinTarget;
    public int reloadSpinCount = 1;
    public Vector3 reloadSpinAxis = Vector3.forward;

    [Header("Shot Settings")]
    [Tooltip("How many bullets are fired per shot.")]
    public int bulletCount = 1;

    [Tooltip("Spread angle in degrees.")]
    public float bulletSpread = 2f;

    // ── Recoil ──────────────────────────────────────────────────────────────
    [Header("Recoil")]
    public float recoilKickUp = 1.5f;
    public float recoilRecoverySpeed = 8f;

    // ── State ───────────────────────────────────────────────────────────────
    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;

    [Header("Internal")]
    public RigidbodyFirstPersonController rbfps;
    public PlayerController playerController;

    private float recoilOffset;
    private PlayerStats _playerStats;

    void Start()
    {
        if (rbfps == null)
            rbfps = GetComponentInParent<RigidbodyFirstPersonController>();

        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        currentAmmo = maxAmmo;
        _playerStats = GetComponentInParent<PlayerStats>();

        if (gunRecoil == null)
        {
            gunRecoil = GetComponentInChildren<GunRecoil>();
        }
    }

    void Update()
    {
        if (playerController != null && playerController.IsParkour)
            return;

        HandleRecoilRecovery();
        HandleInput();
    }

    // ── Input ───────────────────────────────────────────────────────────────
    private void HandleInput()
    {
        if (isReloading) return;

        if (Input.GetKeyDown(KeyCode.R) || (currentAmmo <= 0 && Input.GetButtonDown("Fire1")))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") &&
            Time.time >= nextFireTime &&
            currentAmmo > 0)
        {
            Fire();
        }
    }

    // ── Fire ────────────────────────────────────────────────────────────────
    private void Fire()
    {
        nextFireTime = Time.time + 60f / fireRate;
        currentAmmo--;

        if (gunRecoil != null)
        {
            gunRecoil.PlayRecoil();
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        Vector3 baseDirection = GetAimDirection();

        // Fire multiple bullets with spread
        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 spreadDirection = ApplySpread(baseDirection);

            GameObject bulletGO = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.LookRotation(spreadDirection)
            );

            Bullet bullet = bulletGO.GetComponent<Bullet>();
            if (bullet != null)
            {
                float modifiedSpeed = _playerStats != null
                    ? _playerStats.GetStat(StatType.BulletSpeed)
                    : bulletSpeed;

                bullet.Launch(spreadDirection, modifiedSpeed);

                Rigidbody bulletRb = bulletGO.GetComponent<Rigidbody>();
                if (bulletRb != null && _playerStats != null)
                {
                    bulletRb.mass = _playerStats.GetStat(StatType.BulletMass);
                    bulletRb.linearDamping = _playerStats.GetStat(StatType.BulletDrag);
                }

                bullet.damage = _playerStats != null
                    ? _playerStats.GetStat(StatType.BulletDamage)
                    : bullet.damage;

                bullet.CanRicochet = PerksManager.Instance != null
                    && PerksManager.Instance.HasPerk("ricochet");
            }
        }

        ApplyRecoil();

        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("Recoil");

        if (gunshotAudioSource != null)
            gunshotAudioSource.Play();
    }

    // ── Spread ──────────────────────────────────────────────────────────────
    private Vector3 ApplySpread(Vector3 direction)
    {
        float yaw = Random.Range(-bulletSpread, bulletSpread);
        float pitch = Random.Range(-bulletSpread, bulletSpread);

        Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0f);
        return spreadRotation * direction;
    }

    // ── Aim Direction ───────────────────────────────────────────────────────
    private Vector3 GetAimDirection()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            if (hit.collider.CompareTag("Player") ||
                hit.collider.CompareTag("MainCamera") ||
                hit.collider.CompareTag("PlayerDetect"))
            {
                targetPoint = ray.GetPoint(500f);
            }
            else
            {
                targetPoint = hit.point;
            }
        }
        else
        {
            targetPoint = ray.GetPoint(500f);
        }

        return (targetPoint - firePoint.position).normalized;
    }

    // ── Recoil ──────────────────────────────────────────────────────────────
    private void ApplyRecoil()
    {
        if (rbfps == null) return;

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

    // ── Reload ──────────────────────────────────────────────────────────────
    private IEnumerator Reload()
    {
        if (currentAmmo == maxAmmo) yield break;  // Already full.

        isReloading = true;
        Debug.Log("Reloading...");

        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("Reload");

        float modifiedReloadTime = _playerStats != null
            ? _playerStats.GetStat(StatType.ReloadTime)
            : reloadTime;

        yield return StartCoroutine(SpinGunDuringReload(modifiedReloadTime));

        currentAmmo = maxAmmo;
        isReloading = false;
        Debug.Log("Reload complete.");
    }

    private IEnumerator SpinGunDuringReload(float duration)
    {
        if (gunSpinTarget == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        Quaternion originalRotation = gunSpinTarget.localRotation;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = timer / duration;
            float angle = 360f * reloadSpinCount * progress;

            gunSpinTarget.localRotation =
                originalRotation * Quaternion.AngleAxis(angle, reloadSpinAxis.normalized);

            yield return null;
        }

        gunSpinTarget.localRotation = originalRotation;
    }

    // ── HUD ────────────────────────────────────────────────────────────────
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool IsReloading => isReloading;
}