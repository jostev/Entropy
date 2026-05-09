using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entropy.Perks.UI
{
    public class PerkDebugMenu : MonoBehaviour
    {
        [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
        [SerializeField] private CanvasGroup _canvasGroup;

        private bool _isOpen;

        void Start()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            if (PerksManager.Instance != null)
                PerksManager.Instance.OnPerksChanged += RefreshAllRows;
        }

        void OnDestroy()
        {
            if (PerksManager.Instance != null)
                PerksManager.Instance.OnPerksChanged -= RefreshAllRows;
        }

        void RefreshAllRows()
        {
            var rows = GetComponentsInChildren<PerkDebugRow>();
            foreach (var row in rows)
                row.Refresh();
        }

        void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
                Toggle();
        }

        public void Toggle()
        {
            _isOpen = !_isOpen;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _isOpen ? 1f : 0f;
                _canvasGroup.blocksRaycasts = _isOpen;
                _canvasGroup.interactable = _isOpen;
            }
        }

        public void AddPerk(string perkID)
        {
            PerksManager.Instance?.GrantPerk(perkID);
            RefreshAllRows();
        }

        public void RemovePerk(string perkID)
        {
            var manager = PerksManager.Instance;
            if (manager == null) return;
            var toRemove = manager.ActivePerks.LastOrDefault(p => p.ID == perkID);
            if (toRemove != null) manager.RemovePerkInstance(toRemove);
            RefreshAllRows();
        }

        public int GetStackCount(string perkID)
        {
            return PerksManager.Instance?.ActivePerks.Count(p => p.ID == perkID) ?? 0;
        }

        public IReadOnlyList<string> GetAvailablePerkIDs()
        {
            var manager = PerksManager.Instance;
            if (manager == null) return new List<string>();
            return manager.AvailablePerks.Select(p => p.ID).ToList();
        }

        public string GetPerkName(string perkID)
        {
            var manager = PerksManager.Instance;
            var prefab = manager?.AvailablePerks.FirstOrDefault(p => p.ID == perkID);
            return prefab?.Title ?? perkID;
        }
    }
}
