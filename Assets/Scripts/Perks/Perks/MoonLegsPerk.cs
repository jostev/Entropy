using UnityEngine;

namespace Entropy.Perks
{
    public class MoonLegsPerk : StatPerk
    {
        void Awake()
        {
            _targetStat = StatType.GravityScale;
            _operationValue = 0.55f;
            _modType = ModifierType.Multiply;
        }
    }
}
