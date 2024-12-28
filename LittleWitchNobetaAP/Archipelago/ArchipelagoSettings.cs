using MelonLoader;

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

    public enum MagicUpgradeMode
    {
        Vanilla,
        BossKill
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

    public enum ShortcutGateBehaviourType
    {
        Vanilla,
        AlwaysOpen,
        Randomized,
    }

    public enum MagicPuzzleGateBehaviourType
    {
        Vanilla,
        AlwaysOpen,
        Randomized,
    }

    public ArchipelagoSettings()
    {
    }
}