using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Perks.UI
{
    public class DeckSwitcher : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _deckNameText;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TextMeshProUGUI _pageIndicator;

        [Header("Deck Data")]
        [SerializeField] private string[] _deckNames = { "DECK 1" };

        private int _currentIndex;

        void Start()
        {
            if (_prevButton != null)
                _prevButton.onClick.AddListener(PreviousDeck);
            if (_nextButton != null)
                _nextButton.onClick.AddListener(NextDeck);
        }

        public void Refresh()
        {
            _currentIndex = Mathf.Clamp(_currentIndex, 0, Mathf.Max(0, _deckNames.Length - 1));
            UpdateDisplay();
        }

        public void PreviousDeck()
        {
            if (_deckNames.Length <= 1) return;
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _deckNames.Length - 1;
            UpdateDisplay();
            OnDeckChanged();
        }

        public void NextDeck()
        {
            if (_deckNames.Length <= 1) return;
            _currentIndex++;
            if (_currentIndex >= _deckNames.Length) _currentIndex = 0;
            UpdateDisplay();
            OnDeckChanged();
        }

        private void UpdateDisplay()
        {
            string name = _deckNames.Length > 0 ? _deckNames[_currentIndex] : "NO DECKS";
            if (_deckNameText != null) _deckNameText.text = $"< {name} >";
            if (_pageIndicator != null) _pageIndicator.text = $"{_currentIndex + 1} / {_deckNames.Length}";
        }

        private void OnDeckChanged()
        {
        }

        void OnDestroy()
        {
            if (_prevButton != null) _prevButton.onClick.RemoveListener(PreviousDeck);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(NextDeck);
        }
    }
}
