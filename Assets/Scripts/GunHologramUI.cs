using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GunAmmoHologramUI : MonoBehaviour
{
    [Header("References")]
    public PistolShoot pistolShoot;
    public Transform   segmentContainer;
    public Camera      playerCamera;

    // ── Colours ───────────────────────────────────────────────────────────
    readonly Color colActive  = new Color(0.0f, 1.0f, 0.9f, 1.0f);   // cyan
    readonly Color colEmpty   = new Color(0.0f, 0.2f, 0.2f, 0.4f);   // dim
    readonly Color colLow     = new Color(1.0f, 0.1f, 0.5f, 1.0f);   // magenta
    readonly Color colReload  = new Color(1.0f, 0.6f, 0.0f, 1.0f);   // amber

    // ── Internals ─────────────────────────────────────────────────────────
    List<Image> segs = new List<Image>();
    CanvasGroup cg;
    int  lastAmmo    = -1;
    bool wasReload;
    float animTilt, tiltTarget;

    // ── Start ─────────────────────────────────────────────────────────────
    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        if (playerCamera == null) playerCamera = Camera.main;

        BuildSegments();
        StartCoroutine(Flicker());
    }

    void BuildSegments()
    {
        foreach (Transform t in segmentContainer) Destroy(t.gameObject);
        segs.Clear();

        for (int i = 0; i < pistolShoot.maxAmmo; i++)
        {
            // Create segment as a plain UI Image — no prefab needed.
            var go  = new GameObject("seg_" + i, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(segmentContainer, false);

            var rt  = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(8, 28);

            var img = go.GetComponent<Image>();
            img.color = colActive;
            segs.Add(img);
        }

        Refresh(pistolShoot.CurrentAmmo, false);
    }

    // ── LateUpdate ────────────────────────────────────────────────────────
    void LateUpdate()
    {
        if (pistolShoot == null) return;

        // Billboard — always face the camera.
        transform.LookAt(
            transform.position + playerCamera.transform.rotation * Vector3.forward,
            playerCamera.transform.rotation * Vector3.up);
				// transform.LookAt(playerCamera.transform);

        // Apply tilt offset on top of billboard (for reload droop).
        float spd = tiltTarget != 0f ? 8f : 14f;
        animTilt = Mathf.Lerp(animTilt, tiltTarget, spd * Time.deltaTime);
        transform.rotation *= Quaternion.Euler(animTilt, 0f, 0f);

        // Poll ammo state.
        int  ammo     = pistolShoot.CurrentAmmo;
        bool reloading = pistolShoot.IsReloading;
        if (ammo == lastAmmo && reloading == wasReload) return;



        bool started = reloading && !wasReload;
        lastAmmo = ammo; wasReload = reloading;

        if (started) StartCoroutine(ReloadAnim());
        else if (!reloading) Refresh(ammo, false);
    }

    // ── Refresh segment colours ───────────────────────────────────────────
    void Refresh(int ammo, bool reloading)
    {
        Color on = ammo <= 3 ? colLow : colActive;
        for (int i = 0; i < segs.Count; i++)
            segs[i].color = i < ammo ? on : colEmpty;
    }

    // ── Ambient hologram flicker ───────────────────────────────────────────
    IEnumerator Flicker()
    {
        while (true)
        {
            cg.alpha = Mathf.Lerp(0.88f, 1f, Mathf.PerlinNoise(Time.time * 5f, 0f));
            if (Random.value < 0.003f)       // rare hard glitch
            {
                cg.alpha = 0.1f;
                yield return new WaitForSeconds(Random.Range(0.03f, 0.07f));
            }
            yield return null;
        }
    }

    // ── Reload: droop down + amber scanner sweep ───────────────────────────
    IEnumerator ReloadAnim()
    {
        tiltTarget = 26f;
        float t = 0f;
        while (pistolShoot.IsReloading)
        {
            t += Time.deltaTime * 3.5f;
            int idx = Mathf.FloorToInt(t % segs.Count);
            for (int i = 0; i < segs.Count; i++)
                segs[i].color = new Color(colReload.r, colReload.g, colReload.b,
                                          i == idx ? 1f : 0.2f);
            yield return null;
        }
        tiltTarget = 0f;
        Refresh(pistolShoot.CurrentAmmo, false);
    }


}
