using UnityEngine;

namespace Entropy.Perks
{
    /// <summary>
    /// Simple perk that applies one stat modifier on equip and removes it on cleanup.
    /// </summary>
    public class StatPerk : PerkBase
    {
        [Header("Stat Modifier")]
        [SerializeField] protected StatType _targetStat;
        [SerializeField] protected float _operationValue;
        [SerializeField] protected ModifierType _modType;

        public override void OnEquip(IModdableStats target)
        {
            target.AddModifier(_targetStat, new Modifier(_operationValue, _modType, this));
        }

        public override void OnRemove(IModdableStats target)
        {
            target.RemoveModifier(_targetStat, this);
        }
    }
}