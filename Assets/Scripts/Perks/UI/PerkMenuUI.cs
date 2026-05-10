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

        void Awake()
        {
            if (_database == null)
                _database = FindAnyObjectByType<PerkUIDatabase>();

            EnsureHierarchyBuilt();

            if (_passiveEntryPrefab == null)
                _passiveEntryPrefab = FindAnyObjectByType<PassivePerkEntry>();
            if (_activeSlotPrefab == null)
                _activeSlotPrefab = FindAnyObjectByType<ActivePerkSlot>();
        }

        private void EnsureHierarchyBuilt()
        {
            // Don't create Canvas here — parent PerkMenuManager already has one
            // We just need to ensure this object fills the parent canvas
            var rect = GetComponent<RectTransform>();
            if (rect == null)
                rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _passiveListContainer = FindOrCreateChild("PassivePanel", new Vector2(0.02f, 0.1f), new Vector2(0.32f, 0.9f));
            _activeSlotsContainer = FindOrCreateChild("ActivePanel", new Vector2(0.68f, 0.1f), new Vector2(0.98f, 0.9f));

            var statGo = FindOrCreateChild("StatSummary", new Vector2(0.35f, 0.7f), new Vector2(0.65f, 0.9f));
            _statSummaryPanel = statGo.GetComponent<StatSummaryPanel>() ?? statGo.gameObject.AddComponent<StatSummaryPanel>();

            var legendGo = FindOrCreateChild("ControlLegend", new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.15f));
            _controlLegend = legendGo.GetComponent<ControlLegend>() ?? legendGo.gameObject.AddComponent<ControlLegend>();

            AddPanelBackground(_passiveListContainer, new Color(0.08f, 0.08f, 0.1f, 0.95f));
            AddPanelBackground(_activeSlotsContainer, new Color(0.08f, 0.08f, 0.1f, 0.95f));
            AddPanelBackground(statGo, new Color(0.1f, 0.1f, 0.12f, 0.9f));
            AddPanelBackground(legendGo, new Color(0.06f, 0.06f, 0.07f, 0.9f));

            AddVerticalLayout(_passiveListContainer);
            AddGridLayout(_activeSlotsContainer, 3);
        }

        private RectTransform FindOrCreateChild(string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var existing = transform.Find(name);
            if (existing != null) return existing as RectTransform;

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private void AddPanelBackground(RectTransform target, Color color)
        {
            if (target.GetComponent<Image>() != null) return;
            var img = target.gameObject.AddComponent<Image>();
            img.color = color;
        }

        private void AddVerticalLayout(RectTransform target)
        {
            if (target.GetComponent<VerticalLayoutGroup>() != null) return;
            var vlg = target.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 8;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
        }

        private void AddGridLayout(RectTransform target, int columns)
        {
            if (target.GetComponent<GridLayoutGroup>() != null) return;
            var glg = target.gameObject.AddComponent<GridLayoutGroup>();
            glg.padding = new RectOffset(12, 12, 12, 12);
            glg.spacing = new Vector2(8, 8);
            glg.cellSize = new Vector2(120, 120);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = columns;
            glg.childAlignment = TextAnchor.UpperCenter;
        }

        private RectTransform FindChildRect(string name)
        {
            var t = transform.Find(name);
            if (t != null) return t as RectTransform;

            foreach (RectTransform child in GetComponentsInChildren<RectTransform>(true))
            {
                if (child.name == name) return child;
            }
            return null;
        }

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
                entry.Bind(first, data, stackCount);
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

        private PerkCategory GetCategory(IPerk perk)
        {
            if (perk is PerkBase pb) return pb.Category;
            if (perk is AdvancedPerk) return PerkCategory.Active;
            return PerkCategory.Passive;
        }
    }
}
