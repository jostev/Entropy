using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entropy.Perks
{
    public class PerksManager : MonoBehaviour
    {
        public static PerksManager Instance { get; private set; }

        [Header("Available Perk Prefabs")]
        public List<PerkBase> AvailablePerks;

        public List<IPerk> ActivePerks { get; private set; } = new();

        public event System.Action OnPerksChanged;

        private IModdableStats _playerStats;

        private static List<string> _persistedPerkIDs = new();
        private bool _isRehydrating;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            _playerStats = GetComponentInParent<PlayerStats>();
            if (_playerStats == null) return;

            if (_persistedPerkIDs.Count > 0)
            {
                _isRehydrating = true;
                foreach (var id in _persistedPerkIDs.ToList())
                {
                    InstantiateAndEquip(id);
                }
                _isRehydrating = false;
            }
        }

        public void GrantPerk(string perkID)
        {
            var prefab = AvailablePerks.Find(p => p.ID == perkID);
            if (prefab == null) return;

            if (prefab.ExclusivityGroup != ExclusivityGroup.None)
            {
                var existing = ActivePerks
                    .OfType<PerkBase>()
                    .FirstOrDefault(p => p.ExclusivityGroup == prefab.ExclusivityGroup);

                if (existing != null)
                    RemovePerkInstance(existing);
            }

            InstantiateAndEquip(perkID);

            if (!_isRehydrating)
                _persistedPerkIDs.Add(perkID);

            OnPerksChanged?.Invoke();
        }

        private void InstantiateAndEquip(string perkID)
        {
            var prefab = AvailablePerks.Find(p => p.ID == perkID);
            if (prefab == null) return;

            var instance = Instantiate(prefab, transform);
            instance.OnEquip(_playerStats);
            ActivePerks.Add(instance);
        }

        public bool HasPerk(string perkID)
        {
            return ActivePerks.Any(p => p.ID == perkID);
        }

        public void RemovePerkInstance(IPerk perk)
        {
            perk.OnRemove(_playerStats);
            ActivePerks.Remove(perk);
            if (perk is MonoBehaviour mb)
                Destroy(mb.gameObject);

            OnPerksChanged?.Invoke();
        }

        public void ClearAllPerks()
        {
            foreach (var perk in ActivePerks)
            {
                perk.OnRemove(_playerStats);
                if (perk is MonoBehaviour mb) Destroy(mb.gameObject);
            }
            ActivePerks.Clear();
            _persistedPerkIDs.Clear();
        }
    }
}
