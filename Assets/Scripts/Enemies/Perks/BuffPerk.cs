using UnityEngine;

namespace Entropy.Perks
{
    public class BulkUpPerk : StatPerk
    {
        void Awake()
        {
            _targetStat = StatType.EnemyMaxHealth;
            _operationValue = 1.5f;
            _modType = ModifierType.Multiply;
        }
    }
}
