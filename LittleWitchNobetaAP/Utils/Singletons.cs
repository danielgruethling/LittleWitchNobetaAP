using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace LittleWitchNobetaAP.Utils;

public static class Singletons
{
    public static Game? GameInstance { get; private set; }
    public static NobetaSkin? NobetaSkin { get; private set; }
    public static WizardGirlManage? WizardGirl { get; private set; }
    public static PlayerController? PlayerController => WizardGirl?.playerController;
    public static PlayerInputController? InputController => WizardGirl?.inputController;
    public static CharacterController? CharacterController => WizardGirl?.characterController;
    public static NobetaRuntimeData? RuntimeData => WizardGirl?.playerController?.runtimeData;
    public static SceneManager SceneManager => Game.sceneManager;
    public static ItemSystem? ItemSystem => SceneManager?.itemSystem;
    public static GameSave? GameSave { get; set; }
    public static GameSettings? GameSettings => Game.Config?.gameSettings;
    public static StageUIManager StageUi => Game.stageUI;
    public static UIPauseMenu? UIPauseMenu { get; private set; }
    public static GameUIManager? GameUIManager { get; private set; }

    public static bool SaveLoaded => GameSave is not null;

    [HarmonyPatch(typeof(Game), nameof(Game.Awake))]
    private static class GameAwake
    {
        [HarmonyPostfix]
        private static void GameAwakePostfix(Game __instance)
        {
            GameInstance = __instance;
        }
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.UpdateSkin))]
    private static class PlayerControllerUpdateSkin
    {
        [HarmonyPostfix]
        private static void PlayerControllerUpdateSkinPostfix(NobetaSkin skin)
        {
            Melon<LwnApMod>.Logger.Msg("NobetaSkin updated");

            NobetaSkin = skin;
        }
    }

    [HarmonyPatch(typeof(NobetaSkin), nameof(NobetaSkin.Dispose))]
    private static class NobetaSkinDispose
    {
        [HarmonyPrefix]
        private static void NobetaSkinDisposePrefix(NobetaSkin __instance)
        {
            Melon<LwnApMod>.Logger.Msg("NobetaSkin disposed");

            if (__instance.Pointer == NobetaSkin?.Pointer)
            {
                NobetaSkin = null;
            }
        }
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    private static class WizardGirlManageInit
    {
        [HarmonyPostfix]
        private static void WizardGirlManageInitPostfix(WizardGirlManage __instance)
        {
            Melon<LwnApMod>.Logger.Msg("WizardGirlManage created");

            WizardGirl = __instance;
        }
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Dispose))]
    private static class WizardGirlManageDispose
    {
        [HarmonyPrefix]
        private static void WizardGirlManageDisposePrefix()
        {
            Melon<LwnApMod>.Logger.Msg("WizardGirlManage disposed");

            WizardGirl = null;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.SwitchGameSave))]
    private static class GameSwitchGameSave
    {
        [HarmonyPostfix]
        private static void GameSwitchGameSavePostfix(GameSave gameSave)
        {
            Melon<LwnApMod>.Logger.Msg("Save loaded");

            GameSave = gameSave;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.SwitchTitleScene))]
    private static class GameSwitchTitleScene
    {
        [HarmonyPostfix]
        private static void GameSwitchTitleScenePostfix()
        {
            GameSave = null;
        }
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Enter))]
    private static class SceneManagerEnter
    {
        [HarmonyPostfix]
        private static void EnterScenePostfix()
        {
            Melon<LwnApMod>.Logger.Msg("Entered scene");
        }
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Init))]
    private static class UIPauseMenuInit
    {
        [HarmonyPostfix]
        private static void UIPauseMenuInitPostfix(UIPauseMenu __instance)
        {
            Melon<LwnApMod>.Logger.Msg("UIPauseMenu Init");

            UIPauseMenu = __instance;
        }
    }

    [HarmonyPatch(typeof(GameUIManager), nameof(GameUIManager.Init))]
    private static class GameUIManagerInit
    {
        [HarmonyPostfix]
        private static void GameUIManagerInitPostfix(GameUIManager __instance)
        {
            Melon<LwnApMod>.Logger.Msg("GameUIManager Init");

            GameUIManager = __instance;
        }
    }
}