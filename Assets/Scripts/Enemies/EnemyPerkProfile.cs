using UnityEngine;

namespace Entropy.Perks
{
    [CreateAssetMenu(fileName = "EnemyPerkProfile", menuName = "Entropy/Enemy Perk Profile")]
    public class EnemyPerkProfile : ScriptableObject
    {
        [System.Serializable]
        public class RarityPool
        {
            public PerkRarity rarity;
            public PerkBase[] perks;
        }

        public RarityPool[] pools;

        public PerkBase GetRandomPerk(PerkRarity rarity)
        {
            foreach (var pool in pools)
            {
                if (pool.rarity == rarity && pool.perks.Length > 0)
                {
                    return pool.perks[Random.Range(0, pool.perks.Length)];
                }
            }
            return null;
        }
    }
}
