using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Perks
{
    public class EnemyStats : MonoBehaviour, IModdableStats
    {
        public Dictionary<StatType, float> BaseStats { get; private set; } = new();
        public Dictionary<StatType, List<Modifier>> Modifiers { get; private set; } = new();
        public event System.Action OnStatsChanged;

        [Header("Base Values")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float baseAttackDamage = 10f;
        [SerializeField] private float baseAttackRate = 1f;
        [SerializeField] private float baseMaxHealth = 100f;

        private Health _health;
        private EnemyController _controller;

        void Awake()
        {
            _health = GetComponent<Health>();
            _controller = GetComponent<EnemyController>();

            BaseStats[StatType.EnemyMoveSpeed] = baseMoveSpeed;
            BaseStats[StatType.EnemyAttackDamage] = baseAttackDamage;
            BaseStats[StatType.EnemyAttackRate] = baseAttackRate;
            BaseStats[StatType.EnemyMaxHealth] = baseMaxHealth;
        }

        void Start()
        {
            ApplyStats();
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
            ApplyStats();
        }

        public void RemoveModifier(StatType type, object source)
        {
            if (!Modifiers.ContainsKey(type)) return;
            Modifiers[type].RemoveAll(m => m.Source == source);
            OnStatsChanged?.Invoke();
            ApplyStats();
        }

        private void ApplyStats()
        {
            if (_controller != null)
            {
                _controller.CurrentMoveSpeed = GetStat(StatType.EnemyMoveSpeed);
                _controller.CurrentAttackDamage = GetStat(StatType.EnemyAttackDamage);
                _controller.CurrentAttackCooldown = 1f / Mathf.Max(GetStat(StatType.EnemyAttackRate), 0.01f);
            }

            if (_health != null)
            {
                float newMax = GetStat(StatType.EnemyMaxHealth);
                float ratio = _health.maxHealth > 0f ? _health.currentHealth / _health.maxHealth : 1f;
                _health.maxHealth = newMax;
                _health.currentHealth = Mathf.Min(_health.currentHealth, newMax);

                if (_health.maxHealth > 0f)
                {
                    _health.currentHealth = Mathf.Max(_health.currentHealth, ratio * newMax);
                }
            }
        }
    }
}
