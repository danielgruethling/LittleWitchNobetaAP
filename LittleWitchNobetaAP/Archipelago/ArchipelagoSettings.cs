using MelonLoader;

namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoSettings
{
    public enum GoalType
    {
        Vanilla,
        MagicMaster,
        BossHunt,
        LoreKeeper,
    }

    public enum AbyssTrialRequirementType
    {
        Vanilla,
        RandomizedItem,
        MagicMaster,
        BossHunt,
        LoreKeeper,
    }

    public enum GameDifficulty
    {
        Standard,
        Advanced,
        Hard,
        BossRush
    }

    public enum MagicPuzzleGateBehaviourType
    {
        Vanilla,
        AlwaysOpen,
        Randomized
    }

    public enum RandomizeLore
    {
        Vanilla,
        Randomized,
        ChecksOnly,
        NoLore,
    }

    public enum MagicUpgradeMode
    {
        Vanilla,
        BossKill
    }

    public enum ShortcutGateBehaviourType
    {
        Vanilla,
        AlwaysOpen,
        Randomized
    }

    public enum StartLevelSetting
    {
        Random,
        OkunShrine,
        UndergroundCave,
        LavaRuins,
        DarkTunnel,
        SpiritRealm
    }

    public GoalType Goal;
    public AbyssTrialRequirementType AbyssTrialRequirement;
    public ShortcutGateBehaviourType ShortcutGateBehaviour;
    public MagicPuzzleGateBehaviourType BarrierBehaviour;
    public StartLevelSetting StartLevel;
    public bool NoManaRegeneration;
    public bool RandomizeBossSoulsEnabled;
    public bool RandomizedBossTokensEnabled;
    public bool SkippableBossesEnabled;
    public RandomizeLore RandomizeLoreEnabled;
    public bool RandomizeBreakableWallsEnabled;
    public bool RandomizeJugsEnabled;
    public bool RandomizeBarrelsEnabled;
    public bool RandomizeBrokenDollsEnabled;
    public bool RandomizeLightOrbEnabled;
    public bool RandomizeCrystalBallsEnabled;
    public bool RandomizeCrystalsEnabled;
    public bool EntranceRandomizationEnabled;
    public bool TrialKeysEnabled;
    public bool DeathLinkEnabled;
    public int TrialKeyAmount;
    public int SoulGainBaseValue;
    public int SoulGainFactor;

    public bool DisableDarkTunnelThunderWall;
    public bool DisableDarkTunnelBridgeCollapse;
    public bool DisableUnimportantCutscenes;

    public ArchipelagoSettings(Dictionary<string, object> slotData)
    {
        foreach (var kv in slotData)
        {
            Melon<LwnApMod>.Logger.Msg($"{kv.Key} = {kv.Value}");
        }

        Goal = (GoalType)(long)slotData["goal"];
        AbyssTrialRequirement = (AbyssTrialRequirementType)(long)slotData["abyss_trial_requirement"];
        ShortcutGateBehaviour = (ShortcutGateBehaviourType)(long)slotData["shortcut_gate_behaviour"];
        BarrierBehaviour = (MagicPuzzleGateBehaviourType)(long)slotData["barrier_behaviour"];
        StartLevel = StartLevelSetting.OkunShrine;
        NoManaRegeneration = (bool)slotData["no_mana_regeneration"];
        RandomizeBossSoulsEnabled = (bool)slotData["randomize_boss_souls"];
        RandomizedBossTokensEnabled = (bool)slotData["randomize_boss_tokens"];
        SkippableBossesEnabled = (bool)slotData["skippable_bosses"];
        RandomizeLoreEnabled = (RandomizeLore)(long)slotData["randomize_lore"];
        EntranceRandomizationEnabled = (bool)slotData["entrance_randomization"];
        TrialKeysEnabled = (bool)slotData["trial_keys"];
        DeathLinkEnabled = (bool)slotData["death_link"];
        RandomizeBreakableWallsEnabled = (bool)slotData["randomize_breakable_walls"];
        RandomizeJugsEnabled = (bool)slotData["randomize_jugs"];
        RandomizeBarrelsEnabled = (bool)slotData["randomize_barrels"];
        RandomizeBrokenDollsEnabled = (bool)slotData["randomize_broken_dolls"];
        RandomizeLightOrbEnabled = (bool)slotData["randomize_light_orb"];
        RandomizeCrystalBallsEnabled = (bool)slotData["randomize_crystal_balls"];
        RandomizeCrystalsEnabled = (bool)slotData["randomize_crystals"];
        DisableDarkTunnelThunderWall = (bool)slotData["disable_dark_tunnel_thunder_wall"];
        DisableDarkTunnelBridgeCollapse = (bool)slotData["disable_dark_tunnel_bridge_collapse"];
        DisableUnimportantCutscenes = (bool)slotData["disable_unimportant_cutscenes"];
    }
}