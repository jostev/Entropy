using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Entropy.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [Header("Auto-Setup")]
        [SerializeField] private bool createOnAwake = true;
        [SerializeField] private Health targetHealth;

        [Header("Position")]
        [SerializeField] private Vector2 anchoredPosition = new Vector2(40f, 40f);
        [SerializeField] private Vector2 size = new Vector2(300f, 30f);

        [Header("Colors")]
        [SerializeField] private Color healthyColor = new Color(0.13f, 0.75f, 0.13f);
        [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.15f, 0.15f);
        [SerializeField] private Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        [SerializeField] private float lowHealthThreshold = 0.25f;

        [Header("Damage Flash")]
        [SerializeField] private bool enableDamageFlash = true;
        [SerializeField] private float flashDuration = 0.2f;
        [SerializeField] private float flashMaxAlpha = 0.25f;

        private Canvas canvas;
        private Slider healthSlider;
        private Image fillImage;
        private TextMeshProUGUI healthText;
        private Image damageFlashImage;

        private float currentFill;
        private float flashTimer;
        private bool isSetup;

        void Awake()
        {
            if (targetHealth == null)
                targetHealth = GetComponent<Health>();

            if (createOnAwake && targetHealth != null)
                SetupUI();
        }

        void Update()
        {
            if (!isSetup || targetHealth == null) return;

            float targetFill = targetHealth.maxHealth > 0f
                ? targetHealth.currentHealth / targetHealth.maxHealth
                : 0f;

            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * 10f);

            if (healthSlider != null)
                healthSlider.value = currentFill;

            UpdateColor();
            UpdateText();
            UpdateFlash();
        }

        [ContextMenu("Setup Health Bar UI")]
        public void SetupUI()
        {
            if (isSetup) return;

            CreateCanvas();
            CreateSlider();
            if (enableDamageFlash)
                CreateDamageFlash();
            CreateHealthText();

            currentFill = targetHealth != null && targetHealth.maxHealth > 0f
                ? targetHealth.currentHealth / targetHealth.maxHealth
                : 1f;

            if (targetHealth != null)
                targetHealth.OnDamaged.AddListener(OnDamaged);

            isSetup = true;
        }

        void OnDestroy()
        {
            if (targetHealth != null)
                targetHealth.OnDamaged.RemoveListener(OnDamaged);
        }

        private void OnDamaged()
        {
            if (!enableDamageFlash || damageFlashImage == null) return;

            flashTimer = flashDuration;
            damageFlashImage.gameObject.SetActive(true);

            Color c = damageFlashImage.color;
            c.a = flashMaxAlpha;
            damageFlashImage.color = c;
        }

        private void CreateCanvas()
        {
            GameObject canvasGO = new GameObject("PlayerHealthCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            canvasGO.transform.SetParent(transform, false);
        }

        private void CreateSlider()
        {
            GameObject sliderGO = new GameObject("HealthSlider");
            sliderGO.transform.SetParent(canvas.transform, false);

            RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
            sliderRect.anchorMin = Vector2.zero;
            sliderRect.anchorMax = Vector2.zero;
            sliderRect.pivot = Vector2.zero;
            sliderRect.anchoredPosition = anchoredPosition;
            sliderRect.sizeDelta = size;

            healthSlider = sliderGO.AddComponent<Slider>();
            healthSlider.transition = Selectable.Transition.None;
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;

            GameObject bgGO = new GameObject("Background");
            bgGO.transform.SetParent(sliderGO.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = backgroundColor;

            GameObject fillAreaGO = new GameObject("Fill Area");
            fillAreaGO.transform.SetParent(sliderGO.transform, false);
            RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(fillAreaGO.transform, false);
            RectTransform fillRect = fillGO.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            fillImage = fillGO.AddComponent<Image>();
            fillImage.color = healthyColor;

            healthSlider.fillRect = fillRect;
            healthSlider.targetGraphic = fillImage;
        }

        private void CreateDamageFlash()
        {
            GameObject flashGO = new GameObject("DamageFlash");
            flashGO.transform.SetParent(canvas.transform, false);
            flashGO.SetActive(false);

            RectTransform flashRect = flashGO.AddComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.pivot = new Vector2(0.5f, 0.5f);
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;

            damageFlashImage = flashGO.AddComponent<Image>();
            damageFlashImage.color = new Color(1f, 0f, 0f, 0f);
            damageFlashImage.raycastTarget = false;
        }

        private void CreateHealthText()
        {
            GameObject textGO = new GameObject("HealthText");
            textGO.transform.SetParent(healthSlider.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(5f, 0f);
            textRect.offsetMax = new Vector2(-5f, 0f);

            healthText = textGO.AddComponent<TextMeshProUGUI>();
            healthText.fontSize = 14f;
            healthText.alignment = TextAlignmentOptions.Center;
            healthText.color = Color.white;
            healthText.text = "100 / 100";
        }

        private void UpdateColor()
        {
            if (fillImage == null) return;

            fillImage.color = Color.Lerp(
                lowHealthColor,
                healthyColor,
                Mathf.Clamp01((currentFill - lowHealthThreshold) / (1f - lowHealthThreshold))
            );
        }

        private void UpdateText()
        {
            if (healthText == null || targetHealth == null) return;

            healthText.text = $"{Mathf.CeilToInt(targetHealth.currentHealth)} / {Mathf.CeilToInt(targetHealth.maxHealth)}";
        }

        private void UpdateFlash()
        {
            if (!enableDamageFlash || damageFlashImage == null) return;
            if (flashTimer <= 0f)
            {
                if (damageFlashImage.gameObject.activeSelf)
                    damageFlashImage.gameObject.SetActive(false);
                return;
            }

            flashTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(flashTimer / flashDuration);

            Color c = damageFlashImage.color;
            c.a = flashMaxAlpha * t;
            damageFlashImage.color = c;
        }
    }
}
