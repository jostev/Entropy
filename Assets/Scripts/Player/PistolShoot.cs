using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.Characters.FirstPerson;
using Entropy.Perks;
using Entropy.Perks.UI;

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

    [Header("Crosshair Raycast")]
    [Tooltip("How far the centre-screen crosshair ray can aim.")]
    public float aimRange = 500f;

    [Tooltip("Layers the crosshair ray can hit.")]
    public LayerMask aimMask = ~0;

    [Tooltip("Draws the crosshair ray in the Scene view for debugging.")]
    public bool drawAimDebugRay = true;

    [Header("Bullet Drop Crosshair")]
    [Tooltip("Assign the RectTransform of your UI crosshair here.")]
    public RectTransform crosshairRect;

    [Tooltip("If enabled, the UI crosshair moves down to show expected bullet drop.")]
    public bool useBulletDropCrosshair = true;

    [Tooltip("Gravity multiplier used for calculating expected bullet drop.")]
    public float bulletGravityScale = 1f;

    [Tooltip("How many UI pixels equal 1 metre of bullet drop.")]
    public float crosshairPixelsPerMetre = 40f;

    [Tooltip("Maximum downward crosshair movement in pixels.")]
    public float maxCrosshairDrop = 120f;

    private Vector2 crosshairBasePosition;

    // ── Recoil ──────────────────────────────────────────────────────────────
    [Header("Recoil")]
    public float recoilKickUp = 1.5f;
    public float recoilRecoverySpeed = 8f;

    // ── State ───────────────────────────────────────────────────────────────
    private int _currentAmmo;
    private bool _isReloading;
    private float nextFireTime;

    [Header("Internal")]
    public RigidbodyFirstPersonController rbfps;
    public PlayerController playerController;

    private float recoilOffset;
    private PlayerStats _playerStats;

    public static event System.Action<Bullet> OnBulletFired;

    void Start()
    {
        if (rbfps == null)
            rbfps = GetComponentInParent<RigidbodyFirstPersonController>();

        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        _currentAmmo = maxAmmo;
        _playerStats = GetComponentInParent<PlayerStats>();

        if (gunRecoil == null)
            gunRecoil = GetComponentInChildren<GunRecoil>();

        if (cam == null)
            cam = Camera.main;

        if (crosshairRect != null)
            crosshairBasePosition = crosshairRect.anchoredPosition;
    }

    void Update()
    {
        if (playerController != null && playerController.IsParkour)
            return;

        HandleRecoilRecovery();
        UpdateCrosshairDrop();
        HandleInput();
    }

    // ── Input ───────────────────────────────────────────────────────────────
    private void HandleInput()
    {
        if (_isReloading) return;

        if (PerkMenuManager.Instance != null && PerkMenuManager.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.R) || (_currentAmmo <= 0 && Input.GetButtonDown("Fire1")))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButtonDown("Fire1") &&
            Time.time >= nextFireTime &&
            _currentAmmo > 0)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            Fire();
        }
    }

    // ── Fire ────────────────────────────────────────────────────────────────
    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null || cam == null)
            return;

        nextFireTime = Time.time + 60f / fireRate;
        _currentAmmo--;

        if (gunRecoil != null)
            gunRecoil.PlayRecoil();

        if (muzzleFlash != null)
            muzzleFlash.Play();

        Vector3 baseDirection = GetAimDirection();

        // Fire multiple bullets with spread
        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 spreadDirection = ApplySpread(baseDirection);

            GameObject bulletGo = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.LookRotation(spreadDirection)
            );

            Bullet bullet = bulletGo.GetComponent<Bullet>();
            if (bullet != null)
            {
                float modifiedSpeed = _playerStats != null
                    ? _playerStats.GetStat(StatType.BulletSpeed)
                    : bulletSpeed;

                bullet.Launch(spreadDirection, modifiedSpeed);
                OnBulletFired?.Invoke(bullet);

                Rigidbody bulletRb = bulletGo.GetComponent<Rigidbody>();
                if (bulletRb != null && _playerStats != null)
                {
                    bulletRb.mass = _playerStats.GetStat(StatType.BulletMass);
                    bulletRb.linearDamping = _playerStats.GetStat(StatType.BulletDrag);
                }

                bullet.damage = _playerStats != null
                    ? _playerStats.GetStat(StatType.BulletDamage)
                    : bullet.damage;

                if (PerksManager.Instance != null)
                {
                    if (PerksManager.Instance.HasPerk("ricochet"))
                    {
                        bullet.CanRicochet = true;
                        bullet.MaxRicochets = 1;
                        bullet.RicochetSpeedMultiplier = 0.8f;
                    }
                    else if (PerksManager.Instance.HasPerk("ricochet_king"))
                    {
                        bullet.CanRicochet = true;
                        bullet.MaxRicochets = 5;
                        bullet.RicochetSpeedMultiplier = 1.1f;
                    }
                }
            }
        }

        ApplyRecoil();

        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("Recoil");

				SoundManager.Instance?.PlaySFX(SoundManager.Instance.gunShot, 0.8f, 0.08f);
    }

    // ── Spread ──────────────────────────────────────────────────────────────
    private Vector3 ApplySpread(Vector3 direction)
    {
        float yaw = Random.Range(-bulletSpread, bulletSpread);
        float pitch = Random.Range(-bulletSpread, bulletSpread);

        Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0f);
        return spreadRotation * direction;
    }

    // ── Bullet Drop Crosshair ───────────────────────────────────────────────
    private void UpdateCrosshairDrop()
    {
        if (!useBulletDropCrosshair) return;
        if (crosshairRect == null) return;
        if (cam == null) return;

        float modifiedSpeed = _playerStats != null
            ? _playerStats.GetStat(StatType.BulletSpeed)
            : bulletSpeed;

        float distance = GetCrosshairDistance();
        float drop = CalculateBulletDrop(distance, modifiedSpeed);

        float pixelOffset = drop * crosshairPixelsPerMetre;
        pixelOffset = Mathf.Clamp(pixelOffset, 0f, maxCrosshairDrop);

        crosshairRect.anchoredPosition =
            crosshairBasePosition + Vector2.down * pixelOffset;
    }

    private float CalculateBulletDrop(float distance, float speed)
    {
        if (speed <= 0f)
            return 0f;

        float gravity = Mathf.Abs(Physics.gravity.y) * bulletGravityScale;
        float time = distance / speed;

        return 0.5f * gravity * time * time;
    }

    private float GetCrosshairDistance()
    {
        if (cam == null)
            return aimRange;

        Ray crosshairRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(crosshairRay, out RaycastHit hit, aimRange, aimMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player") ||
                hit.collider.CompareTag("MainCamera") ||
                hit.collider.CompareTag("PlayerDetect"))
            {
                return aimRange;
            }

            return hit.distance;
        }

        return aimRange;
    }

    // ── Crosshair Raycast Aim Direction ─────────────────────────────────────
    private Vector3 GetAimDirection()
    {
        Ray crosshairRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;

        if (Physics.Raycast(crosshairRay, out RaycastHit hit, aimRange, aimMask, QueryTriggerInteraction.Ignore))
        {
            // Ignore your own player/camera/player detection colliders.
            if (hit.collider.CompareTag("Player") ||
                hit.collider.CompareTag("MainCamera") ||
                hit.collider.CompareTag("PlayerDetect"))
            {
                targetPoint = crosshairRay.GetPoint(aimRange);
            }
            else
            {
                targetPoint = hit.point;
            }

            if (drawAimDebugRay)
                Debug.DrawLine(crosshairRay.origin, targetPoint, Color.green, 0.15f);
        }
        else
        {
            targetPoint = crosshairRay.GetPoint(aimRange);

            if (drawAimDebugRay)
                Debug.DrawLine(crosshairRay.origin, targetPoint, Color.red, 0.15f);
        }

        return (targetPoint - firePoint.position).normalized;
    }

    // ── Recoil ──────────────────────────────────────────────────────────────
    private void ApplyRecoil()
    {
        if (rbfps == null) return;

        rbfps.mouseLook.AddPitch(-recoilKickUp);
        recoilOffset += recoilKickUp;
    }

    private void HandleRecoilRecovery()
    {
        if (recoilOffset <= 0f || rbfps == null) return;

        float recovery = recoilRecoverySpeed * Time.deltaTime;
        recovery = Mathf.Min(recovery, recoilOffset);

        rbfps.mouseLook.AddPitch(recovery);
        recoilOffset -= recovery;
    }

    // ── Reload ──────────────────────────────────────────────────────────────
    private IEnumerator Reload()
    {
        if (_currentAmmo == maxAmmo) yield break;

        _isReloading = true;

        if (cameraAnimator != null)
            cameraAnimator.SetTrigger("Reload");

        float modifiedReloadTime = _playerStats != null
            ? _playerStats.GetStat(StatType.ReloadTime)
            : reloadTime;

        yield return StartCoroutine(SpinGunDuringReload(modifiedReloadTime));

        _currentAmmo = maxAmmo;
        _isReloading = false;
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

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
    }

    // ── HUD ────────────────────────────────────────────────────────────────
    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool IsReloading => _isReloading;
}
