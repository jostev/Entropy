using UnityEngine;

namespace Entropy.Perks
{
    public class SprintCoilsPerk : AdvancedPerk
    {
        private const float MULTIPLIER = 1.25f;

        public override void OnEquip(IModdableStats target)
        {
            target.AddModifier(StatType.ForwardSpeed, new Modifier(MULTIPLIER, ModifierType.Multiply, this));
            target.AddModifier(StatType.StrafeSpeed, new Modifier(MULTIPLIER, ModifierType.Multiply, this));
            target.AddModifier(StatType.BackwardSpeed, new Modifier(MULTIPLIER, ModifierType.Multiply, this));
            target.AddModifier(StatType.SpeedInAir, new Modifier(MULTIPLIER, ModifierType.Multiply, this));
        }

        public override void OnRemove(IModdableStats target)
        {
            target.RemoveModifier(StatType.ForwardSpeed, this);
            target.RemoveModifier(StatType.StrafeSpeed, this);
            target.RemoveModifier(StatType.BackwardSpeed, this);
            target.RemoveModifier(StatType.SpeedInAir, this);
        }
    }
}
