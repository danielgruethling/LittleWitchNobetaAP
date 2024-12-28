using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using MelonLoader;

namespace LittleWitchNobetaAP.Patches;

public static class ConfigPatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave))]
    private static class GameWriteGameSave
    {
        [HarmonyPostfix]
        private static void GameWriteGameSavePostfix()
        {
            Melon<LwnApMod>.Logger.Msg("Triggered archipelago data save on game save");
            ArchipelagoSaveFile.Save();
        }
    }
}