using UnityEngine;

namespace Entropy.Perks
{
    public class HeavyBlowPerk : StatPerk
    {
        void Awake()
        {
            _targetStat = StatType.EnemyAttackDamage;
            _operationValue = 1.4f;
            _modType = ModifierType.Multiply;
        }
    }
}
