using UnityEngine;

namespace Entropy.Perks
{
    public class SwiftFootPerk : StatPerk
    {
        void Awake()
        {
            _targetStat = StatType.EnemyMoveSpeed;
            _operationValue = 1.3f;
            _modType = ModifierType.Multiply;
        }
    }
}
