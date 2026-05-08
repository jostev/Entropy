using TMPro;
using UnityEngine;

public class GunAmmoHologramUI : MonoBehaviour
{
    [Header("References")]
    public PistolShoot pistolShoot;
    public TextMeshProUGUI ammoText;
    public Camera playerCamera;

    void Start()
    {
        if (ammoText == null)
        {
            ammoText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        if (pistolShoot == null || ammoText == null)
        {
            return;
        }

        if (pistolShoot.IsReloading)
        {
            ammoText.text = "RELOADING";
        }
        else
        {
            ammoText.text = pistolShoot.CurrentAmmo + " / " + pistolShoot.MaxAmmo;
        }
    }
}