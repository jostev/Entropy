using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Entropy.Perks
{
    public class PlayerStats : MonoBehaviour, IModdableStats
    {
        public Dictionary<StatType, float> BaseStats { get; private set; } = new();
        public Dictionary<StatType, List<Modifier>> Modifiers { get; private set; } = new();
        public event System.Action OnStatsChanged;

        private RigidbodyFirstPersonController _rbfps;
        private PistolShoot _pistol;
        private Rigidbody _rb;

        void Awake()
        {
            _rbfps = GetComponent<RigidbodyFirstPersonController>();
            _pistol = GetComponentInChildren<PistolShoot>();
            _rb = GetComponent<Rigidbody>();

            BaseStats[StatType.ForwardSpeed] = _rbfps.movementSettings.ForwardSpeed;
            BaseStats[StatType.StrafeSpeed] = _rbfps.movementSettings.StrafeSpeed;
            BaseStats[StatType.BackwardSpeed] = _rbfps.movementSettings.BackwardSpeed;
            BaseStats[StatType.SpeedInAir] = _rbfps.movementSettings.SpeedInAir;
            BaseStats[StatType.JumpForce] = _rbfps.movementSettings.JumpForce;
            BaseStats[StatType.ReloadTime] = _pistol != null ? _pistol.reloadTime : 1.5f;
            BaseStats[StatType.BulletMass] = 1f; // Default Unity mass
            BaseStats[StatType.PlayerMass] = _rb != null ? _rb.mass : 1f;
            BaseStats[StatType.BulletDrag] = 0f;
            BaseStats[StatType.BulletDamage] = 25f; // Default from Bullet prefab
            BaseStats[StatType.BulletSpeed] = _pistol != null ? _pistol.bulletSpeed : 60f;
            BaseStats[StatType.GravityScale] = 1f;
        }

        public float GetStat(StatType type)
        {
            if (!BaseStats.ContainsKey(type)) return 0f;

            float baseValue = BaseStats[type];
            if (!Modifiers.ContainsKey(type) || Modifiers[type].Count == 0)
                return baseValue;

            bool overridden = false;
            float overrideValue = 0f;
            float multiplyProduct = 1f;
            float addSum = 0f;

            foreach (var mod in Modifiers[type])
            {
                switch (mod.Type)
                {
                    case ModifierType.Override:
                        overridden = true;
                        overrideValue = mod.Value;
                        break;
                    case ModifierType.Multiply:
                        multiplyProduct *= mod.Value;
                        break;
                    case ModifierType.Add:
                        addSum += mod.Value;
                        break;
                }
            }

            if (overridden) return overrideValue;
            return baseValue * multiplyProduct + addSum;
        }

        public void AddModifier(StatType type, Modifier mod)
        {
            if (!Modifiers.ContainsKey(type)) Modifiers[type] = new List<Modifier>();
            Modifiers[type].Add(mod);
            OnStatsChanged?.Invoke();
        }

        public void RemoveModifier(StatType type, object source)
        {
            if (!Modifiers.ContainsKey(type)) return;
            Modifiers[type].RemoveAll(m => m.Source == source);
            OnStatsChanged?.Invoke();
        }
    }
}
