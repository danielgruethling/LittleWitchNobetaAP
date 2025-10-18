using HarmonyLib;
using Il2Cpp;
using LittleWitchNobetaAP.Archipelago;
using MelonLoader;

namespace LittleWitchNobetaAP.Patches;

public static class SaveGamePatches
{
    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.CreateNewGameSave))]
    private static class UIGameSaveCreateNewGameSave
    {
        [HarmonyPrefix]
        private static bool UIGameSaveCreateNewGameSavePrefix(UIGameSave __instance, GameDifficulty difficulty)
        {
            Melon<LwnApMod>.Logger.Msg("Interrupting create new game save");
            MovementPatches.BlockInput = true;
            LwnApMod.ShowApConnectionUI = true;

            return true;
        }
    }

    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.LoadGameSave))]
    private static class UIGameSaveLoadGameSave
    {
        [HarmonyPrefix]
        private static bool UIGameSaveLoadGameSavePrefix(UIGameSave __instance, int previewDataIndex)
        {
            Melon<LwnApMod>.Logger.Msg("Interrupting load game save");
            MovementPatches.BlockInput = true;
            LwnApMod.ShowApConnectionUI = true;

            return true;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.DeleteGameSave))]
    private static class GameDeleteGameSave
    {
        [HarmonyPostfix]
        private static void GameDeleteGameSavePostfix(int index)
        {
            Melon<LwnApMod>.Logger.Msg($"Triggered archipelago delete data save on delete game save, index: {index}");
            var saveFilePath = $"UserData/Slot{index + 1}.cfg";
            if (File.Exists(saveFilePath)) File.Delete(saveFilePath);
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave), new Type[] { })]
    private static class GameWriteGameSave
    {
        [HarmonyPostfix]
        private static void GameWriteGameSavePostfix()
        {
            Melon<LwnApMod>.Logger.Msg("Triggered archipelago data save on game save");
            ArchipelagoClient.ApSaveFile?.Save();
        }
    }
}