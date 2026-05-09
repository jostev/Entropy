using UnityEngine;

namespace Entropy.Perks
{
    public class QuickFeedPerk : StatPerk
    {
        void Awake()
        {
            _targetStat = StatType.ReloadTime;
            _operationValue = 0.6f;
            _modType = ModifierType.Multiply;
        }
    }
}
