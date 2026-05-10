using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Entropy.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health targetHealth;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Visual Settings")]
        [SerializeField] private Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private float lowHealthThreshold = 0.25f;
        [SerializeField] private float fillSpeed = 8f;

        [Header("Damage Flash")]
        [SerializeField] private Image damageFlashOverlay;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private float flashMaxAlpha = 0.3f;

        private float currentFill;
        private float flashTimer;

        void Start()
        {
            if (targetHealth == null)
                targetHealth = GetComponentInParent<Health>();

            if (targetHealth == null)
            {
                enabled = false;
                return;
            }

            currentFill = targetHealth.currentHealth / targetHealth.maxHealth;
            UpdateVisuals();

            targetHealth.OnDamaged.AddListener(OnTargetDamaged);
        }

        void OnDestroy()
        {
            if (targetHealth != null)
                targetHealth.OnDamaged.RemoveListener(OnTargetDamaged);
        }

        void Update()
        {
            float targetFill = targetHealth.currentHealth / targetHealth.maxHealth;
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSpeed);

            if (healthSlider != null)
                healthSlider.value = currentFill;

            UpdateColor();
            UpdateText();
            UpdateFlash();
        }

        private void OnTargetDamaged()
        {
            flashTimer = flashDuration;

            if (damageFlashOverlay != null)
            {
                Color c = damageFlashOverlay.color;
                c.a = flashMaxAlpha;
                damageFlashOverlay.color = c;
                damageFlashOverlay.gameObject.SetActive(true);
            }
        }

        private void UpdateVisuals()
        {
            if (healthSlider != null)
                healthSlider.value = currentFill;
            UpdateColor();
            UpdateText();
        }

        private void UpdateColor()
        {
            if (healthSlider == null) return;

            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(
                    lowHealthColor,
                    healthyColor,
                    Mathf.Clamp01((currentFill - lowHealthThreshold) / (1f - lowHealthThreshold))
                );
            }
        }

        private void UpdateText()
        {
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(targetHealth.currentHealth)} / {Mathf.CeilToInt(targetHealth.maxHealth)}";
            }
        }

        private void UpdateFlash()
        {
            if (flashTimer <= 0f || damageFlashOverlay == null) return;

            flashTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(flashTimer / flashDuration);

            Color c = damageFlashOverlay.color;
            c.a = flashMaxAlpha * t;
            damageFlashOverlay.color = c;

            if (flashTimer <= 0f)
                damageFlashOverlay.gameObject.SetActive(false);
        }
    }
}
