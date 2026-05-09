using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Perks.UI
{
    public class PerkMenuUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PerkUIDatabase _database;
        [SerializeField] private HexPerkMenuTheme _theme;

        [Header("Panels")]
        [SerializeField] private RectTransform _passiveListContainer;
        [SerializeField] private StatSummaryPanel _statSummaryPanel;
        [SerializeField] private RectTransform _activeSlotsContainer;
        [SerializeField] private ControlLegend _controlLegend;

        [Header("Prefabs")]
        [SerializeField] private PassivePerkEntry _passiveEntryPrefab;
        [SerializeField] private ActivePerkSlot _activeSlotPrefab;

        [Header("Settings")]
        [SerializeField] private int _maxActiveSlots = 6;

        private List<PassivePerkEntry> _passiveEntries = new();
        private List<ActivePerkSlot> _activeSlots = new();

        void Start()
        {
            if (_activeSlotPrefab != null && _activeSlotsContainer != null)
            {
                for (int i = 0; i < _maxActiveSlots; i++)
                {
                    var slot = Instantiate(_activeSlotPrefab, _activeSlotsContainer);
                    slot.SetEmpty();
                    _activeSlots.Add(slot);
                }
            }

            if (PerksManager.Instance != null)
                PerksManager.Instance.OnPerksChanged += OnPerksChanged;
        }

        void OnDestroy()
        {
            if (PerksManager.Instance != null)
                PerksManager.Instance.OnPerksChanged -= OnPerksChanged;
        }

        void OnPerksChanged()
        {
            RefreshPassivePanel();
            _statSummaryPanel?.Refresh(false);
        }

        public void Refresh(bool showStatsSummary = false)
        {
            RefreshPassivePanel();
            _statSummaryPanel?.Refresh(showStatsSummary);
            RefreshActivePanel();
            _controlLegend?.Refresh();
        }

        public void ToggleStatsSummary()
        {
            _statSummaryPanel?.Toggle();
        }

        public void OnMenuClosed()
        {
            _statSummaryPanel?.Hide();
        }

        private void RefreshPassivePanel()
        {
            foreach (var entry in _passiveEntries)
            {
                if (entry != null) Destroy(entry.gameObject);
            }
            _passiveEntries.Clear();

            if (PerksManager.Instance == null) return;

            var grouped = PerksManager.Instance.ActivePerks
                .Where(p => GetCategory(p) == PerkCategory.Passive)
                .GroupBy(p => p.ID)
                .ToList();

            if (_passiveEntryPrefab == null || _passiveListContainer == null) return;
            foreach (var group in grouped)
            {
                var first = group.First();
                var data = _database?.Get(first.ID);
                int stackCount = group.Count();

                var entry = Instantiate(_passiveEntryPrefab, _passiveListContainer);
                entry.Bind(first, data, stackCount, _theme);
                _passiveEntries.Add(entry);
            }
        }

        private void RefreshActivePanel()
        {
            foreach (var slot in _activeSlots)
            {
                slot.SetEmpty();
            }
        }

        private HexPerkMenuTheme GetTheme() => _theme;

        private PerkCategory GetCategory(IPerk perk)
        {
            if (perk is PerkBase pb) return pb.Category;
            if (perk is AdvancedPerk) return PerkCategory.Active;
            return PerkCategory.Passive;
        }
    }
}
