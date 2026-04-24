namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoSettings
{
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

    public GameDifficulty Difficulty;
    public ShortcutGateBehaviourType ShortcutGateBehaviour;
    public MagicPuzzleGateBehaviourType BarrierBehaviour;
    public StartLevelSetting StartLevel;
    public bool RandomizeBossSoulsEnabled;
    public bool RandomizeLoreEnabled;
    public bool EntranceRandomizationEnabled;
    public bool TrialKeysEnabled;
    public bool DeathLinkEnabled;
    public int TrialKeyAmount;
    public int SoulGainBaseValue;
    public int SoulGainFactor;
    
    public ArchipelagoSettings(Dictionary<string, object> slotData)
    {
        Difficulty = (GameDifficulty)(long)slotData["difficulty"];
        ShortcutGateBehaviour =  (ShortcutGateBehaviourType)(long)slotData["shortcut_gate_behaviour"];
        BarrierBehaviour = (MagicPuzzleGateBehaviourType)(long)slotData["barrier_behaviour"];
        StartLevel = StartLevelSetting.OkunShrine;
        RandomizeBossSoulsEnabled = (bool)slotData["randomize_boss_souls"];
        RandomizeLoreEnabled = (bool)slotData["randomize_lore"];
        EntranceRandomizationEnabled = (bool)slotData["entrance_randomization"];
        TrialKeysEnabled = (bool)slotData["trial_keys"];
        DeathLinkEnabled = (bool)slotData["death_link"];
    }
}