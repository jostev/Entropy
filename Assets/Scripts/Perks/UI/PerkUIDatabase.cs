using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Perks.UI
{
    [CreateAssetMenu(fileName = "PerkUIDatabase", menuName = "Entropy/Perk UI Database")]
    public class PerkUIDatabase : ScriptableObject
    {
        public List<PerkDisplayData> Entries = new();

        private Dictionary<string, PerkDisplayData> _lookup;

        public PerkDisplayData Get(string perkID)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(perkID, out var data);
            return data;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, PerkDisplayData>();
            foreach (var entry in Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.PerkID)) continue;
                _lookup[entry.PerkID] = entry;
            }
        }

        void OnValidate()
        {
            _lookup = null;
        }
    }
}
