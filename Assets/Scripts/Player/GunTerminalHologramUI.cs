using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class GunTerminalHologramUI : MonoBehaviour
{
    [Header("References")]
    public PistolShoot pistolShoot;
    public Transform anchor;
    public Camera playerCamera;
    public TMP_Text terminalText;
    public Image background;

    [Header("Facing")]
    [Tooltip("Enable this if the terminal appears backwards / facing away.")]
    public bool flipFacing = false;

    [Tooltip("Extra local rotation after the terminal faces the camera.")]
    public Vector3 rotationOffsetEuler = Vector3.zero;

    [Header("Movement Compliance")]
    [Tooltip("Higher = follows pistol position more rigidly. Lower = floatier / more resistant.")]
    public float positionFollowStrength = 14f;

    [Tooltip("Higher = rotates toward camera more rigidly. Lower = smoother / more resistant.")]
    public float rotationFollowStrength = 18f;

    [Tooltip("Tiny visual bob caused by gun motion.")]
    public float motionSwayAmount = 0.025f;

    [Tooltip("How fast sway reacts to movement.")]
    public float swayResponsiveness = 8f;

    [Header("Terminal Appearance")]
    public Color normalTextColor = new Color(0f, 1f, 0.9f, 1f);
    public Color lowAmmoColor = new Color(1f, 0.1f, 0.5f, 1f);
    public Color reloadColor = new Color(1f, 0.6f, 0f, 1f);
    public Color backgroundColor = new Color(0f, 0.08f, 0.08f, 0.55f);

    [Header("Display Modules")]
    public AmmoBlock ammoBlock = new AmmoBlock();
    public StatusBlock statusBlock = new StatusBlock();
    public DiagnosticsBlock diagnosticsBlock = new DiagnosticsBlock();

    CanvasGroup cg;

    Vector3 smoothedPosition;
    Quaternion smoothedRotation;

    Vector3 previousAnchorPosition;
    Vector3 swayOffset;

    int lastAmmo = -1;
    bool wasReloading;

    readonly StringBuilder sb = new StringBuilder();

    void Start()
    {
        cg = GetComponent<CanvasGroup>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (anchor == null)
            anchor = transform.parent != null ? transform.parent : transform;

        smoothedPosition = transform.position;
        smoothedRotation = transform.rotation;
        previousAnchorPosition = anchor.position;

        if (background != null)
            background.color = backgroundColor;

        ConfigureTerminalText();
        RefreshText();

        StartCoroutine(Flicker());
    }

    void ConfigureTerminalText()
    {
        if (terminalText == null) return;

        terminalText.fontSize = 0.05F;
        terminalText.alignment = TextAlignmentOptions.TopLeft;
        terminalText.enableWordWrapping = false;
        terminalText.color = normalTextColor;
        terminalText.text = "";
    }

    void LateUpdate()
    {
        if (pistolShoot == null || playerCamera == null || terminalText == null || anchor == null)
            return;

        UpdateCompliantPosition();
        UpdateBillboardRotation();
        UpdateTerminalIfChanged();
    }

    void UpdateCompliantPosition()
    {
        Vector3 anchorVelocity = (anchor.position - previousAnchorPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        previousAnchorPosition = anchor.position;

        Vector3 targetSway = -anchorVelocity * motionSwayAmount;
        targetSway = Vector3.ClampMagnitude(targetSway, 0.08f);

        swayOffset = Vector3.Lerp(
            swayOffset,
            targetSway,
            1f - Mathf.Exp(-swayResponsiveness * Time.deltaTime)
        );

        Vector3 targetPosition = anchor.position + swayOffset;

        smoothedPosition = Vector3.Lerp(
            smoothedPosition,
            targetPosition,
            1f - Mathf.Exp(-positionFollowStrength * Time.deltaTime)
        );

        transform.position = smoothedPosition;
    }

    void UpdateBillboardRotation()
    {
        Vector3 directionToCamera = playerCamera.transform.position - transform.position;

        if (directionToCamera.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation;

        if (!flipFacing)
        {
            // Terminal's forward points toward the player.
            targetRotation = Quaternion.LookRotation(-directionToCamera.normalized, playerCamera.transform.up);
        }
        else
        {
            // Use this if the UI appears backwards.
            targetRotation = Quaternion.LookRotation(directionToCamera.normalized, playerCamera.transform.up);
        }

        targetRotation *= Quaternion.Euler(rotationOffsetEuler);

        smoothedRotation = Quaternion.Slerp(
            smoothedRotation,
            targetRotation,
            1f - Mathf.Exp(-rotationFollowStrength * Time.deltaTime)
        );

        transform.rotation = smoothedRotation;
    }

    void UpdateTerminalIfChanged()
    {
        int ammo = pistolShoot.CurrentAmmo;
        bool reloading = pistolShoot.IsReloading;

        if (ammo == lastAmmo && reloading == wasReloading)
            return;

        lastAmmo = ammo;
        wasReloading = reloading;

        RefreshText();
    }

    void RefreshText()
    {
        if (terminalText == null || pistolShoot == null)
            return;

        sb.Clear();

        sb.AppendLine("╔════════════════════╗");
        sb.AppendLine("║  SIDEARM TERMINAL  ║");
        sb.AppendLine("╚════════════════════╝");
        sb.AppendLine();

        if (ammoBlock.enabled)
            ammoBlock.Append(sb, pistolShoot);

        if (statusBlock.enabled)
            statusBlock.Append(sb, pistolShoot);

        if (diagnosticsBlock.enabled)
            diagnosticsBlock.Append(sb, pistolShoot);

        terminalText.text = sb.ToString();

        if (pistolShoot.IsReloading)
            terminalText.color = reloadColor;
        else if (pistolShoot.CurrentAmmo <= ammoBlock.lowAmmoThreshold)
            terminalText.color = lowAmmoColor;
        else
            terminalText.color = normalTextColor;
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            if (cg != null)
            {
                cg.alpha = Mathf.Lerp(
                    0.88f,
                    1f,
                    Mathf.PerlinNoise(Time.time * 5f, 0f)
                );

                if (Random.value < 0.003f)
                {
                    cg.alpha = 0.12f;
                    yield return new WaitForSeconds(Random.Range(0.03f, 0.07f));
                }
            }

            yield return null;
        }
    }

    [System.Serializable]
    public class AmmoBlock
    {
        public bool enabled = true;
        public int lowAmmoThreshold = 3;
        public int barLength = 12;

        public void Append(StringBuilder sb, PistolShoot pistol)
        {
            int ammo = pistol.CurrentAmmo;
            int maxAmmo = Mathf.Max(1, pistol.maxAmmo);

            int filledBars = Mathf.RoundToInt((ammo / (float)maxAmmo) * barLength);

            sb.Append("AMMO  [");

            for (int i = 0; i < barLength; i++)
                sb.Append(i < filledBars ? "█" : "░");

            sb.Append("] ");
            sb.Append(ammo);
            sb.Append("/");
            sb.AppendLine(maxAmmo.ToString());

            if (ammo <= lowAmmoThreshold && !pistol.IsReloading)
                sb.AppendLine("WARN  LOW AMMUNITION");
        }
    }

    [System.Serializable]
    public class StatusBlock
    {
        public bool enabled = true;

        public void Append(StringBuilder sb, PistolShoot pistol)
        {
            sb.Append("STAT  ");

            if (pistol.IsReloading)
                sb.AppendLine("RELOADING...");
            else if (pistol.CurrentAmmo <= 0)
                sb.AppendLine("EMPTY");
            else
                sb.AppendLine("READY");
        }
    }

    [System.Serializable]
    public class DiagnosticsBlock
    {
        public bool enabled = true;
        public bool showSystemLine = true;
        public bool showWeaponLine = true;

        public void Append(StringBuilder sb, PistolShoot pistol)
        {
            if (showSystemLine)
                sb.AppendLine("SYS   HOLO-LINK STABLE");

            if (showWeaponLine)
                sb.AppendLine("WPN   PISTOL CONNECTED");
        }
    }
}
