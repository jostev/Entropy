using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Entropy.Perks.Editor
{
    public class PerkPrefabGenerator : EditorWindow
    {
        [MenuItem("Entropy/Generate Perk Prefabs")]
        static void Open() => GetWindow<PerkPrefabGenerator>("Perk Prefab Generator");

        [SerializeField] private string outputFolder = "Assets/Prefabs/Perks";

        private Vector2 scroll;

        void OnGUI()
        {
            GUILayout.Label("Perk Prefab Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

            GUILayout.Space(10);

            if (GUILayout.Button("Generate All Perk Prefabs", GUILayout.Height(40)))
            {
                GenerateAll();
            }

            GUILayout.Space(10);

            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var def in Definitions)
            {
                GUILayout.Label($"{def.id} ({def.scriptType.Name})", EditorStyles.miniLabel);
            }
            GUILayout.EndScrollView();
        }

        void GenerateAll()
        {
            int created = 0;
            foreach (var def in Definitions)
            {
                string rarityFolder = $"{outputFolder}/{def.rarity}";
                if (!AssetDatabase.IsValidFolder(rarityFolder))
                {
                    string parent = outputFolder;
                    string folderName = def.rarity.ToString();
                    if (!AssetDatabase.IsValidFolder(rarityFolder))
                        AssetDatabase.CreateFolder(parent, folderName);
                }

                string prefabPath = $"{rarityFolder}/{def.id}.prefab";

                GameObject go = new GameObject(def.id);

                var comp = go.AddComponent(def.scriptType);
                if (comp == null)
                {
                    Debug.LogError($"Failed to add {def.scriptType.Name} to {def.id}");
                    DestroyImmediate(go);
                    continue;
                }

                var so = new SerializedObject(comp);
                so.FindProperty("_id").stringValue = def.id;
                so.FindProperty("_title").stringValue = def.title;
                so.FindProperty("_description").stringValue = def.description;
                so.FindProperty("_rarity").enumValueIndex = (int)def.rarity;
                so.FindProperty("_category").enumValueIndex = (int)PerkCategory.Passive;
                so.FindProperty("_exclusivityGroup").enumValueIndex = (int)def.exclusivity;

                if (comp is StatPerk statPerk)
                {
                    so.FindProperty("_targetStat").enumValueIndex = (int)def.targetStat;
                    so.FindProperty("_operationValue").floatValue = def.operationValue;
                    so.FindProperty("_modType").enumValueIndex = (int)def.modType;
                }

                if (comp is ScavengerProtocolPerk scav)
                {
                    so.FindProperty("_dropChanceBonus").floatValue = def.operationValue;
                }

                if (comp is TarRoundsPerk tar)
                {
                    so.FindProperty("_dragMultiplier").floatValue = def.operationValue;
                    so.FindProperty("_duration").floatValue = def.duration;
                }

                if (comp is ConductorPerk cond)
                {
                    so.FindProperty("_maxChains").intValue = (int)def.operationValue;
                    so.FindProperty("_chainRange").floatValue = def.duration;
                    so.FindProperty("_chainDamageMultiplier").floatValue = def.damageMultiplier;
                }

                if (comp is InertiaThiefPerk thief)
                {
                    so.FindProperty("_velocityInheritPercent").floatValue = def.operationValue;
                }

                if (comp is MagneticBootsPerk boots)
                {
                    so.FindProperty("_groundedDrag").floatValue = def.operationValue;
                }

                if (comp is PressureWavePerk wave)
                {
                    so.FindProperty("_shockwaveRadius").floatValue = def.operationValue;
                    so.FindProperty("_forceMultiplier").floatValue = def.duration;
                }

                if (comp is FrictionBurnPerk burn)
                {
                    so.FindProperty("_damageMultiplier").floatValue = def.operationValue;
                    so.FindProperty("_minSpeed").floatValue = def.duration;
                }

                so.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                DestroyImmediate(go);
                created++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"Generated {created} perk prefabs in {outputFolder}");
        }

        static readonly List<PerkDef> Definitions = new()
        {
            new("sprint_coils", "Sprint Coils", "+25% movement speed across all surfaces.", typeof(SprintCoilsPerk), PerkRarity.Common, ExclusivityGroup.MovementFriction, StatType.ForwardSpeed, 1.25f, ModifierType.Multiply),
            new("dense_core", "Dense Core", "Bullet mass increased 60%. Greater momentum transfer staggers light enemies.", typeof(DenseCorePerk), PerkRarity.Common, ExclusivityGroup.Mass, StatType.BulletMass, 1.6f, ModifierType.Multiply),
            new("slipstream", "Slipstream", "Bullet drag reduced 70%. Projectiles maintain velocity across longer distances.", typeof(SlipstreamPerk), PerkRarity.Common, ExclusivityGroup.BulletBehavior, StatType.BulletDrag, 0.3f, ModifierType.Multiply),
            new("thick_shell", "Thick Shell", "Player mass +40%. Knockback forces proportionally reduced.", typeof(ThickShellPerk), PerkRarity.Common, ExclusivityGroup.Mass, StatType.PlayerMass, 1.4f, ModifierType.Multiply),
            new("quick_feed", "Quick Feed", "Reload speed +40%. Nano-actuators cycle the action faster.", typeof(QuickFeedPerk), PerkRarity.Common, ExclusivityGroup.None, StatType.ReloadTime, 0.6f, ModifierType.Multiply),
            new("rubber_soles", "Rubber Soles", "Fall damage reduced 60%.", typeof(StatPerk), PerkRarity.Common, ExclusivityGroup.None, StatType.None, 0f, ModifierType.Add),
            new("grip_tape", "Grip Tape", "Ground friction +50%. Tighter directional turns.", typeof(StatPerk), PerkRarity.Common, ExclusivityGroup.MovementFriction, StatType.None, 1.5f, ModifierType.Multiply),
            new("scavenger_protocol", "Scavenger Protocol", "Enemies drop 35% more ammo.", typeof(ScavengerProtocolPerk), PerkRarity.Common, ExclusivityGroup.None, StatType.None, 0.35f, ModifierType.Add),
            new("ricochet", "Ricochet", "Bullets reflect off hard surfaces once, retaining 80% velocity.", typeof(RicochetPerk), PerkRarity.Uncommon, ExclusivityGroup.BulletBehavior, StatType.None, 0f, ModifierType.Add),
            new("moon_legs", "Moon Legs", "Personal gravity reduced 45%. Floaty descent.", typeof(MoonLegsPerk), PerkRarity.Uncommon, ExclusivityGroup.Gravity, StatType.GravityScale, 0.55f, ModifierType.Multiply),
            new("tar_rounds", "Tar Rounds", "Bullets coat targets in high-friction polymer. Enemy drag triples for 5s.", typeof(TarRoundsPerk), PerkRarity.Uncommon, ExclusivityGroup.BulletBehavior, StatType.None, 3f, ModifierType.Add, 5f),
            new("magnetic_boots", "Magnetic Boots", "Ground friction maxed while standing. Immune to knockback when planted.", typeof(MagneticBootsPerk), PerkRarity.Uncommon, ExclusivityGroup.MovementFriction, StatType.None, 10f, ModifierType.Add),
            new("pressure_wave", "Pressure Wave", "Landing emits radial shockwave. Force scales with fall velocity squared.", typeof(PressureWavePerk), PerkRarity.Uncommon, ExclusivityGroup.MovementFriction, StatType.None, 4f, ModifierType.Add, 2f),
            new("friction_burn", "Friction Burn", "Sliding into enemies deals damage equal to 0.5x velocity magnitude.", typeof(FrictionBurnPerk), PerkRarity.Uncommon, ExclusivityGroup.MovementFriction, StatType.None, 0.5f, ModifierType.Add, 3f),
            new("conductor", "Conductor", "Bullets arc between targets, jumping to up to 3 nearby enemies.", typeof(ConductorPerk), PerkRarity.Uncommon, ExclusivityGroup.BulletBehavior, StatType.None, 3f, ModifierType.Add, 6f, 0.6f),
            new("inertia_thief", "Inertia Thief", "On kill, inherit 20% of target's velocity as a speed boost.", typeof(InertiaThiefPerk), PerkRarity.Uncommon, ExclusivityGroup.None, StatType.None, 0.2f, ModifierType.Add),
        };

        class PerkDef
        {
            public string id;
            public string title;
            public string description;
            public Type scriptType;
            public PerkRarity rarity;
            public ExclusivityGroup exclusivity;
            public StatType targetStat;
            public float operationValue;
            public ModifierType modType;
            public float duration;
            public float damageMultiplier;

            public PerkDef(string id, string title, string description, Type scriptType, PerkRarity rarity, ExclusivityGroup exclusivity, StatType targetStat, float operationValue, ModifierType modType, float duration = 0f, float damageMultiplier = 0f)
            {
                this.id = id;
                this.title = title;
                this.description = description;
                this.scriptType = scriptType;
                this.rarity = rarity;
                this.exclusivity = exclusivity;
                this.targetStat = targetStat;
                this.operationValue = operationValue;
                this.modType = modType;
                this.duration = duration;
                this.damageMultiplier = damageMultiplier;
            }
        }
    }
}
