using System.Collections;
using UnityEngine;

namespace Entropy.Perks.UI
{
    [ExecuteInEditMode]
    public class PerkMenuManager : MonoBehaviour
    {
        public static PerkMenuManager Instance { get; private set; }

        [Header("Input")]
        [SerializeField] private KeyCode _openKey = KeyCode.P;
        [SerializeField] private KeyCode _closeKey = KeyCode.Escape;

        [Header("Time")]
        [Tooltip("0 = full pause. 0.1 = slow-motion. 1 = no effect.")]
        [SerializeField] private float _timeScaleWhenOpen = 0f;

        [Header("Animation")]
        [SerializeField] private float _fadeDuration = 0.25f;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("UI")]
        [SerializeField] private PerkMenuUI _menuUI;
        [SerializeField] private DeckSwitcher _deckSwitcher;

        private bool _isOpen;
        private Coroutine _fadeCoroutine;
        private float _previousTimeScale = 1f;

        public bool IsOpen => _isOpen;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            EnsureMenuUI();
        }

        private void EnsureMenuUI()
        {
            if (_menuUI != null) return;

            _menuUI = GetComponentInChildren<PerkMenuUI>(true);
            if (_menuUI == null)
            {
                var menuGO = new GameObject("PerkMenuUI", typeof(RectTransform));
                menuGO.transform.SetParent(transform, false);
                _menuUI = menuGO.AddComponent<PerkMenuUI>();
            }
        }

        void OnEnable()
        {
            if (!Application.isPlaying && _canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }
        }

        void Update()
        {
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            if (Input.GetKeyDown(_openKey))
            {
                if (shiftHeld)
                {
                    if (!_isOpen) OpenMenu(showStatsSummary: true);
                    else _menuUI?.ToggleStatsSummary();
                }
                else
                {
                    if (!_isOpen) OpenMenu(showStatsSummary: false);
                    else CloseMenu();
                }
            }

            if (_isOpen && Input.GetKeyDown(_closeKey))
            {
                CloseMenu();
            }

            if (_isOpen && _deckSwitcher != null)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) _deckSwitcher.PreviousDeck();
                if (Input.GetKeyDown(KeyCode.RightArrow)) _deckSwitcher.NextDeck();
            }
        }

        public void OpenMenu(bool showStatsSummary = false)
        {
            if (_isOpen) return;
            _isOpen = true;

            if (_canvasGroup == null) return;

            _previousTimeScale = Time.timeScale;
            Time.timeScale = _timeScaleWhenOpen;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            _menuUI?.Refresh(showStatsSummary);
            _deckSwitcher?.Refresh();

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTo(1f));
        }

        public void CloseMenu()
        {
            if (!_isOpen) return;
            _isOpen = false;

            Time.timeScale = _previousTimeScale;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTo(0f, () =>
            {
                _menuUI?.OnMenuClosed();
            }));
        }

        private IEnumerator FadeTo(float targetAlpha, System.Action onComplete = null)
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }
    }
}
