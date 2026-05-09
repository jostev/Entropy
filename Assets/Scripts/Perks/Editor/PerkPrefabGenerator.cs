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

                if (comp is GravityWellPerk grav)
                {
                    so.FindProperty("_pullRadius").floatValue = 6f;
                    so.FindProperty("_pullForce").floatValue = 25f;
                    so.FindProperty("_duration").floatValue = 2.5f;
                    so.FindProperty("_explosionForce").floatValue = 12f;
                    so.FindProperty("_explosionRadius").floatValue = 8f;
                }

                if (comp is RailgunBodyPerk rail)
                {
                    so.FindProperty("_damageMultiplier").floatValue = def.operationValue;
                    so.FindProperty("_minSpeed").floatValue = def.duration;
                    so.FindProperty("_hitRadius").floatValue = 0.6f;
                    so.FindProperty("_damageCooldown").floatValue = 0.2f;
                }

                if (comp is ZeroGFieldPerk zg)
                {
                    so.FindProperty("_fieldRadius").floatValue = 5f;
                    so.FindProperty("_duration").floatValue = 3f;
                }

                if (comp is MassTransferPerk mass)
                {
                    so.FindProperty("_massMultiplier").floatValue = def.operationValue;
                }

                if (comp is ElasticSkinPerk skin)
                {
                    so.FindProperty("_damageReductionMultiplier").floatValue = def.operationValue;
                }

                if (comp is SonicThresholdPerk sonic)
                {
                    so.FindProperty("_speedThreshold").floatValue = def.operationValue;
                    so.FindProperty("_coneLength").floatValue = def.duration;
                    so.FindProperty("_coneAngle").floatValue = def.damageMultiplier;
                    so.FindProperty("_staggerForce").floatValue = 10f;
                    so.FindProperty("_cooldown").floatValue = 2f;
                }

                if (comp is VoidAnchorPerk voida)
                {
                    so.FindProperty("_minImpactVelocity").floatValue = def.operationValue;
                    so.FindProperty("_pinRadius").floatValue = def.duration;
                    so.FindProperty("_pinDuration").floatValue = def.damageMultiplier;
                    so.FindProperty("_pinForce").floatValue = 50f;
                }

                if (comp is RecoilEnginePerk recoil)
                {
                    so.FindProperty("_recoilForce").floatValue = def.operationValue;
                }

                if (comp is WreckingBallPerk wreck)
                {
                    so.FindProperty("_maxGrappleDistance").floatValue = 25f;
                    so.FindProperty("_springStrength").floatValue = 80f;
                    so.FindProperty("_springDamper").floatValue = 10f;
                    so.FindProperty("_maxSwingSpeed").floatValue = 30f;
                }

                if (comp is EntropyFieldPerk entropy)
                {
                    so.FindProperty("_changeInterval").floatValue = 3f;
                    so.FindProperty("_maxGravityDeviation").floatValue = 4f;
                    so.FindProperty("_maxFrictionDeviation").floatValue = 0.4f;
                    so.FindProperty("_maxDragDeviation").floatValue = 2f;
                }

                if (comp is PhaseWeightPerk phase)
                {
                    so.FindProperty("_ghostLayer").intValue = 8;
                }

                if (comp is GravityInversionPerk gravInv)
                {
                    so.FindProperty("_duration").floatValue = def.duration;
                    so.FindProperty("_enemyRecalibrationDelay").floatValue = 1.5f;
                    so.FindProperty("_cooldown").floatValue = def.damageMultiplier;
                }

                if (comp is BubbleCascadePerk bubble)
                {
                    so.FindProperty("_sphereRadius").floatValue = 0.8f;
                    so.FindProperty("_explosionForce").floatValue = 15f;
                    so.FindProperty("_explosionRadius").floatValue = 3f;
                    so.FindProperty("_chainDelay").floatValue = 0.1f;
                    so.FindProperty("_bubbleLifetime").floatValue = 6f;
                }

                if (comp is UnstoppableRoundPerk unstoppable)
                {
                    so.FindProperty("_superMass").floatValue = 999999f;
                    so.FindProperty("_roundInterval").intValue = (int)def.operationValue;
                }

                if (comp is OrbitalDefensePerk orbital)
                {
                    so.FindProperty("_orbitRadius").floatValue = 1.5f;
                    so.FindProperty("_baseOrbitSpeed").floatValue = 180f;
                    so.FindProperty("_orbitHeight").floatValue = 1.2f;
                    so.FindProperty("_orbitalMass").floatValue = 8f;
                    so.FindProperty("_deflectionRadius").floatValue = 0.4f;
                }

                so.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                DestroyImmediate(go);
                created++;
            }

            AssetDatabase.Refresh();
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
            new("gravity_well", "Gravity Well", "Bullets spawn a micro-singularity on impact. Pulls nearby physics objects inward for 2.5s before collapsing violently.", typeof(GravityWellPerk), PerkRarity.Rare, ExclusivityGroup.BulletBehavior, StatType.None, 0f, ModifierType.Add),
            new("railgun_body", "Railgun Body", "Your body deals damage proportional to current velocity on contact. Sprint into enemies.", typeof(RailgunBodyPerk), PerkRarity.Rare, ExclusivityGroup.Mass, StatType.None, 1.0f, ModifierType.Add, 4f),
            new("zero_g_field", "Zero-G Field", "Kills zero out gravity in a 5m radius for 3s. Enemies and loose objects float upward.", typeof(ZeroGFieldPerk), PerkRarity.Rare, ExclusivityGroup.Gravity, StatType.None, 0f, ModifierType.Add, 5f),
            new("mass_transfer", "Mass Transfer", "On kill, absorb the target's mass into your next bullet. Heavier enemies yield heavier projectiles.", typeof(MassTransferPerk), PerkRarity.Rare, ExclusivityGroup.Mass, StatType.None, 1.0f, ModifierType.Add),
            new("elastic_skin", "Elastic Skin", "Incoming attacks lose 50% damage on contact. Reduced momentum; enemies drop at your feet.", typeof(ElasticSkinPerk), PerkRarity.Rare, ExclusivityGroup.None, StatType.None, 0.5f, ModifierType.Add),
            new("ricochet_king", "Ricochet King", "Bullets bounce up to 5 times off any surface. Each bounce multiplies speed by 1.1x.", typeof(RicochetKingPerk), PerkRarity.Rare, ExclusivityGroup.BulletBehavior, StatType.None, 0f, ModifierType.Add),
            new("sonic_threshold", "Sonic Threshold", "Reaching maximum sprint speed emits a forward shockwave, staggering all enemies in a 4m cone.", typeof(SonicThresholdPerk), PerkRarity.Rare, ExclusivityGroup.MovementFriction, StatType.None, 16f, ModifierType.Add, 4f, 60f),
            new("void_anchor", "Void Anchor", "Ground-slam to generate a localized gravity spike. All enemies within 8m are pinned to the floor for 4s.", typeof(VoidAnchorPerk), PerkRarity.Rare, ExclusivityGroup.Gravity, StatType.None, 8f, ModifierType.Add, 8f, 4f),
            new("recoil_engine", "Recoil Engine", "Every shot propels you in the opposite direction of fire. Full aerial maneuverability via firearm physics alone.", typeof(RecoilEnginePerk), PerkRarity.Epic, ExclusivityGroup.None, StatType.None, 12f, ModifierType.Add),
            new("wrecking_ball", "Wrecking Ball", "Grapple hook with full Newtonian momentum conservation. Pendulum physics apply to your arc.", typeof(WreckingBallPerk), PerkRarity.Epic, ExclusivityGroup.None, StatType.None, 0f, ModifierType.Add),
            new("entropy_field", "Entropy Field", "Room physics constants drift randomly the longer you stay. Gravity, drag, and friction become increasingly unstable.", typeof(EntropyFieldPerk), PerkRarity.Epic, ExclusivityGroup.RoomEffect, StatType.None, 0f, ModifierType.Add),
            new("phase_weight", "Phase Weight", "Toggle between normal and near-zero mass. At zero mass you pass through physics colliders entirely.", typeof(PhaseWeightPerk), PerkRarity.Epic, ExclusivityGroup.Mass, StatType.None, 0f, ModifierType.Add),
            new("gravity_inversion", "Gravity Inversion", "Invert room gravity for 8s. Enemy AI takes 1.5s to recalibrate to the new orientation.", typeof(GravityInversionPerk), PerkRarity.Epic, ExclusivityGroup.Gravity, StatType.None, 0f, ModifierType.Add, 8f, 15f),
            new("bubble_cascade", "Bubble Cascade", "Killed enemies become unstable physics spheres. Shooting one detonates it, chaining to neighbors.", typeof(BubbleCascadePerk), PerkRarity.Epic, ExclusivityGroup.RoomEffect, StatType.None, 0f, ModifierType.Add),
            new("unstoppable_round", "Unstoppable Round", "One bullet per magazine has near-infinite effective mass. It passes through all geometry without slowing.", typeof(UnstoppableRoundPerk), PerkRarity.Epic, ExclusivityGroup.Mass, StatType.None, 1f, ModifierType.Add),
            new("orbital_defense", "Orbital Defense", "A dense object orbits you via centripetal force. Deflects bullets via actual collision physics.", typeof(OrbitalDefensePerk), PerkRarity.Epic, ExclusivityGroup.Defense, StatType.None, 0f, ModifierType.Add),
            new("bullet_time", "Bullet Time", "Global timeScale drops to 12%. Your input and UI run at full unscaled speed. The world pauses — you don't.", typeof(BulletTimePerk), PerkRarity.Legendary, ExclusivityGroup.Time, StatType.None, 0.12f, ModifierType.Add, 10f),
            new("void_collapse", "Void Collapse", "Room-scale gravity pulse: all objects crushed to a central point, then detonated outward with the full accumulated kinetic energy.", typeof(VoidCollapsePerk), PerkRarity.Legendary, ExclusivityGroup.RoomEffect, StatType.None, 0f, ModifierType.Add, 2f),
            new("temporal_anchor", "Temporal Anchor", "Freeze yourself in spacetime for 3s. Immune to all forces. Bullets and explosions pass through you without interaction.", typeof(TemporalAnchorPerk), PerkRarity.Legendary, ExclusivityGroup.Time, StatType.None, 0f, ModifierType.Add, 3f, 20f),
            new("inertia_mirror", "Inertia Mirror", "All kinetic forces applied to you are reflected back at 150% magnitude. Walk into bullets. Let explosions destroy the room.", typeof(InertiaMirrorPerk), PerkRarity.Legendary, ExclusivityGroup.Defense, StatType.None, 1.5f, ModifierType.Add),
            new("density_star", "Density Star", "Briefly become neutron-star density. Everything within 15m is crushed inward. You cannot move, but nothing can kill you.", typeof(DensityStarPerk), PerkRarity.Legendary, ExclusivityGroup.Mass, StatType.None, 500f, ModifierType.Add, 5f),
            new("event_horizon", "Event Horizon", "Create a point of no return. Any object crossing a 5m radius cannot escape the gravity gradient regardless of applied force.", typeof(EventHorizonPerk), PerkRarity.Legendary, ExclusivityGroup.Gravity, StatType.None, 50f, ModifierType.Add),
            new("hawking_resonance", "Hawking Resonance", "Passively radiate physics-damaging energy proportional to your current mass. The heavier you are, the more damage you emit.", typeof(HawkingResonancePerk), PerkRarity.Legendary, ExclusivityGroup.Mass, StatType.None, 2f, ModifierType.Add),
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
