using UnityEngine;

namespace Entropy.Perks
{
    public class RapidStrikePerk : StatPerk
    {
        void Awake()
        {
            _targetStat = StatType.EnemyAttackRate;
            _operationValue = 1.5f;
            _modType = ModifierType.Multiply;
        }
    }
}
