using UnityEngine;

namespace Entropy.Perks
{
    public class StatPerk : PerkBase
    {
        [Header("Stat Modifier")]
        [SerializeField] protected StatType _targetStat;
        [SerializeField] protected float _operationValue;
        [SerializeField] protected ModifierType _modType;

        public StatType TargetStat => _targetStat;
        public float OperationValue => _operationValue;
        public ModifierType ModType => _modType;

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