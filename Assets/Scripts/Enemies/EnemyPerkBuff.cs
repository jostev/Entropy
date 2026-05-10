using System;
using Entropy.Perks;
using UnityEngine;

namespace Entropy.Enemies
{
    public class EnemyPerkBuff : MonoBehaviour
    {
        public enum BuffType
        {
            SprintCoils,
            ThickShell,
            DenseCore,
            MoonLegs,
            MagneticBoots
        }

        [Serializable]
        public class BuffConfig
        {
            public BuffType Type;
            public string DisplayName;
            public Color VisualColor = Color.white;
            public float SpeedMultiplier = 1f;
            public float HealthMultiplier = 1f;
            public float DamageMultiplier = 1f;
            public float ScaleMultiplier = 1f;
        }

        [Header("Buffs Pool")]
        public BuffConfig[] Buffs = new BuffConfig[]
        {
            new BuffConfig
            {
                Type = BuffType.SprintCoils,
                DisplayName = "Sprint Coils",
                VisualColor = new Color(0.4f, 0.9f, 0.4f),
                SpeedMultiplier = 1.25f
            },
            new BuffConfig
            {
                Type = BuffType.ThickShell,
                DisplayName = "Thick Shell",
                VisualColor = new Color(0.75f, 0.75f, 0.75f),
                HealthMultiplier = 1.4f
            },
            new BuffConfig
            {
                Type = BuffType.DenseCore,
                DisplayName = "Dense Core",
                VisualColor = new Color(0.3f, 0.6f, 1f),
                DamageMultiplier = 1.3f
            },
            new BuffConfig
            {
                Type = BuffType.MoonLegs,
                DisplayName = "Moon Legs",
                VisualColor = new Color(0.7f, 0.3f, 1f),
                ScaleMultiplier = 1.15f
            },
            new BuffConfig
            {
                Type = BuffType.MagneticBoots,
                DisplayName = "Magnetic Boots",
                VisualColor = new Color(1f, 0.7f, 0.2f),
                SpeedMultiplier = 1.1f,
                HealthMultiplier = 1.2f
            }
        };

        public BuffConfig ActiveBuff { get; private set; }

        public void ApplyRandomBuff()
        {
            if (Buffs == null || Buffs.Length == 0) return;
            var buff = Buffs[UnityEngine.Random.Range(0, Buffs.Length)];
            ApplyBuff(buff);
        }

        public void ApplyBuff(BuffConfig buff)
        {
            ActiveBuff = buff;

            var ec = GetComponent<EnemyController>();
            if (ec != null && buff.SpeedMultiplier != 1f)
                ec.CurrentMoveSpeed *= buff.SpeedMultiplier;

            var health = GetComponent<Health>();
            if (health != null && buff.HealthMultiplier != 1f)
            {
                health.maxHealth *= buff.HealthMultiplier;
                health.currentHealth = health.maxHealth;
            }

            var weapon = GetComponent<EnemyWeapon>();
            if (weapon != null && buff.DamageMultiplier != 1f)
                weapon.damage *= buff.DamageMultiplier;

            if (buff.ScaleMultiplier != 1f)
                transform.localScale *= buff.ScaleMultiplier;

            ApplyVisuals(buff);
        }

        private void ApplyVisuals(BuffConfig buff)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                if (rend == null) continue;

                var mat = new Material(rend.material);
                mat.color = Color.Lerp(mat.color, buff.VisualColor, 0.4f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", buff.VisualColor * 0.25f);
                rend.material = mat;
            }
        }
    }
}
