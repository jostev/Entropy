using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Entropy.Perks
{
    public class PerksShopGenerator : MonoBehaviour
    {
        [Header("Pools")]
        public List<PerkBase> AllPerks;

        private List<PerkBase> _currentSelection;

        public List<IPerk> GetRandomSelection(int count)
        {
            _currentSelection = new List<PerkBase>();
            var pool = new List<PerkBase>(AllPerks);

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int idx = Random.Range(0, pool.Count);
                _currentSelection.Add(pool[idx]);
                pool.RemoveAt(idx);
            }

            return _currentSelection.Cast<IPerk>().ToList();
        }

        public void Reroll()
        {
            if (_currentSelection != null)
                GetRandomSelection(_currentSelection.Count);
        }
    }
}
