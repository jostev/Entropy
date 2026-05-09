using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Entropy.Perks.UI
{
    public class StatSummaryPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _summaryText;
        [SerializeField] private GameObject _emptyState;

        private bool _isVisible;

        void Awake()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
            _isVisible = false;
        }

        public void Refresh(bool forceVisible)
        {
            _isVisible = forceVisible;
            UpdateVisibility();
            if (_isVisible) RebuildSummary();
        }

        public void Toggle()
        {
            _isVisible = !_isVisible;
            UpdateVisibility();
            if (_isVisible) RebuildSummary();
        }

        public void Hide()
        {
            _isVisible = false;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (_panelRoot != null) _panelRoot.SetActive(_isVisible);
        }

        private void RebuildSummary()
        {
            var playerStats = FindAnyObjectByType<PlayerStats>();
            if (playerStats == null)
            {
                _summaryText.text = "No player stats found.";
                return;
            }

            var lines = new List<string>();
            bool hasModifiers = false;

            foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
            {
                if (stat == StatType.None) continue;
                if (!playerStats.Modifiers.ContainsKey(stat) || playerStats.Modifiers[stat].Count == 0)
                    continue;

                hasModifiers = true;
                float baseValue = playerStats.BaseStats.ContainsKey(stat) ? playerStats.BaseStats[stat] : 0f;
                float currentValue = playerStats.GetStat(stat);

                float multiplyProduct = 1f;
                float addSum = 0f;
                bool overridden = false;
                float overrideValue = 0f;

                foreach (var mod in playerStats.Modifiers[stat])
                {
                    switch (mod.Type)
                    {
                        case ModifierType.Multiply:
                            multiplyProduct *= mod.Value;
                            break;
                        case ModifierType.Add:
                            addSum += mod.Value;
                            break;
                        case ModifierType.Override:
                            overridden = true;
                            overrideValue = mod.Value;
                            break;
                    }
                }

                string line = FormatStatLine(stat, baseValue, currentValue, multiplyProduct, addSum, overridden, overrideValue);
                lines.Add(line);
            }

            if (!hasModifiers)
            {
                _summaryText.text = "No stat modifiers active.";
                if (_emptyState != null) _emptyState.SetActive(true);
            }
            else
            {
                _summaryText.text = string.Join("\n", lines);
                if (_emptyState != null) _emptyState.SetActive(false);
            }
        }

        private string FormatStatLine(StatType stat, float baseVal, float currentVal,
            float multiplyProduct, float addSum, bool overridden, float overrideValue)
        {
            string statName = NiceStatName(stat);

            if (overridden)
            {
                return $"<color=#ff6666>{statName}: OVERRIDE {overrideValue:0.##} (was {baseVal:0.##})</color>";
            }

            var parts = new List<string>();

            if (Mathf.Abs(multiplyProduct - 1f) > 0.001f)
            {
                float pct = (multiplyProduct - 1f) * 100f;
                string sign = pct >= 0 ? "+" : "";
                parts.Add($"{sign}{pct:0.#}%");
            }

            if (Mathf.Abs(addSum) > 0.001f)
            {
                string sign = addSum >= 0 ? "+" : "";
                parts.Add($"{sign}{addSum:0.##}");
            }

            string changeText = string.Join(" ", parts);
            if (string.IsNullOrEmpty(changeText)) changeText = "No change";

            return $"<b>{statName}</b>: {currentVal:0.##} <size=80%><color=#aaaaaa>(base {baseVal:0.##})</color></size>\n  <color=#88ff88>{changeText}</color>";
        }

        private string NiceStatName(StatType stat)
        {
            return stat switch
            {
                StatType.ForwardSpeed => "Forward Speed",
                StatType.StrafeSpeed => "Strafe Speed",
                StatType.BackwardSpeed => "Backward Speed",
                StatType.SpeedInAir => "Air Speed",
                StatType.JumpForce => "Jump Force",
                StatType.ReloadTime => "Reload Speed",
                StatType.BulletMass => "Bullet Mass",
                StatType.PlayerMass => "Player Mass",
                StatType.BulletDrag => "Bullet Drag",
                StatType.BulletDamage => "Bullet Damage",
                StatType.BulletSpeed => "Bullet Speed",
                StatType.GravityScale => "Gravity",
                _ => stat.ToString()
            };
        }
    }
}
