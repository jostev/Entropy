using System.Collections.Generic;

namespace Entropy.Perks
{
    public interface IModdableStats
    {
        Dictionary<StatType, float> BaseStats { get; }
        Dictionary<StatType, List<Modifier>> Modifiers { get; }
        float GetStat(StatType type);
        void AddModifier(StatType type, Modifier mod);
        void RemoveModifier(StatType type, object source);
        event System.Action OnStatsChanged;
    }
}
